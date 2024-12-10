#property strict
#include "Lib.mqh"

string Type = "Subscriber";

input EnumLotCalculationType LotCalculationType = EnumLotCalculationTypeFollowPublisher;
input double LotCalculationValue = 98;

input int Slippage = 10;

double DEFAULT_ORDER_PERCENTAGE = 0.98;

Logger *logger = NULL;
EaState *state = NULL;
TrsysClient *client = NULL;
PositionManager *positionManager = NULL;
RemoteOrderState *remoteOrders = NULL;
LocalOrderState *localOrders = NULL;
FailedCopyTradeList *failedOrders = NULL;

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
{
   //--- create timer
   subscriberConfig.LotCalculationType = LotCalculationType;
   if (subscriberConfig.LotCalculationType == EnumLotCalculationTypeLot)
   {
      subscriberConfig.LotCalculationValue = LotCalculationValue;
   }
   else
   {
      subscriberConfig.LotCalculationValue = MathMax(0, MathMin(100, LotCalculationValue)) / 100;
   }
   EventSetMillisecondTimer(100);
   logger = new Logger();
   state = new EaState(logger, Type);
   client = new TrsysClient(state, logger, Type);
   positionManager = new PositionManager(logger);
   remoteOrders = new RemoteOrderState(positionManager, client, logger);
   localOrders = new LocalOrderState(positionManager);
   failedOrders = new FailedCopyTradeList();
   logger.WriteLog("DEBUG", "Init");
   client.PostPing();
   state.SetInitializationFinish();
   client.PostLog(logger);
   //---
   return (INIT_SUCCEEDED);
}
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
   //--- destroy timer
   logger.WriteLog("DEBUG", "Deinit. Reason = " + IntegerToString(reason));
   client.PostLog(logger);
   EventKillTimer();
   delete failedOrders;
   delete localOrders;
   delete remoteOrders;
   delete positionManager;
   delete client;
   delete state;
   delete logger;
   Comment("");
}
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
{
   //---
}
//+------------------------------------------------------------------+
//| Timer function                                                   |
//+------------------------------------------------------------------+
void OnTimer()
{
   //---
   state.Begin();
   if (state.ShouldPing())
   {
      client.PostPing();
   }

   if (!state.IsEaEnabled())
   {
      client.PostLog(logger);
      return;
   }

   TicketNoDifference<LocalOrderInfo> localDiff = localOrders.GetDifference();
   if (localDiff.HasDifference())
   {
      for (int i = 0; i < localDiff.ClosedCount(); i++)
      {
         LocalOrderInfo closedInfo = localDiff.GetClosed(i);
         logger.WriteLog("DEBUG", "Local order closed. LocalOrder = " + closedInfo.ToString());
         long arr_close_ticket_no[];
         int close_order_count = localOrders.FindByServerTicketNo(closedInfo.server_ticket_no, arr_close_ticket_no);
         if (close_order_count > 0)
         {
            for (int j = 0; j < close_order_count; j++)
            {
               if (closedInfo.local_ticket_no == arr_close_ticket_no[j])
                  continue;
               if (localOrders.ExistsLocalTicketNo(arr_close_ticket_no[j]))
               {
                  positionManager.ClosePosition(closedInfo.server_ticket_no, closedInfo.symbol, closedInfo.order_type, arr_close_ticket_no[j], Slippage);
               }
            }
         }
         localOrders.Remove(closedInfo.local_ticket_no);
         positionManager.OrderClosed(closedInfo.server_ticket_no, closedInfo.symbol, closedInfo.order_type, closedInfo.local_ticket_no);
      }
      for (int i = 0; i < localDiff.OpenedCount(); i++)
      {
         LocalOrderInfo openedInfo = localDiff.GetOpened(i);
         logger.WriteLog("DEBUG", "Local order opened. LocalOrder = " + openedInfo.ToString());
         localOrders.Add(openedInfo.server_ticket_no, openedInfo.local_ticket_no, openedInfo.symbol, openedInfo.order_type);
         positionManager.OrderOpened(openedInfo.server_ticket_no, openedInfo.symbol, openedInfo.order_type, openedInfo.local_ticket_no);
      }
   }

   TicketNoDifference<CopyTradeInfo> serverDiff = remoteOrders.GetDifference();
   if (serverDiff.HasDifference())
   {
      for (int i = 0; i < serverDiff.ClosedCount(); i++)
      {
         CopyTradeInfo closedInfo = serverDiff.GetClosed(i);
         logger.WriteLog("DEBUG", "Server order closed. ServerOrder = " + closedInfo.ToString());
         long arr_close_ticket_no[];
         int close_order_count = localOrders.FindByServerTicketNo(closedInfo.server_ticket_no, arr_close_ticket_no);
         if (close_order_count == 0)
         {
            remoteOrders.Remove(closedInfo.server_ticket_no);
            failedOrders.Remove(closedInfo);
            continue;
         }
         for (int j = 0; j < close_order_count; j++)
         {
            positionManager.ClosePosition(closedInfo.server_ticket_no, closedInfo.symbol, closedInfo.order_type, arr_close_ticket_no[j], Slippage);
         }
      }
      for (int i = 0; i < serverDiff.OpenedCount(); i++)
      {
         CopyTradeInfo openedInfo = serverDiff.GetOpened(i);
         logger.WriteLog("DEBUG", "Server order opened. ServerOrder = " + openedInfo.ToString());
         if (localOrders.ExistsServerTicketNo(openedInfo.server_ticket_no))
         {
            remoteOrders.Add(openedInfo);
            continue;
         }
         int failed_count = failedOrders.ServerTicketNoFailedCount(openedInfo.server_ticket_no);
         if (failed_count >= 3)
         {
            logger.WriteLog("DEBUG", "Order failed. Skipping order. ServerOrder = " + openedInfo.ToString());
            remoteOrders.Add(openedInfo);
            continue;
         }
         if (positionManager.CreatePosition(openedInfo.server_ticket_no, openedInfo.symbol, openedInfo.order_type, openedInfo.percentage, Slippage))
         {
            failedOrders.Remove(openedInfo);
         }
         else
         {
            failedOrders.Failed(openedInfo);
         }
      }
   }
   client.PostLog(logger);
   state.End();
}
//+------------------------------------------------------------------+