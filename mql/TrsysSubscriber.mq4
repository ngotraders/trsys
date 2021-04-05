#property strict

bool DEBUG = false;
bool PERFORMANCE = false;

string Endpoint = "https://copy-trading-system.azurewebsites.net";
string TokenEndpoint = Endpoint + "/api/token";
string OrderEndpoint = Endpoint + "/api/orders";
string Token = NULL;
double NextTokenFetchTime = -1;
string ETag = NULL;
string ETagResponse = NULL;

int LastErrorCode = 0;
int PreviousRes = -1;
string ProcessedData = NULL;

input double PercentOfFreeMargin = 98;
input int Slippage = 10;

double Percent = MathMax(0, MathMin(100, PercentOfFreeMargin));

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
   if (!IsExpertEnabled()) {
      Comment("TrsysSubscriber: 自動売買が無効です");
      return;
   }
   uint startTime = 0;
   if (PERFORMANCE) {
      // Timer
      startTime = GetTickCount();
      Print("OnTimer: start");
   }
   
   if (Token == NULL) {
      if (NextTokenFetchTime > GetTickCount()) {
         return;
      }
      NextTokenFetchTime = GetTickCount() + 1000; // 1sec later
      string SecretKey = GenerateSecretKey();
      int skResult = PostSecretKey(SecretKey, Token);
      if (PERFORMANCE) {
         // Timer
         Print("OnTimer: PostSecretKey finish in ", GetTickCount() - startTime, "ms");
      }
      if (skResult == -1) {
         return;
      }
      PreviousRes = -1;
      if (DEBUG) {
         // Timer
         Print("Successfully Obtained Token: ", Token);
      }
   }

   string RecievedData;
   int goResult = GetOrders(Token, RecievedData);
   if (PERFORMANCE) {
      // Timer
      Print("OnTimer: GetOrders finish in ", GetTickCount() - startTime, "ms");
   }
   if (goResult == -1) {
      // Error
     return;
   }
   if (RecievedData != ProcessedData) {
      if (DEBUG) {
         Print("Processing:", RecievedData);
      }
      string Data = RecievedData;
      bool Success = True;
      int OrderCount = 0;
      int OrderData_ticket[100];
      string OrderData_symbol[100];
      string OrderData_type[100];
   
      while( Data != "" )
      {
         if (OrderCount > 100) break;

         int AtPos = StringFind(Data,"@");
         string OrderData = "";
         if (AtPos > 0) {
            OrderData = StringSubstr(Data, 0, AtPos);
            Data = StringSubstr(Data, AtPos+1);
         } else {
            OrderData = Data;
            Data = StringSubstr(Data, StringLen(OrderData));
         }
         
         int splittedCount;
         string splittedValues[];
         splittedCount = StringSplit(OrderData, StringGetCharacter(":", 0), splittedValues);
         if (splittedCount < 3) {
            Print("Invalid Data: ", RecievedData);
            return;
         }
         int i = OrderCount;
         //separate the trading data
         OrderData_ticket[i] = (int) StringToInteger(splittedValues[0]);
         OrderData_symbol[i] = splittedValues[1];
         OrderData_type[i] = splittedValues[2];
         OrderCount++;
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
         
         if (MarketInfo(OrderSymbol(), MODE_TRADEALLOWED) != 1) {
            continue;
         } 
         if (DEBUG) {
            Print("OrderClose executing: ", OrderMagicNumber(), ", OrderTicket = ", OrderTicket());
         }
         int OrderCloseResult = OrderClose(OrderTicket(), OrderLots(), OrderClosePrice(), Slippage);
         if (OrderCloseResult < 0) {
            Success = false;
            Print("OrderClose failed.", OrderData_ticket[i], ", OrderTicket = ", OrderTicket(), ", Error = ", GetLastError());
         } else if (DEBUG) {
            Print("OrderClose success: ", OrderMagicNumber(), ", OrderTicket = ", OrderTicket());
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
         if (MarketInfo(Symbol_, MODE_TRADEALLOWED) != 1) {
            continue;
         } 
         double orderLots = CalculateVolume(Symbol_);
         if (orderLots <= 0) {
            continue;
         }
         if (DEBUG) {
            Print("OrderSend executing: ", OrderData_ticket[i], "/", Symbol_, "/", OrderData_type[i], "/", orderLots);
         }
         int OrderResult;
         if (OrderData_type[i] == "0") {
            OrderResult = OrderSend(Symbol_, OP_BUY, orderLots, SymbolInfoDouble(Symbol_, SYMBOL_ASK), Slippage, 0, 0, NULL, OrderData_ticket[i]);
         } else if (OrderData_type[i] == "1") {
            OrderResult = OrderSend(Symbol_, OP_SELL, orderLots, SymbolInfoDouble(Symbol_, SYMBOL_BID), Slippage, 0, 0, NULL, OrderData_ticket[i]);
         } else {
            continue;
         }
         if (OrderResult < 0) {
            Success = false;
            Print("OrderSend failed.", OrderData_ticket[i], " Error = ", GetLastError());
         } else if (DEBUG) {
            Print("OrderSend success: ", OrderData_ticket[i], ", OrderTicket = ", OrderResult);
         }
      }
      if (Success) {
         ProcessedData = RecievedData;
         Comment("TrsysSubscriber: 正常");
      } else {
         Comment("TrsysSubscriber: エラー");
      }
   }
   if (PERFORMANCE) {
      // Timer
      Print("OnTimer: finish in ", GetTickCount() - startTime, "ms");
   }
}
//+------------------------------------------------------------------+

