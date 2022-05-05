using System;

namespace ThemeViewer.Models {
    public class TradeRecord {
        public string TradeID { get; set; }
        public string DealID { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? TradeRegistrationDateTime { get; set; }
        public string MarketSegmentID { get; set; }
        public string SecurityID { get; set; }
        public string Instrument { get; set; }
        public string TrdType { get; set; }
        public DateTime? ExpireDate { get; set; }
        public decimal Price { get; set; }
        public decimal? Strike { get; set; }
        public decimal QtyInLots { get; set; }
        public string TrdStatus { get; set; }
        public DateTime? ClearedDate { get; set; }
        public string ClearMsg { get; set; }
        public DateTime? ApprovalDateTime { get; set; }
        public DateTime TrdExecDateTime{ get; set; }
        public string BuyerCM { get; set; }
        public string BuyerTrdCom { get; set; }
        public string BuyerTrader { get; set; }
        public string BuyerTradeRegistrant { get; set; }
        public string BuyerAccount { get; set; }
        public string BuyerSecurID { get; set; }
        public string BuyerBroker { get; set; }
        public string BuyerComment { get; set; }
        public string BuyerInternalTrdID { get; set; }
        public string BuySource { get; set; }
        public string SellerCM { get; set; }
        public string SellerTrdCom { get; set; }
        public string SellerTrader { get; set; }
        public string SellerTradeRegistrant { get; set; }
        public string SellerAccount { get; set; }
        public string SellerSecurID { get; set; }
        public string SellerBroker { get; set; }
        public string SellerComment { get; set; }
        public string SellerInternalTrdID { get; set; }
        public string SellSource { get; set; }
        public string USI { get; set; }
       // public string? TradingSessionID { get; set; }
        public string TradingSessionSubID { get; set; }
        public int QtyInUnits { get; set; }
        public string QtyType { get; set; }
        public bool NewlyReceived { get; set; }
    }
}
