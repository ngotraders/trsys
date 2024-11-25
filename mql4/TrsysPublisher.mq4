#property strict
#include "Lib.mqh"

string Type = "Publisher";

input double OrderPercentage = 98;

double DEFAULT_ORDER_PERCENTAGE = 0.98;

Logger *logger = NULL;
EaState *state = NULL;
TrsysClient *client = NULL;
PositionManager *positionManager = NULL;
RemoteOrderState *remoteOrders = NULL;
LocalOrderState *localOrders = NULL;
FailedLocalOrderList *failedOrders = NULL;

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
{
   //--- create timer
   publisherConfig.OrderPercentageToSendSubscriber = MathMax(0, MathMin(100, OrderPercentage)) / 100;
   EventSetMillisecondTimer(100);
   logger = new Logger();
   state = new EaState(logger, Type);
   client = new TrsysClient(state, logger, Type);
   positionManager = new PositionManager(logger);
   remoteOrders = new RemoteOrderState(positionManager, client, logger);
   localOrders = new LocalOrderState(positionManager);
   failedOrders = new FailedLocalOrderList();
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

   TicketNoDifference<LocalOrderInfo> localDiff = localOrders.GetDifference(true);
   if (localDiff.HasDifference())
   {
      PositionInfo arr_positions[];
      int arr_positions_count = positionManager.GetPositions(arr_positions, true);
      string send_data = NULL;
      for (int i = 0; i < arr_positions_count; i++)
      {
         if (send_data != NULL)
         {
            send_data += "@";
         }
         string symbol = arr_positions[i].symbol;
         if (StringFind(symbol, "@", 1) > -1)
         {
            symbol = StringSubstr(symbol, 0, StringFind(symbol, "@", 1));
         }
         send_data += IntegerToString(arr_positions[i].local_ticket_no) + ":" + symbol + ":" + IntegerToString(arr_positions[i].order_type) + ":" + StringFormat("%i", arr_positions[i].position_time) + ":" + DoubleToString(arr_positions[i].price_open) + ":" + DoubleToString(publisherConfig.OrderPercentageToSendSubscriber);
      }
      int res = client.PostOrders(send_data);
      for (int i = 0; i < localDiff.ClosedCount(); i++)
      {
         LocalOrderInfo closedInfo = localDiff.GetClosed(i);
         if (res == 200)
         {
            logger.WriteLog("DEBUG", "Local order closed. LocalOrder = " + closedInfo.ToString());
            localOrders.Remove(closedInfo.local_ticket_no);
         }
         else
         {
            failedOrders.Failed(closedInfo);
            int failed_count = failedOrders.TicketNoFailedCount(closedInfo.local_ticket_no);
            if (failed_count >= 100)
            {
               logger.WriteLog("DEBUG", "Order close failed. 100 times retried. ServerOrder = " + closedInfo.ToString());
               failedOrders.Remove(closedInfo);
               continue;
            }
         }
      }
      for (int i = 0; i < localDiff.OpenedCount(); i++)
      {
         LocalOrderInfo openedInfo = localDiff.GetOpened(i);
         if (res == 200)
         {
            logger.WriteLog("DEBUG", "Local order opened. LocalOrder = " + openedInfo.ToString());
            localOrders.Add(openedInfo.server_ticket_no, openedInfo.local_ticket_no, openedInfo.symbol, openedInfo.order_type);
            failedOrders.Remove(openedInfo);
         }
         else
         {
            failedOrders.Failed(openedInfo);
            int failed_count = failedOrders.TicketNoFailedCount(openedInfo.local_ticket_no);
            if (failed_count >= 3)
            {
               logger.WriteLog("DEBUG", "Order open failed. Skipping order. ServerOrder = " + openedInfo.ToString());
               localOrders.Add(openedInfo.server_ticket_no, openedInfo.local_ticket_no, openedInfo.symbol, openedInfo.order_type);
               failedOrders.Remove(openedInfo);
               continue;
            }
         }
      }
   }
   client.PostLog(logger);
   state.End();
}
//+------------------------------------------------------------------+
