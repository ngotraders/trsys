bool DEBUG = false;
bool PERFORMANCE = false;
bool DRY_RUN = false;

string Endpoint = "https://copy-trading-system.azurewebsites.net";
string Version = "20241113";

enum EnumLotCalculationType {
  EnumLotCalculationTypeLot, // Lot
  EnumLotCalculationTypePercentage, // Percentage
  EnumLotCalculationTypeFollowPublisher // PublisherPercentage
};

//+------------------------------------------------------------------+
//| Custom classes                                                   |
//+------------------------------------------------------------------+
struct PublisherConfiguration {
   double OrderPercentageToSendSubscriber;
};

PublisherConfiguration publisherConfig;

struct SubscriberConfiguration {
   EnumLotCalculationType LotCalculationType;
   double LotCalculationValue;
};

SubscriberConfiguration subscriberConfig;

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
   ~List() {
      ArrayFree(m_array);
   };
   void Add(T &item) {
      m_count++;
      if (m_count > m_actual_array_length) {
         m_resize();
      }
      m_array[m_count - 1] = item;
   };
   void Update(int index, T &item) {
      if (index >= m_count) {
         Print("index must under the count");
         return;
      }
      m_array[index] = item;
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
      if (index >= m_count) {
         Print("index must under the count");
         return m_array[0];
      }
      return m_array[index];
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
   ~LogQueue() {
      ArrayFree(m_queue);
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
         str_array[i] = m_queue[(m_current_index + i) % MAX_QUEUE_COUNT];
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
      string text = IntegerToString(TimeCurrent()) + ":" + logType + ":" + message;
      if (logType == "DEBUG") {
         if (DEBUG) {
            Print(message);
         }
      } else {
         Print(message);
      }
      Enqueue(text);
   }
};

