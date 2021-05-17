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
   
   void WriteOrderOpenSuccessLog(long serverTicketNo, string serverSymbol, int serverOrderType, int ticketNo) {
      int waitCount = 0;
      bool found = false;
      while (waitCount < 5) {
         found = PositionSelectByTicket(ticketNo);
         if (found) {
            break;
         }
         Sleep(10);
      }
      string text = IntegerToString(serverTicketNo) + ":" + serverSymbol + ":" + IntegerToString(serverOrderType) + ":" + IntegerToString(ticketNo) + ":";
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
   void Remove(int index) {
      if (index >= m_count) {
         Print("index must under the count");
         return;
      }
      for (int i = index + 1; i < m_count; i++) {
         m_array[i - 1] = m_array[i];
      }
      m_count--;
   };
   T Get(int index) {
      T ret;
      if (index >= m_count) {
         Print("index must under the count");
         return ret;
      }
      ret = m_array[index];
      return ret;
   };
   int Length() {
      return m_count;
   };
};

struct CopyTradeInfo {
   long server_ticket_no;
   string symbol;
   int order_type;
   CopyTradeInfo() { 
      server_ticket_no = 0;
      symbol = "";
      order_type = -1;
   };
   static CopyTradeInfo Create(long server_ticket_no, string symbol, int order_type) {
      CopyTradeInfo info;
      info.server_ticket_no = server_ticket_no;
      info.symbol = symbol;
      info.order_type = order_type;
      return info;
   };
   static CopyTradeInfo Parse(string copy_trade_string, string &parse_error) {
      string splittedValues[];
      if (StringSplit(copy_trade_string, StringGetCharacter(":", 0), splittedValues) < 3) {
         parse_error = "Invalid Data: " + copy_trade_string;
         CopyTradeInfo info;
         return info;
      }
      return Create((long) StringToInteger(splittedValues[0]), splittedValues[1], (int) StringToInteger(splittedValues[2]));
   };
   
   string ToString() {
      return IntegerToString(server_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
};

class CopyTradeInfoList: public List<CopyTradeInfo> {
public:   
   int IndexOfTicketNo(long server_ticket_no) {
      for (int i = 0; i < Length(); i++) {
         CopyTradeInfo info = Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            return i;
         }
      }
      return -1;
   };
   bool Exists(long server_ticket_no) {
      return IndexOfTicketNo(server_ticket_no) >= 0;
   };
};

class CopyTradeInfoParser {
public:
   static CopyTradeInfoList *Parse(string response, string &parse_error) {
      CopyTradeInfoList *list = new CopyTradeInfoList();
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
         
         CopyTradeInfo info = CopyTradeInfo::Parse(order_data, parse_error);
         if (parse_error != NULL) {
            parse_error = "Invalid Data: " + order_data;
            delete list;
            return NULL;
         }
         list.Add(info);
      }
      return list;
   };
};

struct LocalOrderInfo {
   long server_ticket_no;
   long local_ticket_no;
   string symbol;
   int order_type;
   LocalOrderInfo() { 
      server_ticket_no = 0;
      symbol = "";
      order_type = -1;
   };
   static LocalOrderInfo Create(long server_ticket_no, long local_ticket_no, string symbol, int order_type) {
      LocalOrderInfo info;
      info.server_ticket_no = server_ticket_no;
      info.local_ticket_no = local_ticket_no;
      info.symbol = symbol;
      info.order_type = order_type;
      return info;
   };
   
