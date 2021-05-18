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
      Print("m_count = ", m_count, ", length = ", length);
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
      if (logType == "DEBUG") {
         if (DEBUG) {
            Print(message);
         }
      } else if (logType != "OPEN" && logType != "CLOSE") {
         Print(message);
      }
      Enqueue(text);
   }
};

class EaState {
   bool m_ea_enabled;
   string m_error_message;
   long m_start_time;
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
   void Begin() {
      if (PERFORMANCE) {
         m_start_time = GetTickCount();
         Print("OnTimer: start");
      }
   };
   void Lap(string comment) {
      if (PERFORMANCE) {
         Print("OnTimer: ", comment, " in ", GetTickCount() - m_start_time, "ms");
      }
   };
   void End() {
      if (PERFORMANCE) {
         Print("OnTimer: finish in ", GetTickCount() - m_start_time, "ms");
      }
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

template<typename T>
struct TicketNoDifference {
   T m_opened_ticket[];
   int m_opened_ticket_count;
   T m_closed_ticket[];
   int m_closed_ticket_count;
public:
   TicketNoDifference() {
      m_opened_ticket_count = 0;
      m_closed_ticket_count = 0;
   };
   void Opened(T &server_ticket) {
      ArrayResize(m_opened_ticket, m_opened_ticket_count + 1);
      m_opened_ticket[m_opened_ticket_count] = server_ticket;
      m_opened_ticket_count++;
   };
   void Closed(T &server_ticket) {
      ArrayResize(m_closed_ticket, m_closed_ticket_count + 1);
      m_closed_ticket[m_closed_ticket_count] = server_ticket;
      m_closed_ticket_count++;
   };
   bool HasDifference() {
      return m_opened_ticket_count > 0 || m_closed_ticket_count > 0;
   };
   int OpenedCount() {
      return m_opened_ticket_count;
   };
   T GetOpened(int i) {
      return m_opened_ticket[i];
   };
   int ClosedCount() {
      return m_closed_ticket_count;
   };
   T GetClosed(int i) {
      return m_closed_ticket[i];
   };
};

class RemoteOrderState {
   List<CopyTradeInfo> m_orders;
   string m_last_server_response;

   void m_initialize() {
      int position_count = PositionsTotal();
      for(int i = 0; i < position_count; i++) {
         long local_ticket_no = (long)PositionGetTicket(i);
         if (local_ticket_no == 0) continue;
         long server_ticket_no = (long)PositionGetInteger(POSITION_MAGIC);
         if (server_ticket_no == 0) continue;
         if (m_index_of(server_ticket_no) >= 0) continue;
         string symbol = PositionGetString(POSITION_SYMBOL);
         int order_type = ConvertToOrderType((ENUM_POSITION_TYPE)PositionGetInteger(POSITION_TYPE));
         m_orders.Add(CopyTradeInfo::Create(server_ticket_no, symbol, order_type));
      }
   };
   int m_index_of(long server_ticket_no) {
      for (int i = 0; i < m_orders.Length(); i++) {
         CopyTradeInfo info = m_orders.Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            return i;
         }
      }
      return -1;
   };
public:
   RemoteOrderState() {
      m_last_server_response = NULL;
      m_initialize();
   };
   TicketNoDifference<CopyTradeInfo> GetDifference() {
      TicketNoDifference<CopyTradeInfo> diff;
      string response;
      if (client.GetOrders(response) == 200) {
         if (m_last_server_response != response) {
            string error = NULL;
            CopyTradeInfoList serverInfo = CopyTradeInfoParser::Parse(response, error);
            if (error == NULL) {
               for (int i = 0; i < m_orders.Length(); i++) {
                  CopyTradeInfo ii = m_orders.Get(i);
                  bool exists = false;
                  for (int j = 0; j < serverInfo.Length(); j++) {
                     CopyTradeInfo ij = serverInfo.Get(j);
                     if (ii.server_ticket_no == ij.server_ticket_no) {
                        exists = true;
                        break;
                     }
                  }
                  if (!exists) {
                     diff.Closed(ii);
                  }
               }
               for (int i = 0; i < serverInfo.Length(); i++) {
                  CopyTradeInfo ii = serverInfo.Get(i);
                  bool exists = false;
                  for (int j = 0; j < m_orders.Length(); j++) {
                     CopyTradeInfo ij = m_orders.Get(j);
                     if (ii.server_ticket_no == ij.server_ticket_no) {
                        exists = true;
                        break;
                     }
                  }
                  if (!exists) {
                     diff.Opened(ii);
                  }
               }
               state.ClearError();
            } else {
               state.SetError(error);
               logger.WriteLog("DEBUG", error);
            }
            m_last_server_response = response;
         }
      } else {
         state.SetError("サーバーと通信できません。");
      }
      return diff;
   };
   void Add(CopyTradeInfo &info) {
      m_orders.Add(info);
   };
   void Remove(long server_ticket_no) {
      int index = m_index_of(server_ticket_no);
      if (index > -1) {
         m_orders.Remove(index);
      }
   };
};

class LocalOrderState {
   List<LocalOrderInfo> m_orders;
   