class EaState {
   Logger *m_logger;
   bool m_initializing;
   bool m_ea_enabled;
   bool m_has_server_connection;
   bool m_key_is_valid;
   bool m_key_is_authorized;
   string m_env;
   string m_ea_type;
   long m_start_time;
   long m_next_ping_time;
   string m_ea_state;
   void m_update_comment() {
      if (m_initializing) {
         return;
      }
      if (!m_ea_enabled) {
         Comment("Trsys" + m_ea_type + ": 自動売買が無効です");
         m_ea_state = "DISABLED";
         m_logger.WriteLog("INFO", "Automated trading disabled");
         return;
      }
      if (!m_has_server_connection) {
         Comment("Trsys" + m_ea_type + ": サーバーと通信できません。");
         m_ea_state = "CONNECTION_ERROR";
         m_logger.WriteLog("INFO", "Server connection failed");
         return;
      }
      string envText = StringFind(m_env, "Development") >= 0 ? "<検証環境>" : "";
      if (!m_key_is_valid) {
         Comment("Trsys" + m_ea_type + envText + ": シークレットキーが異常です。");
         m_ea_state = "KEY_INVALID";
         m_logger.WriteLog("INFO", "Secret key is invalid");
         return;
      }
      if (!m_key_is_authorized) {
         Comment("Trsys" + m_ea_type + envText + ": シークレットキーが異常です。");
         m_ea_state = "KEY_UNAUTHORIZED";
         m_logger.WriteLog("INFO", "Secret key is not authorized");
         return;
      }
      m_ea_state = "NORMAL";
      if (m_ea_type == "Publisher") {
         Comment("Trsys" + m_ea_type + envText + ": 正常 (取引割合: " + DoubleToString(publisherConfig.OrderPercentageToSendSubscriber * 100) + "%)");
         m_logger.WriteLog("INFO", "Normal (Order Percentage: " + DoubleToString(publisherConfig.OrderPercentageToSendSubscriber * 100) + "%)");
      } else {
         if (subscriberConfig.LotCalculationType == EnumLotCalculationTypeLot) {
            Comment("Trsys" + m_ea_type + envText + ": 正常 (固定ロット: " + DoubleToString(subscriberConfig.LotCalculationValue) + ")");
            m_logger.WriteLog("INFO", "Normal (Fixed Lot: " + DoubleToString(subscriberConfig.LotCalculationValue * 100) + "%)");
         } else if(subscriberConfig.LotCalculationType == EnumLotCalculationTypePercentage) {
            Comment("Trsys" + m_ea_type + envText + ": 正常 (固定割合: " + DoubleToString(subscriberConfig.LotCalculationValue * 100) + "%)");
            m_logger.WriteLog("INFO", "Normal (Subscriber Percentage: " + DoubleToString(subscriberConfig.LotCalculationValue * 100) + "%)");
         } else {
            Comment("Trsys" + m_ea_type + envText + ": 正常 (サブ指定割合: " + DoubleToString(subscriberConfig.LotCalculationValue * 100) + "%)");
            m_logger.WriteLog("INFO", "Normal (Publisher Percentage: " + DoubleToString(subscriberConfig.LotCalculationValue * 100) + "%)");
         }
      }
   };
public:
   EaState(Logger *l_logger, string l_ea_type) {
      m_initializing = true;
      m_logger = l_logger;
      m_ea_enabled = true;
      m_has_server_connection = false;
      m_key_is_valid = false;
      m_ea_type = l_ea_type;
      m_next_ping_time = -1;
      IsEaEnabled();
   };
   void SetInitializationFinish() {
      m_initializing = false;
      m_update_comment();
   }
   bool ShouldPing() {
      if (m_has_server_connection) {
         return m_next_ping_time < GetTickCount();
      }
      return false;
   }
   bool IsEaEnabled() {
      bool ea_enabled = MQLInfoInteger(MQL_TRADE_ALLOWED) == 1 && AccountInfoInteger(ACCOUNT_TRADE_EXPERT) == 1 && AccountInfoInteger(ACCOUNT_TRADE_ALLOWED) == 1 && TerminalInfoInteger(TERMINAL_TRADE_ALLOWED) == 1;
      if (m_ea_enabled != ea_enabled) {
         m_ea_enabled = ea_enabled;
         m_update_comment();
      }
      return m_ea_enabled;
   };
   bool KeyIsValid() {
      return m_key_is_valid;
   };
   void SetServerConnection(bool has_server_connection) {
      if (has_server_connection) {
         m_next_ping_time = GetTickCount() + 2000; // 2sec later
      }
      if (m_has_server_connection != has_server_connection) {
         m_has_server_connection = has_server_connection;
         m_update_comment();
      }
   };
   void SetKeyIsValid(bool key_is_valid) {
      if (m_key_is_valid != key_is_valid) {
         m_key_is_valid = key_is_valid;
         m_update_comment();
      }
   };
   void SetKeyIsAuthorize(bool key_is_authorized) {
      if (m_key_is_authorized != key_is_authorized) {
         m_key_is_authorized = key_is_authorized;
         m_update_comment();
      }
   }
   void SetEnvironment(string env) {
      if (m_env != env) {
         m_env = env;
         m_update_comment();
      }
   };
   string GetState() {
      return m_ea_state;
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

struct PendingOrder {
   long server_ticket_no;
   string symbol;
   int order_type;
   long local_ticket_no;
   
   string ToString() {
      return "PENDING_ORDER:" + IntegerToString(server_ticket_no) + "/" + IntegerToString(local_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
   
   static PendingOrder Create(long server_ticket_no, string server_symbol, int server_order_type, long local_ticket_no) {
      PendingOrder order;
      order.server_ticket_no = server_ticket_no;
      order.symbol = server_symbol;
      order.order_type = server_order_type;
      order.local_ticket_no = local_ticket_no;
      return order;
   };
};

class PendingOrderList : public List<PendingOrder> {
public:
   int IndexOfLocalTicket(long local_ticket_no) {
      for (int i = 0; i < Length(); i++) {
         if (Get(i).local_ticket_no == local_ticket_no) {
            return i;
         }
      };
      return -1;
   };
   bool ExistsLocalTicket(long local_ticket_no) {
      for (int i = 0; i < Length(); i++) {
         if (Get(i).local_ticket_no == local_ticket_no) {
            return true;
         }
      };
      return false;
   };
   bool ExistsServerTicket(long server_ticket_no) {
      for (int i = 0; i < Length(); i++) {
         if (Get(i).server_ticket_no == server_ticket_no) {
            return true;
         }
      };
      return false;
   };
};

struct PositionInfo {
   long server_ticket_no;
   long local_ticket_no;
   string symbol;
   int order_type;
   datetime position_time;
   double price_open;
   double volume;
   double percentage;
   
   string ToString() {
      return "POSITION:" + IntegerToString(server_ticket_no) + "/" + IntegerToString(local_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
};

struct DealInfo {
   long deal_ticket_no;
   string symbol;
   int order_type;
   double price;
   double volume;
   double profit;
   datetime time;

   string ToString() {
      return "DEAL:" + IntegerToString(deal_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
};

class PositionManager {
   Logger *m_logger;
   PendingOrderList m_opening_ticket_no_list;
   PendingOrderList m_closing_ticket_no_list;

   double m_calculate_volume(string symbol, int order_type, double current_price, double percentage) {
      double one_lot = 0;
      double step = 0;
      double account_free_margin = 0;
      double account_balance = 0;
      double lots;
      double order_percentage;
      m_fetch_calculate_volume_params(symbol, order_type, current_price, one_lot, step, account_free_margin, account_balance);
      if (account_free_margin == 0) {
         return 0;
      }
      if (account_balance == 0) {
         return 0;
      }
      if (one_lot == 0) {
         m_logger.WriteLog("ERROR", "CalculateVolume: One lot is 0. Symbol = " + symbol);
         return 0;
      }
      if (step == 0) {
         step = 1;
         m_logger.WriteLog("WARN", "CalculateVolume: Step is 0. using 1 as lot step, Symbol = " + symbol);
      }
      double lotsFree = MathFloor(account_free_margin / one_lot / step) * step;
      if (subscriberConfig.LotCalculationType == EnumLotCalculationTypeLot) {
         lots = subscriberConfig.LotCalculationValue;
         if (lotsFree < lots) {
            m_logger.WriteLog("ERROR", "CalculateVolume: Fixed lots less than free lots. Symbol = " + symbol + ", Margin for a lot = " + DoubleToString(one_lot) + ", Step = " + DoubleToString(step) + ", Balance = " + DoubleToString(account_balance) + ",  Free margin = " + DoubleToString(account_free_margin) + ", Leverage = " + IntegerToString(AccountInfoInteger(ACCOUNT_LEVERAGE)) + ", Calculated volume = " + DoubleToString(lots) + ", Calculated free volume = " + DoubleToString(lotsFree));
            return 0;
         }
         m_logger.WriteLog("DEBUG", "CalculateVolume: Fixed Lots. Symbol = " + symbol + ", Margin for a lot = " + DoubleToString(one_lot) + ", Step = " + DoubleToString(step) + ", Balance = " + DoubleToString(account_balance) + ",  Free margin = " + DoubleToString(account_free_margin) + ", Leverage = " + IntegerToString(AccountInfoInteger(ACCOUNT_LEVERAGE)) + ", Calculated volume = " + DoubleToString(lots) + ", Calculated free volume = " + DoubleToString(lotsFree));
         return lots;
      } else if(subscriberConfig.LotCalculationType == EnumLotCalculationTypeLot) {
         order_percentage = subscriberConfig.LotCalculationValue;
         lots = MathFloor(account_balance * order_percentage / one_lot / step) * step;
         m_logger.WriteLog("DEBUG", "CalculateVolume: Fixed Percentage. Symbol = " + symbol + ", Margin for a lot = " + DoubleToString(one_lot) + ", Step = " + DoubleToString(step) + ", Balance = " + DoubleToString(account_balance) + ",  Free margin = " + DoubleToString(account_free_margin) + ", Leverage = " + IntegerToString(AccountInfoInteger(ACCOUNT_LEVERAGE)) + ", Percentage = " + DoubleToString(order_percentage) + ", Calculated volume = " + DoubleToString(lots) + ", Calculated free volume = " + DoubleToString(lotsFree));
         return MathMin(lotsFree, lots);
      } else {
         order_percentage = percentage * (subscriberConfig.LotCalculationValue);
         lots = MathFloor(account_balance * order_percentage / one_lot / step) * step;
         m_logger.WriteLog("DEBUG", "CalculateVolume: Publisher Percentage. Symbol = " + symbol + ", Margin for a lot = " + DoubleToString(one_lot) + ", Step = " + DoubleToString(step) + ", Balance = " + DoubleToString(account_balance) + ",  Free margin = " + DoubleToString(account_free_margin) + ", Leverage = " + IntegerToString(AccountInfoInteger(ACCOUNT_LEVERAGE)) + ", Percentage = " + DoubleToString(order_percentage) + ", Calculated volume = " + DoubleToString(lots) + ", Calculated free volume = " + DoubleToString(lotsFree));
         return MathMin(lotsFree, lots);
      }
   };
#ifdef __MQL5__
   void m_fetch_calculate_volume_params(string symbol, int order_type, double current_price, double &one_lot, double &step, double &account_free_margin, double &account_balance) {
      ENUM_ORDER_TYPE mt5_order_type = m_convert_to_local_order_type(order_type);
      // fetch one lot cost
      if (!OrderCalcMargin(mt5_order_type, symbol, 1, current_price, one_lot)) {
         m_logger.WriteLog("DEBUG", "OrderCalcMargin returned false");
      }
      step = SymbolInfoDouble(symbol,SYMBOL_VOLUME_STEP);
      account_free_margin = AccountInfoDouble(ACCOUNT_MARGIN_FREE);
      account_balance = AccountInfoDouble(ACCOUNT_BALANCE);
   };
   string m_find_symbol(string symbol_str) {
      for (int i = 0; i < SymbolsTotal(true); i++) {
         if (StringFind(SymbolName(i, true), symbol_str) >= 0) {
            return SymbolName(i, true);
         }
      }
      return NULL;
   };
   
   ENUM_ORDER_TYPE m_convert_to_local_order_type(int order_type) {
      if (order_type == 0) {
         return ORDER_TYPE_BUY;
      } else if (order_type == 1) {
         return ORDER_TYPE_SELL;
      }
      return -1;
   };

   int m_convert_from_order_type(ENUM_ORDER_TYPE order_type) {
      if (order_type == ORDER_TYPE_BUY) {
         return 0;
      } else if (order_type == ORDER_TYPE_SELL) {
         return 1;
      }
      return -1;
   };

   int m_convert_from_position_type(ENUM_POSITION_TYPE position_type) {
      if (position_type == POSITION_TYPE_BUY) {
         return 0;
      } else if (position_type == POSITION_TYPE_SELL) {
         return 1;
      }
      return -1;
   };

   int m_convert_from_deal_type(ENUM_DEAL_TYPE deal_type) {
      if (deal_type == DEAL_TYPE_BUY) {
         return 0;
      } else if (deal_type == DEAL_TYPE_SELL) {
         return 1;
      }
      return -1;
   };
   
   double m_get_current_price(string symbol, int order_type) {
      if (order_type == 0) {
         return SymbolInfoDouble(symbol, SYMBOL_ASK);
      } else {
         return SymbolInfoDouble(symbol, SYMBOL_BID);
      }
   };
   
   double m_get_min_volume(string symbol) {
      return SymbolInfoDouble(symbol, SYMBOL_VOLUME_MIN);
   };
   double m_get_max_volume(string symbol) {
      return SymbolInfoDouble(symbol, SYMBOL_VOLUME_MAX);
   };

   int m_send_open_order(long server_ticket_no, string symbol, int order_type, double volume, double price, int slippage, string &error_message) {
      if (DRY_RUN) {
         Print("m_send_open_order: server_ticket_no = ", server_ticket_no, ", symbol = ", symbol, ", order_type = ", order_type, ", volume = ", volume, ", price = ", price); 
         return 1;
      }
      ENUM_ORDER_TYPE mt5_order_type = m_convert_to_local_order_type(order_type);
      if (mt5_order_type < 0) {
         error_message = "invalid order type: " + IntegerToString(order_type);
         return -1;
      }
      //--- リクエストを準備する
      MqlTradeRequest request;
      ZeroMemory(request);
      request.action       = TRADE_ACTION_DEAL; // 取引操作タイプ
      request.symbol       = symbol;            // シンボル
      request.volume       = volume;            // ロットのボリューム
      request.type         = mt5_order_type;    // 注文タイプ
      request.price        = price;             // 発注価格
      request.deviation    = slippage;          // 価格からの許容偏差
      request.magic        = server_ticket_no;  // 注文のMagicNumber
      request.type_filling = ORDER_FILLING_IOC;
      MqlTradeCheckResult checkResult;
      if (!OrderCheck(request, checkResult)) {
         error_message = "OrderCheck fail:" + IntegerToString(checkResult.retcode) + "/" + checkResult.comment;
         return -1;
      }
      MqlTradeResult result;
      if (!OrderSend(request, result)) {
         error_message = "OrderSend fail:" + IntegerToString(result.retcode) + "/" + result.comment;
         return -1;
      }
      return (int)result.order;
   }
   
   bool m_send_close_order(long server_ticket_no, long local_ticket_no, string symbol, int order_type, double volume, double price, int slippage, string &error_message) {
      if (DRY_RUN) {
         Print("m_send_close_order: server_ticket_no = ", server_ticket_no, ", local_ticket_no = ", local_ticket_no, ", symbol = ", symbol, ", order_type = ", order_type, ", volume = ", volume, ", price = ", price);
         return true;
      }
      ENUM_ORDER_TYPE mt5_order_type = m_convert_to_local_order_type(order_type);
      if (mt5_order_type < 0) {
         error_message = "invalid order type" + IntegerToString(order_type);
         return false;
      }
      MqlTradeRequest request;
      ZeroMemory(request);
      request.action       = TRADE_ACTION_DEAL; // - type of trade operation
      request.position     = local_ticket_no;   // - ticket of the position
      request.symbol       = symbol;            // - symbol 
      request.volume       = volume;            // - volume of the position
      request.deviation    = slippage;          // - allowed deviation from the price
      request.magic        = server_ticket_no;  // - MagicNumber of the position
      request.type_filling = ORDER_FILLING_IOC;
      request.price        = price;
      request.type         = mt5_order_type;
      MqlTradeCheckResult checkResult;
      if (!OrderCheck(request, checkResult)) {
         error_message = "OrderCheck fail:" + IntegerToString(checkResult.retcode) + "/" + checkResult.comment;
         return false;
      }
      MqlTradeResult result;
      if (!OrderSend(request, result)) {
         error_message = "OrderSend fail:" + IntegerToString(result.retcode) + "/" + result.comment;
         return false;
      }
      return true;
   }

   int m_get_positions(PositionInfo &arr_positions[], bool includeAll) {
      List<PositionInfo> list;
      int position_count = PositionsTotal();
      for(int i = 0; i < position_count ; i++) {
         long local_ticket_no = (long)PositionGetTicket(i);
         if (local_ticket_no == 0) continue;
         long server_ticket_no = PositionGetInteger(POSITION_MAGIC);
         if (!includeAll) {
            if (server_ticket_no == 0) continue;
         }
         PositionInfo info;
         info.server_ticket_no = server_ticket_no;
         info.local_ticket_no = local_ticket_no;
         info.symbol = PositionGetString(POSITION_SYMBOL);
         info.order_type = m_convert_from_position_type((ENUM_POSITION_TYPE)PositionGetInteger(POSITION_TYPE));
         info.position_time = (datetime)PositionGetInteger(POSITION_TIME);
         info.price_open = PositionGetDouble(POSITION_PRICE_OPEN);
         info.volume = PositionGetDouble(POSITION_VOLUME);
         double one_lot;
         if (OrderCalcMargin((ENUM_ORDER_TYPE) info.order_type, info.symbol, 1, info.price_open, one_lot)) {
            info.percentage = info.volume / AccountInfoDouble(ACCOUNT_BALANCE) / one_lot;
         } else {
            info.percentage = 0;
         }
         list.Add(info);
      }
      if (list.Length() == 0) {
         return 0;
      }
      ArrayResize(arr_positions, list.Length());
      for (int i = 0; i < list.Length(); i++) {
         arr_positions[i] = list.Get(i);
      }
      return list.Length();
   };
   
   bool m_get_position(long local_ticket_no, PositionInfo &info) {
      int waitCount = 0;
      while (waitCount < 5) {
         if (PositionSelectByTicket(local_ticket_no)) {
            info.server_ticket_no = PositionGetInteger(POSITION_MAGIC);
            info.local_ticket_no = PositionGetInteger(POSITION_TICKET);
            info.symbol = PositionGetString(POSITION_SYMBOL);
            info.order_type = m_convert_from_position_type((ENUM_POSITION_TYPE)PositionGetInteger(POSITION_TYPE));
            info.position_time = (datetime)PositionGetInteger(POSITION_TIME);
            info.price_open = PositionGetDouble(POSITION_PRICE_OPEN);
            info.volume = PositionGetDouble(POSITION_VOLUME);
            double one_lot;
            if (OrderCalcMargin((ENUM_ORDER_TYPE) info.order_type, info.symbol, 1, info.price_open, one_lot)) {
               info.percentage = info.volume / AccountInfoDouble(ACCOUNT_BALANCE) / one_lot;
            } else {
               info.percentage = 0;
            }
            return true;
         }
         Sleep(10);
      }
      return false;
   };
   
   int m_get_deals(long local_ticket_no, DealInfo &info[]) {
      int waitCount = 0;
      int count = 0;
      while (waitCount < 5) {
         if (HistorySelectByPosition(local_ticket_no)) {
            HistoryOrderSelect(local_ticket_no);
            int position_type = m_convert_from_order_type((ENUM_ORDER_TYPE)HistoryOrderGetInteger(local_ticket_no, ORDER_TYPE));
            //--- リスト中の約定の数の合計
            int deals = HistoryDealsTotal();
            //--- 取引をひとつづつ処理する
            for (int i = 0; i < deals; i++) {
               long deal_ticket_no = (long)HistoryDealGetTicket(i);
               int deal_type = m_convert_from_deal_type((ENUM_DEAL_TYPE)HistoryDealGetInteger(deal_ticket_no, DEAL_TYPE));
               if (deal_type == position_type) {
                  continue;
               }
               ArrayResize(info, count + 1);
               info[count].deal_ticket_no = deal_ticket_no;
               info[count].symbol = HistoryDealGetString(deal_ticket_no, DEAL_SYMBOL);
               info[count].order_type = position_type;
               info[count].price = HistoryDealGetDouble(deal_ticket_no, DEAL_PRICE);
               info[count].volume = HistoryDealGetDouble(deal_ticket_no, DEAL_VOLUME);
               info[count].profit = HistoryDealGetDouble(deal_ticket_no, DEAL_PROFIT);
               info[count].time = (datetime)HistoryDealGetInteger(deal_ticket_no, DEAL_TIME);
               count++;
            }
            return count;
         }
         Sleep(10);
      }
      return count;
   };
#endif
#ifdef __MQL4__
   void m_fetch_calculate_volume_params(string symbol, int order_type, double current_price, double &one_lot, double &step, double &account_free_margin, double &account_balance) {
      if (MarketInfo(symbol, MODE_MARGINREQUIRED) < 0) {
         RefreshRates();
      }
      one_lot             = MarketInfo(symbol, MODE_MARGINREQUIRED); //!-lot cost
      step                = MarketInfo(symbol, MODE_LOTSTEP);        // Step in volume changing
      account_free_margin = AccountFreeMargin();                     // Free margin
      account_balance     = AccountBalance();                        // Total Balance
   };
   string m_find_symbol(string symbol_str) {
      for (int i = 0; i < SymbolsTotal(true); i++) {
         if (StringFind(SymbolName(i, true), symbol_str) >= 0) {
            return SymbolName(i, true);
         }
      }
      return NULL;
   };
   
   int m_convert_to_local_order_type(int order_type) {
      if (order_type == 0) {
         return OP_BUY;
      } else if (order_type == 1) {
         return OP_SELL;
      }
      return -1;
   };

   ENUM_ORDER_TYPE m_convert_to_local_position_type(int order_type) {
      if (order_type == 0) {
         return ORDER_TYPE_BUY;
      } else if (order_type == 1) {
         return ORDER_TYPE_SELL;
      }
      return -1;
   };

   int m_convert_from_position_type(ENUM_ORDER_TYPE order_type) {
      if (order_type == ORDER_TYPE_BUY) {
         return 0;
      } else if (order_type == ORDER_TYPE_SELL) {
         return 1;
      }
      return -1;
   };

   int m_convert_from_deal_type(ENUM_ORDER_TYPE deal_type) {
      return m_convert_from_position_type(deal_type);
   };
   
   double m_get_current_price(string symbol, int order_type) {
      if (order_type == 0) {
         return SymbolInfoDouble(symbol, SYMBOL_ASK);
      } else {
         return SymbolInfoDouble(symbol, SYMBOL_BID);
      }
   };
   
   double m_get_min_volume(string symbol) {
      return MarketInfo(symbol, MODE_MINLOT);
   };
   double m_get_max_volume(string symbol) {
      return MarketInfo(symbol, MODE_MAXLOT);
   };

   int m_send_open_order(long server_ticket_no, string symbol, int order_type, double volume, double price, int slippage, string &error_message) {
      if (DRY_RUN) {
         Print("m_send_open_order: server_ticket_no = ", server_ticket_no, ", symbol = ", symbol, ", order_type = ", order_type, ", volume = ", volume, ", price = ", price); 
         return 1;
      }
      int mt4_order_type = m_convert_to_local_order_type(order_type);
      if (mt4_order_type == -1) {
         return -1;
      }
      int result = OrderSend(symbol, mt4_order_type, volume, price, slippage, 0, 0, NULL, (int) server_ticket_no);
      if (result < 0) {
         int error_code = GetLastError();
         error_message = IntegerToString(error_code) + ":" + ErrorCodeToString(error_code);
         return -1;
      } else {
         return result;
      }
   }
   
   bool m_send_close_order(long server_ticket_no, long local_ticket_no, string symbol, int order_type, double volume, double price, int slippage, string &error_message) {
      if (DRY_RUN) {
         Print("m_send_close_order: server_ticket_no = ", server_ticket_no, ", local_ticket_no = ", local_ticket_no, ", symbol = ", symbol, ", order_type = ", order_type, ", volume = ", volume, ", price = ", price); 
         return true;
      }
      if (!OrderClose((int)local_ticket_no, volume, price, slippage)) {
         int error_code = GetLastError();
         error_message = IntegerToString(error_code) + ":" + ErrorCodeToString(error_code);
         return false;
      } else {
         return true;
      }
   }

   int m_get_positions(PositionInfo &arr_positions[], bool includeAll) {
      List<PositionInfo> list;
      int position_count = OrdersTotal();
      int i;
      for(i = 0; i < position_count ; i++) {
         if (!OrderSelect(i, SELECT_BY_POS)) continue;
         long server_ticket_no = OrderMagicNumber();
         if (!includeAll) {
            if (server_ticket_no == 0) continue;
         }
         PositionInfo info;
         info.server_ticket_no = server_ticket_no;
         info.local_ticket_no = OrderTicket();
         info.symbol = OrderSymbol();
         info.order_type = m_convert_from_position_type((ENUM_ORDER_TYPE)OrderType());
         info.position_time = (datetime)OrderOpenTime();
         info.price_open = OrderOpenPrice();
         info.volume = OrderLots();
         if (MarketInfo(info.symbol, MODE_MARGINREQUIRED) < 0) {
            info.percentage = 0;
         } else {
            info.percentage = info.volume / AccountInfoDouble(ACCOUNT_BALANCE) / MarketInfo(info.symbol, MODE_MARGINREQUIRED);
         }
         list.Add(info);
      }
      if (list.Length() == 0) {
         return 0;
      }
      ArrayResize(arr_positions, list.Length());
      for (i = 0; i < list.Length(); i++) {
         arr_positions[i] = list.Get(i);
      }
      return list.Length();
   };
   
   bool m_get_position(long local_ticket_no, PositionInfo &info) {
      int waitCount = 0;
      while (waitCount < 5) {
         if (OrderSelect((int)local_ticket_no, SELECT_BY_TICKET)) {
            info.server_ticket_no = OrderMagicNumber();
            info.local_ticket_no = OrderTicket();
            info.symbol = OrderSymbol();
            info.order_type = m_convert_from_position_type((ENUM_ORDER_TYPE)OrderType());
            info.position_time = (datetime)OrderOpenTime();
            info.price_open = OrderOpenPrice();
            info.volume = OrderLots();
            if (MarketInfo(info.symbol, MODE_MARGINREQUIRED) < 0) {
               info.percentage = 0;
            } else {
               info.percentage = info.volume / AccountInfoDouble(ACCOUNT_BALANCE) / MarketInfo(info.symbol, MODE_MARGINREQUIRED);
            }
            return true;
         }
         Sleep(10);
      }
      return false;
   };
   
   int m_get_deals(long local_ticket_no, DealInfo &info[]) {
      bool select = OrderSelect((int)local_ticket_no, SELECT_BY_TICKET);
      if (select) {
         ArrayResize(info, 1);
         info[0].deal_ticket_no = OrderTicket();
         info[0].symbol = OrderSymbol();
         info[0].order_type = m_convert_from_deal_type((ENUM_ORDER_TYPE)OrderType());
         info[0].price = OrderClosePrice();
         info[0].volume = OrderLots();
         info[0].profit = OrderProfit();
         info[0].time = (datetime)OrderCloseTime();
         return 1;
      }
      return 0;
   };
#endif
public:
   PositionManager(Logger *l_logger) {
      m_logger = l_logger;
   };
   int GetPositions(PositionInfo &arr_positions[], bool includeAll = false) {
      return m_get_positions(arr_positions, includeAll);
   };
   bool CreatePosition(long server_ticket_no, string server_symbol, int server_order_type, double percentage, int slippage) {
      if (m_opening_ticket_no_list.ExistsServerTicket(server_ticket_no)) {
         return true;
      }
      string symbol = m_find_symbol(server_symbol);
      if (symbol == NULL) {
         m_logger.WriteLog("ERROR", "OrderSend fail: Symbol not found. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type));
         return false;
      }
      
      double order_price = m_get_current_price(symbol, server_order_type);
      if (order_price <= 0) {
         m_logger.WriteLog("ERROR", "OrderSend fail: Could not retrieve order price. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type) + ", LocalSymbol = " + symbol);
         return false;
      }
      int order_type = server_order_type;
      if (order_type != 0 && order_type != 1) {
         m_logger.WriteLog("ERROR", "OrderSend fail: Invalid OrderType. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type));
         return false;
      }

      double order_lots = m_calculate_volume(symbol, order_type, order_price, percentage);
      double min_lots = m_get_min_volume(symbol); // Min. amount of lots
      double max_lots = m_get_max_volume(symbol); // Max amount of lotsr
      if (order_lots == 0) {
         m_logger.WriteLog("WARN", "OrderSend fail: Calculated order lot was 0. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type));
         return false;
      }
      if (order_lots < min_lots) {
         m_logger.WriteLog("WARN", "OrderSend fail: Not enough margin. ServerOrder = " + IntegerToString(server_ticket_no) + "/" + server_symbol + "/" + IntegerToString(server_order_type) + ", Calculated lots = " + DoubleToString(order_lots));
         return false;
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
         string error_message = NULL;
         long local_ticket_no = m_send_open_order(server_ticket_no, symbol, order_type, lots, m_get_current_price(symbol, order_type), slippage, error_message);
         if (local_ticket_no < 0) {
            m_logger.WriteLog("ERROR", "OrderSend failed: " + IntegerToString(server_ticket_no) + ", Error = " + error_message);
            break;
         } else {
            success = true;
            m_logger.WriteLog("INFO", "OrderSend succeeded: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
            m_opening_ticket_no_list.Add(PendingOrder::Create(server_ticket_no, server_symbol, server_order_type, local_ticket_no));
         }
      }
      return success;
   };
   void OrderOpened(long server_ticket_no, string server_symbol, int server_order_type, long local_ticket_no) {
      WriteOrderOpenSuccessLog(server_ticket_no, server_symbol, server_order_type, local_ticket_no);
      int index = m_opening_ticket_no_list.IndexOfLocalTicket(local_ticket_no);
      if (index < 0) {
         return;
      }
      m_opening_ticket_no_list.Remove(index);
   };
   bool ClosePosition(long server_ticket_no, string server_symbol, int server_order_type, long local_ticket_no, int slippage) {
      if (m_closing_ticket_no_list.ExistsLocalTicket(local_ticket_no)) {
         return true;
      }
      m_logger.WriteLog("DEBUG", "OrderClose executing: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
      PositionInfo info;
      if (!m_get_position(local_ticket_no, info)) {
         m_logger.WriteLog("ERROR", "OrderClose fail: position not exists. " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
         return true;
      }
      int order_type = -1;
      if (info.order_type == 0) {
         order_type = 1;
      } else if (info.order_type == 1) {
         order_type = 0;
      } else {
         m_logger.WriteLog("ERROR", "OrderClose fail: position order type is invalid. " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
         return true;
      }
      string error_message = NULL;
      if (m_send_close_order(server_ticket_no, local_ticket_no, info.symbol, order_type, info.volume, m_get_current_price(info.symbol, order_type), slippage, error_message)) {
         m_logger.WriteLog("INFO", "OrderClose succeeded: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no));
         m_closing_ticket_no_list.Add(PendingOrder::Create(server_ticket_no, server_symbol, server_order_type, local_ticket_no));
         return true;
      } else {
         m_logger.WriteLog("ERROR", "OrderClose failed: " + IntegerToString(server_ticket_no) + ", OrderTicket = " + IntegerToString(local_ticket_no) + ", Error = " + error_message);
         return false;
      }
   };
   void OrderClosed(long server_ticket_no, string server_symbol, int server_order_type, long local_ticket_no) {
      WriteOrderCloseSuccessLog(server_ticket_no, server_symbol, server_order_type, local_ticket_no);
      int index = m_closing_ticket_no_list.IndexOfLocalTicket(local_ticket_no);
      if (index < 0) {
         return;
      }
      m_closing_ticket_no_list.Remove(index);
   };

   void WriteOrderOpenSuccessLog(long server_ticket_no, string server_symbol, int server_order_type, long local_ticket_no) {
      PositionInfo info;
      bool found = m_get_position(local_ticket_no, info);
      string text = "OPEN:" + IntegerToString(server_ticket_no) + ":" + server_symbol + ":" + IntegerToString(server_order_type) + ":" + IntegerToString(local_ticket_no) + ":";
      if (found) {
         text = text + IntegerToString(info.local_ticket_no) + ":" + info.symbol + ":" + IntegerToString(info.order_type) + ":" + DoubleToString(info.price_open) + ":" + DoubleToString(info.volume) + ":" + IntegerToString(info.position_time);
      } else {
         text = text + "NA:NA:NA:NA:NA:NA";
      }
      m_logger.WriteLog("DEBUG", text);
   }  
   
   void WriteOrderCloseSuccessLog(long server_ticket_no, string server_symbol, int server_order_type, long local_ticket_no) {
      DealInfo info[];
      string text;
      int count = m_get_deals(local_ticket_no, info);
      if (count > 0) {
         for (int i = 0; i < count; i++) {
            text = "CLOSE:" + IntegerToString(server_ticket_no) + ":" + server_symbol + ":" + IntegerToString(server_order_type) + ":" + IntegerToString(local_ticket_no) + ":";
            text = text + IntegerToString(info[i].deal_ticket_no) + ":" + info[i].symbol + ":" + IntegerToString(info[i].order_type) + ":" + DoubleToString(info[i].price) + ":" + DoubleToString(info[i].volume) + ":" + IntegerToString(info[i].time) + ":" + DoubleToString(info[i].profit);
            m_logger.WriteLog("DEBUG", text);
         }
      } else {
         text = "CLOSE:" + IntegerToString(server_ticket_no) + ":" + server_symbol + ":" + IntegerToString(server_order_type) + ":" + IntegerToString(local_ticket_no) + ":";
         text = text + "NA:NA:NA:NA:NA:NA:NA";
         m_logger.WriteLog("DEBUG", text);
      }
   }
};


struct CopyTradeInfo {
   long server_ticket_no;
   string symbol;
   int order_type;
   long timestamp;
   double price;
   double percentage;
   CopyTradeInfo() { 
      server_ticket_no = 0;
      symbol = "";
      order_type = -1;
   };
   static CopyTradeInfo Create(long server_ticket_no, string symbol, int order_type, long timestamp, double price, double percentage) {
      CopyTradeInfo info;
      info.server_ticket_no = server_ticket_no;
      info.symbol = symbol;
      info.order_type = order_type;
      info.timestamp = timestamp;
      info.price = price;
      info.percentage = percentage;
      return info;
   };
   static CopyTradeInfo Parse(string copy_trade_string, string &parse_error) {
      string splittedValues[];
      if (StringSplit(copy_trade_string, StringGetCharacter(":", 0), splittedValues) != 6) {
         parse_error = "Invalid Data: " + copy_trade_string;
         CopyTradeInfo info;
         return info;
      }
      return Create((long) StringToInteger(splittedValues[0]), splittedValues[1], (int) StringToInteger(splittedValues[2]), StringToInteger(splittedValues[3]), StringToDouble(splittedValues[4]), StringToDouble(splittedValues[5]));
   };
   
   string ToString() {
      return IntegerToString(server_ticket_no) + "/" + symbol + "/" + IntegerToString(order_type);
   };
};

class CopyTradeInfoParser {
public:
   static List<CopyTradeInfo> *Parse(string response, string &parse_error) {
      List<CopyTradeInfo> *list = new List<CopyTradeInfo>();
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
   string m_ea_type;
   double m_next_token_fetch_time;
   ApiStatus *m_post_secret_key_status;
   ApiStatus *m_post_token_release_status;
   ApiStatus *m_get_orders_status;
   ApiStatus *m_post_orders_status;
   ApiStatus *m_post_ping_status;
   ApiStatus *m_post_log_status;
   string m_get_orders_etag;
   string m_get_orders_etag_response;
   EaState *m_state;
   Logger *m_logger;

   string m_generate_secret_key() {
#ifdef __MQL5__
      return "MT5/" + AccountInfoString(ACCOUNT_COMPANY) + "/" + IntegerToString(AccountInfoInteger(ACCOUNT_LOGIN)) + "/" + IntegerToString(AccountInfoInteger(ACCOUNT_TRADE_MODE));
#endif
#ifdef __MQL4__
      return "MT4/" + AccountInfoString(ACCOUNT_COMPANY) + "/" + IntegerToString(AccountInfoInteger(ACCOUNT_LOGIN)) + "/" + IntegerToString(AccountInfoInteger(ACCOUNT_TRADE_MODE));
#endif
   };
   
   string m_generate_header(string method, bool useToken, bool needToken) {
      string header = "X-Ea-Id: " + m_generate_secret_key() + "\r\nX-Ea-Type: " + m_ea_type + "\r\nX-Ea-Version: " + Version;
      string ea_state = m_state.GetState();
      if (ea_state != NULL) {
         header += "\r\nX-Ea-State: " + ea_state;
      }
      if (useToken) {
         string secret_token = m_get_secret_token();
         if (needToken && secret_token == NULL) {
            return NULL;
         }
         if (secret_token != NULL) {
            header += "\r\nX-Secret-Token: " + secret_token;
         }
      }
      if (method == "POST") {
         header += "\r\nContent-Type: text/plain; charset=UTF-8;";
      }
      return header;
   };
   
   string m_get_secret_token() {
      if (m_secret_token == NULL) {
         if (m_post_secret_key() > 0) {
            return m_secret_token;
         }
         return NULL;
      }
      if (m_state.KeyIsValid()) {
         return m_secret_token;
      }
      if (m_post_secret_key() > 0) {
         return m_secret_token;
      }
      return NULL;
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
      if (m_next_token_fetch_time > GetTickCount()) {
         return -1;
      }
      m_next_token_fetch_time = GetTickCount() + 2000; // 2sec later
      string request_headers = m_generate_header("POST", false, false);
      string request_data = m_secret_key;
      string response_headers;
      string response_data;
      int error_code;
      int res = m_send_web_request(m_post_secret_key_status, "POST", Endpoint + "/api/ea/token/generate", request_headers, request_data, response_headers, response_data, error_code);
      if (res > 500) {
         return -1;
      }
      if (res >= 400) {
         m_state.SetKeyIsValid(false);
         m_secret_token = NULL;
         return -1;
      } else {
         m_state.SetKeyIsValid(true);
         m_secret_token = response_data;
         return res;
      }
   };
   
   int m_post_token_release()
   {
      string request_headers = m_generate_header("POST", true, true);
      string request_data;
      string response_headers;
      string response_data;
      int error_code;
   
      int res = m_send_web_request(m_post_token_release_status, "POST", Endpoint + "/api/ea/token/release", request_headers, request_data, response_headers, response_data, error_code);
      m_state.SetKeyIsValid(false);
      if(res != 200) {
         return -1;
      }
      return res;
   };

   int m_send_web_request(ApiStatus *status, string method, string url, string request_headers, string request_data_string, string &response_headers, string &response_data_string, int &error_code) {
      int timeout = 5000;
      char request_data[];
      char response_data[];
      
      if (request_data_string != NULL || request_data_string != "") {
         StringToCharArray(request_data_string, request_data, 0, WHOLE_ARRAY, CP_UTF8);
      }

      m_state.Lap(status.GetName() +  " begin");
      int res = WebRequest(method, url, request_headers, timeout, request_data, response_data, response_headers);
      m_state.Lap(status.GetName() +  " end");
      if (res == -1) {
         m_state.SetServerConnection(false);
         error_code = GetLastError();
         status.SetErrorCode(error_code);
         return res;
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
         if (StringCompare(StringSubstr(line, 0, 14), "X-Environment:", false) == 0) {
            string env = StringSubstr(line, 14);
            StringTrimRight(env);
            StringTrimLeft(env);
            m_state.SetEnvironment(env);
         }
      }
      if (res > 500) {
         m_state.SetServerConnection(false);
      } else {
         m_state.SetServerConnection(true);
      }
      error_code = -1;
      status.SetErrorCode(error_code);
      status.SetStatusCode(res);
      response_data_string = CharArrayToString(response_data, 0, WHOLE_ARRAY, CP_UTF8);
      return res;
   }

public:
   TrsysClient(EaState *l_state, Logger *l_logger, string l_ea_type) {
      m_secret_key = m_generate_secret_key();
      m_secret_token = NULL;
      m_next_token_fetch_time = -1;
      m_post_secret_key_status = new ApiStatus("PostSecretKey");
      m_post_token_release_status = new ApiStatus("PostTokenRelease");
      m_get_orders_status = new ApiStatus("GetOrders");
      m_post_orders_status = new ApiStatus("PostOrders");
      m_post_ping_status = new ApiStatus("PostPing");
      m_post_log_status = new ApiStatus("PostLog");
      m_get_orders_etag = NULL;
      m_get_orders_etag_response = NULL;
      m_state = l_state;
      m_logger = l_logger;
      m_ea_type = l_ea_type;
   }
   ~TrsysClient() {
      m_clear_secret_token();
      delete m_post_secret_key_status;
      delete m_post_token_release_status;
      delete m_get_orders_status;
      delete m_post_orders_status;
      delete m_post_ping_status;
      delete m_post_log_status;
   }
   
   int GetOrders(string &response)
   {
      string request_headers = m_generate_header("GET", true, true);
      if (request_headers == NULL) {
         return -1;
      }   
      string request_data;
      string response_headers;
      string response_data;
      int error_code;
    
      if (m_get_orders_etag != NULL) {
         request_headers += "\r\nIf-None-Match: " + m_get_orders_etag;
      }

      int res = m_send_web_request(m_get_orders_status, "GET", Endpoint + "/api/ea/orders", request_headers, request_data, response_headers, response_data, error_code);
      if (res == 304) {
         response = m_get_orders_etag_response;
         return 200;
      }
      if (res == 400) {
         m_state.SetKeyIsValid(false);
         return -1;
      }
      if (res == 401) {
         m_state.SetKeyIsAuthorize(false);
         m_clear_secret_token();
         return -1;
      }
      m_state.SetKeyIsValid(true);
      m_state.SetKeyIsAuthorize(true);
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
   
   int PostOrders(string data)
   {
      string request_headers = m_generate_header("POST", true, true);
      if (request_headers == NULL) {
         return -1;
      }
      string request_data = data;
      string response_headers;
      string response_data;
      int error_code;

      int res = m_send_web_request(m_post_orders_status, "POST", Endpoint + "/api/ea/orders", request_headers, request_data, response_headers, response_data, error_code);
      if (res == 400) {
         m_state.SetKeyIsValid(false);
         return -1;
      }
      if (res == 401) {
         m_state.SetKeyIsAuthorize(false);
         m_clear_secret_token();
         return -1;
      }
      m_state.SetKeyIsValid(true);
      m_state.SetKeyIsAuthorize(true);
      if(res != 200) {
         return -1;
      }

      return res;
   }
   
   int PostPing()
   {
      string request_headers = m_generate_header("POST", true, true);
      if (request_headers == NULL) {
         return -1;
      }
      string request_data = "";
      string response_headers;
      string response_data;
      int error_code;

      int res = m_send_web_request(m_post_ping_status, "POST", Endpoint + "/api/ea/ping", request_headers, request_data, response_headers, response_data, error_code);
      if (res == 400) {
         m_state.SetKeyIsValid(false);
         return -1;
      }
      if (res == 401) {
         m_state.SetKeyIsAuthorize(false);
         m_clear_secret_token();
         return -1;
      }
      m_state.SetKeyIsValid(true);
      m_state.SetKeyIsAuthorize(true);
      if(res != 200) {
         return -1;
      }

      return res;
   }
   
   int PostLog(Logger &q)
   {
      if (m_post_log_status.GetLastStatus() == 404) {
         return 0;
      }
      string logs[];
      int peak = q.Peak(logs, 10);
      if (peak == 0) {
         return 0;
      }
      string request_headers = m_generate_header("POST", true, false);
      if (request_headers == NULL) {
         return -1;
      }
      string request_data = "";
      string response_headers;
      string response_data;
      int error_code;
      
      for (int i = 0; i < peak; i++) {
         request_data = request_data + logs[i] + "\r\n";
      }
   
      int res = m_send_web_request(m_post_log_status, "POST", Endpoint + "/api/ea/logs", request_headers, request_data, response_headers, response_data, error_code);
      if(res != 202) {
         return -1;
      }
      q.Dequeue(peak);
      return 0;
   }

};

class RemoteOrderState {
   PositionManager *m_position_manager;
   TrsysClient *m_client;
   Logger *m_logger;
   List<CopyTradeInfo> m_orders;
   List<CopyTradeInfo> *m_server_orders;
   string m_last_server_response;

   void m_initialize() {
      PositionInfo arr_position[];
      int arr_position_count = m_position_manager.GetPositions(arr_position);
      for (int i = 0; i < arr_position_count; i++) {
         PositionInfo info = arr_position[i];
         if (m_index_of(info.server_ticket_no) >= 0) continue;
         m_orders.Add(CopyTradeInfo::Create(info.server_ticket_no, info.symbol, info.order_type, 0, 0, 0));
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
   RemoteOrderState(PositionManager *l_position_manager, TrsysClient *l_client, Logger *l_logger) {
      m_position_manager = l_position_manager;
      m_client = l_client;
      m_logger = l_logger;
      m_last_server_response = NULL;
      m_initialize();
      m_server_orders = new List<CopyTradeInfo>();
   };
   ~RemoteOrderState() {
      delete m_server_orders;
   };
   TicketNoDifference<CopyTradeInfo> GetDifference() {
      TicketNoDifference<CopyTradeInfo> diff;
      int i,j;
      bool exists;
      string response;
      if (m_client.GetOrders(response) == 200) {
         if (m_last_server_response != response) {
            string error = NULL;
            List<CopyTradeInfo> *serverInfo = CopyTradeInfoParser::Parse(response, error);
            if (error == NULL) {
               delete m_server_orders;
               m_server_orders = serverInfo;
            } else {
               delete serverInfo;
               m_logger.WriteLog("ERROR", error);
            }
            m_last_server_response = response;
         }
         for (i = 0; i < m_orders.Length(); i++) {
            CopyTradeInfo ii = m_orders.Get(i);
            exists = false;
            for (j = 0; j < m_server_orders.Length(); j++) {
               CopyTradeInfo ij = m_server_orders.Get(j);
               if (ii.server_ticket_no == ij.server_ticket_no) {
                  exists = true;
                  break;
               }
            }
            if (!exists) {
               diff.Closed(ii);
            }
         }
         for (i = 0; i < m_server_orders.Length(); i++) {
            CopyTradeInfo ii = m_server_orders.Get(i);
            exists = false;
            for (j = 0; j < m_orders.Length(); j++) {
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
   PositionManager *m_position_manager;
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
      PositionInfo arr_position[];
      int arr_position_count = m_position_manager.GetPositions(arr_position);
      for (int i = 0; i < arr_position_count; i++) {
         PositionInfo info = arr_position[i];
         m_orders.Add(LocalOrderInfo::Create(info.server_ticket_no, info.local_ticket_no, info.symbol, info.order_type));
      }
   };
public:
   LocalOrderState(PositionManager *l_position_manager) {
      m_position_manager = l_position_manager;
      m_initialize();
   };

   TicketNoDifference<LocalOrderInfo> GetDifference(bool includeAll = false) {
      TicketNoDifference<LocalOrderInfo> diff;
      List<LocalOrderInfo> localInfo;
      int i,j;
      bool exists;

      PositionInfo arr_position[];
      int arr_position_count = m_position_manager.GetPositions(arr_position, includeAll);
      for (i = 0; i < arr_position_count; i++) {
         PositionInfo info = arr_position[i];
         localInfo.Add(LocalOrderInfo::Create(info.server_ticket_no, info.local_ticket_no, info.symbol, info.order_type));
      }

      for (i = 0; i < m_orders.Length(); i++) {
         LocalOrderInfo ii = m_orders.Get(i);
         exists = false;
         for (j = 0; j < localInfo.Length(); j++) {
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
      for (i = 0; i < localInfo.Length(); i++) {
         LocalOrderInfo ii = localInfo.Get(i);
         exists = false;
         for (j = 0; j < m_orders.Length(); j++) {
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
      int i;
      for (i = 0; i < m_orders.Length(); i++) {
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
      for (i = 0; i < m_orders.Length(); i++) {
         LocalOrderInfo info = m_orders.Get(i);
         if (info.server_ticket_no == server_ticket_no) {
            arr_ticket_no[j] = info.local_ticket_no;
            j++;
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

class FailedOrderList {
   List<CopyTradeInfo> *m_copy_trade_info_list;
public:
   FailedOrderList() {
      m_copy_trade_info_list = new List<CopyTradeInfo>();
   };
   ~FailedOrderList() {
      delete m_copy_trade_info_list;
   };
   void Failed(CopyTradeInfo &copy_trade_info) {
      m_copy_trade_info_list.Add(copy_trade_info);
   };
   void Remove(CopyTradeInfo &copy_trade_info) {
      for (int i = 0; i < m_copy_trade_info_list.Length(); i++) {
         if (m_copy_trade_info_list.Get(i).server_ticket_no == copy_trade_info.server_ticket_no) {
            m_copy_trade_info_list.Remove(i);
         }
      };
   };
   int ServerTicketNoFailedCount(long server_ticket_no) {
      int failed_count = 0;
      for (int i = 0; i < m_copy_trade_info_list.Length(); i++) {
         if (m_copy_trade_info_list.Get(i).server_ticket_no == server_ticket_no) {
            failed_count++;
         }
      };
      return failed_count;
   };
};

string ErrorCodeToString(int error_code) {
   switch (error_code) {
#ifdef __MQL4__
      case ERR_NO_ERROR:
          return "No error returned.";
      case ERR_NO_RESULT:
          return "No error returned, but the result is unknown.";
      case ERR_COMMON_ERROR:
          return "Common error.";
      case ERR_INVALID_TRADE_PARAMETERS:
          return "Invalid trade parameters.";
      case ERR_SERVER_BUSY:
          return "Trade server is busy.";
      case ERR_OLD_VERSION:
          return "Old version of the client terminal.";
      case ERR_NO_CONNECTION:
          return "No connection with trade server.";
      case ERR_NOT_ENOUGH_RIGHTS:
          return "Not enough rights.";
      case ERR_TOO_FREQUENT_REQUESTS:
          return "Too frequent requests.";
      case ERR_MALFUNCTIONAL_TRADE:
          return "Malfunctional trade operation.";
      case ERR_ACCOUNT_DISABLED:
          return "Account disabled.";
      case ERR_INVALID_ACCOUNT:
          return "Invalid account.";
      case ERR_TRADE_TIMEOUT:
          return "Trade timeout.";
      case ERR_INVALID_PRICE:
          return "Invalid price.";
      case ERR_INVALID_STOPS:
          return "Invalid stops.";
      case ERR_INVALID_TRADE_VOLUME:
          return "Invalid trade volume.";
      case ERR_MARKET_CLOSED:
          return "Market is closed.";
      case ERR_TRADE_DISABLED:
          return "Trade is disabled.";
      case ERR_NOT_ENOUGH_MONEY:
          return "Not enough money.";
      case ERR_PRICE_CHANGED:
          return "Price changed.";
      case ERR_OFF_QUOTES:
          return "Off quotes.";
      case ERR_BROKER_BUSY:
          return "Broker is busy.";
      case ERR_REQUOTE:
          return "Requote.";
      case ERR_ORDER_LOCKED:
          return "Order is locked.";
      case ERR_LONG_POSITIONS_ONLY_ALLOWED:
          return "Long positions only allowed.";
      case ERR_TOO_MANY_REQUESTS:
          return "Too many requests.";
      case ERR_TRADE_MODIFY_DENIED:
          return "Modification denied because an order is too close to market.";
      case ERR_TRADE_CONTEXT_BUSY:
          return "Trade context is busy.";
      case ERR_TRADE_EXPIRATION_DENIED:
          return "Expirations are denied by broker.";
      case ERR_TRADE_TOO_MANY_ORDERS:
          return "The amount of opened and pending orders has reached the limit set by a broker.";
      case ERR_NO_MQLERROR:
          return "No error.";
      case ERR_WRONG_FUNCTION_POINTER:
          return "Wrong function pointer.";
      case ERR_ARRAY_INDEX_OUT_OF_RANGE:
          return "Array index is out of range.";
      case ERR_RECURSIVE_STACK_OVERFLOW:
          return "Recursive stack overflow.";
      case ERR_NO_MEMORY_FOR_TEMP_STRING:
          return "No memory for temp string.";
      case ERR_NOT_INITIALIZED_STRING:
          return "Not initialized string.";
      case ERR_NOT_INITIALIZED_ARRAYSTRING:
          return "Not initialized string in an array.";
      case ERR_NO_MEMORY_FOR_ARRAYSTRING:
          return "No memory for an array string.";
      case ERR_TOO_LONG_STRING:
          return "Too long string.";
      case ERR_REMAINDER_FROM_ZERO_DIVIDE:
          return "Remainder from zero divide.";
      case ERR_ZERO_DIVIDE:
          return "Zero divide.";
      case ERR_UNKNOWN_COMMAND:
          return "Unknown command.";
      case ERR_WRONG_JUMP:
          return "Wrong jump.";
      case ERR_NOT_INITIALIZED_ARRAY:
          return "Not initialized array.";
      case ERR_DLL_CALLS_NOT_ALLOWED:
          return "DLL calls are not allowed.";
      case ERR_CANNOT_LOAD_LIBRARY:
          return "Cannot load library.";
      case ERR_CANNOT_CALL_FUNCTION:
          return "Cannot call function.";
      case ERR_SYSTEM_BUSY:
          return "System is busy.";
      case ERR_SOME_ARRAY_ERROR:
          return "Some array error.";
      case ERR_CUSTOM_INDICATOR_ERROR:
          return "Custom indicator error.";
      case ERR_INCOMPATIBLE_ARRAYS:
          return "Arrays are incompatible.";
      case ERR_GLOBAL_VARIABLE_NOT_FOUND:
          return "Global variable not found.";
      case ERR_FUNCTION_NOT_CONFIRMED:
          return "Function is not confirmed.";
      case ERR_SEND_MAIL_ERROR:
          return "Mail sending error.";
      case ERR_STRING_PARAMETER_EXPECTED:
          return "String parameter expected.";
      case ERR_INTEGER_PARAMETER_EXPECTED:
          return "Integer parameter expected.";
      case ERR_DOUBLE_PARAMETER_EXPECTED:
          return "Double parameter expected.";
      case ERR_ARRAY_AS_PARAMETER_EXPECTED:
          return "Array as parameter expected.";
      case ERR_HISTORY_WILL_UPDATED:
          return "Requested history data in updating state.";
      case ERR_TRADE_ERROR:
          return "Some error in trade operation execution.";
      case ERR_END_OF_FILE:
          return "End of a file.";
      case ERR_SOME_FILE_ERROR:
          return "Some file error.";
      case ERR_WRONG_FILE_NAME:
          return "Wrong file name.";
      case ERR_TOO_MANY_OPENED_FILES:
          return "Too many opened files.";
      case ERR_CANNOT_OPEN_FILE:
          return "Cannot open file.";
      case ERR_NO_ORDER_SELECTED:
          return "No order selected.";
      case ERR_UNKNOWN_SYMBOL:
          return "Unknown symbol.";
      case ERR_INVALID_PRICE_PARAM:
          return "Invalid price.";
      case ERR_INVALID_TICKET:
          return "Invalid ticket.";
      case ERR_TRADE_NOT_ALLOWED:
          return "Trade is not allowed.";
      case ERR_LONGS_NOT_ALLOWED:
          return "Longs are not allowed.";
      case ERR_SHORTS_NOT_ALLOWED:
          return "Shorts are not allowed.";
      case ERR_OBJECT_ALREADY_EXISTS:
          return "Object already exists.";
      case ERR_UNKNOWN_OBJECT_PROPERTY:
          return "Unknown object property.";
      case ERR_OBJECT_DOES_NOT_EXIST:
          return "Object does not exist.";
      case ERR_UNKNOWN_OBJECT_TYPE:
          return "Unknown object type.";
      case ERR_NO_OBJECT_NAME:
          return "No object name.";
      case ERR_OBJECT_COORDINATES_ERROR:
          return "Object coordinates error.";
      case ERR_NO_SPECIFIED_SUBWINDOW:
          return "No specified subwindow.";
      case ERR_SOME_OBJECT_ERROR:
          return "Some error in object operation.";
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
#endif 
#ifdef __MQL5__
      case ERR_SUCCESS:
          return "The operation completed successfully";
      case ERR_INTERNAL_ERROR:
          return "Unexpected internal error";
      case ERR_WRONG_INTERNAL_PARAMETER:
          return "Wrong parameter in the inner call of the client terminal function";
      case ERR_INVALID_PARAMETER:
          return "Wrong parameter when calling the system function";
      case ERR_NOT_ENOUGH_MEMORY:
          return "Not enough memory to perform the system function";
      case ERR_STRUCT_WITHOBJECTS_ORCLASS:
          return "The structure contains objects of strings and/or dynamic arrays and/or structure of such objects and/or classes";
      case ERR_INVALID_ARRAY:
          return "Array of a wrong type, wrong size, or a damaged object of a dynamic array";
      case ERR_ARRAY_RESIZE_ERROR:
          return "Not enough memory for the relocation of an array, or an attempt to change the size of a static array";
      case ERR_STRING_RESIZE_ERROR:
          return "Not enough memory for the relocation of string";
      case ERR_NOTINITIALIZED_STRING:
          return "Not initialized string";
      case ERR_INVALID_DATETIME:
          return "Invalid date and/or time";
      case ERR_ARRAY_BAD_SIZE:
          return "Total amount of elements in the array cannot exceed 2147483647";
      case ERR_INVALID_POINTER:
          return "Wrong pointer";
      case ERR_INVALID_POINTER_TYPE:
          return "Wrong type of pointer";
      case ERR_FUNCTION_NOT_ALLOWED:
          return "Function is not allowed for call";
      case ERR_RESOURCE_NAME_DUPLICATED:
          return "The names of the dynamic and the static resource match";
      case ERR_RESOURCE_NOT_FOUND:
          return "Resource with this name has not been found in EX5";
#ifdef __MQL4__
      case ERR_RESOURCE_UNSUPPOTED_TYPE:
          return "Unsupported resource type or its size exceeds 16 Mb";
#endif
      case ERR_RESOURCE_NAME_IS_TOO_LONG:
          return "The resource name exceeds 63 characters";
      case ERR_MATH_OVERFLOW:
          return "Overflow occurred when calculating math function";
      case ERR_SLEEP_ERROR:
          return "Out of test end date after calling Sleep()";
      case ERR_PROGRAM_STOPPED:
          return "Test forcibly stopped from the outside. For example, optimization interrupted, visual testing window closed or testing agent stopped";
      case ERR_CHART_WRONG_ID:
          return "Wrong chart ID";
      case ERR_CHART_NO_REPLY:
          return "Chart does not respond";
      case ERR_CHART_NOT_FOUND:
          return "Chart not found";
      case ERR_CHART_NO_EXPERT:
          return "No Expert Advisor in the chart that could handle the event";
      case ERR_CHART_CANNOT_OPEN:
          return "Chart opening error";
      case ERR_CHART_CANNOT_CHANGE:
          return "Failed to change chart symbol and period";
      case ERR_CHART_WRONG_PARAMETER:
          return "Error value of the parameter for the function of working with charts";
      case ERR_CHART_CANNOT_CREATE_TIMER:
          return "Failed to create timer";
      case ERR_CHART_WRONG_PROPERTY:
          return "Wrong chart property ID";
      case ERR_CHART_SCREENSHOT_FAILED:
          return "Error creating screenshots";
      case ERR_CHART_NAVIGATE_FAILED:
          return "Error navigating through chart";
      case ERR_CHART_TEMPLATE_FAILED:
          return "Error applying template";
      case ERR_CHART_WINDOW_NOT_FOUND:
          return "Subwindow containing the indicator was not found";
      case ERR_CHART_INDICATOR_CANNOT_ADD:
          return "Error adding an indicator to chart";
      case ERR_CHART_INDICATOR_CANNOT_DEL:
          return "Error deleting an indicator from the chart";
      case ERR_CHART_INDICATOR_NOT_FOUND:
          return "Indicator not found on the specified chart";
      case ERR_OBJECT_ERROR:
          return "Error working with a graphical object";
      case ERR_OBJECT_NOT_FOUND:
          return "Graphical object was not found";
      case ERR_OBJECT_WRONG_PROPERTY:
          return "Wrong ID of a graphical object property";
      case ERR_OBJECT_GETDATE_FAILED:
          return "Unable to get date corresponding to the value";
      case ERR_OBJECT_GETVALUE_FAILED:
          return "Unable to get value corresponding to the date";
      case ERR_MARKET_UNKNOWN_SYMBOL:
          return "Unknown symbol";
      case ERR_MARKET_NOT_SELECTED:
          return "Symbol is not selected in MarketWatch";
      case ERR_MARKET_WRONG_PROPERTY:
          return "Wrong identifier of a symbol property";
      case ERR_MARKET_LASTTIME_UNKNOWN:
          return "Time of the last tick is not known (no ticks)";
      case ERR_MARKET_SELECT_ERROR:
          return "Error adding or deleting a symbol in MarketWatch";
      case ERR_HISTORY_NOT_FOUND:
          return "Requested history not found";
      case ERR_HISTORY_WRONG_PROPERTY:
          return "Wrong ID of the history property";
      case ERR_HISTORY_TIMEOUT:
          return "Exceeded history request timeout";
      case ERR_HISTORY_BARS_LIMIT:
          return "Number of requested bars limited by terminal settings";
      case ERR_HISTORY_LOAD_ERRORS:
          return "Multiple errors when loading history";
      case ERR_HISTORY_SMALL_BUFFER:
          return "Receiving array is too small to store all requested data";
      case ERR_GLOBALVARIABLE_NOT_FOUND:
          return "Global variable of the client terminal is not found";
      case ERR_GLOBALVARIABLE_EXISTS:
          return "Global variable of the client terminal with the same name already exists";
      case ERR_GLOBALVARIABLE_NOT_MODIFIED:
          return "Global variables were not modified";
      case ERR_GLOBALVARIABLE_CANNOTREAD:
          return "Cannot read file with global variable values";
      case ERR_GLOBALVARIABLE_CANNOTWRITE:
          return "Cannot write file with global variable values";
      case ERR_MAIL_SEND_FAILED:
          return "Email sending failed";
      case ERR_PLAY_SOUND_FAILED:
          return "Sound playing failed";
      case ERR_MQL5_WRONG_PROPERTY:
          return "Wrong identifier of the program property";
      case ERR_TERMINAL_WRONG_PROPERTY:
          return "Wrong identifier of the terminal property";
      case ERR_FTP_SEND_FAILED:
          return "File sending via ftp failed";
      case ERR_NOTIFICATION_SEND_FAILED:
          return "Failed to send a notification";
      case ERR_NOTIFICATION_WRONG_PARAMETER:
          return "Invalid parameter for sending a notification – an empty string or NULL has been passed to the SendNotification() function";
      case ERR_NOTIFICATION_WRONG_SETTINGS:
          return "Wrong settings of notifications in the terminal (ID is not specified or permission is not set)";
      case ERR_NOTIFICATION_TOO_FREQUENT:
          return "Too frequent sending of notifications";
      case ERR_FTP_NOSERVER:
          return "FTP server is not specified";
      case ERR_FTP_NOLOGIN:
          return "FTP login is not specified";
      case ERR_FTP_FILE_ERROR:
          return "File not found in the MQL5\\Files directory to send on FTP server";
      case ERR_FTP_CONNECT_FAILED:
          return "FTP connection failed";
      case ERR_FTP_CHANGEDIR:
          return "FTP path not found on server";
      case ERR_BUFFERS_NO_MEMORY:
          return "Not enough memory for the distribution of indicator buffers";
      case ERR_BUFFERS_WRONG_INDEX:
          return "Wrong indicator buffer index";
      case ERR_CUSTOM_WRONG_PROPERTY:
          return "Wrong ID of the custom indicator property";
      case ERR_ACCOUNT_WRONG_PROPERTY:
          return "Wrong account property ID";
      case ERR_TRADE_WRONG_PROPERTY:
          return "Wrong trade property ID";
      case ERR_TRADE_DISABLED:
          return "Trading by Expert Advisors prohibited";
      case ERR_TRADE_POSITION_NOT_FOUND:
          return "Position not found";
      case ERR_TRADE_ORDER_NOT_FOUND:
          return "Order not found";
      case ERR_TRADE_DEAL_NOT_FOUND:
          return "Deal not found";
      case ERR_TRADE_SEND_FAILED:
          return "Trade request sending failed";
      case ERR_TRADE_CALC_FAILED:
          return "Failed to calculate profit or margin";
      case ERR_INDICATOR_UNKNOWN_SYMBOL:
          return "Unknown symbol";
      case ERR_INDICATOR_CANNOT_CREATE:
          return "Indicator cannot be created";
      case ERR_INDICATOR_NO_MEMORY:
          return "Not enough memory to add the indicator";
      case ERR_INDICATOR_CANNOT_APPLY:
          return "The indicator cannot be applied to another indicator";
      case ERR_INDICATOR_CANNOT_ADD:
          return "Error applying an indicator to chart";
      case ERR_INDICATOR_DATA_NOT_FOUND:
          return "Requested data not found";
      case ERR_INDICATOR_WRONG_HANDLE:
          return "Wrong indicator handle";
      case ERR_INDICATOR_WRONG_PARAMETERS:
          return "Wrong number of parameters when creating an indicator";
      case ERR_INDICATOR_PARAMETERS_MISSING:
          return "No parameters when creating an indicator";
      case ERR_INDICATOR_CUSTOM_NAME:
          return "The first parameter in the array must be the name of the custom indicator";
      case ERR_INDICATOR_PARAMETER_TYPE:
          return "Invalid parameter type in the array when creating an indicator";
      case ERR_INDICATOR_WRONG_INDEX:
          return "Wrong index of the requested indicator buffer";
      case ERR_BOOKS_CANNOT_ADD:
          return "Depth Of Market can not be added";
      case ERR_BOOKS_CANNOT_DELETE:
          return "Depth Of Market can not be removed";
      case ERR_BOOKS_CANNOT_GET:
          return "The data from Depth Of Market can not be obtained";
      case ERR_BOOKS_CANNOT_SUBSCRIBE:
          return "Error in subscribing to receive new data from Depth Of Market";
      case ERR_TOO_MANY_FILES:
          return "More than 64 files cannot be opened at the same time";
      case ERR_WRONG_FILENAME:
          return "Invalid file name";
      case ERR_TOO_LONG_FILENAME:
          return "Too long file name";
      case ERR_CANNOT_OPEN_FILE:
          return "File opening error";
      case ERR_FILE_CACHEBUFFER_ERROR:
          return "Not enough memory for cache to read";
      case ERR_CANNOT_DELETE_FILE:
          return "File deleting error";
      case ERR_INVALID_FILEHANDLE:
          return "A file with this handle was closed, or was not opening at all";
      case ERR_WRONG_FILEHANDLE:
          return "Wrong file handle";
      case ERR_FILE_NOTTOWRITE:
          return "The file must be opened for writing";
      case ERR_FILE_NOTTOREAD:
          return "The file must be opened for reading";
      case ERR_FILE_NOTBIN:
          return "The file must be opened as a binary one";
      case ERR_FILE_NOTTXT:
          return "The file must be opened as a text";
      case ERR_FILE_NOTTXTORCSV:
          return "The file must be opened as a text or CSV";
      case ERR_FILE_NOTCSV:
          return "The file must be opened as CSV";
      case ERR_FILE_READERROR:
          return "File reading error";
      case ERR_FILE_BINSTRINGSIZE:
          return "String size must be specified, because the file is opened as binary";
      case ERR_INCOMPATIBLE_FILE:
          return "A text file must be for string arrays, for other arrays - binary";
      case ERR_FILE_IS_DIRECTORY:
          return "This is not a file, this is a directory";
      case ERR_FILE_NOT_EXIST:
          return "File does not exist";
      case ERR_FILE_CANNOT_REWRITE:
          return "File can not be rewritten";
      case ERR_WRONG_DIRECTORYNAME:
          return "Wrong directory name";
      case ERR_DIRECTORY_NOT_EXIST:
          return "Directory does not exist";
      case ERR_FILE_ISNOT_DIRECTORY:
          return "This is a file, not a directory";
      case ERR_CANNOT_DELETE_DIRECTORY:
          return "The directory cannot be removed";
      case ERR_CANNOT_CLEAN_DIRECTORY:
          return "Failed to clear the directory (probably one or more files are blocked and removal operation failed)";
      case ERR_FILE_WRITEERROR:
          return "Failed to write a resource to a file";
      case ERR_FILE_ENDOFFILE:
          return "Unable to read the next piece of data from a CSV file (FileReadString, FileReadNumber, FileReadDatetime, FileReadBool), since the end of file is reached";
      case ERR_NO_STRING_DATE:
          return "No date in the string";
      case ERR_WRONG_STRING_DATE:
          return "Wrong date in the string";
      case ERR_WRONG_STRING_TIME:
          return "Wrong time in the string";
      case ERR_STRING_TIME_ERROR:
          return "Error converting string to date";
      case ERR_STRING_OUT_OF_MEMORY:
          return "Not enough memory for the string";
      case ERR_STRING_SMALL_LEN:
          return "The string length is less than expected";
      case ERR_STRING_TOO_BIGNUMBER:
          return "Too large number, more than ULONG_MAX";
      case ERR_WRONG_FORMATSTRING:
          return "Invalid format string";
      case ERR_TOO_MANY_FORMATTERS:
          return "Amount of format specifiers more than the parameters";
      case ERR_TOO_MANY_PARAMETERS:
          return "Amount of parameters more than the format specifiers";
      case ERR_WRONG_STRING_PARAMETER:
          return "Damaged parameter of string type";
      case ERR_STRINGPOS_OUTOFRANGE:
          return "Position outside the string";
      case ERR_STRING_ZEROADDED:
          return "0 added to the string end, a useless operation";
      case ERR_STRING_UNKNOWNTYPE:
          return "Unknown data type when converting to a string";
      case ERR_WRONG_STRING_OBJECT:
          return "Damaged string object";
      case ERR_INCOMPATIBLE_ARRAYS:
          return "Copying incompatible arrays. String array can be copied only to a string array, and a numeric array - in numeric array only";
      case ERR_SMALL_ASSERIES_ARRAY:
          return "The receiving array is declared as AS_SERIES, and it is of insufficient size";
      case ERR_SMALL_ARRAY:
          return "Too small array, the starting position is outside the array";
      case ERR_ZEROSIZE_ARRAY:
          return "An array of zero length";
      case ERR_NUMBER_ARRAYS_ONLY:
          return "Must be a numeric array";
      case ERR_ONEDIM_ARRAYS_ONLY:
          return "Must be a one-dimensional array";
      case ERR_SERIES_ARRAY:
          return "Timeseries cannot be used";
      case ERR_DOUBLE_ARRAY_ONLY:
          return "Must be an array of type double";
      case ERR_FLOAT_ARRAY_ONLY:
          return "Must be an array of type float";
      case ERR_LONG_ARRAY_ONLY:
          return "Must be an array of type long";
      case ERR_INT_ARRAY_ONLY:
          return "Must be an array of type int";
      case ERR_SHORT_ARRAY_ONLY:
          return "Must be an array of type short";
      case ERR_CHAR_ARRAY_ONLY:
          return "Must be an array of type char";
      case ERR_STRING_ARRAY_ONLY:
          return "String array only";
      case ERR_OPENCL_NOT_SUPPORTED:
          return "OpenCL functions are not supported on this computer";
      case ERR_OPENCL_INTERNAL:
          return "Internal error occurred when running OpenCL";
      case ERR_OPENCL_INVALID_HANDLE:
          return "Invalid OpenCL handle";
      case ERR_OPENCL_CONTEXT_CREATE:
          return "Error creating the OpenCL context";
      case ERR_OPENCL_QUEUE_CREATE:
          return "Failed to create a run queue in OpenCL";
      case ERR_OPENCL_PROGRAM_CREATE:
          return "Error occurred when compiling an OpenCL program";
      case ERR_OPENCL_TOO_LONG_KERNEL_NAME:
          return "Too long kernel name (OpenCL kernel)";
      case ERR_OPENCL_KERNEL_CREATE:
          return "Error creating an OpenCL kernel";
      case ERR_OPENCL_SET_KERNEL_PARAMETER:
          return "Error occurred when setting parameters for the OpenCL kernel";
      case ERR_OPENCL_EXECUTE:
          return "OpenCL program runtime error";
      case ERR_OPENCL_WRONG_BUFFER_SIZE:
          return "Invalid size of the OpenCL buffer";
      case ERR_OPENCL_WRONG_BUFFER_OFFSET:
          return "Invalid offset in the OpenCL buffer";
      case ERR_OPENCL_BUFFER_CREATE:
          return "Failed to create an OpenCL buffer";
      case ERR_OPENCL_TOO_MANY_OBJECTS:
          return "Too many OpenCL objects";
      case ERR_OPENCL_SELECTDEVICE:
          return "OpenCL device selection error";
      case ERR_DATABASE_INTERNAL:
          return "Internal database error";
      case ERR_DATABASE_INVALID_HANDLE:
          return "Invalid database handle";
      case ERR_DATABASE_TOO_MANY_OBJECTS:
          return "Exceeded the maximum acceptable number of Database objects";
      case ERR_DATABASE_CONNECT:
          return "Database connection error";
      case ERR_DATABASE_EXECUTE:
          return "Request execution error";
      case ERR_DATABASE_PREPARE:
          return "Request generation error";
      case ERR_DATABASE_NO_MORE_DATA:
          return "No more data to read";
      case ERR_DATABASE_STEP:
          return "Failed to move to the next request entry";
      case ERR_DATABASE_NOT_READY:
          return "Data for reading request results are not ready yet";
      case ERR_DATABASE_BIND_PARAMETERS:
          return "Failed to auto substitute parameters to an SQL request";
      case ERR_WEBREQUEST_INVALID_ADDRESS:
          return "Invalid URL";
      case ERR_WEBREQUEST_CONNECT_FAILED:
          return "Failed to connect to specified URL";
      case ERR_WEBREQUEST_TIMEOUT:
          return "Timeout exceeded";
      case ERR_WEBREQUEST_REQUEST_FAILED:
          return "HTTP request failed";
      case ERR_NETSOCKET_INVALIDHANDLE:
          return "Invalid socket handle passed to function";
      case ERR_NETSOCKET_TOO_MANY_OPENED:
          return "Too many open sockets (max 128)";
      case ERR_NETSOCKET_CANNOT_CONNECT:
          return "Failed to connect to remote host";
      case ERR_NETSOCKET_IO_ERROR:
          return "Failed to send/receive data from socket";
      case ERR_NETSOCKET_HANDSHAKE_FAILED:
          return "Failed to establish secure connection (TLS Handshake)";
      case ERR_NETSOCKET_NO_CERTIFICATE:
          return "No data on certificate protecting the connection";
      case ERR_NOT_CUSTOM_SYMBOL:
          return "A custom symbol must be specified";
      case ERR_CUSTOM_SYMBOL_WRONG_NAME:
          return "The name of the custom symbol is invalid. The symbol name can only contain Latin letters without punctuation, spaces or special characters (may only contain \".\", \"_\", \"&\" and \"#\"). It is not recommended to use characters <, >, :, \", /,\\, |, ?, *.";
      case ERR_CUSTOM_SYMBOL_NAME_LONG:
          return "The name of the custom symbol is too long. The length of the symbol name must not exceed 32 characters including the ending 0 character";
      case ERR_CUSTOM_SYMBOL_PATH_LONG:
          return "The path of the custom symbol is too long. The path length should not exceed 128 characters including \"Custom\\\", the symbol name, group separators and the ending 0";
      case ERR_CUSTOM_SYMBOL_EXIST:
          return "A custom symbol with the same name already exists";
      case ERR_CUSTOM_SYMBOL_ERROR:
          return "Error occurred while creating, deleting or changing the custom symbol";
      case ERR_CUSTOM_SYMBOL_SELECTED:
          return "You are trying to delete a custom symbol selected in Market Watch";
      case ERR_CUSTOM_SYMBOL_PROPERTY_WRONG:
          return "An invalid custom symbol property";
      case ERR_CUSTOM_SYMBOL_PARAMETER_ERROR:
          return "A wrong parameter while setting the property of a custom symbol";
      case ERR_CUSTOM_SYMBOL_PARAMETER_LONG:
          return "A too long string parameter while setting the property of a custom symbol";
      case ERR_CUSTOM_TICKS_WRONG_ORDER:
          return "Ticks in the array are not arranged in the order of time";
      case ERR_CALENDAR_MORE_DATA:
          return "Array size is insufficient for receiving descriptions of all values";
      case ERR_CALENDAR_TIMEOUT:
          return "Request time limit exceeded";
      case ERR_CALENDAR_NO_DATA:
          return "Country is not found";
      case ERR_DATABASE_ERROR:
          return "Generic error";
      case ERR_DATABASE_PERM:
          return "Access denied";
      case ERR_DATABASE_ABORT:
          return "Callback routine requested abort";
      case ERR_DATABASE_BUSY:
          return "Database file locked";
      case ERR_DATABASE_LOCKED:
          return "Database table locked";
      case ERR_DATABASE_NOMEM:
          return "Insufficient memory for completing operation";
      case ERR_DATABASE_READONLY:
          return "Attempt to write to readonly database";
      case ERR_DATABASE_IOERR:
          return "Disk I/O error";
      case ERR_DATABASE_CORRUPT:
          return "Database disk image corrupted";
      case ERR_DATABASE_FULL:
          return "Insertion failed because database is full";
      case ERR_DATABASE_CANTOPEN:
          return "Unable to open the database file";
      case ERR_DATABASE_PROTOCOL:
          return "Database lock protocol error";
      case ERR_DATABASE_SCHEMA:
          return "Database schema changed";
      case ERR_DATABASE_TOOBIG:
          return "String or BLOB exceeds size limit";
      case ERR_DATABASE_CONSTRAINT:
          return "Abort due to constraint violation";
      case ERR_DATABASE_MISMATCH:
          return "Data type mismatch";
      case ERR_DATABASE_MISUSE:
          return "Library used incorrectly";
      case ERR_DATABASE_AUTH:
          return "Authorization denied";
      case ERR_DATABASE_RANGE:
          return "Bind parameter error, incorrect index";
      case ERR_DATABASE_NOTADB:
          return "File opened that is not database file";
      case ERR_USER_ERROR_FIRST:
          return "User defined errors start with this code";
      default:
         return "Unknown Error, Error = " + IntegerToString(error_code);
#endif
   }
}