string GenerateSecretKey() {
   return AccountCompany() + "/" + IntegerToString(AccountNumber()) + "/" + IntegerToString(IsDemo());
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

double CalculateVolume(string Symb) {
   double One_Lot=MarketInfo(Symb,MODE_MARGINREQUIRED); //!-lot cost
   double Min_Lot=MarketInfo(Symb,MODE_MINLOT);         // Min. amount of lots
   double Step   =MarketInfo(Symb,MODE_LOTSTEP);        //Step in volume changing
   double Free   =AccountFreeMargin();                  // Free margin
   double Lots;
   return MathFloor(Free*Percent/100/One_Lot);
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

int GetOrders(string &token, string &response)
{
   string request_headers = "Version: 20210331\r\nX-Secret-Token: " + token;
   string request_data;
   string response_headers;
   string response_data;
   int error_code;
 
   if (ETag != NULL) {
      request_headers += "\r\nIf-None-Match: " + ETag;
   }

   int res = WebRequestWrapper("GET", OrderEndpoint, request_headers, request_data, response_headers, response_data, error_code);
   if(res==-1) {
      if (LastErrorCode != error_code) {
         LastErrorCode = error_code;
         LogWebRequestError("GetOrders", error_code);
      }
      PreviousRes = -1;
      return -1;
   }
   if (LastErrorCode != 0) {
      LastErrorCode = 0;
      Print("GetOrders: Recover from Error");
   }

   if (res == 304) {
      response = ETagResponse;
      return 200;
   }
   
   if (res == 401 || res == 403) {
      token = NULL;
   }

   if (PreviousRes != res) {
      if(res == 200) {
         Print("GetOrders: OK");
      } else {
         Print("GetOrders: Not OK, StatusCode = ", res);
      }
   }
   PreviousRes = res;

   if(res != 200) {
      return -1;
   }

   string tmp_header = response_headers;
   while (tmp_header != "") {
      string line;
      int eol_pos = StringFind(tmp_header, "\r\n");
      if (eol_pos < 0) {
         line = tmp_header;
         tmp_header = "";
      } else {
         line = StringSubstr(tmp_header, 0, eol_pos);
         tmp_header = StringSubstr(tmp_header, eol_pos + 2);
      }
      if (StringCompare(StringSubstr(line, 0, 5), "ETag:", false) == 0) {
         ETag = StringTrimLeft(StringTrimRight(StringSubstr(line, 5)));
         ETagResponse = response_data;
      }
   }

   response = response_data;
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
