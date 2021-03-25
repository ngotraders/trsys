 #property strict
 
 string Endpoint = "https://copy-trading-system.azurewebsites.net";
 string OrderEndpoint = Endpoint + "/api/orders";
 string SentData = "";
 
int LastErrorCode = 0;
int PreviousRes = -1;
 
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
   string Data = TradingData();
   if (SentData != Data) {
      Print("Sending: ", Data);
      if (SendPOST(OrderEndpoint, Data)) {
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
         PreTradingData += IntegerToString(OrderTicket())+":"+PositionSymbol+":"+IntegerToString(OrderType());
      } 
   }
   return(PreTradingData);
}

int SendPOST(string URL, string str)
{
   int timeout = 5000;
   string request_headers = "Content-Type: text/plain; charset=UTF-8";
   char request_data[];
   string response_headers = NULL;
   char response_data[];
 
   StringToCharArray(str, request_data, 0, WHOLE_ARRAY, CP_UTF8);
   
   int res = WebRequest( "POST", URL, request_headers, timeout, request_data, response_data, response_headers);
   if(res==-1) {
      int error_code = GetLastError();
      if (LastErrorCode != error_code) {
         LastErrorCode = error_code;
         switch (error_code) {
            case ERR_WEBREQUEST_INVALID_ADDRESS:
               Print("WebRequest: Invalid URL");
               break;
            case ERR_WEBREQUEST_CONNECT_FAILED:
               Print("WebRequest: Failed to connect");
               break;
            case ERR_WEBREQUEST_TIMEOUT:
               Print("WebRequest: Timeout");
               break;
            case ERR_WEBREQUEST_REQUEST_FAILED:
               Print("WebRequest: HTTP request failed");
               break;
            default:
               Print("WebRequest: Unknown Error, Error = ", error_code);
               break;
         }
      }
      PreviousRes = -1;
      return -1;
   }
   if (LastErrorCode != 0) {
      LastErrorCode = 0;
      Print("WebRequest: Recover from Error");
   }

   if (PreviousRes != res) {
      if(res == 200) {
         Print("WebRequest: OK");
      } else {
         Print("WebRequest: Not OK, StatusCode = ", res);
      }
   }
   PreviousRes = res;

   if(res != 200) {
      return -1;
   }
 
   return res;
}

