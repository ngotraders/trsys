#property strict

bool DEBUG = false;
bool PERFORMANCE = false;

string Endpoint = "https://copy-trading-system.azurewebsites.net";

input double PercentOfFreeMargin = 98;
input int Slippage = 10;

double Percent = MathMax(0, MathMin(100, PercentOfFreeMargin));



//+------------------------------------------------------------------+
//| Custom classes                                                   |
//+------------------------------------------------------------------+
class EaState {
   bool m_ea_enabled;
   string m_error_message;
   void m_update_comment() {
      if (!m_ea_enabled) {
         Comment("TrsysSubscriber: 自動売買が無効です");
         return;
      }
      if (m_error_message != NULL) {
         Comment("TrsysSubscriber: " + m_error_message);
         return;
      }
      Comment("TrsysSubscriber: 正常");
   };
public:
   EaState() {
      m_ea_enabled = true;
      m_error_message = NULL;
   };
   bool IsEaEnabled() {
      m_ea_enabled = MQLInfoInteger(MQL_TRADE_ALLOWED) == 1 && AccountInfoInteger(ACCOUNT_TRADE_EXPERT) == 1 && AccountInfoInteger(ACCOUNT_TRADE_ALLOWED) == 1 && TerminalInfoInteger(TERMINAL_TRADE_ALLOWED) == 1;
      m_update_comment();
      return m_ea_enabled;
   };
   void SetError(string error_message) {
      m_error_message = error_message;
      m_update_comment();
   };
   void ClearError() {
      m_error_message = NULL;
      m_update_comment();
   };
};

const static int MAX_QUEUE_COUNT = 1000;
class LogQueue {
   string m_queue[];
   int m_current_index;
   int m_count;
public:
   LogQueue() {
      ArrayResize(m_queue, MAX_QUEUE_COUNT);
      m_current_index = 0;
      m_count = 0;
   };
   void Enqueue(string item) {
      if (m_count + 1 > MAX_QUEUE_COUNT) {
         Print("LogQueue:log message truncated, " + item);
         return;
      }
      m_queue[(m_current_index + m_count) % MAX_QUEUE_COUNT] = item;
      m_count++;
   };
   int Peak(string &str_array[], int length) {
      int peak_length = MathMin(m_count, length);
      if (peak_length == 0) {
         return 0;
      }
      ArrayResize(str_array, peak_length);
      for (int i = 0; i < peak_length; i++) {
         str_array[i] = m_queue[m_current_index + i];
      }
      return peak_length;
   };
   bool Dequeue(int length) {
      if (m_count >= length) {
         m_current_index = (m_current_index + length) % MAX_QUEUE_COUNT;
         m_count -= length;
         return true;
      }
      return false;
   };
};

class Logger : public LogQueue {
public:
   void WriteLog(string logType, string message) {
      string text = IntegerToString(GetTickCount()) + ":" + logType + ":" + message;
      if (DEBUG && logType == "DEBUG") {
         Print(message);
      }
      Enqueue(text);
   }
   
   void WriteOrderOpenSuccessLog(long serverTicketNo, string serverSymbol, string serverOrderType, int ticketNo) {
      int waitCount = 0;
      bool found = false;
      while (waitCount < 5) {
         found = PositionSelectByTicket(ticketNo);
         if (found) {
            break;
         }
         Sleep(10);
      }
      string text = IntegerToString(serverTicketNo) + ":" + serverSymbol + ":" + serverOrderType + ":" + IntegerToString(ticketNo) + ":";
      if (found) {
         text = text + IntegerToString(PositionGetInteger(POSITION_TICKET)) + ":" + PositionGetString(POSITION_SYMBOL) + ":" + IntegerToString(PositionGetInteger(POSITION_TYPE)) + ":" + DoubleToString(PositionGetDouble(POSITION_PRICE_OPEN)) + ":" + DoubleToString(PositionGetDouble(POSITION_VOLUME)) + ":" + IntegerToString(PositionGetInteger(POSITION_TIME));
      } else {
         text = text + "NA:NA:NA:NA:NA:NA";
      }
      WriteLog("OPEN", text);
   }  
   
