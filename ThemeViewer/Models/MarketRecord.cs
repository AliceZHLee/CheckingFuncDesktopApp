using System;

namespace ThemeViewer.Models {
    public class MarketRecord {
        public string Market { get; set; }
        public string Instrument { get; set; }//symbol in API doc
        public string Product { get; set; }
        public string Contract { get; set; }
        public string ComboType { get; set; }
        public int? Legs { get; set; }
        public decimal Price { get; set; }//MDEntryPxmarke
        public decimal? Strike { get; set; }
        public decimal QtyInLots { get; set; }//MDEntrySize
        public int? Amount { get; set; }
        public string Unit { get; set; }//currency
        public DateTime? ExecutedSGT { get; set; }
        public DateTime? SubmittedSGT { get; set; }
        public DateTime? ClearedDate { get; set; }//SettlDate in API doc,format: YYYYMMDD
        public string Session { get; set; }
        public DateTime? ClearedSGT { get; set; }//format :dd/mm/yyyy hh:mm
        public DateTime? DeletedSGT { get; set; }
        public string Status { get; set; }
        public bool IsProductRelated { get; set; }
        public string DealID { get; set; }

        public bool NewlyReceived { get; set; }
    }
}
