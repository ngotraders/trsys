#property strict

double OrderVolume = 1;
int Slippage = 1;

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
  {
//---
   
//---
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
  {
//---
   
  }
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
//---
   string Data = SendGET("http://localhost/api/orders");
   if (Data != "") {
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
      
      // Search for newly added orders and place order.
      for (int i = 0; i < OrderCount; i++) {
         if (IsOrderExists(OrderData_ticket[i])) {
            break;
         }
         string Symbol_ = FindSymbol(OrderData_symbol[i]);
         if (Symbol_ == NULL) {
            break;
         }
         int Cmd;
         double Price;
         if (OrderData_type[i] == "BUY") {
            Cmd = OP_BUY;
            Price = Ask;
         } else if (OrderData_type[i] == "SELL") {
            Cmd = OP_SELL;
            Price = Bid;
         } else {
            break;
         }
         int OrderResult = OrderSend(Symbol_, Cmd, OrderVolume, Price, Slippage, 0, 0, NULL, OrderData_ticket[i]);
         if (!OrderResult) {
            Print("Order Send failed.", OrderData_ticket[i], " Error: ", GetLastError());
         }
      }
      
      // Search for closed orders and close order.
      for(int i = OrdersTotal() - 1; i >= 0 ; i--) {
         if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES)) continue;
         if (OrderType() != OP_BUY && OrderType() != OP_SELL) continue;

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
            Print("Order Close failed, order number: ", OrderTicket(), " Error: ", GetLastError());
         }
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
 
   WebR = WebRequest( "POST", URL, cookie, NULL, timeout, post, 0, ReceivedData, headers );
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
      if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES)) continue;
      
      if(OrderMagicNumber() == MagicNo && (OrderType() == OP_BUY || OrderType() == OP_SELL)) {
         return true;
      }
   } 
   return false;
}