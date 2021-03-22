#property strict

string URL = "http://localhost/api/orders";
string ProcessedData = "";
double OrderVolume = 1;
int Slippage = 10;

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
  {
//---
   EventSetMillisecondTimer(100);
//---
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
  {
//---
   EventKillTimer();
  }
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
//---
   
}
//+------------------------------------------------------------------+
//| Expert timer function                                             |
//+------------------------------------------------------------------+
void OnTimer(){
   string RecievedData = SendGET(URL);
   if (RecievedData != ProcessedData) {
      Print("Processing:", RecievedData);
      string Data = RecievedData;
      bool Success = True;
      int OrderData_ticket[100];
      string OrderData_symbol[100];
      string OrderData_type[100];
      int OrderCount = 0;
      int ColonPos;
   
      for(int i = 0; i < 100; i++)
      {
         if( Data == "" || Data == "@") break;
         
         OrderCount++;
         
         int AtPos = StringFind(Data,"@");
         string OrderData = "";
         if (AtPos > 0) {
            OrderData = StringSubstr(Data, 0, AtPos);
            Data = StringSubstr(Data, AtPos+1);
         } else {
            OrderData = Data;
            Data = StringSubstr(Data, StringLen(OrderData));
         }
         
         
         //separate the trading data
         ColonPos = StringFind(OrderData, ":");
         OrderData_ticket[i] = StringToInteger(StringSubstr(OrderData,0,ColonPos));
         OrderData = StringSubstr(OrderData, ColonPos+1);

         ColonPos = StringFind(OrderData, ":");
         OrderData_symbol[i] = StringSubstr(OrderData,0,ColonPos);
         OrderData_type[i] = StringSubstr(OrderData, ColonPos+1);
      }
      
      // Search for closed orders and close order.
      for(int i = OrdersTotal() - 1; i >= 0 ; i--) {
         if (!OrderSelect(i, SELECT_BY_POS)) continue;
         if (OrderType() != OP_BUY && OrderType() != OP_SELL) continue;
         if (OrderMagicNumber() == 0) continue;

         bool Found = false;
         for (int j = 0; j < OrderCount; j++) {
            if (OrderMagicNumber() == OrderData_ticket[i]) {
               Found = true;
               break;
            }
         }
         if (Found) continue;
         
         int OrderCloseResult = OrderClose(OrderTicket(), OrderLots(), OrderClosePrice(), Slippage);
         if (!OrderCloseResult) {
            Success = false;
            Print("Order Close failed, order number: ", OrderTicket(), " Error: ", GetLastError());
         }
      } 
      
      // Search for newly added orders and place order.
      for (int i = 0; i < OrderCount; i++) {
         if (IsOrderExists(OrderData_ticket[i])) {
            continue;
         }
         string Symbol_ = FindSymbol(OrderData_symbol[i]);
         if (Symbol_ == NULL) {
            continue;
         }
         Print("Order Sending: ", OrderData_ticket[i], "/", Symbol_, "/", OrderData_type[i]);
         int OrderResult;
         if (OrderData_type[i] == "0") {
            OrderResult = OrderSend(Symbol_, OP_BUY, OrderVolume, SymbolInfoDouble(Symbol_, SYMBOL_ASK), Slippage, 0, 0, NULL, OrderData_ticket[i]);
         } else if (OrderData_type[i] == "1") {
            OrderResult = OrderSend(Symbol_, OP_SELL, OrderVolume, SymbolInfoDouble(Symbol_, SYMBOL_BID), Slippage, 0, 0, NULL, OrderData_ticket[i]);
         } else {
            continue;
         }
         if (!OrderResult) {
            Success = false;
            Print("Order Send failed.", OrderData_ticket[i], " Error: ", GetLastError());
         }
      }
      if (Success) {
         ProcessedData = RecievedData;
      }
   }
}
//+------------------------------------------------------------------+

string SendGET(string URL)
{
   
   int WebR; 
   int timeout = 5000;
   string cookie = NULL,headers; 
   char post[],ReceivedData[]; 
 
   WebR = WebRequest( "GET", URL, cookie, NULL, timeout, post, 0, ReceivedData, headers );
   if(!WebR) Print("Web request failed");   
   
   return(CharArrayToString(ReceivedData)); 
}

string FindSymbol(string SymbolStr) {
   for (int i = 0; i < SymbolsTotal(false); i++) {
      if (StringFind(SymbolName(i, false), SymbolStr) >= 0) {
         return SymbolName(i, false);
      }
   }
   Print("No symbol found: ", SymbolStr);
   return NULL;
}

bool IsOrderExists(int MagicNo) {
   int TotalNumberOfOrders = OrdersTotal();
   for(int i = TotalNumberOfOrders - 1; i >= 0 ; i--) {
      if (!OrderSelect(i, SELECT_BY_POS)) continue;
      
      if(OrderMagicNumber() == MagicNo) {
         return true;
      }
   } 
   return false;
}