   string ToString() {
      return IntegerToString(server_ticket_no) + "/" + IntegerToString(local_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
};

class LocalOrderInfoList: public List<LocalOrderInfo> {   
   int m_index_of_local_ticket(long local_ticket_no) {
      for (int i = 0; i < Length(); i++) {
         LocalOrderInfo info = Get(i);
         if (info.local_ticket_no == local_ticket_no) {
            return i;
         }
      }
      return -1;
   };

   int m_index_of_server_ticket(long server_ticket_no) {
      for (int i = 0; i < Length(); i++) {
         LocalOrderInfo info = Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            return i;
         }
      }
      return -1;
   };
public:
   bool Create(long server_ticket_no, long local_ticket_no, string symbol, int order_type) {
      if (m_index_of_local_ticket(local_ticket_no) >= 0) {
         Print("Local ticket already exists: ", local_ticket_no);
         return false;
      }
      Add(LocalOrderInfo::Create(server_ticket_no, local_ticket_no, symbol, order_type));
      return true;
   };
   
   bool ServerTicketNoExists(long server_ticket_no) {
      return m_index_of_server_ticket(server_ticket_no) >= 0;
   };

   bool RemoveByLocalTicketNo(long local_ticket_no) {
      int index = m_index_of_local_ticket(local_ticket_no);
      if (index < 0) {
         Print("Local ticket has been closed: ", local_ticket_no);
         return false;
      }
      Remove(index);
      return true;
   };
};

struct TicketNoDifference {
   long m_opened_ticket[];
   int m_opened_ticket_count;
   long m_closed_ticket[];
   int m_closed_ticket_count;
public:
   TicketNoDifference() {
      m_opened_ticket_count = 0;
      m_closed_ticket_count = 0;
   };
   void Opened(long server_ticket_no) {
      ArrayResize(m_opened_ticket, m_opened_ticket_count + 1);
      m_opened_ticket[m_opened_ticket_count] = server_ticket_no;
      m_opened_ticket_count++;
   };
   void Closed(long server_ticket_no) {
      ArrayResize(m_closed_ticket, m_closed_ticket_count + 1);
      m_closed_ticket[m_closed_ticket_count] = server_ticket_no;
      m_closed_ticket_count++;
   };
   int OpenedCount() {
      return m_opened_ticket_count;
   };
   long GetOpened(int i) {
      return m_opened_ticket[i];
   };
   int ClosedCount() {
      return m_closed_ticket_count;
   };
   long GetClosed(int i) {
      return m_closed_ticket[i];
   };
};

class RemoteOrderState {
   CopyTradeInfoList orders;
   void m_initialize() {
      int positionCount = PositionsTotal();
      for(int i = 0; i < positionCount ; i++) {
         long position_magic = (long)PositionGetInteger(POSITION_MAGIC);
         if (position_magic == 0) continue;
         if (orders.Exists(position_magic)) continue;
         string symbol = PositionGetString(POSITION_SYMBOL);
         int order_type = (int)PositionGetInteger(POSITION_TYPE);
         orders.Add(CopyTradeInfo::Create(position_magic, symbol, order_type));
      } 
   };
public:
   RemoteOrderState() {
      m_initialize();
   };
   TicketNoDifference Diff(CopyTradeInfoList &serverInfo) {
      TicketNoDifference diff;
      for (int i = 0; i < orders.Length(); i++) {
         CopyTradeInfo ii = orders.Get(i);
         bool exists = false;
         for (int j = 0; j < serverInfo.Length(); j++) {
            CopyTradeInfo ij = serverInfo.Get(j);
            if (ii.server_ticket_no == ij.server_ticket_no) {
               exists = true;
               break;
            }
         }
         if (!exists) {
            diff.Closed(ii.server_ticket_no);
         }
      }
      for (int i = 0; i < serverInfo.Length(); i++) {
         CopyTradeInfo ii = serverInfo.Get(i);
         bool exists = false;
         for (int j = 0; j < orders.Length(); j++) {
            CopyTradeInfo ij = orders.Get(j);
            if (ii.server_ticket_no == ij.server_ticket_no) {
               exists = true;
               break;
            }
         }
         if (!exists) {
            diff.Opened(ii.server_ticket_no);
         }
      }
      return diff;
   };
   void Add(CopyTradeInfo &info) {
      orders.Add(info);
   };
   void Remove(long server_ticket_no) {
      int index = orders.IndexOfTicketNo(server_ticket_no);
      if (index > -1) {
         orders.Remove(index);
      }
   };
};

class LocalOrderState {
   LocalOrderInfoList orders;
   void m_initialize() {
      int positionCount = PositionsTotal();
      for(int i = 0; i < positionCount ; i++) {
         long position_ticket_no = (long)PositionGetTicket(i);
         if (position_ticket_no == 0) continue;
         long position_magic = PositionGetInteger(POSITION_MAGIC);
         if (position_magic == 0) continue;
         string symbol = PositionGetString(POSITION_SYMBOL);
         int order_type = (int)PositionGetInteger(POSITION_TYPE);
         orders.Create(position_magic, position_ticket_no, symbol, order_type);
      } 
   };
public:
   LocalOrderState() {
      m_initialize();
   };
   bool Exists(long server_ticket_no) {
      return orders.ServerTicketNoExists(server_ticket_no);
   };
   bool Open(long server_ticket_no, long local_ticket_no, string symbol, int order_type) {
      return orders.Create(server_ticket_no, local_ticket_no, symbol, order_type);
   };

