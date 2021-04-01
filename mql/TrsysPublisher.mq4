#property strict

string Endpoint = "https://copy-trading-system.azurewebsites.net";
string TokenEndpoint = Endpoint + "/api/token";
string OrderEndpoint = Endpoint + "/api/orders";
string SentData = NULL;
 
int LastErrorCode = 0;
int PreviousRes = -1;

string Token = NULL;
 
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
   if (Token != NULL) {
      PostTokenRelease(Token);
   }
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
   
   if (Token == NULL) {
      string SecretKey = GenerateSecretKey();
      int skResult = PostSecretKey(SecretKey, Token);
      if (skResult == -1) {
         return;
      }
      PreviousRes = -1;
   }

   string Data = TradingData();
   if (SentData != Data) {
      if (PreviousRes == 200) {
         PreviousRes = -1;
      }
      Print("Sending: ", Data);
      if (PostOrders(Token, Data) > 0) {
         SentData = Data;
      }
   }
}
//+------------------------------------------------------------------+

string GenerateSecretKey() {
   return AccountCompany() + "/" + IntegerToString(AccountNumber()) + "/" + IntegerToString(IsDemo());
}

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
         PreTradingData += IntegerToString(OrderTicket())+":"+PositionSymbol+":"+IntegerToString(OrderType())+":"+DoubleToString(AccountBalance()/OrderLots());
      } 
   }
   return(PreTradingData);
}

int WebRequestWrapper(string method, string url, string request_headers, string request_data_string, string &response_headers, string &response_data_string, int &error_code) {
   int timeout = 5000;
   char request_data[];
   char response_data[];
   
   if (request_data_string != NULL || request_data_string != "") {
      StringToCharArray(request_data_string, request_data, 0, WHOLE_ARRAY, CP_UTF8);
   }
   
   int res = WebRequest(method, url, request_headers, timeout, request_data, response_data, response_headers);
   if (res == -1) {
      error_code = GetLastError();
   } else {
      error_code = 0;
   }

   response_data_string = CharArrayToString(response_data, 0, WHOLE_ARRAY, CP_UTF8);
   return res;
}

int PostSecretKey(string secretKey, string &token)
{
   string request_headers = "Content-Type: text/plain; charset=UTF-8";
   string request_data = secretKey;
   string response_headers;
   string response_data;
   int error_code;

   int res = WebRequestWrapper("POST", TokenEndpoint, request_headers, request_data, response_headers, response_data, error_code);
   if(res==-1) {
      if (LastErrorCode != error_code) {
         LastErrorCode = error_code;
         LogWebRequestError("PostSecretKey", error_code);
      }
      PreviousRes = -1;
      return -1;
   }
   if (LastErrorCode != 0) {
      LastErrorCode = 0;
      Print("PostSecretKey: Recover from Error");
   }

   if (PreviousRes != res) {
      if(res == 200) {
         Print("PostSecretKey: OK");
      } else {
         Print("PostSecretKey: Not OK, StatusCode = ", res);
      }
   }
   PreviousRes = res;

   if(res != 200) {
      return -1;
   }

   token = response_data;
   return res;
}

int PostTokenRelease(string token)
{
   string request_headers = "Content-Type: text/plain; charset=UTF-8";
   string request_data;
   string response_headers;
   string response_data;
   int error_code;

   int res = WebRequestWrapper("POST", TokenEndpoint + "/" + token + "/release", request_headers, request_data, response_headers, response_data, error_code);
   if(res==-1) {
      LogWebRequestError("PostTokenRelease", error_code);
      return -1;
   }

   if(res == 200) {
      Print("PostTokenRelease: OK");
   } else {
      Print("PostTokenRelease: Not OK, StatusCode = ", res);
   }

   if(res != 200) {
      return -1;
   }
   return res;
}

int PostOrders(string &token, string orders)
{
   string request_headers = "Content-Type: text/plain; charset=UTF-8\r\nVersion: 20210331\r\nX-Secret-Token: " + token;
   string request_data = orders;
   string response_headers;
   string response_data;
   int error_code;

   int res = WebRequestWrapper("POST", OrderEndpoint, request_headers, request_data, response_headers, response_data, error_code);
   if(res==-1) {
      if (LastErrorCode != error_code) {
         LastErrorCode = error_code;
         LogWebRequestError("PostOrders", error_code);
      }
      PreviousRes = -1;
      return -1;
   }
   if (LastErrorCode != 0) {
      LastErrorCode = 0;
      Print("PostOrders: Recover from Error");
   }
   
   if (res == 401 || res == 403) {
      token = NULL;
   }

   if (PreviousRes != res) {
      if(res == 200) {
         Print("PostOrders: OK");
      } else {
         Print("PostOrders: Not OK, StatusCode = ", res);
      }
   }
   PreviousRes = res;

   if(res != 200) {
      return -1;
   }
 
   return res;
}

void LogWebRequestError(string name, int error_code) {
   switch (error_code) {
      case ERR_WEBREQUEST_INVALID_ADDRESS:
         Print(name, ": Invalid URL");
         break;
      case ERR_WEBREQUEST_CONNECT_FAILED:
         Print(name, ": Failed to connect");
         break;
      case ERR_WEBREQUEST_TIMEOUT:
         Print(name, ": Timeout");
         break;
      case ERR_WEBREQUEST_REQUEST_FAILED:
         Print(name, ": HTTP request failed");
         break;
      default:
         Print(name, ": Unknown Error, Error = ", error_code);
         break;
   }
}