   int m_index_of_local_ticket(long local_ticket_no) {
      for (int i = 0; i < m_orders.Length(); i++) {
         LocalOrderInfo info = m_orders.Get(i);
         if (info.local_ticket_no == local_ticket_no) {
            return i;
         }
      }
      return -1;
   };

   int m_index_of_server_ticket(long server_ticket_no) {
      for (int i = 0; i < m_orders.Length(); i++) {
         LocalOrderInfo info = m_orders.Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            return i;
         }
      }
      return -1;
   };

   void m_initialize() {
      int position_count = PositionsTotal();
      for(int i = 0; i < position_count ; i++) {
         long position_ticket_no = (long)PositionGetTicket(i);
         if (position_ticket_no == 0) continue;
         long server_ticket_no = PositionGetInteger(POSITION_MAGIC);
         if (server_ticket_no == 0) continue;
         string symbol = PositionGetString(POSITION_SYMBOL);
         int order_type = ConvertToOrderType((ENUM_POSITION_TYPE)PositionGetInteger(POSITION_TYPE));
         m_orders.Add(LocalOrderInfo::Create(server_ticket_no, position_ticket_no, symbol, order_type));
      } 
   };
public:
   LocalOrderState() {
      m_initialize();
   };

   TicketNoDifference<LocalOrderInfo> GetDifference() {
      TicketNoDifference<LocalOrderInfo> diff;
      List<LocalOrderInfo> localInfo;
      int position_count = PositionsTotal();
      for(int i = 0; i < position_count ; i++) {
         long position_ticket_no = (long)PositionGetTicket(i);
         if (position_ticket_no == 0) continue;
         long server_ticket_no = PositionGetInteger(POSITION_MAGIC);
         if (server_ticket_no == 0) continue;
         string symbol = PositionGetString(POSITION_SYMBOL);
         int order_type = ConvertToOrderType((ENUM_POSITION_TYPE)PositionGetInteger(POSITION_TYPE));
         localInfo.Add(LocalOrderInfo::Create(server_ticket_no, position_ticket_no, symbol, order_type));
      } 
      
      for (int i = 0; i < m_orders.Length(); i++) {
         LocalOrderInfo ii = m_orders.Get(i);
         bool exists = false;
         for (int j = 0; j < localInfo.Length(); j++) {
            LocalOrderInfo ij = localInfo.Get(j);
            if (ii.local_ticket_no == ij.local_ticket_no) {
               exists = true;
               break;
            }
         }
         if (!exists) {
            diff.Closed(ii);
         }
      }
      for (int i = 0; i < localInfo.Length(); i++) {
         LocalOrderInfo ii = localInfo.Get(i);
         bool exists = false;
         for (int j = 0; j < m_orders.Length(); j++) {
            LocalOrderInfo ij = m_orders.Get(j);
            if (ii.local_ticket_no == ij.local_ticket_no) {
               exists = true;
               break;
            }
         }
         if (!exists) {
            diff.Opened(ii);
         }
      }
      return diff;
   };
   
   bool ExistsLocalTicketNo(long local_ticket_no) {
      return m_index_of_local_ticket(local_ticket_no) >= 0;
   };

   bool ExistsServerTicketNo(long server_ticket_no) {
      return m_index_of_server_ticket(server_ticket_no) >= 0;
   };

   int FindByServerTicketNo(long server_ticket_no, long &arr_ticket_no[]) {
      int count = 0;
      for (int i = 0; i < m_orders.Length(); i++) {
         LocalOrderInfo info = m_orders.Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            count++;
         }
      }
      if (count == 0) {
         return 0;
      }
      ArrayResize(arr_ticket_no, count);
      int j = 0;
      for (int i = 0; i < m_orders.Length(); i++) {
         LocalOrderInfo info = m_orders.Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            arr_ticket_no[j] = info.local_ticket_no;
         }
      }
      return count;
   };

   void Add(long server_ticket_no, long position_ticket_no, string symbol, int order_type) {
      m_orders.Add(LocalOrderInfo::Create(server_ticket_no, position_ticket_no, symbol, order_type));
   };

   void Remove(long local_ticket_no) {
      int index = m_index_of_local_ticket(local_ticket_no);
      if (index > -1) {
         m_orders.Remove(index);
      }
   };
};