   bool Close(long local_ticket_no) {
      return orders.RemoveByLocalTicketNo(local_ticket_no);
   };

   int FindByServerTicketNo(long server_ticket_no, long &arr_ticket_no[]) {
      int count = 0;
      for (int i = 0; i < orders.Length(); i++) {
         LocalOrderInfo info = orders.Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            count++;
         }
      }
      if (count == 0) {
         return 0;
      }
      ArrayResize(arr_ticket_no, count);
      int j = 0;
      for (int i = 0; i < orders.Length(); i++) {
         LocalOrderInfo info = orders.Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            arr_ticket_no[j] = info.local_ticket_no;
         }
      }
      return count;
   };
};

class PositionManager {
   Logger *m_logger;
   double m_calculate_volume(string symbol, ENUM_ORDER_TYPE order_type, double price) {
      double one_lot;//!-lot cost
      if (!OrderCalcMargin(order_type, symbol, 1, price, one_lot)) {
         Print("OrderCalcMargin returned false");
         return 0;
      }
      if (one_lot == 0) {
         Print("one_lot is zero");
         return 0;
      }
      double step   =SymbolInfoDouble(symbol,SYMBOL_VOLUME_STEP); // Step in volume changing
      if (step == 0) {
         Print("SymbolInfoDouble(symbol,SYMBOL_VOLUME_STEP) returned zero");
         return 0;
      }
      double free   =AccountInfoDouble(ACCOUNT_FREEMARGIN);// Free margin
      double lots   =MathFloor(free*Percent/100/one_lot/step)*step;
      return lots;
   }
   string m_find_symbol(string symbol_str) {
      for (int i = 0; i < SymbolsTotal(false); i++) {
         if (StringFind(SymbolName(i, false), symbol_str) >= 0) {
            return SymbolName(i, false);
         }
      }
      return NULL;
   }
   int m_send_open_order(string orderSymbol, ENUM_ORDER_TYPE orderType, double orderPrice, double orderLots, long magicNo) {
      //--- リクエストを準備する
      MqlTradeRequest request={0};
      request.action   =TRADE_ACTION_DEAL;                     // 取引操作タイプ
      request.symbol   =orderSymbol;                           // シンボル
      request.volume   =orderLots;                             // ロットのボリューム
      request.type     =orderType;                             // 注文タイプ
      request.price    =orderPrice;                            // 発注価格
      request.deviation=Slippage;                              // 価格からの許容偏差
      request.magic    =magicNo;                               // 注文のMagicNumber
      request.type_filling = ORDER_FILLING_IOC;
      MqlTradeCheckResult checkResult={0};
      if (!OrderCheck(request, checkResult)) {
         Print(__FUNCTION__,":", checkResult.retcode, "/", checkResult.comment);
         return -1;
      }
      MqlTradeResult result={0};
      if (!OrderSend(request, result)) {
         Print(__FUNCTION__,":", result.retcode, "/", result.comment);
         return -1;
      }
      return (int) result.order;
   }
   
