 #property strict
 
 string URL = "http://localhost/api/orders";
 string SentData = "";
 
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
   
}
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
{
   string Data = TradingData();
   if (SentData != Data) {
      Print("Sending: ", Data);
      if (SendPOST(URL, Data)) {
         SentData = Data;
      }
   }
}
//+------------------------------------------------------------------+

string TradingData()
{
   string PreTradingData = "";
   string PositionSymbol = "";
   
   for(int i=0;i<OrdersTotal();i++)
   {  
      if (!OrderSelect(i,SELECT_BY_POS)) continue;
 
      if( OrderType() == OP_BUY || OrderType() == OP_SELL) 
      { 
         if (PreTradingData != "") {
            PreTradingData += "@";
         }
         //only forex
         PositionSymbol=StringSubstr(OrderSymbol(),0,6); 
         PreTradingData += OrderTicket()+":"+PositionSymbol+":"+OrderType();
      } 
   }
   return(PreTradingData);
}

int SendPOST(string URL, string str)
{
   
   int WebR; 
   int timeout = 5000;
   string cookie = NULL,headers; 
   char post[],ReceivedData[]; 
 
   StringToCharArray( str, post );
   WebR = WebRequest( "POST", URL, cookie, NULL, timeout, post, 0, ReceivedData, headers );
   if(!WebR) Print("Web request failed");
   return (WebR);
}

