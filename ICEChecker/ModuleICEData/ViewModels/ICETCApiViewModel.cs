using ICEChecker.ModuleCheckerInput.Models;
using ICEChecker.ModuleCheckerOutput.Models;
using ICEChecker.ModuleICEData.Models;
using ICEFixAdapter.Models;
using ICEFixAdapter.Models.Request;
using ICEFixAdapter.Models.Response;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ICEChecker.ModuleICEData.ViewModels {
    public class ICETCApiViewModel : BindableBase {
        public const int TRDREQSTATUS_REJECTED = 2;
        public const int TRDREQRESULT_OTHER = 99;
        private bool FirstCheckDone = false;

        private IEventAggregator _ea;
        private readonly object _lock = new();

        private string _userID;
        public string UserID {
            get => _userID;
            set => SetProperty(ref _userID, value);
        }
        private string _comparedResult = "";//consistent or inconsistent
        public string ComparedResult {
            get => _comparedResult;
            set => SetProperty(ref _comparedResult, value);
        }

        private string _similarRecordNum;
        public string SimilarRecordNum {
            get => _similarRecordNum;
            set => SetProperty(ref _similarRecordNum, value);
        }

        private string _sprDetails = "";
        public string SprDetails {
            get => _sprDetails;
            set => SetProperty(ref _sprDetails, value);
        }

        private int _totalFeeds;
        public int TotalFeeds {
            get => _totalFeeds;
            set => SetProperty(ref _totalFeeds, value);
        }

        private int _mappedFeeds;
        public int MappedFeeds {
            get => _mappedFeeds;
            set => SetProperty(ref _mappedFeeds, value);
        }
        private int PendingFeeds;
        private int _pendingOtherFeeds;
        public int PendingOtherFeeds {
            get => _pendingOtherFeeds;
            set => SetProperty(ref _pendingOtherFeeds, value);
        }
        private int _pendingBlockFeeds;
        public int PendingBlockFeeds {
            get => _pendingBlockFeeds;
            set => SetProperty(ref _pendingBlockFeeds, value);
        }

        private int _canceledFeeds;
        public int CanceledFeeds {
            get => _canceledFeeds;
            set => SetProperty(ref _canceledFeeds, value);
        }
        public ICommand Send_AD_ReqCommand { get; private set; }
        public ICommand Add_News_Command { get; private set; }
        public ICommand Add_Allocation { get; private set; }
        public ObservableCollection<DifferenceDetail> CompareDetails { get; } = new();
        //public CollectionViewSource TradeDataViewSource { get; } = new();
        public List<TradeCaptureData> MappedTC { get; set; } = new();//if a TC is already totally mapped to an input record,then for the checking for the rest of the input, we should not compare with this TC

        public ObservableCollection<TradeCaptureData> APIData { get; } = new();

        public Dictionary<CheckerOutput, Dictionary<int, List<TradeCaptureData>>> RelativeTrades = new();
        public Dictionary<string, string> TrdTypeMapping = new() {
            ["0"] = "Regular Type",
            ["2"] = "ICE EFRP",
            ["3"] = "ICEBLK",
            ["4"] = "Basic Trade",
            ["5"] = "Guaranteed Cross",
            ["6"] = "Volatility Contingent Trade",
            ["7"] = "Stock Contingent Trade",
            ["9"] = "CCX EFP Trade",
            ["A"] = "Other Clearing Venue",
            ["D"] = "N2EX",
            ["E"] = "EFP Trade/Against Actual",
            ["G"] = "EEX",
            ["F"] = "EFS/EFP Contra Trade",
            ["I"] = "EFM Trade",
            ["J"] = "EFR Trade",
            ["K"] = "Block Trade",
            ["O"] = "NG EFP/EFS Trade",
            ["Q"] = "EOO Trade",
            ["S"] = "EFS Trade",
            ["T"] = "Contra Trade",
            ["U"] = "CPBLK",
            ["V"] = "Bilateral Off-Exchange Trade",
            ["Y"] = "Cross Contra Trade",
            ["AA"] = "Asset Allocation"
        };
        public Dictionary<int, string> ClientAppTypeMapping = new() {
            [0] = "WebICE",
            [1] = "FIX OS",
            [2] = "FIXml",
            [3] = "ICEBlock",
            [4] = "Other",
            [5] = "FPML",
            [6] = "UPS",
            [7] = "Mobile",
            [8] = "FIX POF",
            [9] = "YJ ISV",
            [11] = "IOAS"
        };

        public Dictionary<int, string> HubSymbolDescMapping = new();
        public Dictionary<int, string> HubSymbolAliasMapping = new();
        public Dictionary<int, int> HubSymbolLotSizeMapping = new();
        public Dictionary<int, string> HubSymbolPriceUnitMapping = new();
        public Dictionary<int, string> HubSymbolProductNameMapping = new();
        public Dictionary<int, string> HubSymbolStripMapping = new();
        public Dictionary<int, string> HubSymbolQtyUnitMapping = new();
        public Dictionary<int, string> HubSymbolMICMapping = new();

        public Dictionary<string, List<string>> ProductAbbrMapping = new();
        public Lookup<string, string> ShopBRKMapping;

        #region UI column options
        public Dictionary<string, string> PropertyColumnMapping { get; } = new Dictionary<string, string> {
            ["Status"] = "Status",
            ["TrdDate"] = "Trade Date",
            ["TrdTime"] = "Trade Time",
            ["DealID"] = "Deal ID",
            ["OrigID"] = "Orig ID",
            ["B_S"] = "Buy/Sell",
            ["Source"] = "Source",
            ["Product"] = "Product",
            ["Hub"] = "Hub",
            ["Contract"] = "Contract",
            ["BeginDate"] = "Begin Date",
            ["EndDate"] = "End Date",
            ["ClearingAcct"] = "Clearing Acct",
            ["ClearingFirm"] = "Clearing Firm",
            ["Price"] = "Price",
            ["PriceUnit"] = "Price Units",
            ["Lots"] = "Lots",
            ["QtyUnits"] = "Qty Units",
            ["TrdType"] = "TT",
            ["BRK"] = "Broker",
            ["BrokerFirm"] = "Broker Company",
            ["Trader"] = "Trader",
            ["UserID"] = "User ID",
            ["Memo"] = "Memo",
            ["ClearingVenue"] = "Clearing Venue",
            ["GroupIndicator"] = "Group Indicator"
        };
        public List<FilterObj> APIFeedColumns { get; } = new();


        #region Display User's Filter Selections
        public CollectionViewSource APICollectionView { get; } = new();
        public ObservableCollection<FilterObj> FilterLabel { get; } = new();
        public Dictionary<string, ObservableCollection<FilterObj>> FilterDic { get; } = new();

        internal void AddDisplayLabel(string title, string value) {
            string displayLabel = PropertyColumnMapping[title];
            if (value.Length > 16) {
                value = value.Substring(0, 16) + "...";
            }
            foreach (var fltLabel in FilterLabel) {
                if (fltLabel.Label == displayLabel) {
                    fltLabel.SelectedFilterDic = value;
                    return;
                }
            }
            FilterLabel.Add(new FilterObj() {
                Label = displayLabel,
                SelectedFilterDic = value
            });
        }

        internal void RemoveDisplayLabel(string title) {
            foreach (var flt in FilterLabel) {
                if (flt.Label == title) {
                    FilterLabel.Remove(flt);
                    break;
                }
            }
        }
        #endregion
        #endregion

        public ICETCApiViewModel(IEventAggregator ea) {
            _ea = ea;
            _ea.GetEvent<SendCheckInputEvent>().Subscribe(InputReceived);
            _ea.GetEvent<FindSimilarRecordEvent>().Subscribe(FindSimilarRecord);
            _ea.GetEvent<FindSameRecordEvent>().Subscribe(FindMappedRecord);
            _ea.GetEvent<RetrieveUserIDEvent>().Subscribe(SyncUserID);

            Send_AD_ReqCommand = new DelegateCommand(SendADReq);

            BindingOperations.EnableCollectionSynchronization(APIData, _lock);
            APICollectionView.Source = APIData;

            BindingOperations.EnableCollectionSynchronization(DealAllocationSeries, _lock);

            foreach (var column in PropertyColumnMapping.Values) {
                APIFeedColumns.Add(new FilterObj() { Label = column.ToString(), IsChecked = true });
            }
            OtcHelper.Client.OnErrorRsp += TO_ErrorRsp;
            OtcHelper.Client.OnLogonRsp += TO_LogonRsp;
            OtcHelper.Client.OnSecurityDefinition += TO_OnSecurities;
            OtcHelper.Client.OnDefinedStrategy += TO_OnDefinedStrategy;
            OtcHelper.Client.OnUserCompanyResponse += TO_OnUserCompanyResponse;
            OtcHelper.Client.OnTradeCaptureReport += TO_OnTradeCaptureReport;
            OtcHelper.Client.OnTradeCaptureReportRequestAck += TO_OnTradeCaptureReportRequestAck;
            OtcHelper.Client.OnAllocationReport += TO_OnAllocationReport;
            OtcHelper.Client.OnNews += TO_OnNews;

            LoadSecuritiesInfo();

            SendADReq();
        }

        private void SyncUserID(string id) {
            UserID = id;
            Thread.Sleep(2000);

            TotalFeeds = APIData.Count(x => x.UserID == UserID);
            MappedFeeds = MappedTC.Count();
            CanceledFeeds = APIData.Count(x => x.UserID == UserID && x.Status.ToLower() == "cancel");
            PendingFeeds = TotalFeeds - MappedFeeds - CanceledFeeds;

            PendingBlockFeeds = APIData.Count(x => x.UserID == UserID && x.Source.ToLower().Contains("block") && x.Status.ToLower() != "cancel") - MappedTC.Count(x => x.UserID == UserID && x.Source.ToLower().Contains("block"));
            PendingOtherFeeds = PendingFeeds - PendingBlockFeeds;
        }

        private void LoadSecuritiesInfo() {
            try { HubSymbolDescMapping = File.ReadLines(@"config\SecurityDict_desc.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => line[1]); }
            catch (Exception e) { }
            try { HubSymbolLotSizeMapping = File.ReadLines(@"config\SecurityDict_lotSize.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => Convert.ToInt32(line[1])); }
            catch (Exception e) { }
            try { HubSymbolPriceUnitMapping = File.ReadLines(@"config\SecurityDict_priceUnit.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => line[1]); }
            catch (Exception e) { }
            try { HubSymbolProductNameMapping = File.ReadLines(@"config\SecurityDict_ProductName.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => line[1]); }
            catch (Exception e) { }
            try { HubSymbolStripMapping = File.ReadLines(@"config\SecurityDict_Strip.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => line[1]); }
            catch (Exception e) { }
            try { HubSymbolQtyUnitMapping = File.ReadLines(@"config\SecurityDict_QtyUnit.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => line[1]); }
            catch (Exception e) { }
            try { HubSymbolMICMapping = File.ReadLines(@"config\SecurityDict_MIC.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => line[1]); }
            catch (Exception e) { }
            try { HubSymbolAliasMapping = File.ReadLines(@"config\SecurityDict_HubAlias.csv").Select(line => line.Split(';')).ToDictionary(line => Convert.ToInt32(line[0]), line => line[1]); }
            catch (Exception e) { }

            PopulateProductAbbrDictionary();
            PopulateShopBRKDictionary();
        }

        private void PopulateShopBRKDictionary() {
            try { ShopBRKMapping = (Lookup<string, string>)File.ReadLines(@"config\BrokerDict.csv").Select(line => line.Split(';')).ToLookup(line => line[0].ToLower().Trim(), line => line[2].ToLower().Trim()); }
            catch (Exception e) { }
        }

        private void PopulateProductAbbrDictionary() {
            try {
                Dictionary<string, string> dict = File.ReadLines(@"config\AbbrDict.csv").Select(line => line.Split(';')).ToDictionary(line => line[0].ToLower(), line => line[1].ToLower());
                foreach (var item in dict) {
                    if (!ProductAbbrMapping.ContainsKey(item.Value)) {
                        List<string> products = new List<string>();
                        products.Add(item.Key);
                        ProductAbbrMapping.Add(item.Value, products);
                    }
                    else { ProductAbbrMapping[item.Value].Add(item.Key); }
                }
            }
            catch (Exception e) { }
        }

        private void TO_LogonRsp(IceLogonReqRsp logon) {
            MessageBox.Show("logon again");
        }
        private bool IsRefreshed = false;
        private void SendADReq() {
            IsRefreshed = true;
            APIData.Clear();
            MappedTC.Clear();
            Task.Delay(600).Wait();
            OtcHelper.StartTradeData();
        }

        private void TO_ErrorRsp(IceErrorRsp err) {
            MessageBox.Show(err.Text);
        }

        #region ICE API Processing
        #region trade capture
        private void TO_OnTradeCaptureReportRequestAck(IceTradeCaptureReportRequestAckRsp ackRsp) {
            string errMsg = "";
            if (ackRsp.TradeRequestStatus == TRDREQSTATUS_REJECTED) {
                errMsg += "Trade Request Rejected. ";
                if (ackRsp.TradeRequestResult == TRDREQRESULT_OTHER) {
                    errMsg += "Something is wrong during retrieving snapshot data! ";
                }
                errMsg += ackRsp.Text;
            }
            if (!string.IsNullOrEmpty(errMsg)) {
                MessageBox.Show(errMsg);
            }
        }
        private TradeCaptureData ProcessLiveAndHistory(TradeCaptureData tr, IceTradeCaptureReportRsp tradeRsp) {
            tr.TrdDate = DateTime.ParseExact(tradeRsp.TradeDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            tr.TrdTime = tradeRsp.TransactTime.ToLocalTime();
            tr.DealID = tradeRsp.ExecID;
            tr.DealID_TradeLinkID = tradeRsp.TradeLinkID;
            tr.DealID_TradeLinkMktID = tradeRsp.TradeLinkMktID;
            tr.OrigID = tradeRsp.OrigTradeID == 0 ? "" : tradeRsp.OrigTradeID.ToString();
            tr.GroupIndicator = tradeRsp.GroupIndicator;
            tr.Price = (double)tradeRsp.LastPx;
            tr.Lots = (int)tradeRsp.LastQty;
            tr.WaiverInd = tradeRsp.WaiverIndicator;
            tr.BeginDate = DateTime.ParseExact(tradeRsp.StartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            tr.EndDate = DateTime.ParseExact(tradeRsp.EndDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            tr.Source = ClientAppTypeMapping[tradeRsp.ClientAppType];
            tr.RestDaysInFlowContract = tradeRsp.NumberOfCycles;
            tr.Location = tradeRsp.LocationCode;
            tr.Meter = tradeRsp.MeterNumber;
            tr.LeadTime = tradeRsp.LeadTime == 0 ? "" : tradeRsp.LeadTime.ToString();
            tr.ReasonCode = tradeRsp.ReasonCode;
            tr.Symbol = tradeRsp.Symbol;
            try {
                switch (tradeRsp.ExchangeSilo) {
                    case 0:
                        tr.ExchangeSilo = "ICE";
                        break;
                    case 1:
                        tr.ExchangeSilo = "Endex";
                        break;
                    case 2:
                        tr.ExchangeSilo = "Liffe";
                        break;
                    default: break;
                }
            }
            catch { }
            try { tr.TrdType = TrdTypeMapping[tradeRsp.TrdType]; }
            catch {
                if (tr.Source == "ICEBlock") {
                    tr.TrdType = "Block Trade";
                }
            }
            switch (tradeRsp.CFICode) {
                case "FXXXXX": {
                        tr.CFICode = "Futures";
                        break;
                    }
                case "OPXXXX": {
                        tr.CFICode = "Put";
                        tr.Strike = tradeRsp.StrikePrice.ToString();
                        break;
                    }
                case "OCXXXX": {
                        tr.CFICode = "Call";
                        tr.Strike = tradeRsp.StrikePrice.ToString();
                        break;
                    }
                case "OMXXXX": {
                        tr.CFICode = "Other(UDS)";
                        break;
                    }
            }
            try { tr.TotalQty = tr.Lots * HubSymbolLotSizeMapping[tradeRsp.Symbol]; }
            catch { }
            try { tr.Product = HubSymbolProductNameMapping[tradeRsp.Symbol]; }
            catch { }
            try { tr.Hub = HubSymbolAliasMapping[tradeRsp.Symbol]; }
            catch { }
            try { tr.Contract = HubSymbolStripMapping[tradeRsp.Symbol]; }
            catch { }
            try { tr.PriceUnit = HubSymbolPriceUnitMapping[tradeRsp.Symbol]; }
            catch { }
            try { tr.QtyUnits = HubSymbolQtyUnitMapping[tradeRsp.Symbol]; }
            catch { }
            //tr.MIC = HubSymbolMICMapping[tradeRsp.Symbol];

            if (tradeRsp.OptionsSymbol != 0) {
                tr.Option = tradeRsp.OptionsSymbol.ToString();
            }
            tr.SecurityID = tradeRsp.SecurityID.Replace("????", "");
            #region process sides                    
            foreach (var side in tradeRsp.Sides) {//"Sides" always count 1
                tr.B_S = side.Side == '1' ? "Bought" : "Sold";
                tr.OrderID = side.OrderID.ToString();
                tr.CIOrdID = side.CIOrdID;
                tr.ComplianceID = side.ComplianceID;
                tr.Memo = side.MemoField;
                foreach (var party in side.Parties) {
                    switch (party.PartyRole) {
                        case 11://order origination trader
                            tr.UserID = party.PartyID;
                            break;
                        case 13:// order origination firm
                            break;
                        case 56://order origination firm ID
                            break;
                        case 35:// Liquidity Provider
                            break;
                        case 51://clearing account
                            tr.ClearingAcct = party.PartyID;
                            break;
                        case 4://clearing firm
                            break;
                        case 60://clearing firm name
                            tr.ClearingFirm = party.PartyID;
                            break;
                        case 63:// clearing firm mnemonic
                            break;
                        case 54://account code
                            break;
                        case 1:
                            tr.BrokerFirm = party.PartyID;
                            break;
                        case 12:
                            tr.BRK = party.PartyID;
                            break;
                        case 37:
                            tr.isBilateral = true;
                            tr.ContraTrader = party.PartyID;
                            break;
                        case 50:
                            tr.isBilateral = true;
                            tr.ContraFirmID = party.PartyID;
                            break;
                        case 17:
                            tr.isBilateral = true;
                            tr.ContraFirm = party.PartyID;
                            break;
                        default: break;
                    }
                }
                foreach (var alloc in side.Allocs) {
                    tr.AllocAccount = alloc.AllocAccount;
                }
            }
            #endregion

            #region process legs
            foreach (var leg in tradeRsp.Legs) {
                var trLeg = new IceLeg();
                trLeg.LegSymbol = leg.LegSymbol;
                trLeg.LegSecurityID = leg.LegSecurityID;
                trLeg.LegCFICode = (leg.LegCFICode == "FXXXXX") ? "Futures" : ((leg.LegCFICode == "OPXXXX") ? "Put" : "Call");
                trLeg.LegStrikePrice = leg.LegStrikePrice;
                trLeg.LegOptionSymbol = leg.LegOptionSymbol;
                trLeg.LegB_S = leg.LegSide == '1' ? "Bought" : "Sold";
                trLeg.LegMemoField = leg.LegMemoField;
                trLeg.LegComplianceID = leg.LegComplianceID;
                trLeg.LegLastPx = leg.LegLastPx;
                trLeg.LegQty = leg.LegQty;
                trLeg.LegRefID = leg.LegRefID;
                trLeg.LinkExecID = leg.LinkExecID;
                try { trLeg.Strip = HubSymbolStripMapping[leg.LegSymbol]; }
                catch { }
                try { trLeg.TotalQty = (double)leg.LegQty * HubSymbolLotSizeMapping[leg.LegSymbol]; }
                catch { }
                try { trLeg.Product = HubSymbolProductNameMapping[leg.LegSymbol]; }
                catch { trLeg.Product = "Product not mapped, Symbol ID: " + leg.LegSymbol.ToString(); }
                try { trLeg.LegOptionSymbolName = HubSymbolDescMapping[leg.LegSymbol]; }
                catch { }
                tr.Legs.Add(trLeg);
            }
            #endregion

            if (string.IsNullOrEmpty(tr.BRK)) {
                tr.BRK = "";
            }
            if (string.IsNullOrEmpty(tr.BrokerFirm)) {
                tr.BrokerFirm = "";
            }
            return tr;
        }
        private HashSet<string> AlrCheckedDealIDs = new HashSet<string>();

        private void TO_OnTradeCaptureReport(IceTradeCaptureReportRsp tradeRsp) {
            if (tradeRsp.ExecType == '0') {//0=snapshot:snapshot data does not contain ExecType in AE report, need to apply this in future
                var tr = new TradeCaptureData();
                ProcessLiveAndHistory(tr, tradeRsp);
                tr.Status = tradeRsp.OrdStatus != '2' ? "cancel" : (tr.isBilateral ? "bilateral" : "trade");//history uses this
                // if (tr.Source.ToLower() != "webice") {
                APIData.Insert(0, tr);
                HighlightFeeds(tr);
                // }
            }
            else if (tradeRsp.ExecType == 'F') { //F=new live update
                var tr = new TradeCaptureData();
                ProcessLiveAndHistory(tr, tradeRsp);
                tr.Status = tradeRsp.DealAdjustIndicator == 1 ? "Bust-Adjust" : (tr.isBilateral ? "Bilateral" : "Trade");
                //if (tr.Source.ToLower() != "webice") {
                APIData.Insert(0, tr);
                HighlightFeeds(tr);
                //}
            }
            else if (tradeRsp.ExecType == '5') {//replace
                if (!APIData.Any(x => x.DealID == tradeRsp.ExecID)) {
                    return;
                }
                TradeCaptureData originalRecord = APIData.First(x => x.DealID == tradeRsp.ExecID);
                if (APIData.Remove(originalRecord)) {
                    ProcessLiveAndHistory(originalRecord, tradeRsp);
                    originalRecord.Status = tradeRsp.DealAdjustIndicator == 1 ? "Bust-Adjust" : (originalRecord.isBilateral ? "Bilateral" : "Replace");
                    //if (originalRecord.Source.ToLower() != "webice") {
                    APIData.Insert(0, originalRecord);
                    HighlightFeeds(originalRecord);
                    //}
                }
            }
            else if (tradeRsp.ExecType == 'H') {//TRADE CANCEL: should trigger the event handler to neglect the very mapped input like removing or some notification
                var tr = new TradeCaptureData();
                ProcessLiveAndHistory(tr, tradeRsp);
                tr.Status = tradeRsp.DealAdjustIndicator == 1 ? "Bust-Adjust" : (tr.isBilateral ? "Bilateral" : "Cancel");
                // if (tr.Source.ToLower() != "webice") {
                APIData.Insert(0, tr);
                HighlightFeeds(tr);
                // }
            }


            TotalFeeds = APIData.Count(x => x.UserID == UserID);
            CanceledFeeds = APIData.Count(x => x.UserID == UserID && x.Status.ToLower() == "cancel");
            PendingFeeds = TotalFeeds - MappedFeeds - CanceledFeeds;

            PendingBlockFeeds = APIData.Count(x => x.UserID == UserID && x.Source.ToLower().Contains("block") && x.Status.ToLower() != "cancel") - MappedTC.Count(x => x.Source.ToLower().Contains("block"));
            PendingOtherFeeds = PendingFeeds - PendingBlockFeeds;
        }

        private void HighlightFeeds(TradeCaptureData tr) {
            if (FirstCheckDone) {
                tr.CheckStatusVisible = true;
            }
            if (AlrCheckedDealIDs.Contains(tr.DealID) && tr.Status.ToLower() != "cancel") {
                tr.IsFeedMapped = true;
                if (!MappedTC.Any(x => x.DealID == tr.DealID)) {
                    MappedTC.Add(tr);
                }
            }
            if (tr.Status.ToLower() == "cancel") {
                tr.IsFeedCanceled = true;
            }
        }
        #endregion

        #region Securities & UDS
        private void TO_OnSecurities(IceSecurityDefinitionRsp securRsp) {
            foreach (var underlying in securRsp.Underlyings) {
                if (!HubSymbolDescMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    if (!string.IsNullOrEmpty(underlying.UnderlyingSecurityDesc)) {
                        HubSymbolDescMapping.Add(underlying.UnderlyingSymbol, underlying.UnderlyingSecurityDesc);//full name(contains month)
                    }
                    else {
                        string desc = underlying.ProductName + " - " + underlying.HubAlias + " - " + underlying.StripName;
                        HubSymbolDescMapping.Add(underlying.UnderlyingSymbol, desc);
                    }
                }
                if (!HubSymbolAliasMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    HubSymbolAliasMapping.Add(underlying.UnderlyingSymbol, underlying.HubAlias);
                }
                if (!HubSymbolLotSizeMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    HubSymbolLotSizeMapping.Add(underlying.UnderlyingSymbol, (int)underlying.LotSize);
                }
                if (!HubSymbolPriceUnitMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    HubSymbolPriceUnitMapping.Add(underlying.UnderlyingSymbol, underlying.PriceUnit);
                }
                if (!HubSymbolProductNameMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    HubSymbolProductNameMapping.Add(underlying.UnderlyingSymbol, underlying.ProductName);
                }
                if (!HubSymbolStripMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    HubSymbolStripMapping.Add(underlying.UnderlyingSymbol, underlying.StripName);
                }
                if (!HubSymbolQtyUnitMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    HubSymbolQtyUnitMapping.Add(underlying.UnderlyingSymbol, underlying.UnderlyingUnitOfMeasure);
                }
                if (!HubSymbolMICMapping.ContainsKey(underlying.UnderlyingSymbol)) {
                    HubSymbolMICMapping.Add(underlying.UnderlyingSymbol, underlying.UnderlyingSecurityExchange);
                }
            }
            if (securRsp.ListSeqNo == securRsp.NoRpts) {
                SaveSecurityAndUDSToDictionary();
            }
        }
        private void SaveSecurityAndUDSToDictionary() {
            string pathToCsv = @"config\SecurityDict_desc.csv";
            string csv = string.Join(Environment.NewLine, HubSymbolDescMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);

            pathToCsv = @"config\SecurityDict_lotSize.csv";
            csv = string.Join(Environment.NewLine, HubSymbolLotSizeMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);

            pathToCsv = @"config\SecurityDict_priceUnit.csv";
            csv = string.Join(Environment.NewLine, HubSymbolPriceUnitMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);

            pathToCsv = @"config\SecurityDict_ProductName.csv";
            csv = string.Join(Environment.NewLine, HubSymbolProductNameMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);

            pathToCsv = @"config\SecurityDict_Strip.csv";
            csv = string.Join(Environment.NewLine, HubSymbolStripMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);

            pathToCsv = @"config\SecurityDict_QtyUnit.csv";
            csv = string.Join(Environment.NewLine, HubSymbolQtyUnitMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);

            pathToCsv = @"config\SecurityDict_MIC.csv";
            csv = string.Join(Environment.NewLine, HubSymbolMICMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);

            pathToCsv = @"config\SecurityDict_HubAlias.csv";
            csv = string.Join(Environment.NewLine, HubSymbolAliasMapping.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(pathToCsv, csv);
        }
        private void TO_OnDefinedStrategy(IceDefinedStrategyRsp udsRsp) {
            if (!HubSymbolDescMapping.ContainsKey(udsRsp.Symbol)) {
                if (!string.IsNullOrEmpty(udsRsp.SecurityDesc)) {
                    HubSymbolDescMapping.Add(udsRsp.Symbol, udsRsp.SecurityDesc);
                }
                else {
                    string desc = udsRsp.ProductName + " - " + udsRsp.HubAlias + " - " + udsRsp.StripName;
                    HubSymbolDescMapping.Add(udsRsp.Symbol, desc);
                }
            }
            if (!HubSymbolAliasMapping.ContainsKey(udsRsp.Symbol)) {
                HubSymbolAliasMapping.Add(udsRsp.Symbol, udsRsp.HubAlias);
            }
            if (!HubSymbolLotSizeMapping.ContainsKey(udsRsp.Symbol)) {
                HubSymbolLotSizeMapping.Add(udsRsp.Symbol, (int)udsRsp.LotSize);
            }
            if (!HubSymbolPriceUnitMapping.ContainsKey(udsRsp.Symbol)) {
                HubSymbolPriceUnitMapping.Add(udsRsp.Symbol, udsRsp.PriceUnit);
            }
            if (!HubSymbolProductNameMapping.ContainsKey(udsRsp.Symbol)) {
                HubSymbolProductNameMapping.Add(udsRsp.Symbol, udsRsp.ProductName);
            }
            if (!HubSymbolStripMapping.ContainsKey(udsRsp.Symbol)) {
                HubSymbolStripMapping.Add(udsRsp.Symbol, udsRsp.StripName);
            }
            if (!HubSymbolQtyUnitMapping.ContainsKey(udsRsp.Symbol)) {
                HubSymbolQtyUnitMapping.Add(udsRsp.Symbol, udsRsp.UnitOfMeasure);
            }
            if (!HubSymbolMICMapping.ContainsKey(udsRsp.Symbol)) {
                HubSymbolMICMapping.Add(udsRsp.Symbol, udsRsp.SecurityExchange);
            }
        }
        #endregion

        #region News
        public ObservableCollection<News> ICENews { get; } = new();
        private void TO_OnNews(IceNewsRsp newsRsp) {
            var news = new News();
            news.Headline = newsRsp.Headline;
            switch (newsRsp.Urgency) {
                case 0:
                    news.Urgency = "Normal";
                    break;
                case 1:
                    news.Urgency = "Flash";
                    break;
                case 2:
                    news.Urgency = "Background";
                    break;
                case 3:
                    news.Urgency = "Error";
                    break;
                default:
                    break;
            }
            news.GatewayID = newsRsp.UserName;
            string text = "";
            foreach (var line in newsRsp.Texts) {
                text += line + " ";
            }
            news.Text = text;

            ICENews.Insert(0, news);
            SaveSecurityAndUDSToDictionary();
        }
        #endregion

        #region Deal Allocation
        private string _dealAllocation = "";
        public string DealAllocation {
            get => _dealAllocation;
            set => SetProperty(ref _dealAllocation, value);
        }
        public ObservableCollection<string> DealAllocationSeries { get; } = new();
        public Dictionary<char, string> AllocTranTypeMapping = new Dictionary<char, string>() {
            ['0'] = "New",
            ['1'] = "Replace",
            ['2'] = "Cancel(Bust)",
            ['6'] = "Reversal(custom value)"
        };
        public Dictionary<int, string> AllocReportTypeMapping = new Dictionary<int, string>() {
            [2] = "Preliminary Request to Intermediary",
            [8] = "Request to Intermediary",
            [9] = "Accept (only for Give-Up side)",
            [10] = "Reject",
            [12] = "Complete (only for Take-Up side)",
            [14] = " Reverse Pending",
            [30] = "Full Service Trade"
        };
        public Dictionary<int, string> AllocStatusMapping = new Dictionary<int, string>() {
            [0] = "Accepted",
            [3] = "Received",
            [5] = "Rejected in error (by clearing house)",
            [6] = "Pending Acceptance"
        };
        private void TO_OnAllocationReport(IceAllocationReportRsp allocRsp) {
            string allocationDetail = "";
            allocationDetail += string.Format("AllocID: {0}", allocRsp.AllocID + ";\n");
            try {
                allocationDetail += string.Format("Tran Type: {0}", AllocTranTypeMapping[allocRsp.AllocTransType] + ";\n");
            }
            catch { }
            allocationDetail += string.Format("Clearing Biz Date: {0}", allocRsp.ClearingBusinessDate + ";\n");
            try {
                allocationDetail += string.Format("Report Type: {0}", AllocReportTypeMapping[allocRsp.AllocReportType] + ";\n");
            }
            catch { }
            try {
                allocationDetail += string.Format("Alloc Status: {0}", AllocStatusMapping[allocRsp.AllocStatus] + ";\n");
            }
            catch { }

            foreach (var exec in allocRsp.Execs) {
                allocationDetail += string.Format("TradeID of the allocated trade: {0}", exec.TradeID + ";\n");
                allocationDetail += string.Format("Num of Share In Individual Execution: {0}", exec.LastShares + ";\n");
                allocationDetail += string.Format("Exec ID: {0}", exec.ExecID + ";\n");
            }
            switch (allocRsp.Side) {
                case '1':
                    allocationDetail += string.Format("Buy/Sell:Buy;\n");
                    break;
                case '2':
                    allocationDetail += string.Format("Buy/Sell:Sell;\n");
                    break;
                default: break;
            }
            if (!string.IsNullOrEmpty(allocRsp.TrdType)) {
                try {
                    allocationDetail += string.Format("Trade Type: {0}", TrdTypeMapping[allocRsp.TrdType] + ";\n");
                }
                catch {
                }
            }
            try {
                allocationDetail += string.Format("Market: {0}", HubSymbolDescMapping[allocRsp.Symbol] + ";\n");
            }
            catch { }
            allocationDetail += string.Format("Total num of Shares allocated: {0}", allocRsp.Shares + ";\n");
            allocationDetail += string.Format("AvgPx: {0}", allocRsp.AvgPx + ";\n");
            allocationDetail += string.Format("Transact Time: {0}", allocRsp.TransactTime + ";\n");
            switch (allocRsp.LiquidityIndicator) {
                case 'A':
                    allocationDetail += string.Format("liquidity Indicator=Added Liquidity;\n");
                    break;
                case 'R':
                    allocationDetail += string.Format("liquidity Indicator= Remove Liquidity ;\n");
                    break;
                default: break;
            }
            foreach (var alloc in allocRsp.Allocs) {
                if (!string.IsNullOrEmpty(alloc.AllocAccount)) {
                    allocationDetail += string.Format("Alloc Account: {0}", alloc.AllocAccount + ";\n");
                }
                foreach (var allocInfo in alloc.AllocInfos) {
                    switch (allocInfo.AllocSideInfo) {
                        case 1: {
                                allocationDetail += string.Format("Alloc Side: Give Up;\n");
                                break;
                            }
                        case 2: {
                                allocationDetail += string.Format("Alloc Side: Take Up;\n");
                                break;
                            }
                        default: break;
                    }
                    allocationDetail += string.Format("ClientID(company ID): {0}", allocInfo.ClientID + ";\n");
                    if (allocInfo.BrokerCompID != 0) {
                        allocationDetail += string.Format("BrokerCompID(for the give up): {0}", allocInfo.BrokerCompID + ";\n");
                    }
                    if (!string.IsNullOrEmpty(allocInfo.CustomerAccountReflID)) {
                        allocationDetail += string.Format("Customer Account Reference ID: {0}", allocInfo.CustomerAccountReflID + ";\n");
                    }
                }
            }
            DealAllocation = allocationDetail;
            DealAllocationSeries.Insert(0, DealAllocation);
        }
        #endregion

        #region UCR
        private void TO_OnUserCompanyResponse(IceUserCompanyResponse ucrRsp) {// we don't need to process this actually, only one company as rsp:our company
        }
        #endregion
        #endregion

        #region check function
        public Dictionary<CheckerOutput, string> SprDetailsForConsistDeals = new Dictionary<CheckerOutput, string>();
        public List<TraderInput> checkedInputs = new List<TraderInput>();
        private void FindSimilarRecord(CheckerOutput selectedOutput) {
            SprDetails = "";
            SimilarRecordNum = "";
            ComparedResult = "";
            CompareDetails.Clear();
            foreach (var item in APIData) {//clear previously highlighted record
                item.IsCurrentlyHighlighted = false;
                item.IsBRKDifferent = false;
                item.IsB_SDifferent = false;
                item.IsContractDifferent = false;
                item.IsHubDifferent = false;
                item.IsLotDifferent = false;
                item.IsPriceDifferent = false;
                item.IsUserIDDifferent = false;
            }

            if (selectedOutput == null) {
                return;
            }

            ComparedResult = "Inconsistent";

            var dic = RelativeTrades[selectedOutput];
            if (selectedOutput.Contract.ToLower().Contains("crack")) {
                foreach (var key in dic.Keys) {
                    foreach (var value in dic[key]) {
                        if (MappedTC.Contains(value)) {
                            dic.Remove(key);
                        }
                    }
                }
                foreach (var rcd in dic.Values) {
                    foreach (var rd in rcd) {
                        rd.IsSimilarInconsistentRecord = true;
                        rd.IsCurrentlyHighlighted = true;
                        if (rd.HaveConsistentRecordFromTrader == null) {
                            rd.HaveConsistentRecordFromTrader = false;
                        }
                        //check the different field
                        if (!rcd.Any(x => x.Hub.Contains("/")) && rcd.Count() > 1) {
                            CheckDifferenceFieldForCrack(rcd, selectedOutput);
                        }
                        else {
                            CheckDifferentFields(rd, selectedOutput);
                        }
                    }
                }
            }
            else {
                if (dic.ContainsKey(criticalCompareFieldNum - 1)) {
                    MappedTC.ForEach(x => dic[criticalCompareFieldNum - 1].Remove(x));

                    foreach (var rd in dic[criticalCompareFieldNum - 1]) {
                        rd.IsSimilarInconsistentRecord = true;
                        rd.IsCurrentlyHighlighted = true;
                        if (rd.HaveConsistentRecordFromTrader == null) {
                            rd.HaveConsistentRecordFromTrader = false;
                        }
                        //check the different field
                        CheckDifferentFields(rd, selectedOutput);
                    }
                }
                if (dic.ContainsKey(criticalCompareFieldNum - 2)) {
                    MappedTC.ForEach(x => dic[criticalCompareFieldNum - 2].Remove(x));
                    foreach (var rd in dic[criticalCompareFieldNum - 2]) {
                        rd.IsSimilarInconsistentRecord = true;
                        rd.IsCurrentlyHighlighted = true;
                        if (rd.HaveConsistentRecordFromTrader == null) {
                            rd.HaveConsistentRecordFromTrader = false;
                        }
                        //check the different field
                        CheckDifferentFields(rd, selectedOutput);
                    }
                }
            }
            try { SprDetails = SprDetailsForConsistDeals[selectedOutput]; }
            catch { }
            if (CompareDetails.Count == 1) {
                SimilarRecordNum += " (1 similar record)";
            }
            else {
                SimilarRecordNum += string.Format(" ({0} similar records)", CompareDetails.Count);
            }
        }
        private void FindMappedRecord(CheckerOutput selectedOutput) {
            SprDetails = "";
            ComparedResult = "";
            foreach (var item in APIData) {//clear previously highlighted record
                item.IsCurrentlyHighlighted = false;
            }
            if (selectedOutput == null) {
                return;
            }

            ComparedResult = "Success";

            var dic = RelativeTrades[selectedOutput];
            if (selectedOutput.Contract.ToLower().Contains("crack")) {
                foreach (var list in dic.Values) {
                    foreach (var rd in list) {
                        rd.HaveConsistentRecordFromTrader = true;
                        rd.IsCurrentlyHighlighted = true;
                    }
                }
            }
            else if (dic.ContainsKey(criticalCompareFieldNum)) {
                foreach (var rd in dic[criticalCompareFieldNum]) {
                    rd.HaveConsistentRecordFromTrader = true;
                    rd.IsCurrentlyHighlighted = true;
                }
            }
            try { SprDetails = SprDetailsForConsistDeals[selectedOutput]; }
            catch { }

        }

        private void CheckDifferenceFieldForCrack(List<TradeCaptureData> rds, CheckerOutput selectedOutput) {
            if (rds.Count != 2) return;
            try {
                foreach (var value in ProductAbbrMapping[selectedOutput.Contract.ToLower()]) {
                    string[] items = value.Trim().ToLower().Split('/');//380cst sing,brent 1st line
                    foreach (var rd in rds) {
                        rd.IsHubDifferent = !items.Contains(rd.Hub.Trim().ToLower().Replace("mini", "")) ? true : false;
                        if (!rd.IsHubDifferent) break;
                    }
                }
            }
            catch { }

            try {
                var rd = rds[0];
                if (!string.IsNullOrEmpty(selectedOutput.Shop)) {
                    if (rd.Source.ToLower().Contains("webice") && selectedOutput.Shop.Trim().ToLower().Contains("ice")) {//some WebICE trades will also be recorded
                        rd.IsBRKDifferent = false;
                    }
                    else {
                        foreach (var brkFirm in ShopBRKMapping[selectedOutput.Shop.Trim().ToLower()]) {
                            rd.IsBRKDifferent = !rd.BrokerFirm.ToLower().Contains(brkFirm.ToLower()) ? true : false;
                            if (!rd.IsBRKDifferent) break;
                        }
                    }
                }
            }
            catch { }

            try {
                var rd = rds[0];
                rd.IsContractDifferent = selectedOutput.Month.Replace("-", "").ToLower() != rd.Contract.Replace("-", "").ToLower() ? true : false;
            }
            catch { }

            //compare qty
            decimal qty_nobrent = rds.Where(x => !x.Hub.ToLower().Contains("brent") && !x.Hub.ToLower().Contains("mini")).Sum(x => x.Lots) +
                  rds.Where(x => !x.Hub.ToLower().Contains("brent") && x.Hub.ToLower().Contains("mini")).Sum(x => (decimal)x.Lots / 10);
            decimal qty_brent = rds.Where(x => x.Hub.ToLower().Contains("brent")).Sum(x => x.Lots);
            try {
                foreach (var rd in rds) {
                    rd.IsLotDifferent = Math.Abs(Math.Round(qty_nobrent * (decimal)6.35) - qty_brent) <= 1 ? true : false;
                }
            }
            catch { }

            //compare price
            decimal price_nobrent = (decimal)rds.Where(x => !x.Hub.ToLower().Contains("brent")).Sum(x => x.Price);
            decimal price_brent = (decimal)rds.Where(x => x.Hub.ToLower().Contains("brent")).Sum(x => x.Price);
            try {
                foreach (var rd in rds) {
                    rd.IsPriceDifferent = (decimal)selectedOutput.Price == decimal.Divide(price_brent, (decimal)6.35) - price_nobrent ? true : false;
                }
            }
            catch { }

            try {
                foreach (var rd in rds) {
                    rd.IsUserIDDifferent = selectedOutput.Trader != rd.UserID ? true : false;
                }
            }
            catch { }

            DifferenceDetail detail = new DifferenceDetail();
            foreach (var record in rds) { detail.DealID += record.DealID; }
            string diffDesc = "";
            string diffFields = "";
            var rd1 = rds.FirstOrDefault(x => x.Hub.ToLower().Contains("brent"));
            var rd2 = rds.Where(x => !x.Hub.ToLower().Contains("brent"));
            if (rd1 == null || rd2 == null || rd2.Count() == 0) { Debug.WriteLine("Error: Cannot find grouped crack deals in legs"); }
            if (rd1.IsUserIDDifferent) {
                diffFields += "User ID & ";
                diffDesc += "ICE UserID: \"" + rd1.UserID + "\" vs Your UserID: \"" + selectedOutput.Trader + "\"\n";
            }
            if (rd1.IsPriceDifferent) {
                diffFields += "Price & ";
                diffDesc += "ICE Price: " + "Brent Price:\" " + price_brent + ";" +
                    rd2.Select(x => x.Hub.Replace("mini", "")).ToString() + " Price: \"" + price_nobrent +
                    "\" vs Your Price: \"" + selectedOutput.Price + "\"\n";
            }
            if (rd1.IsLotDifferent) {
                diffFields += "Lots & ";
                diffDesc += "ICE Lots: " + "Brent: \"" + qty_brent + ";" +
                    rd2.FirstOrDefault().Hub.Replace("mini", "") + ": \"" + qty_nobrent +
                    "\" vs Your Lots: \"" + Math.Abs((decimal)selectedOutput.Qty) + "\"\n";
            }
            if (rd1.IsHubDifferent) {
                diffFields += "Hub & ";
                diffDesc += "ICE Hub: \"" + rd1.Hub + ";" + rd2.FirstOrDefault().Hub.Replace("mini", "") + "\" vs Your Hub: \"" + selectedOutput.Contract + "\"\n";
            }
            if (rd1.IsBRKDifferent) {
                diffFields += "Broker & ";
                diffDesc += "ICE BRK: \"" + rd1.BrokerFirm + "\" vs Your BRK: \"" + selectedOutput.Shop + "\"\n";
            }

            if (rd1.IsContractDifferent) {
                diffFields += "Contract & ";
                diffDesc += "ICE Contract: \"" + rd1.Contract + "\" vs Your Contract: \"" + selectedOutput.Month + "\"\n";
            }
            diffFields = diffFields.TrimEnd();
            diffFields = diffFields.TrimEnd('&');
            detail.DifferenceDesc = diffDesc.TrimEnd('\n');
            detail.DifferentFields = diffFields;

            CompareDetails.Add(detail);
        }
        private void CheckDifferentFields(TradeCaptureData rd, CheckerOutput selectedOutput) {
            try {
                foreach (var value in ProductAbbrMapping[selectedOutput.Contract.ToLower()]) {
                    rd.IsHubDifferent = !HubSymbolDescMapping[rd.Symbol].ToLower().Contains(value) ? true : false;
                    if (!rd.IsHubDifferent) break;
                }
            }
            catch { rd.IsHubDifferent = true; }
            try {
                if (!string.IsNullOrEmpty(selectedOutput.Shop)) {
                    if (rd.Source.ToLower().Contains("webice") && selectedOutput.Shop.Trim().ToLower().Contains("ice")) {//some WebICE trades will also be recorded
                        rd.IsBRKDifferent = false;
                    }
                    else {
                        foreach (var brkFirm in ShopBRKMapping[selectedOutput.Shop.Trim().ToLower()]) {
                            rd.IsBRKDifferent = !rd.BrokerFirm.ToLower().Contains(brkFirm.ToLower()) ? true : false;
                            if (!rd.IsBRKDifferent) break;
                        }
                    }
                }
            }
            catch { rd.IsBRKDifferent = true; }

            try { rd.IsContractDifferent = selectedOutput.Month.Replace("-", "").ToLower() != rd.Contract.Replace("-", "").ToLower() ? true : false; }
            catch { rd.IsContractDifferent = true; }
            rd.IsPriceDifferent = selectedOutput.Price != rd.Price ? true : false;
            rd.IsUserIDDifferent = selectedOutput.Trader != rd.UserID ? true : false;
            if (rd.Hub.ToLower().Contains("mini")) {
                rd.IsLotDifferent = Math.Abs((decimal)selectedOutput.Qty * 10) != Math.Abs(rd.Lots) ? true : false;
            }
            else {
                rd.IsLotDifferent = Math.Abs((decimal)selectedOutput.Qty) != Math.Abs(rd.Lots) ? true : false;
            }
            rd.IsB_SDifferent = (selectedOutput.Qty < 0 && rd.B_S != "Sold") || (selectedOutput.Qty > 0 && rd.B_S != "Bought") ? true : false;

            DifferenceDetail detail = new DifferenceDetail();
            detail.DealID = rd.DealID;
            string diffDesc = "";
            string diffFields = "";
            if (rd.IsUserIDDifferent) {
                diffFields += "User ID & ";
                diffDesc += "ICE UserID: \"" + rd.UserID + "\" vs Your UserID: \"" + selectedOutput.Trader + "\"\n";
            }
            if (rd.IsPriceDifferent) {
                diffFields += "Price & ";
                diffDesc += "ICE Price: \"" + rd.Price + "\" vs Your Price: \"" + selectedOutput.Price + "\"\n";
            }
            if (rd.IsLotDifferent) {
                diffFields += "Lots & ";
                if (rd.Hub.Contains("mini")) {
                    diffDesc += "ICE Lots: \"" + rd.Lots + "\" vs Your Lots: \"" + Math.Abs((decimal)selectedOutput.Qty * 10) + " (for mini)\"\n";

                }
                else {
                    diffDesc += "ICE Lots: \"" + rd.Lots + "\" vs Your Lots: \"" + Math.Abs((decimal)selectedOutput.Qty) + "\"\n";
                }
            }
            if (rd.IsHubDifferent) {
                diffFields += "Hub & ";
                diffDesc += "ICE Hub: \"" + rd.Hub + "\" vs Your Hub: \"" + selectedOutput.Contract + "\"\n";
            }
            if (rd.IsBRKDifferent) {
                diffFields += "Broker & ";
                diffDesc += "ICE BRK: \"" + rd.BrokerFirm + "\" vs Your BRK: \"" + selectedOutput.Shop + "\"\n";
            }
            if (rd.IsB_SDifferent) {
                diffFields += "Buy/Sell side & ";
                diffDesc += "ICE Side: \"" + rd.B_S + "\" vs Your Side: \"" + ((selectedOutput.Qty > 0) ? "Bought" : "Sold") + "\"\n";
            }
            if (rd.IsContractDifferent) {
                diffFields += "Contract & ";
                diffDesc += "ICE Contract: \"" + rd.Contract + "\" vs Your Contract: \"" + selectedOutput.Month + "\"\n";
            }
            diffFields = diffFields.TrimEnd();
            diffFields = diffFields.TrimEnd('&');
            detail.DifferenceDesc = diffDesc.TrimEnd('\n');
            detail.DifferentFields = diffFields;
            CompareDetails.Add(detail);
        }

        private int criticalCompareFieldNum = 7;
        private int pendingInputsNum = 0;
        private int passedInputsNum = 0;
        private int failedInputsNum = 0;
        private int sprRecordsNum = 0;
        private void InputReceived(List<TraderInput> inputs) {//compare logic:which is the critical field to focus the trade
            pendingInputsNum = 0;
            passedInputsNum = 0;
            failedInputsNum = 0;
            sprRecordsNum = 0;
            FirstCheckDone = true;
            foreach (var data in APIData) {
                data.CheckStatusVisible = true;
            }
            if (APIData.Count == 0) {
                MessageBox.Show("No data from API. Pls click sync button and compare again");
                return;
            }
            firstRoundCheck = true;
            CrackCheck(inputs);
            checkedInputs.ForEach(x => inputs.Remove(x));

            SprdCheck(inputs);
            checkedInputs.ForEach(x => inputs.Remove(x));
            OutrightCheck(inputs);

            firstRoundCheck = false;
            checkedInputs.ForEach(x => inputs.Remove(x));
            SprdCheck(inputs);
            checkedInputs.ForEach(x => inputs.Remove(x));
            OutrightCheck(inputs);

            if (passedInputsNum + failedInputsNum == 0) {
                MessageBox.Show("No record is similar ! Pls wait for a while and check again");
            }
            else {
                MessageBox.Show(string.Format("{0} of {1} record(s) has been checked. Outcome is as below", passedInputsNum + failedInputsNum, passedInputsNum + failedInputsNum + pendingInputsNum));
            }
            _ea.GetEvent<DisplayCheckedNumEvent>().Publish(pendingInputsNum + passedInputsNum + failedInputsNum);
            _ea.GetEvent<DisplayPassedNumEvent>().Publish(passedInputsNum);
            _ea.GetEvent<DisplayFailedNumEvent>().Publish(failedInputsNum);
            _ea.GetEvent<DisplayPendingNumEvent>().Publish(pendingInputsNum);
            _ea.GetEvent<DisplayMergedSprNumEvent>().Publish(sprRecordsNum);

            MappedFeeds = MappedTC.Count();
            PendingFeeds = TotalFeeds - MappedFeeds - CanceledFeeds;
            PendingBlockFeeds = APIData.Count(x => x.UserID == UserID && x.Source.ToLower().Contains("block") && x.Status.ToLower() != "cancel") - MappedTC.Count(x => x.Source.ToLower().Contains("block"));
            PendingOtherFeeds = PendingFeeds - PendingBlockFeeds;
        }
        private bool firstRoundCheck;
        #endregion

        private void CrackCheck(List<TraderInput> inputs) {
            int inputNum = inputs.Count;
            int trdCaptureNum;
            int[] dp = new int[inputNum];//initialize with all zero

            for (int i = 0; i < inputNum; i++) {
                TraderInput inputData = inputs[i];

                //if shop is "internal", then dont' compare
                if (inputData.Shop.Trim().ToLower().Contains("internal")) {
                    inputData.IsConsistent = true;
                    continue;
                }

                int maxValue = 0;
                Dictionary<int, Dictionary<int, List<TradeCaptureData>>> CrackHt = new Dictionary<int, Dictionary<int, List<TradeCaptureData>>>();
                trdCaptureNum = APIData.Count;

                DateTime time;
                try { time = (DateTime)inputData.Time; }
                catch { time = DateTime.Now; }

                #region inputdata is a crack deal
                if (inputData.Contract.ToLower().Contains("crack")) {

                    // check whether API feed is an outright or legs

                    //first check outright:same as below for loop for APIData

                    //then check legs

                    //var result = APIData.Where(x => x.UserID == UserID && x.Status.ToLower() != "cancel").GroupBy(x => new { x.TrdTime, x.TrdDate, x.BrokerFirm, x.BRK, x.Contract, x.Source, x.Status }).Where(g => g.Count() > 1).ToList();
                    var result = APIData.Where(x => !string.IsNullOrEmpty(x.GroupIndicator) && x.UserID == UserID && x.Status.ToLower() != "cancel" && !string.IsNullOrEmpty(x.Contract)).
                                         GroupBy(x => x.GroupIndicator).Where(g => g.Count() > 1).ToList();
                    if (result != null && result.Count() > 0) {
                        bool findPossibleCrackGroup = false;
                        foreach (var group in result) {
                            int sameFieldNum = 1;//userid is the same
                            var dic2 = new Dictionary<int, List<TradeCaptureData>>();

                            HashSet<string> productSet = group.Select(x => x.Hub.ToLower().Replace("mini", "").Trim()).ToHashSet();//380cst sing,brent 1st line,380cst sing mini
                            try {
                                foreach (var value in ProductAbbrMapping[inputData.Contract.ToLower().Trim()]) {
                                    bool isMatch = true;
                                    foreach (var product in productSet) {
                                        if (!value.Contains(product)) {
                                            isMatch = false;
                                            break;
                                        }
                                    }
                                    if (isMatch) {
                                        findPossibleCrackGroup = true;
                                        sameFieldNum++;
                                        break;
                                    }
                                }
                            }
                            catch { }
                            if (findPossibleCrackGroup) {
                                //compare qty
                                decimal qty_nobrent = group.Where(x => !x.Hub.ToLower().Contains("brent") && !x.Hub.ToLower().Contains("mini")).Sum(x => x.Lots) +
                                    group.Where(x => !x.Hub.ToLower().Contains("brent") && x.Hub.ToLower().Contains("mini")).Sum(x => (decimal)x.Lots / 10);
                                decimal qty_brent = group.Where(x => x.Hub.ToLower().Contains("brent")).Sum(x => x.Lots);

                                //compare price
                                decimal price_nobrent = (decimal)group.Where(x => !x.Hub.ToLower().Contains("brent")).Sum(x => x.Price);
                                decimal price_brent = (decimal)group.Where(x => x.Hub.ToLower().Contains("brent")).Sum(x => x.Price);
                                if ((decimal)inputData.Price == decimal.Divide(price_nobrent, (decimal)6.35) - price_brent) {
                                    sameFieldNum += 2;//combined price and b_s side compare
                                }
                                if (Math.Abs(Math.Round(qty_nobrent * (decimal)6.35) - qty_brent) <= 1) {
                                    sameFieldNum++;
                                    inputData.IsConsistent = true;
                                }
                                try {
                                    if (inputData.Shop.Trim().ToLower().Contains("ice") && group.Count(x => x.Source.ToLower().Contains("webice")) > 0) {
                                        sameFieldNum++;
                                    }
                                    else if (!string.IsNullOrEmpty(group.FirstOrDefault().BrokerFirm)) {
                                        foreach (var brkFirm in ShopBRKMapping[inputData.Shop.ToLower().Trim()]) {
                                            if (group.FirstOrDefault().BrokerFirm.ToLower().Contains(brkFirm.ToLower())) {
                                                sameFieldNum++;
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch { }
                                if (inputData.Month.Replace("-", "").ToLower().Trim() == group.FirstOrDefault().Contract.Replace("-", "").ToLower().Trim()) {
                                    sameFieldNum++;
                                }
                                try {
                                    if (CrackHt.ContainsKey(sameFieldNum)) {
                                        CrackHt[sameFieldNum].Add(CrackHt[sameFieldNum].Count, group.ToList());
                                    }
                                    else {
                                        if (maxValue < sameFieldNum) {
                                            maxValue = sameFieldNum;
                                        }
                                        List<TradeCaptureData> list = new List<TradeCaptureData>();

                                        foreach (var item in group) {
                                            list.Add(item);
                                        }
                                        dic2.Add(dic2.Count, list);
                                        CrackHt.Add(sameFieldNum, dic2);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    dp[i] = maxValue;
                    inputData.IsConsistent = dp[i] == criticalCompareFieldNum ? true : false;

                    CheckerOutput output = new CheckerOutput() {
                        Trader = inputData.Trader,
                        Contract = inputData.Contract.Trim(),
                        Shop = inputData.Shop.Trim(),
                        IsConsistent = inputData.IsConsistent
                    };

                    if (inputData.IsConsistent) {
                        output.Month = CrackHt[criticalCompareFieldNum][0].FirstOrDefault().Contract;
                        output.TradeDate = CrackHt[criticalCompareFieldNum][0].FirstOrDefault().TrdDate;
                        output.Time = CrackHt[criticalCompareFieldNum][0].FirstOrDefault().TrdTime;
                        output.BRK = CrackHt[criticalCompareFieldNum][0].FirstOrDefault().BRK;
                        output.TrdStatus = CrackHt[criticalCompareFieldNum][0].FirstOrDefault().Status;
                        output.Price = inputData.Price;
                        output.Qty = inputData.Qty;
                        if (CrackHt[criticalCompareFieldNum].Count > 1) {
                            Debug.WriteLine("Error occurs: one input crack record is mapped to multiple TC records");

                            bool canMapp = false;
                            foreach (var rcdList in CrackHt[criticalCompareFieldNum].Values) {
                                foreach (var rd in rcdList) {
                                    if (!MappedTC.Contains(rd)) {
                                        MappedTC.Add(rd);
                                        AlrCheckedDealIDs.Add(rd.DealID);
                                        rd.IsFeedMapped = true;
                                        canMapp = true;
                                        break;
                                    }
                                    else {
                                        rcdList.Remove(rd);
                                    }
                                }
                            }
                            RelativeTrades.Add(output, CrackHt[criticalCompareFieldNum]);

                            if (!canMapp) { return; }
                        }
                        else {
                            foreach (var item in CrackHt[criticalCompareFieldNum][0]) {
                                if (!MappedTC.Contains(item)) {
                                    MappedTC.Add(item);
                                    AlrCheckedDealIDs.Add(item.DealID);
                                    item.IsFeedMapped = true;
                                }
                            }
                            RelativeTrades.Add(output, CrackHt[criticalCompareFieldNum]);

                        }
                        _ea.GetEvent<AddOutputEvent>().Publish(output);
                        _ea.GetEvent<CheckerCallBackEvent>().Publish(inputData);
                        checkedInputs.Add(inputData);

                        passedInputsNum++;
                    }
                    else if (dp[i] == criticalCompareFieldNum - 1 || dp[i] == criticalCompareFieldNum - 2) {
                        output.Month = inputData.Month.Trim();
                        output.Qty = (double)inputData.Qty;
                        output.Time = inputData.Time;
                        output.Price = inputData.Price;

                        bool isEmpty = false;
                        if (dp[i] == criticalCompareFieldNum - 1) {
                            foreach (var tc in MappedTC) {
                                foreach (var key in CrackHt[criticalCompareFieldNum - 1].Keys) {
                                    if (CrackHt[criticalCompareFieldNum - 1][key].Contains(tc)) {
                                        CrackHt[criticalCompareFieldNum - 1].Remove(key);
                                    }
                                }
                            }
                            if (CrackHt[criticalCompareFieldNum - 1].Count == 0) {
                                isEmpty = true;
                            }
                        }
                        if (dp[i] == criticalCompareFieldNum - 2) {
                            foreach (var tc in MappedTC) {
                                foreach (var key in CrackHt[criticalCompareFieldNum - 2].Keys) {
                                    if (CrackHt[criticalCompareFieldNum - 2][key].Contains(tc)) {
                                        CrackHt[criticalCompareFieldNum - 2].Remove(key);
                                    }
                                }
                            }
                            if (CrackHt[criticalCompareFieldNum - 2].Count == 0) {
                                isEmpty = true;
                            }
                        }
                        if (!isEmpty) {
                            _ea.GetEvent<AddOutputEvent>().Publish(output);
                            _ea.GetEvent<CheckerCallBackEvent>().Publish(inputData);
                            checkedInputs.Add(inputData);

                            failedInputsNum++;
                        }
                        else {
                            pendingInputsNum++;
                        }
                        RelativeTrades.Add(output, CrackHt[dp[i]]);
                    }
                    else {// very insimilar records will be left in trader input region
                        pendingInputsNum++;
                    }
                }
                #endregion
            }
        }
        private void SprdCheck(List<TraderInput> inputs) {
            int inputNum = inputs.Count;
            int trdCaptureNum;
            int[] dp = new int[inputNum];//initialize with all zero

            TraderInput originalInput = null;
            for (int i = 0; i < inputNum; i++) {
                TraderInput inputData = inputs[i];

                //if shop is "internal", then dont' compare
                if (inputData.Shop.Trim().ToLower().Contains("internal")) {
                    inputData.IsConsistent = true;
                    continue;
                }

                int maxValue = 0;
                Dictionary<int, List<TradeCaptureData>> ht = new Dictionary<int, List<TradeCaptureData>>();
                trdCaptureNum = APIData.Count;

                DateTime time;
                try { time = (DateTime)inputData.Time; }
                catch { time = DateTime.Now; }

                #region try to combine trades into spr first
                if (originalInput != null) {
                    try {
                        if (originalInput.Time == inputData.Time &&
                            originalInput.Shop.ToLower().Trim() == inputData.Shop.ToLower().Trim() &&
                            Math.Abs((double)originalInput.Qty) == Math.Abs((double)inputData.Qty) && (double)originalInput.Qty + (double)inputData.Qty == 0 &&
                            originalInput.Contract.Trim().ToLower() == inputData.Contract.Trim().ToLower()) {

                        }
                        else {
                            originalInput = inputData;
                            continue;
                        }
                    }
                    catch { continue; }
                }
                else {
                    originalInput = inputData;
                    continue;
                }

                string strip1 = originalInput.Month.Replace("-", "").ToLower().Trim();
                string strip2 = inputData.Month.Replace("-", "").ToLower().Trim();
                decimal sell_Price = 0, buy_Price = 0;
                try { sell_Price = (decimal)((double)originalInput.Qty < 0 ? originalInput.Price : inputData.Price); }
                catch { }
                try { buy_Price = (decimal)((double)originalInput.Qty > 0 ? originalInput.Price : inputData.Price); }
                catch { }
                //combine the inputs into a sprd
                List<TraderInput> combinedInputs = new List<TraderInput>();
                combinedInputs.Add(inputData);
                combinedInputs.Add(originalInput);
                #endregion

                for (int j = 0; j < trdCaptureNum; j++) {
                    int sameFieldNum = 0;
                    TradeCaptureData tcd = APIData[j];
                    if (tcd.Status == "cancel" || tcd.Status == "Cancel") { continue; }// cancelled trade should be ignored
                    if (string.IsNullOrEmpty(tcd.Contract)) { continue; }// product symbol not stored in dictionary, cannot compare when value missing

                    try { if (!HubSymbolProductNameMapping[tcd.Symbol].ToLower().Contains(" spr")) { continue; } }
                    catch { }

                    //process checking for spread deals                            

                    try {
                        foreach (var value in ProductAbbrMapping[inputData.Contract.ToLower().Trim()]) {
                            if (HubSymbolDescMapping[tcd.Symbol].ToLower().Contains(value)) {
                                sameFieldNum++;
                                break;
                            }
                        }
                    }
                    catch { }
                    try {
                        if (inputData.Shop.Trim().ToLower().Contains("ice") && tcd.Source.ToLower().Contains("webice")) {
                            sameFieldNum++;
                        }
                        else if (!string.IsNullOrEmpty(tcd.BrokerFirm)) {
                            foreach (var brkFirm in ShopBRKMapping[inputData.Shop.ToLower().Trim()]) {
                                if (tcd.BrokerFirm.ToLower().Contains(brkFirm.ToLower())) {
                                    sameFieldNum++;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    try {
                        if (tcd.Contract.Replace("-", "").ToLower().Trim() == strip1 + "/" + strip2 || tcd.Contract.Replace("-", "").ToLower().Trim() == strip2 + "/" + strip1) {
                            sameFieldNum++;
                        }
                    }
                    catch { }
                    try {
                        if (inputData.Trader == tcd.UserID) {
                            sameFieldNum++;
                        }
                    }
                    catch { }
                    try {
                        if (tcd.Hub.ToLower().Contains("mini")) {
                            if (Math.Abs(tcd.Lots) == Math.Abs((decimal)inputData.Qty) * 10) {
                                sameFieldNum++;
                            }
                        }
                        else {
                            if (Math.Abs(tcd.Lots) == Math.Abs((decimal)inputData.Qty)) {
                                sameFieldNum++;
                            }
                        }
                    }
                    catch { }

                    try {
                        if (tcd.B_S == "Sold") {
                            sameFieldNum++;
                            if (tcd.Price == (double)(sell_Price - buy_Price)) {
                                sameFieldNum++;
                            }
                        }
                        else if (tcd.B_S == "Bought") {
                            sameFieldNum++;
                            if (tcd.Price == (double)(buy_Price - sell_Price)) {
                                sameFieldNum++;
                            }
                        }
                    }
                    catch { }

                    ///store a dictionary<int, List<TradeCaptureData>> for each traderinput record,
                    ///first param means "same field num"
                    ///second param means fulfilling tradeCaptureData
                    if (ht.ContainsKey(sameFieldNum)) {
                        ht[sameFieldNum].Add(tcd);
                    }
                    else {
                        if (maxValue < sameFieldNum) {
                            maxValue = sameFieldNum;
                        }
                        List<TradeCaptureData> list = new List<TradeCaptureData>();
                        list.Add(tcd);
                        ht.Add(sameFieldNum, list);
                    }
                }

                dp[i] = maxValue;
                inputData.IsConsistent = dp[i] == criticalCompareFieldNum ? true : false;

                CheckerOutput output = new CheckerOutput() {
                    Trader = inputData.Trader,
                    Contract = inputData.Contract.Trim(),
                    Shop = inputData.Shop.Trim(),
                    IsConsistent = inputData.IsConsistent
                };

                if (inputData.IsConsistent) {
                    output.Month = ht[criticalCompareFieldNum].FirstOrDefault().Contract;
                    output.TradeDate = ht[criticalCompareFieldNum].FirstOrDefault().TrdDate;
                    output.Time = ht[criticalCompareFieldNum].FirstOrDefault().TrdTime;
                    output.BRK = ht[criticalCompareFieldNum].FirstOrDefault().BRK;
                    output.TrdStatus = ht[criticalCompareFieldNum].FirstOrDefault().Status;
                    output.Price = ht[criticalCompareFieldNum].FirstOrDefault().Price;
                    //output.Qty = inputData.Qty;
                    if (ht[criticalCompareFieldNum].FirstOrDefault().B_S.ToLower() == "bought") {
                        output.Qty = ht[criticalCompareFieldNum].FirstOrDefault().Lots;
                    }
                    else {
                        output.Qty = (-1) * ht[criticalCompareFieldNum].FirstOrDefault().Lots;
                    }


                    if (ht[criticalCompareFieldNum].Count > 1) {
                        Debug.WriteLine("Error occurs: one input record is mapped to multiple TC records");

                        bool canMapp = false;
                        foreach (var rd in ht[criticalCompareFieldNum]) {
                            if (!MappedTC.Contains(rd)) {
                                MappedTC.Add(rd);
                                AlrCheckedDealIDs.Add(rd.DealID);
                                rd.IsFeedMapped = true;
                                canMapp = true;
                                List<TradeCaptureData> list = new List<TradeCaptureData>();
                                list.Add(rd);
                                ht[criticalCompareFieldNum] = list;
                                break;
                            }
                        }
                        RelativeTrades.Add(output, ht);

                        if (!canMapp) { return; }
                    }
                    else {
                        if (MappedTC.Contains(ht[criticalCompareFieldNum][0])) { return; }
                        var rd = ht[criticalCompareFieldNum][0];
                        MappedTC.Add(rd);
                        AlrCheckedDealIDs.Add(rd.DealID);
                        rd.IsFeedMapped = true;
                        RelativeTrades.Add(output, ht);
                    }

                    string MergedSprDetails = "Spread Input Info: ";
                    MergedSprDetails += output.Trader + ": " + output.Contract + "-\n";
                    foreach (var rcd in combinedInputs) {
                        MergedSprDetails += output.Month + "-->";
                        MergedSprDetails += (rcd.Qty > 0) ? "Bought " : "Sold ";
                        MergedSprDetails += Math.Abs((decimal)rcd.Qty) + " lots, at a price of " + rcd.Price + " under \"" + rcd.Shop + "\"\n";
                    }
                    MergedSprDetails += "Price Difference= " + output.Price;
                    if (!SprDetailsForConsistDeals.ContainsKey(output)) {
                        SprDetailsForConsistDeals.Add(output, MergedSprDetails);
                    }
                    else {
                        SprDetailsForConsistDeals[output] = MergedSprDetails;
                    }
                    sprRecordsNum++;
                    _ea.GetEvent<AddOutputEvent>().Publish(output);

                    try {
                        foreach (var rcd in combinedInputs) {
                            _ea.GetEvent<CheckerCallBackEvent>().Publish(rcd);
                            checkedInputs.Add(rcd);
                        }
                    }
                    catch { }
                    finally {
                        passedInputsNum += 2;

                    }

                }
                else if (!firstRoundCheck) {
                    if (dp[i] == criticalCompareFieldNum - 1 || dp[i] == criticalCompareFieldNum - 2) {
                        output.Time = inputData.Time;
                        bool isEmpty = false;
                        if (dp[i] == criticalCompareFieldNum - 1) {
                            MappedTC.ForEach(x => ht[criticalCompareFieldNum - 1].Remove(x));
                            if (ht[criticalCompareFieldNum - 1].Count == 0) {
                                isEmpty = true;
                            }
                        }
                        if (dp[i] == criticalCompareFieldNum - 2) {
                            MappedTC.ForEach(x => ht[criticalCompareFieldNum - 2].Remove(x));
                            if (ht[criticalCompareFieldNum - 2].Count == 0) {
                                isEmpty = true;
                            }
                        }
                        if (!isEmpty) {
                            output.Price = (buy_Price > sell_Price) ? (double)(buy_Price - sell_Price) : (double)(sell_Price - buy_Price);
                            output.Qty = (buy_Price > sell_Price) ? Math.Abs((double)inputData.Qty) : (-1) * Math.Abs((double)inputData.Qty);

                            if (dp[i] == criticalCompareFieldNum - 1) {
                                if (ht[criticalCompareFieldNum - 1].Any(x => x.Contract.Replace("-", "").ToLower().Trim() == strip1 + "/" + strip2)) {
                                    output.Month = strip1 + "/" + strip2;
                                }
                                else if (ht[criticalCompareFieldNum - 1].Any(x => x.Contract.Replace("-", "").ToLower().Trim() == strip2 + "/" + strip1)) {
                                    output.Month = strip2 + "/" + strip1;
                                }
                            }
                            else if (dp[i] == criticalCompareFieldNum - 2) {
                                if (ht[criticalCompareFieldNum - 2].Any(x => x.Contract.Replace("-", "").ToLower().Trim() == strip1 + "/" + strip2)) {
                                    output.Month = strip1 + "/" + strip2;
                                }
                                else if (ht[criticalCompareFieldNum - 2].Any(x => x.Contract.Replace("-", "").ToLower().Trim() == strip2 + "/" + strip1)) {
                                    output.Month = strip2 + "/" + strip1;
                                }
                            }

                            try {
                                string MergedSprDetails = "Spread Input Info:\n";
                                MergedSprDetails += output.Trader + ": " + output.Contract + "-\n";
                                foreach (var rcd in combinedInputs) {
                                    MergedSprDetails += rcd.Month + "-->";
                                    MergedSprDetails += (rcd.Qty > 0) ? "Bought " : "Sold ";
                                    MergedSprDetails += Math.Abs((double)rcd.Qty) + " lots, at a price of " + rcd.Price + " under " + rcd.Shop + "\n";
                                }
                                MergedSprDetails += "Price Difference= " + output.Price;
                                SprDetailsForConsistDeals.Add(output, MergedSprDetails);
                                sprRecordsNum++;
                            }
                            catch {
                                MessageBox.Show("Invalid Check");
                                return;
                            }
                            _ea.GetEvent<AddOutputEvent>().Publish(output);
                            foreach (var rcd in combinedInputs) {
                                _ea.GetEvent<CheckerCallBackEvent>().Publish(rcd);
                                checkedInputs.Add(rcd);
                            }

                            failedInputsNum += 2;
                        }
                        else {
                            pendingInputsNum += 2;
                        }
                        RelativeTrades.Add(output, ht);
                    }
                    else {// very insimilar records will be left in trader input region
                        pendingInputsNum += 2;
                        RelativeTrades.Add(output, ht);
                    }
                }

                originalInput = null;
            }

            #region Trade owner converting
            ///also need to check whether deals belong to the correct trader, previously it is checked at above if block, but we should leave here a section for future feature implementation
            #endregion
        }
        private void OutrightCheck(List<TraderInput> inputs) {
            int inputNum = inputs.Count;
            int trdCaptureNum;
            int[] dp = new int[inputNum];//initialize with all zero

            for (int i = 0; i < inputNum; i++) {
                TraderInput inputData = inputs[i];

                //if shop is "internal", then dont' compare
                if (inputData.Shop.Trim().ToLower().Contains("internal")) {
                    inputData.IsConsistent = true;
                    continue;
                }

                int maxValue = 0;
                Dictionary<int, List<TradeCaptureData>> ht = new Dictionary<int, List<TradeCaptureData>>();
                trdCaptureNum = APIData.Count;

                DateTime time;
                try { time = (DateTime)inputData.Time; }
                catch { time = DateTime.Now; }

                for (int j = 0; j < trdCaptureNum; j++) {
                    int sameFieldNum = 0;
                    TradeCaptureData tcd = APIData[j];
                    if (tcd.Status == "cancel" || tcd.Status == "Cancel") { continue; }// cancelled trade should be ignored
                    if (string.IsNullOrEmpty(tcd.Contract)) { continue; }// product symbol not stored in dictionary, cannot compare when value missing
                    try {
                        foreach (var value in ProductAbbrMapping[inputData.Contract.ToLower().Trim()]) {
                            if (HubSymbolDescMapping[tcd.Symbol].ToLower().Contains(value)) {
                                sameFieldNum++;
                                break;
                            }
                        }
                    }
                    catch { }
                    try {
                        if (inputData.Shop.Trim().ToLower().Contains("ice") && tcd.Source.ToLower().Contains("webice")) {
                            sameFieldNum++;
                        }
                        else if (!string.IsNullOrEmpty(tcd.BrokerFirm)) {
                            foreach (var brkFirm in ShopBRKMapping[inputData.Shop.ToLower().Trim()]) {
                                if (tcd.BrokerFirm.ToLower().Contains(brkFirm.ToLower())) {
                                    sameFieldNum++;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    try {
                        if (inputData.Month.Replace("-", "").ToLower().Trim() == tcd.Contract.Replace("-", "").ToLower().Trim()) {
                            sameFieldNum++;
                        }
                    }
                    catch { }
                    try {
                        if (inputData.Price == tcd.Price) {
                            sameFieldNum++;
                        }
                    }
                    catch { }
                    try {
                        if (inputData.Trader == tcd.UserID) {
                            sameFieldNum++;
                        }
                    }
                    catch { }
                    try {
                        if (inputData.Qty < 0) { //sell
                            if (tcd.B_S == "Sold") {
                                sameFieldNum++;
                            }
                            if (tcd.Hub.ToLower().Contains("mini")) {
                                if (Math.Abs(tcd.Lots) == Math.Abs((decimal)inputData.Qty) * 10) {
                                    sameFieldNum++;
                                }
                            }
                            else {
                                if (Math.Abs(tcd.Lots) == Math.Abs((decimal)inputData.Qty)) {
                                    sameFieldNum++;
                                }
                            }
                        }
                        else {//buy
                            if (tcd.B_S == "Bought") {
                                sameFieldNum++;
                            }
                            if (tcd.Hub.ToLower().Contains("mini")) {
                                if (Math.Abs(tcd.Lots) == Math.Abs((decimal)inputData.Qty) * 10) {
                                    sameFieldNum++;
                                }
                            }
                            else {
                                if (Math.Abs(tcd.Lots) == Math.Abs((decimal)inputData.Qty)) {
                                    sameFieldNum++;
                                }
                            }
                        }
                    }
                    catch { }

                    if (ht.ContainsKey(sameFieldNum)) {
                        ht[sameFieldNum].Add(tcd);
                    }
                    else {
                        if (maxValue < sameFieldNum) {
                            maxValue = sameFieldNum;
                        }
                        List<TradeCaptureData> list = new List<TradeCaptureData>();
                        list.Add(tcd);
                        ht.Add(sameFieldNum, list);
                    }
                }
                dp[i] = maxValue;
                inputData.IsConsistent = dp[i] == criticalCompareFieldNum ? true : false;

                CheckerOutput output = new CheckerOutput() {
                    Trader = inputData.Trader,
                    Contract = inputData.Contract.Trim(),
                    Shop = inputData.Shop.Trim(),
                    IsConsistent = inputData.IsConsistent
                };

                if (inputData.IsConsistent) {
                    output.Month = ht[criticalCompareFieldNum].FirstOrDefault().Contract;
                    output.TradeDate = ht[criticalCompareFieldNum].FirstOrDefault().TrdDate;
                    output.Time = ht[criticalCompareFieldNum].FirstOrDefault().TrdTime;
                    output.BRK = ht[criticalCompareFieldNum].FirstOrDefault().BRK;
                    output.TrdStatus = ht[criticalCompareFieldNum].FirstOrDefault().Status;
                    output.Price = ht[criticalCompareFieldNum].FirstOrDefault().Price;
                    output.Qty = inputData.Qty;
                    //if (ht[criticalCompareFieldNum].FirstOrDefault().B_S.ToLower() == "bought") {
                    //    output.Qty = ht[criticalCompareFieldNum].FirstOrDefault().Lots;
                    //}
                    //else {
                    //    output.Qty = (-1) * ht[criticalCompareFieldNum].FirstOrDefault().Lots;
                    //}
                    if (ht[criticalCompareFieldNum].Count > 1) {
                        Debug.WriteLine("Error occurs: one input record is mapped to multiple TC records");

                        bool canMapp = false;
                        foreach (var rd in ht[criticalCompareFieldNum]) {
                            if (!MappedTC.Contains(rd)) {
                                MappedTC.Add(rd);
                                AlrCheckedDealIDs.Add(rd.DealID);
                                rd.IsFeedMapped = true;
                                canMapp = true;
                                List<TradeCaptureData> list = new List<TradeCaptureData>();
                                list.Add(rd);
                                ht[criticalCompareFieldNum] = list;
                                break;
                            }
                        }
                        RelativeTrades.Add(output, ht);

                        if (!canMapp) { return; }
                    }
                    else {
                        if (MappedTC.Contains(ht[criticalCompareFieldNum][0])) { return; }
                        var rd = ht[criticalCompareFieldNum][0];
                        MappedTC.Add(rd);
                        AlrCheckedDealIDs.Add(rd.DealID);
                        rd.IsFeedMapped = true;
                        RelativeTrades.Add(output, ht);
                    }
                    _ea.GetEvent<AddOutputEvent>().Publish(output);
                    _ea.GetEvent<CheckerCallBackEvent>().Publish(inputData);
                    checkedInputs.Add(inputData);

                    passedInputsNum++;
                }
                else if (!firstRoundCheck) {
                    if (dp[i] == criticalCompareFieldNum - 1 || dp[i] == criticalCompareFieldNum - 2) {
                        output.Month = inputData.Month.Trim();
                        output.Qty = (double)inputData.Qty;
                        output.Time = inputData.Time;
                        output.Price = inputData.Price;

                        bool isEmpty = false;
                        if (dp[i] == criticalCompareFieldNum - 1) {
                            MappedTC.ForEach(x => ht[criticalCompareFieldNum - 1].Remove(x));
                            if (ht[criticalCompareFieldNum - 1].Count == 0) {
                                isEmpty = true;
                            }
                        }
                        if (dp[i] == criticalCompareFieldNum - 2) {
                            MappedTC.ForEach(x => ht[criticalCompareFieldNum - 2].Remove(x));
                            if (ht[criticalCompareFieldNum - 2].Count == 0) {
                                isEmpty = true;
                            }
                        }
                        if (!isEmpty) {
                            _ea.GetEvent<AddOutputEvent>().Publish(output);
                            _ea.GetEvent<CheckerCallBackEvent>().Publish(inputData);
                            checkedInputs.Add(inputData);

                            failedInputsNum++;
                        }
                        else {
                            pendingInputsNum++;
                        }
                        RelativeTrades.Add(output, ht);
                    }
                    else {// very insimilar records will be left in trader input region
                        pendingInputsNum++;
                        RelativeTrades.Add(output, ht);
                    }
                }

            }
            #region Trade owner converting
            ///also need to check whether deals belong to the correct trader, previously it is checked at above if block, but we should leave here a section for future feature implementation
            #endregion
        }

        #region display deal details
        private string _dealDetails;
        public string DealDetails {
            get => _dealDetails;
            set => SetProperty(ref _dealDetails, value);
        }

        private TradeCaptureData _selectedAPIRecord;
        public TradeCaptureData SelectedAPIRecord {
            get => _selectedAPIRecord;
            set => SetProperty(ref _selectedAPIRecord, value, () => DisplayDealDetails(value));
        }

        public void DisplayDealDetails(TradeCaptureData deal) {
            if (deal == null) { DealDetails = ""; return; }
            string detail = "";
            if (!string.IsNullOrEmpty(deal.DealID)) {
                detail += string.Format("Deal ID: {0}", deal.DealID + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.Status)) {
                if (deal.Status.ToLower() == "cancel") {
                    detail += string.Format("Cleared/Bilateral: {0}", deal.Status + ";  ");
                }
                else if (deal.Status.ToLower() == "trade") {
                    detail += string.Format("Cleared/Bilateral: cleared;  ");

                }

            }
            if (!string.IsNullOrEmpty(deal.UserID)) {
                detail += string.Format("UserID: {0}", deal.UserID + ";  ");
            }
            detail += string.Format("Clearing Account: {0}", deal.ClearingAcct + ";  ");

            try { detail += string.Format("Product Desc: {0}", HubSymbolDescMapping[deal.Symbol] + ";  "); }
            catch { }
            if (!string.IsNullOrEmpty(deal.OrigID)) {
                detail += string.Format("Origin Trade ID: {0}", deal.OrigID + ";  ");
            }
            //detail += string.Format("Strip: {0}", deal.Contract + ";  ");
            detail += string.Format("B/S: {0}", deal.B_S + ";  ");
            if (deal.CFICode == "Put" || deal.CFICode == "Call") {//CALL or PULL
                detail += string.Format("Call/Put: {0}", deal.CFICode + ";  ");
                detail += string.Format("StrikePrice: {0}", deal.Strike + ";  ");
            }
            detail += string.Format("Price: {0}", deal.Price + ";  ");
            detail += string.Format("Lots: {0}", deal.Lots + ";  ");
            if (deal.TotalQty != 0) {
                detail += string.Format("TotalQty: {0}", deal.TotalQty + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.ExchangeSilo)) {
                detail += string.Format("Exchange Silo: {0}", deal.ExchangeSilo + ";  ");
            }
            detail += string.Format("Trade Type: {0}", deal.TrdType + ";  ");
            if (!string.IsNullOrEmpty(deal.BrokerFirm)) {
                detail += string.Format("Broker Firm: {0}", deal.BrokerFirm + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.BRK)) {
                detail += string.Format("Broker: {0}", deal.BRK + ";  ");
            }

            if (!string.IsNullOrEmpty(deal.Location)) {
                detail += string.Format("Location: {0}", deal.Location + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.Meter)) {
                detail += string.Format("Meter Number: {0}", deal.Meter + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.LeadTime)) {
                detail += string.Format("Lead Time: {0}", deal.LeadTime + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.ReasonCode)) {
                detail += string.Format("Reason: {0}", deal.ReasonCode + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.Memo)) {
                detail += string.Format("Memo: {0}", deal.Memo + ";  ");
            }
            if (!string.IsNullOrEmpty(deal.AllocAccount)) {
                detail += string.Format("AllocAccount: {0}", deal.AllocAccount + ";  ");
            }
            if (deal.Legs.Count > 0) {
                detail += "\nLegs Info: \n";
                int legNo = 1;
                foreach (var leg in deal.Legs) {
                    detail += string.Format("Leg{0}: ", legNo++);
                    try {
                        detail += string.Format("Market(product): {0}", HubSymbolDescMapping[leg.LegSymbol] + ";  ");
                    }
                    catch {
                        detail += string.Format("Market(product): {0}", leg.Product + ";  ");
                    }
                    detail += string.Format("CFICode: {0}", leg.LegCFICode + ";  ");
                    detail += string.Format("Strip: {0}", leg.Strip + ";  ");
                    if (leg.LegStrikePrice != 0.0M) {
                        detail += string.Format("Strike: {0}", leg.LegStrikePrice + ";  ");
                    }
                    if (!string.IsNullOrEmpty(leg.LegB_S)) {
                        detail += string.Format("Buy/Sell: {0}", leg.LegB_S + ";  ");
                    }
                    detail += string.Format("Price: {0}", leg.LegLastPx + ";  ");
                    detail += string.Format("Lots: {0}", leg.LegQty + ";  ");
                    if (leg.TotalQty != 0) {
                        detail += string.Format("TotalQty: {0}", leg.TotalQty + ";  ");
                    }
                    detail += string.Format("Memo: {0}", leg.LegMemoField + ";  ");
                    if (leg.LinkExecID != 0) {
                        detail += string.Format("LinkExecID: {0}", leg.LinkExecID + ";  ");
                    }

                    detail += string.Format("LegTradeID: {0}", leg.LegRefID + ";  \n");
                }
                detail.TrimEnd('\n');
            }
            DealDetails = detail;
        }
        #endregion
    }
}