class PositionManager {
   Logger *m_logger;
   double m_calculate_volume(string symbol, ENUM_ORDER_TYPE order_type, double price) {
      double one_lot;//!-lot cost
      if (!OrderCalcMargin(order_type, symbol, 1, price, one_lot)) {
         m_logger.WriteLog("DEBUG", "OrderCalcMargin returned false");
         return 0;
      }
      if (one_lot == 0) {
         m_logger.WriteLog("WARN", "one_lot is zero");
         return 0;
      }
      double step   =SymbolInfoDouble(symbol,SYMBOL_VOLUME_STEP); // Step in volume changing
      if (step == 0) {
         m_logger.WriteLog("WARN", "SymbolInfoDouble(symbol,SYMBOL_VOLUME_STEP) returned zero");
         return 0;
      }
      double free   =AccountInfoDouble(ACCOUNT_FREEMARGIN);// Free margin
      double lots   =MathFloor(free*Percent/100/one_lot/step)*step;
      return lots;
   };
   string m_find_symbol(string symbol_str) {
      for (int i = 0; i < SymbolsTotal(false); i++) {
         if (StringFind(SymbolName(i, false), symbol_str) >= 0) {
            return SymbolName(i, false);
         }
      }
      return NULL;
   };
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
         return 0;
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
         m_logger.WriteLog("ERROR", "OrderSend fail: Symbol not found. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type));
         return true;
      }
      ENUM_ORDER_TYPE order_type;
      double order_price;
      MqlTick tick;
      if (!SymbolInfoTick(symbol, tick)) {
         m_logger.WriteLog("ERROR", "OrderSend fail: SymbolInfoTick returned false. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type) + ", LocalSymbol = " + symbol);
         return false;
      }
      if (server_order_type == 0) {
         order_type = ORDER_TYPE_BUY;
         order_price = tick.ask;
      } else if (server_order_type == 1) {
         order_type = ORDER_TYPE_SELL;
         order_price = tick.bid;
      } else {
         m_logger.WriteLog("ERROR", "OrderSend fail: Invalid OrderType. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type));
         return true;
      }
      double order_lots = m_calculate_volume(symbol, order_type, order_price);
      double min_lots = SymbolInfoDouble(symbol, SYMBOL_VOLUME_MIN);         // Min. amount of lots
      double max_lots = SymbolInfoDouble(symbol, SYMBOL_VOLUME_MAX);         // Max amount of lotsr
      if (order_lots == 0) {
         m_logger.WriteLog("WARN", "OrderSend fail: Calculated order lot was 0. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type));
         return true;
      }
      if (order_lots < min_lots) {
         m_logger.WriteLog("WARN", "OrderSend fail: Not enough margin. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type) + ", Calculated lots = " + DoubleToString(order_lots));
         return true;
      }
      m_logger.WriteLog("DEBUG", "OrderSend executing: ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type) + ", Calculated lots = " + DoubleToString(order_lots));
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
         int local_ticket_no = m_send_open_order(symbol, order_type, order_price, lots, server_ticket_no);
         if (local_ticket_no < 0) {
            m_logger.WriteLog("ERROR", "OrderSend failed: " + IntegerToString(server_ticket_no) + ", Error = " + IntegerToString(GetLastError()));
            break;
         } else {
            ArrayResize(ticket_no_arr, ArraySize(ticket_no_arr) + 1);
            ticket_no_arr[ArraySize(ticket_no_arr) - 1] = local_ticket_no;
            success = true;
            m_logger.WriteLog("INFO", "OrderSend succeeded: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
            WriteOrderOpenSuccessLog(server_ticket_no, server_symbol, server_order_type, local_ticket_no);
         }
      }
      return success;
   };
   bool ClosePosition(long server_ticket_no, long local_ticket_no) {
      m_logger.WriteLog("DEBUG", "OrderClose executing: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
      int result = m_send_close_order(local_ticket_no);
      if (result == 0) {
         m_logger.WriteLog("WARN", "OrderClose failed: Already closed. " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
         return true;
      }
      if (result < 0) {
         m_logger.WriteLog("ERROR", "OrderClose failed: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no) + ", Error = " + IntegerToString(GetLastError()));
         return false;
      } else {
         m_logger.WriteLog("INFO", "OrderClose succeeded: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
         WriteOrderCloseSuccessLog(server_ticket_no, local_ticket_no);
         return true;
      }
   };
   
   void WriteOrderOpenSuccessLog(long server_ticket_no, string serverSymbol, int serverOrderType, int ticketNo) {
      int waitCount = 0;
      bool found = false;
      while (waitCount < 5) {
         found = PositionSelectByTicket(ticketNo);
         if (found) {
            break;
         }
         Sleep(10);
      }
      string text = IntegerToString(server_ticket_no) + ":" + serverSymbol + ":" + IntegerToString(serverOrderType) + ":" + IntegerToString(ticketNo) + ":";
      if (found) {
         text = text + IntegerToString(PositionGetInteger(POSITION_TICKET)) + ":" + PositionGetString(POSITION_SYMBOL) + ":" + IntegerToString(PositionGetInteger(POSITION_TYPE)) + ":" + DoubleToString(PositionGetDouble(POSITION_PRICE_OPEN)) + ":" + DoubleToString(PositionGetDouble(POSITION_VOLUME)) + ":" + IntegerToString(PositionGetInteger(POSITION_TIME));
      } else {
         text = text + "NA:NA:NA:NA:NA:NA";
      }
      m_logger.WriteLog("OPEN", text);
   }  
   
   void WriteOrderCloseSuccessLog(long server_ticket_no, long local_ticket_no) {
      int waitCount = 0;
      bool found = false;
      while (waitCount < 5) {
         found = HistorySelectByPosition(local_ticket_no);
         if (found) {
            break;
         }
         Sleep(10);
      }
      if (found) {
         PositionSelectByTicket(local_ticket_no);
         long position_type = PositionGetInteger(POSITION_TYPE);
         //--- リスト中の約定の数の合計
         int deals=HistoryDealsTotal();
         //--- 取引をひとつづつ処理する
         for(int i=0;i<deals;i++) {
            ulong deal_ticket = HistoryDealGetTicket(i);
            if (HistoryDealGetInteger(deal_ticket,DEAL_TYPE) == position_type) {
               continue;
            }
            string text = IntegerToString(server_ticket_no) + ":" + IntegerToString(local_ticket_no) + ":";
            text = text + IntegerToString(deal_ticket) + ":" + HistoryDealGetString(deal_ticket, DEAL_SYMBOL) + ":" + IntegerToString(HistoryDealGetInteger(deal_ticket, DEAL_TYPE)) + ":" + DoubleToString(HistoryDealGetDouble(deal_ticket, DEAL_PRICE)) + ":" + DoubleToString(HistoryDealGetDouble(deal_ticket,DEAL_VOLUME)) + ":" + DoubleToString(HistoryDealGetDouble(deal_ticket,DEAL_PROFIT)) + ":" + IntegerToString(HistoryDealGetInteger(deal_ticket, DEAL_TIME));
            m_logger.WriteLog("CLOSE", text);
          }
      } else {
         string text = IntegerToString(server_ticket_no) + ":" + IntegerToString(local_ticket_no) + ":";
         text = text + "NA:NA:NA:NA:NA:NA:NA";
         m_logger.WriteLog("CLOSE", text);
      }
   }
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
   
   string GetName() {
      return m_api_name;
   };

   void SetErrorCode(int error_code) {
      if (m_last_error_code != error_code) {
         if (error_code == -1) {
            Print(m_api_name + ": Recover from Error");
         } else {
            Print(m_api_name + ": " + ErrorCodeToString(error_code));
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
   
   int GetLastStatus() {
      return m_last_status_code;
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
   Logger *m_logger;

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

      state.Lap(status.GetName() +  " begin");
      int res = WebRequest(method, url, request_headers, timeout, request_data, response_data, response_headers);
      state.Lap(status.GetName() +  " end");
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
   TrsysClient(Logger *l_logger) {
      m_secret_key = m_generate_secret_key();
      m_secret_token = NULL;
      m_next_token_fetch_time = -1;
      m_post_secret_key_status = new ApiStatus("PostSecretKey");
      m_post_token_release_status = new ApiStatus("PostTokenRelease");
      m_get_orders_status = new ApiStatus("GetOrders");
      m_post_log_status = new ApiStatus("PostLog");
      m_get_orders_etag = NULL;
      m_get_orders_etag_response = NULL;
      m_logger = l_logger;
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
      if (m_post_log_status.GetLastStatus() == 404) {
         return 0;
      }
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

int ConvertToOrderType(ENUM_POSITION_TYPE position_type) {
   if (position_type == POSITION_TYPE_BUY) {
      return 0;
   } else if (position_type == POSITION_TYPE_SELL) {
      return 1;
   }
   return -1;
}

string ErrorCodeToString(int error_code) {
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
EaState *state = NULL;
TrsysClient *client = NULL;
PositionManager *positionManager = NULL;
RemoteOrderState *remoteOrders = NULL;
LocalOrderState *localOrders = NULL;

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
{
//--- create timer
   EventSetMillisecondTimer(100);
   logger = new Logger();
   state = new EaState();
   client = new TrsysClient(logger);
   positionManager = new PositionManager(logger);
   remoteOrders = new RemoteOrderState();
   localOrders = new LocalOrderState();
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
   delete localOrders;
   delete remoteOrders;
   delete positionManager;
   delete client;
   delete state;
   delete logger;
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
   state.Begin();
   if (!state.IsEaEnabled()) {
      return;
   }

   TicketNoDifference<LocalOrderInfo> localDiff = localOrders.GetDifference();
   if (localDiff.HasDifference()) {
      for (int i = 0; i < localDiff.ClosedCount(); i++) {
         LocalOrderInfo closedInfo = localDiff.GetClosed(i);
         logger.WriteLog("DEBUG", "Local order closed. LocalOrder = " + closedInfo.ToString());
         long arr_close_ticket_no[];
         int close_order_count = localOrders.FindByServerTicketNo(closedInfo.server_ticket_no, arr_close_ticket_no);
         if (close_order_count > 0) {
            for (int j = 0; j < close_order_count; j++) {
               if (closedInfo.local_ticket_no == arr_close_ticket_no[j]) continue;
               if (localOrders.ExistsLocalTicketNo(arr_close_ticket_no[j])) {
                  if (positionManager.ClosePosition(closedInfo.server_ticket_no, arr_close_ticket_no[j])) {
                     localOrders.Remove(arr_close_ticket_no[j]);
                  }
               }
            }
         }
         localOrders.Remove(closedInfo.local_ticket_no);
      }
      for (int i = 0; i < localDiff.OpenedCount(); i++) {
         LocalOrderInfo openedInfo = localDiff.GetOpened(i);
         logger.WriteLog("DEBUG", "Local order opened. LocalOrder = " + openedInfo.ToString());
         localOrders.Add(openedInfo.server_ticket_no, openedInfo.local_ticket_no, openedInfo.symbol, openedInfo.order_type);
      }
   }

   TicketNoDifference<CopyTradeInfo> serverDiff = remoteOrders.GetDifference();
   if (serverDiff.HasDifference()) {
      for (int i = 0; i < serverDiff.ClosedCount(); i++) {
         CopyTradeInfo closedInfo = serverDiff.GetClosed(i);
         logger.WriteLog("DEBUG", "Server order closed. ServerOrder = " + closedInfo.ToString());
         long arr_close_ticket_no[];
         int close_order_count = localOrders.FindByServerTicketNo(closedInfo.server_ticket_no, arr_close_ticket_no);
         if (close_order_count > 0) {
            for (int j = 0; j < close_order_count; j++) {
               if (positionManager.ClosePosition(closedInfo.server_ticket_no, arr_close_ticket_no[j])) {
                  localOrders.Remove(arr_close_ticket_no[j]);
               }
            }
         }
         remoteOrders.Remove(closedInfo.server_ticket_no);
      }
      for (int i = 0; i < serverDiff.OpenedCount(); i++) {
         CopyTradeInfo openedInfo = serverDiff.GetOpened(i);
         logger.WriteLog("DEBUG", "Server order opened. ServerOrder = " + openedInfo.ToString());
         if (!localOrders.ExistsServerTicketNo(openedInfo.server_ticket_no)) {
            long arr_open_ticket_no[];
            if (positionManager.CreatePosition(openedInfo.server_ticket_no, openedInfo.symbol, openedInfo.order_type, arr_open_ticket_no)) {
               int size = ArraySize(arr_open_ticket_no);
               for (int j = 0; j < size; j++) {
                  localOrders.Add(openedInfo.server_ticket_no, arr_open_ticket_no[j], openedInfo.symbol, openedInfo.order_type);
               }
            }
         }
         remoteOrders.Add(openedInfo);
      }
   }
   client.PostLog(logger);
   state.End();
}
//+------------------------------------------------------------------+