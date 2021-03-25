#property strict

bool DEBUG = false;
bool PERFORMANCE = false;

string Endpoint = "https://copy-trading-system.herokuapp.com";
string OrderEndPoint = Endpoint + "/api/orders";
string ETag = NULL;
string ETagResponse = NULL;

int LastErrorCode = 0;
int PreviousRes = -1;
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
   uint startTime = 0;
   if (PERFORMANCE) {
      // Timer
      startTime = GetTickCount();
      Print("OnTimer: start");
   }

   string RecievedData;
   int sgResult = SendGET(OrderEndPoint, RecievedData);
   if (PERFORMANCE) {
      // Timer
      Print("OnTimer: recieve data in ", GetTickCount() - startTime, "ms");
   }
   if (sgResult == -1) {
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
         if (splittedCount != 3) {
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
         
         if (DEBUG) {
            Print("OrderClose executing: ", OrderMagicNumber());
         }
         int OrderCloseResult = OrderClose(OrderTicket(), OrderLots(), OrderClosePrice(), Slippage);
         if (OrderCloseResult < 0) {
            Success = false;
            Print("OrderClose failed.", OrderData_ticket[i], ", OrderTicket = ", OrderTicket(), ", Error = ", GetLastError());
         } else if (DEBUG) {
            Print("OrderClose success: ", OrderMagicNumber());
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
         if (DEBUG) {
            Print("OrderSend executing: ", OrderData_ticket[i], "/", Symbol_, "/", OrderData_type[i]);
         }
         int OrderResult;
         if (OrderData_type[i] == "0") {
            OrderResult = OrderSend(Symbol_, OP_BUY, OrderVolume, SymbolInfoDouble(Symbol_, SYMBOL_ASK), Slippage, 0, 0, NULL, OrderData_ticket[i]);
         } else if (OrderData_type[i] == "1") {
            OrderResult = OrderSend(Symbol_, OP_SELL, OrderVolume, SymbolInfoDouble(Symbol_, SYMBOL_BID), Slippage, 0, 0, NULL, OrderData_ticket[i]);
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
      }
   }
   if (PERFORMANCE) {
      // Timer
      Print("OnTimer: finish in ", GetTickCount() - startTime, "ms");
   }
}
//+------------------------------------------------------------------+

int SendGET(string URL, string &response)
{
   int timeout = 5000;
   string header = NULL;
   char data[];
   string result_headers;
   char result_data[];
 
   if (ETag != NULL) {
      header = "If-None-Match: \"" + ETag + "\"";
   }

   int res = WebRequest("GET", URL, header, timeout, data, result_data, result_headers);
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

   if (res == 304) {
      response = ETagResponse;
      return 200;
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

   string tmp_header = result_headers;
   bool etag_set = false;
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
         etag_set = true;
      }
   }

   response = (CharArrayToString(result_data));
   if (etag_set) {
      ETagResponse = response;
   }

   PreviousRes = res;
   if(res != 200) {
      return -1;
   }
   return res;
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