 #property strict
 
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
      SendPOST("http://localhost/api/orders", Data);
   }
}
//+------------------------------------------------------------------+

string TradingData()
{
   string PreTradingData = "";
   string PositionSymbol = "";
   
   for(int i=0;i<OrdersTotal();i++)
   {  
      int R = OrderSelect(i,SELECT_BY_POS);
 
      if( OrderType() == OP_BUY || OrderType() == OP_SELL) 
      { 
         //only forex
         PositionSymbol=StringSubstr(OrderSymbol(),0,6); 
         PreTradingData += OrderTicket()+":"+PositionSymbol+":"+OrderType()+"@";
      } 
   }
   return(PreTradingData);
}

void SendPOST(string URL, string str)
{
   
   int WebR; 
   int timeout = 5000;
   string cookie = NULL,headers; 
   char post[],ReceivedData[]; 
 
   StringToCharArray( str, post );
   WebR = WebRequest( "POST", URL, cookie, NULL, timeout, post, 0, ReceivedData, headers );
   if(!WebR) Print("Web request failed");   
   
   Comment(CharArrayToString(ReceivedData)); 
}