   int m_send_close_order(ulong position_ticket) {
      if (!PositionSelectByTicket(position_ticket)) {
         return -1;
      }
      string position_symbol = PositionGetString(POSITION_SYMBOL);
      double volume = PositionGetDouble(POSITION_VOLUME);
      ENUM_POSITION_TYPE type = (ENUM_POSITION_TYPE)PositionGetInteger(POSITION_TYPE);
      long magic = PositionGetInteger(POSITION_MAGIC);
   
      MqlTradeRequest request={0};
      request.action    = TRADE_ACTION_DEAL;                   //          - type of trade operation
      request.position  = position_ticket;                     //          - ticket of the position
      request.symbol    = position_symbol;                     //          - symbol 
      request.volume    = volume;                              //          - volume of the position
      request.deviation = Slippage;                            //          - allowed deviation from the price
      request.magic     = magic;                               //          - MagicNumber of the position
      request.type_filling = ORDER_FILLING_IOC;
      if (type == POSITION_TYPE_BUY) {
         request.price = SymbolInfoDouble(position_symbol, SYMBOL_BID);
         request.type  = ORDER_TYPE_SELL;
      } else {
         request.price = SymbolInfoDouble(position_symbol, SYMBOL_ASK);
         request.type  = ORDER_TYPE_BUY;
      }
      MqlTradeCheckResult checkResult={0};
      if (!OrderCheck(request, checkResult)) {
         Print(__FUNCTION__,",OrderCheck:", checkResult.retcode, "/", checkResult.comment);
         return -1;
      }
      MqlTradeResult result={0};
      if (!OrderSend(request, result)) {
         Print(__FUNCTION__,",OrderSend:", result.retcode, "/", result.comment);
         return -1;
      }
      return (int) result.order;
   }
public:
   PositionManager(Logger *l_logger) {
      m_logger = l_logger;
   };
   bool CreatePosition(long server_ticket_no, string server_symbol, int server_order_type, long &ticket_no_arr[]) {
      string symbol = m_find_symbol(server_symbol);
      if (symbol == NULL) {
         m_logger.WriteLog("ERROR", "OrderSend fail: Symbol not found. " + IntegerToString(server_ticket_no) + ", Symbol = " + server_symbol);
         return true;
      }
      ENUM_ORDER_TYPE order_type;
      double order_price;
      MqlTick tick;
      if (!SymbolInfoTick(symbol, tick)) {
         m_logger.WriteLog("ERROR", "OrderSend fail: SymbolInfoTick returned false. " + IntegerToString(server_ticket_no) + ", Symbol = " + server_symbol);
         return false;
      }
      if (server_order_type == 0) {
         order_type = ORDER_TYPE_BUY;
         order_price = tick.ask;
      } else if (server_order_type == 1) {
         order_type = ORDER_TYPE_SELL;
         order_price = tick.bid;
      } else {
         m_logger.WriteLog("ERROR", "OrderSend fail: Invalid OrderType. " + IntegerToString(server_ticket_no) + ", OrderType = " + IntegerToString(server_order_type));
         return true;
      }
      double order_lots = m_calculate_volume(symbol, order_type, order_price);
      double min_lots = SymbolInfoDouble(symbol, SYMBOL_VOLUME_MIN);         // Min. amount of lots
      double max_lots = SymbolInfoDouble(symbol, SYMBOL_VOLUME_MAX);         // Max amount of lotsr
      if (order_lots <= min_lots) {
         m_logger.WriteLog("WARN", "OrderSend fail: Not enough margin. " + IntegerToString(server_ticket_no) + ", Symbol = " + server_symbol + ", Calculated lots = " + DoubleToString(order_lots));
         return true;
      }
      m_logger.WriteLog("DEBUG", "OrderSend executing: " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type) + "/" + DoubleToString(order_lots));
      bool success = false;
      while (order_lots > 0) {
         double lots;
         if (order_lots >= max_lots) {
            lots = max_lots;
            order_lots -= max_lots;
         } else {
            lots  = order_lots;
            order_lots -= order_lots;
         }
         int OrderOpenResult = m_send_open_order(symbol, order_type, order_price, lots, server_ticket_no);
         if (OrderOpenResult < 0) {
            m_logger.WriteLog("ERROR", "OrderSend failed." + IntegerToString(server_ticket_no) + ", Error = " + IntegerToString(GetLastError()));
            break;
         } else {
            ArrayResize(ticket_no_arr, ArraySize(ticket_no_arr) + 1);
            ticket_no_arr[ArraySize(ticket_no_arr) - 1] = OrderOpenResult;
            success = true;
            m_logger.WriteLog("INFO", "OrderSend success: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(OrderOpenResult));
            m_logger.WriteOrderOpenSuccessLog(server_ticket_no, server_symbol, server_order_type, OrderOpenResult);
         }
      }
      return success;
   };
   bool ClosePosition(long local_ticket_no) {
      return m_send_close_order(local_ticket_no) >= 0;
   };
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

Logger *logger = NULL;
TrsysClient *client = NULL;
PositionManager *positionManager = NULL;

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
{
//--- create timer
   EventSetMillisecondTimer(100);
   client = new TrsysClient();
   logger = new Logger();
   positionManager = new PositionManager(logger);
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
   delete positionManager;
   delete logger;
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
   static RemoteOrderState serverOrders;
   static LocalOrderState localOrders;
   static string last_response;
   if (!state.IsEaEnabled()) {
      return;
   }
   
   string response;
   if (client.GetOrders(response) == 200) {
      if (last_response != response) {
         string error = NULL;
         CopyTradeInfoList *list = CopyTradeInfoParser::Parse(response, error);
         if (error == NULL) {
            TicketNoDifference diff = serverOrders.Diff(list);
            for (int i = 0; i < diff.ClosedCount(); i++) {
               long closed_server_ticket_no = diff.GetClosed(i);
               long arr_close_ticket_no[];
               int close_order_count = localOrders.FindByServerTicketNo(closed_server_ticket_no, arr_close_ticket_no);
               if (close_order_count > 0) {
                  for (int j = 0; j < close_order_count; j++) {
                     if (positionManager.ClosePosition(arr_close_ticket_no[j])) {
                        localOrders.Close(arr_close_ticket_no[j]);
                     }
                  }
               }
               serverOrders.Remove(closed_server_ticket_no);
            }
            for (int i = 0; i < diff.OpenedCount(); i++) {
               long opened_server_ticket_no = diff.GetOpened(i);
               if (!localOrders.Exists(opened_server_ticket_no)) {
                  int index = list.IndexOfTicketNo(opened_server_ticket_no);
                  if (index >= 0) {
                     CopyTradeInfo info = list.Get(index);
                     Print("Open order", opened_server_ticket_no);
                     long arr_open_ticket_no[];
                     if (positionManager.CreatePosition(info.server_ticket_no, info.symbol, info.order_type, arr_open_ticket_no)) {
                        int size = ArraySize(arr_open_ticket_no);
                        for (int j = 0; j < size; j++) {
                           localOrders.Open(info.server_ticket_no, arr_open_ticket_no[j], info.symbol, info.order_type);
                        }
                        serverOrders.Add(info);
                     }
                  }
               }
            }
            state.ClearError();
         } else {
            state.SetError(error);
            logger.WriteLog("DEBUG", error);
         }
         if (list != NULL) {
            delete list;
         }
      }
   } else {
      state.SetError("サーバーと通信できません。");
   }
   client.PostLog(logger);
}
//+------------------------------------------------------------------+