   void WriteOrderCloseSuccessLog(long serverTicketNo, int ticketNo) {
      int waitCount = 0;
      bool found = false;
      while (waitCount < 5) {
         found = HistorySelectByPosition(ticketNo);
         if (found) {
            break;
         }
         Sleep(10);
      }
      if (found) {
         PositionSelectByTicket(ticketNo);
         long position_type = PositionGetInteger(POSITION_TYPE);
         //--- リスト中の約定の数の合計
         int deals=HistoryDealsTotal();
         //--- 取引をひとつづつ処理する
         for(int i=0;i<deals;i++) {
            ulong deal_ticket = HistoryDealGetTicket(i);
            if (HistoryDealGetInteger(deal_ticket,DEAL_TYPE) == position_type) {
               continue;
            }
            string text = IntegerToString(serverTicketNo) + ":" + IntegerToString(ticketNo) + ":";
            text = text + IntegerToString(deal_ticket) + ":" + HistoryDealGetString(deal_ticket, DEAL_SYMBOL) + ":" + IntegerToString(HistoryDealGetInteger(deal_ticket, DEAL_TYPE)) + ":" + DoubleToString(HistoryDealGetDouble(deal_ticket, DEAL_PRICE)) + ":" + DoubleToString(HistoryDealGetDouble(deal_ticket,DEAL_VOLUME)) + ":" + DoubleToString(HistoryDealGetDouble(deal_ticket,DEAL_PROFIT)) + ":" + IntegerToString(HistoryDealGetInteger(deal_ticket, DEAL_TIME));
            WriteLog("CLOSE", text);
          }
      } else {
         string text = IntegerToString(serverTicketNo) + ":" + IntegerToString(ticketNo) + ":";
         text = text + "NA:NA:NA:NA:NA:NA:NA";
         WriteLog("CLOSE", text);
      }
   }
};

template<typename T>
class List {
   T m_array[];
   int m_actual_array_length;
   int m_count;
   void m_resize() {
      m_actual_array_length *= 2;
      ArrayResize(m_array, m_actual_array_length);
   };
public:
   List() {
      m_count = 0;
      m_actual_array_length = 4;
      m_resize();
   };
   void Add(T &item) {
      m_count++;
      if (m_count > m_actual_array_length) {
         m_resize();
      }
      m_array[m_count - 1] = item;
   };
   bool Get(int index, T &item) {
      if (index > m_count) {
         return false;
      }
      item = m_array[index];
      return true;
   };
   int Length() {
      return m_count;
   };
};

struct CopyTradeInfo {
   int server_ticket_no;
   string symbol;
   int order_type;
   CopyTradeInfo() { 
      server_ticket_no = 0;
      symbol = "";
      order_type = -1;
   };
   static CopyTradeInfo Parse(string copy_trade_string, string &parse_error) {
      CopyTradeInfo info;
      string splittedValues[];
      if (StringSplit(copy_trade_string, StringGetCharacter(":", 0), splittedValues) < 3) {
         parse_error = "Invalid Data: " + copy_trade_string;
         return info;
      }

      info.server_ticket_no = (int) StringToInteger(splittedValues[0]);
      info.symbol = splittedValues[1];
      info.order_type = (int) StringToInteger(splittedValues[2]);
      return info;
   };
   
