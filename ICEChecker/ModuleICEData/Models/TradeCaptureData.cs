using ICEFixAdapter.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ICEChecker.ModuleICEData.Models {
    public class TradeCaptureData : BindableBase {
        public DateTime TrdDate { get; set; }
        public DateTime TrdTime { get; set; }
        public string TrdType { get; set; }
        public string DealID { get; set; }
        public int DealID_TradeLinkID { get; set; }
        public int DealID_TradeLinkMktID { get; set; }
        public string LegID { get; set; }
        public string OrigID { get; set; }
        public string CFICode { get; set; }
        public string GroupIndicator { get; set; }//group the components of a block
        public string SecurityID { get; set; }
        public string MIC { get; set; }//mapped to msg.UnderlyingSecurityExchange--market identifier code values: Examples of possible values are “IFEU”, “IFUS”, “IFCA”, “IFED”, etc.Not be sent for options markets.
        public string B_S { get; set; }

        private bool _isB_SDifferent;
        public bool IsB_SDifferent {
            get => _isB_SDifferent;
            set => SetProperty(ref _isB_SDifferent, value);
        }
        public string Product { get; set; }
        private bool _isProductDifferent;
        public bool IsProductDifferent {
            get => _isProductDifferent;
            set => SetProperty(ref _isProductDifferent, value);
        }
        public string Hub { get; set; }
        private bool _isHubDifferent;
        public bool IsHubDifferent {
            get => _isHubDifferent;
            set => SetProperty(ref _isHubDifferent, value);
        }
        public string Strip { get; set; }
        public string Contract { get; set; }
        private bool _isContractDifferent;
        public bool IsContractDifferent {
            get => _isContractDifferent;
            set => SetProperty(ref _isContractDifferent, value);
        }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ClearingAcct { get; set; }
        public string CustAcct { get; set; }
        public string ClearingFirm { get; set; }
        public double Price { get; set; }

        private bool _isPriceDifferent;
        public bool IsPriceDifferent {
            get => _isPriceDifferent;
            set => SetProperty(ref _isPriceDifferent, value);
        }
        public string PriceUnit { get; set; }
        public string Option { get; set; }
        public string Strike { get; set; }
        public string Strike2 { get; set; }
        public string Style { get; set; }
        public string Counterparty { get; set; }
        public int Lots { get; set; }

        private bool _isLotDifferent;
        public bool IsLotDifferent {
            get => _isLotDifferent;
            set => SetProperty(ref _isLotDifferent, value);
        }
        public int TotalQty { get; set; }
        public int QtyPerPeriod { get; set; }
        public string Period { get; set; }
        public string QtyUnits { get; set; }
        public string TT { get; set; }
        public string BRK { get; set; }

        private bool _isBRKDifferent;
        public bool IsBRKDifferent {
            get => _isBRKDifferent;
            set => SetProperty(ref _isBRKDifferent, value);
        }
        public string Trader { get; set; }
        public string Memo { get; set; }
        public string ClearingVenue { get; set; }
        public string UserID { get; set; }
        private bool _isUserIDDifferent;
        public bool IsUserIDDifferent {
            get => _isUserIDDifferent;
            set => SetProperty(ref _isUserIDDifferent, value);
        }
        public int RestDaysInFlowContract { get; set; }
        public string Source { get; set; }
        public string LinkID { get; set; }
        public string OrderID { get; set; }
        public string CIOrdID { get; set; }
        public string ComplianceID { get; set; }
        public string USI { get; set; }
        public string AuthorizedTraderID { get; set; }
        public string Location { get; set; }
        public string Meter { get; set; }
        public string LeadTime { get; set; }
        public string WaiverInd { get; set; }
        public DateTime TradeTime_micros { get; set; }
        public string ReasonCode { get; set; }
        public List<IceLeg> Legs = new List<IceLeg>();
        public string AllocAccount { get; set; }
        public string ExecType { get; set; }
        public int Symbol { get; set; }
        public string ExchangeSilo { get; set; }
        public bool isBilateral { get; set; }
        public string ContraTrader { get; set; }//bilateral trades--user ID of counterparty trader
        public string ContraFirmID { get; set; }//bilateral trades--ID of counterparty trader’s company
        public string ContraFirm { get; set; }//bilateral trades--Name of counterparty trader’s company
        public string BrokerFirm { get; set; }

        private string _status;
        public string Status {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        private bool _checkStatusVisible ;
        public bool CheckStatusVisible {
            get => _checkStatusVisible;
            set => SetProperty(ref _checkStatusVisible, value);
        }

        private bool? _haveConsistentRecordFromTrader;
        public bool? HaveConsistentRecordFromTrader {
            get => _haveConsistentRecordFromTrader;
            set => SetProperty(ref _haveConsistentRecordFromTrader, value);
        }

        private bool _isSimilarInconsistentRecord;
        public bool IsSimilarInconsistentRecord {
            get => _isSimilarInconsistentRecord;
            set => SetProperty(ref _isSimilarInconsistentRecord, value);
        }

        private bool _isFeedMapped;
        public bool IsFeedMapped {
            get => _isFeedMapped;
            set => SetProperty(ref _isFeedMapped, value);
        }

        private bool _isFeedCanceled;
        public bool IsFeedCanceled {
            get => _isFeedCanceled;
            set => SetProperty(ref _isFeedCanceled, value);
        }

        private bool _isCurrentlyHighlighted;
        public bool IsCurrentlyHighlighted {
            get => _isCurrentlyHighlighted;
            set => SetProperty(ref _isCurrentlyHighlighted, value);
        }      

        public bool NewlyReceived { get; set; }
    }
}