   string ToString() {
      return IntegerToString(server_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
};

class CopyTradeInfoList: List<CopyTradeInfo> {
   string m_error;
   void m_initialize(string response) {
      string processing_data = response;
      while(processing_data != "" )
      {
         string order_data = "";
         int at_position = StringFind(processing_data,"@");
         if (at_position > 0) {
            order_data = StringSubstr(processing_data, 0, at_position);
            processing_data = StringSubstr(processing_data, at_position + 1);
         } else {
            order_data = processing_data;
            processing_data = "";
         }
         
         string parse_error;
         CopyTradeInfo info = CopyTradeInfo::Parse(order_data, parse_error);
         if (parse_error != NULL) {
            m_error = "Invalid Data: " + order_data;
            return;
         }
         Add(info);
      }
   };
public:
   CopyTradeInfoList(string response) {
      m_error = NULL;
      m_initialize(response);
   };
   bool IsError() {
      return m_error != NULL;
   };
   string GetError() {
      return m_error;
   };
};

struct LocalOrderInfo {
   int server_ticket_no;
   string symbol;
   int order_type;
   LocalOrderInfo() { 
      server_ticket_no = 0;
      symbol = "";
      order_type = -1;
   };
   static LocalOrderInfo Create(int server_ticket_no, string symbol) {
      LocalOrderInfo info;
      return info;
   };
   
   string ToString() {
      return IntegerToString(server_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
};

class LocalOrderInfoList: List<LocalOrderInfo> {
};

class ApiStatus {
   string m_api_name;
   int m_last_status_code;
   int m_last_error_code;
public:
   ApiStatus(string api_name) {
      m_api_name = api_name;
      m_last_status_code = -1;
      m_last_error_code = -1;
   };

   void SetErrorCode(int error_code) {
      if (m_last_error_code != error_code) {
         if (error_code == -1) {
            Print(m_api_name + ": Recover from Error");
         } else {
            Print(m_api_name + ": " + error_code_to_string(error_code));
         }
      }
      m_last_error_code = error_code;
   };

   void SetStatusCode(int status_code) {
      int local_status_code = status_code;
      if ((200 <= status_code && status_code < 300) || status_code == 304) {
         local_status_code = 200;
      }
      if (m_last_status_code != local_status_code) {
         if (local_status_code == 200) {
            Print(m_api_name + ": OK");
         } else {
            Print(m_api_name + ": Not OK, StatusCode = ", status_code);
         }
      }
      m_last_status_code = local_status_code;
   };
};

class TrsysClient {
   string m_secret_key;
   string m_secret_token;
   double m_next_token_fetch_time;
   ApiStatus *m_post_secret_key_status;
   ApiStatus *m_post_token_release_status;
   ApiStatus *m_get_orders_status;
   ApiStatus *m_post_log_status;
   string m_get_orders_etag;
   string m_get_orders_etag_response;

   string m_generate_secret_key() {
      return "MT5/" + AccountInfoString(ACCOUNT_COMPANY) + "/" + IntegerToString(AccountInfoInteger(ACCOUNT_LOGIN)) + "/" + IntegerToString(AccountInfoInteger(ACCOUNT_TRADE_MODE));
   };
   
   string m_get_secret_token() {
      if (m_secret_token != NULL) {
         return m_secret_token;
      }
      if (m_next_token_fetch_time > GetTickCount()) {
         return NULL;
      }
      m_next_token_fetch_time = GetTickCount() + 2000; // 2sec later
      m_post_secret_key();
      return m_secret_token;
   };
   
   void m_clear_secret_token() {
      if (m_secret_token == NULL) {
         return;
      }
      m_post_token_release();
      m_secret_token = NULL;
   };

   int m_post_secret_key()
   {
      string request_headers = "Version: 20210331\r\nContent-Type: text/plain; charset=UTF-8";
      string request_data = m_secret_key;
      string response_headers;
      string response_data;
      int error_code;
      int res = m_send_web_request(m_post_secret_key_status, "POST", Endpoint + "/api/token", request_headers, request_data, response_headers, response_data, error_code);
      if(res != 200) {
         m_secret_token = NULL;
         return -1;
      }
      m_secret_token = response_data;
      return res;
   };
   
   int m_post_token_release()
   {
      string request_headers = "Version: 20210331\r\nContent-Type: text/plain; charset=UTF-8";
      string request_data;
      string response_headers;
      string response_data;
      int error_code;
   
      int res = m_send_web_request(m_post_token_release_status, "POST", Endpoint + "/api/token/" + m_secret_token + "/release", request_headers, request_data, response_headers, response_data, error_code);
      if(res != 200) {
         return -1;
      }
      m_secret_token = NULL;
      return res;
   };

   int m_send_web_request(ApiStatus *status, string method, string url, string request_headers, string request_data_string, string &response_headers, string &response_data_string, int &error_code) {
      int timeout = 5000;
      char request_data[];
      char response_data[];
      
      if (request_data_string != NULL || request_data_string != "") {
         StringToCharArray(request_data_string, request_data, 0, WHOLE_ARRAY, CP_UTF8);
      }
      
      int res = WebRequest(method, url, request_headers, timeout, request_data, response_data, response_headers);
      if (res == -1) {
         error_code = GetLastError();
         status.SetErrorCode(error_code);
         return res;
      }
      error_code = -1;
      status.SetErrorCode(error_code);
      status.SetStatusCode(res);
      if (res == 401 || res == 403) {
         m_secret_token = NULL;
         return res;
      }
      response_data_string = CharArrayToString(response_data, 0, WHOLE_ARRAY, CP_UTF8);
      return res;
   }

public:
   TrsysClient() {
      m_secret_key = m_generate_secret_key();
      m_secret_token = NULL;
      m_next_token_fetch_time = -1;
      m_post_secret_key_status = new ApiStatus("PostSecretKey");
      m_post_token_release_status = new ApiStatus("PostTokenRelease");
      m_get_orders_status = new ApiStatus("GetOrders");
      m_post_log_status = new ApiStatus("PostLog");
      m_get_orders_etag = NULL;
      m_get_orders_etag_response = NULL;
   }
   ~TrsysClient() {
      m_clear_secret_token();
      delete m_post_secret_key_status;
      delete m_post_token_release_status;
      delete m_get_orders_status;
      delete m_post_log_status;
   }
   
   int GetOrders(string &response)
   {
      string secret_token = m_get_secret_token();
      if (secret_token == NULL) {
         return -1;
      }
   
      string request_headers = "Version: 20210331\r\nX-Secret-Token: " + secret_token;
      string request_data;
      string response_headers;
      string response_data;
      int error_code;
    
      if (m_get_orders_etag != NULL) {
         request_headers += "\r\nIf-None-Match: " + m_get_orders_etag;
      }

      int res = m_send_web_request(m_get_orders_status, "GET", Endpoint + "/api/orders", request_headers, request_data, response_headers, response_data, error_code);
      if (res == 304) {
         response = m_get_orders_etag_response;
         return 200;
      }
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
            m_get_orders_etag = StringSubstr(line, 5);
            StringTrimRight(m_get_orders_etag);
            StringTrimLeft(m_get_orders_etag);
            m_get_orders_etag_response = response_data;
         }
      }

      response = response_data;
      return res;
   }
   
   int PostLog(Logger &q)
   {
      string secret_token = m_get_secret_token();
      if (secret_token == NULL) {
         return -1;
      }
      string logs[];
      int peak = q.Peak(logs, 10);
      if (peak == 0) {
         return 0;
      }

      string request_headers = "Content-Type: text/plain; charset=UTF-8\r\nVersion: 20210331\r\nX-Secret-Token: " + secret_token;
      string request_data = "";
      string response_headers;
      string response_data;
      int error_code;
      
      for (int i = 0; i < peak; i++) {
         request_data = request_data + logs[i] + "\r\n";
      }
   
      int res = m_send_web_request(m_post_log_status, "POST", Endpoint + "/api/logs", request_headers, request_data, response_headers, response_data, error_code);
      if(res != 202) {
         return -1;
      }
      q.Dequeue(peak);
      return 0;
   }

};

string error_code_to_string(int error_code) {
   switch (error_code) {
      case ERR_WEBREQUEST_INVALID_ADDRESS:
         return "Invalid URL";
      case ERR_WEBREQUEST_CONNECT_FAILED:
         return "Failed to connect";
      case ERR_WEBREQUEST_TIMEOUT:
         return "Timeout";
      case ERR_WEBREQUEST_REQUEST_FAILED:
         return "HTTP request failed";
      default:
         return "Unknown Error, Error = " + IntegerToString(error_code);
   }
}

TrsysClient *client = NULL;

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
{
//--- create timer
   EventSetMillisecondTimer(100);
   client = new TrsysClient();
//---
   return(INIT_SUCCEEDED);
}
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
//--- destroy timer
   EventKillTimer();
   delete client;
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
   static EaState state;
   static Logger logger;
   static string last_response;
   if (!state.IsEaEnabled()) {
      return;
   }

   string response;
   if (client.GetOrders(response) == 200) {
      if (last_response != response) {
         CopyTradeInfoList *list = new CopyTradeInfoList(response);
         if (!list.IsError()) {
            
            state.ClearError();
         } else {
            state.SetError(list.GetError());
            logger.WriteLog("DEBUG", list.GetError());
         }
         delete list;
      }
   } else {
      state.SetError("サーバーと通信できません。");
   }
   client.PostLog(logger);
}
//+------------------------------------------------------------------+