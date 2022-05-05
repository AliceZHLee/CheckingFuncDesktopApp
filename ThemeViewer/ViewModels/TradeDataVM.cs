using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using FixAdapter.Models.Response;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThemeViewer.Models;
using ThemeViewer.Views;

namespace ThemeViewer.ViewModels {
    public class TradeDataVM : BindableBase, IDisposable {
        public Dictionary<string, string> InstrumentMapping = new();
        public Dictionary<string, string> PartyFullnameMapping = new();
        public Dictionary<string, string> PartyEligibleCounterpartyMapping = new();
        public Dictionary<string, int> contractMonthCodeMapping = new Dictionary<string, int> {
            ["F"] = 1,
            ["G"] = 2,
            ["H"] = 3,
            ["J"] = 4,
            ["K"] = 5,
            ["M"] = 6,
            ["N"] = 7,
            ["Q"] = 8,
            ["U"] = 9,
            ["V"] = 10,
            ["X"] = 11,
            ["Z"] = 12
        };
        public Dictionary<string, string> PropertyColumnMapping = new Dictionary<string, string> {
            ["TradeID"] = "Trade ID",
            ["DealID"] = "Deal ID",
            ["SubmittedBy"] = "Submitted By",
            ["TradeRegistrationDateTime"] = "Trade Registration Date & Time (SGT)",
            ["MarketSegmentID"] = "Market",
            ["SecurityID"] = "Security",
            ["Instrument"] = "Instrument",
            ["TrdType"] = "Trade Type",
            ["ExpireDate"] = "Expiry",
            ["Price"] = "Price",
            ["Strike"] = "Strike",
            ["QtyInLots"] = "Quantity In Lots",
            ["TrdStatus"] = "Trade Status",
            ["ClearedDate"] = "Cleared Date (SGT)",
            ["ClearMsg"] = "CLearing Message",
            ["ApprovalDateTime"] = "Approval Registration Date & Time (SGT)",
            ["TrdExecDateTime"] = "Trade Execution Date & Time (SGT)",
            ["BuyerCM"] = "Buyer CM",
            ["BuyerTrdCom"] = "Buyer Tradeing Company",
            ["BuyerTrader"] = "Buyer Trader",
            ["BuyerTradeRegistrant"] = "Buyer Trade Registrant",
            ["BuyerAccount"] = "Buyer Account",
            ["BuyerSecurID"] = "Buyer Secur ID",
            ["BuyerBroker"] = "Buyer Broker",
            ["BuyerComment"] = "Buyer Comment",
            ["BuyerInternalTrdID"] = "Buyer Internal Trade ID",
            ["BuySource"] = "Buy Source",
            ["SellerCM"] = "Seller CM",
            ["SellerTrdCom"] = "Seller Trade Company",
            ["SellerTrader"] = "SellerTrader",
            ["SellerTradeRegistrant"] = "Seller Trade Registrant",
            ["SellerAccount"] = "Seller Account",
            ["SellerSecurID"] = "Seller Secur ID",
            ["SellerBroker"] = "Seller Broker",
            ["SellerComment"] = "Seller Comment",
            ["SellerInternalTrdID"] = "Selle rInternal Trade ID",
            ["SellSource"] = "Sell Source",
            ["USI"] = "USI",
            ["TradingSessionSubID"] = "Trading Session",
            ["QtyInUnits"] = "Quantity In Units",
            ["QtyType"] = "Quantity Unit"
        };

        private readonly object _lock = new();
        public ObservableCollection<TradeRecord> AllTradeData { get; } = new();
        public CollectionViewSource TradeDataViewS { get; } = new();
        public List<FilterObj> TradeColumns { get; } = new();
        //public ICommand ExportCommand { get; private set; }
        //public string SelectAllToolTip { get; } = "This option will select all values when apply the filter";

        #region Constructor
        public TradeDataVM(IEventAggregator ea) {
            ea.GetEvent<NewMarketDataEvent>().Subscribe(MarketdataReceived);

            BindingOperations.EnableCollectionSynchronization(AllTradeData, _lock);
            TradeDataViewS.Source = AllTradeData;
            foreach (var column in PropertyColumnMapping.Values) {
                TradeColumns.Add(new FilterObj() { Label = column.ToString(), IsChecked = true });
            }
            //ExportCommand = new DelegateCommand<DataGrid>(ExportToExcel);

            OtcHelper.Client.OnSecurities += TO_OnSecurities;
            OtcHelper.Client.OnPartyListRequest += TO_OnPartyListRequest;
            OtcHelper.Client.OnTradeCaptureReport += TO_OnTradeCaptureReport;
        }
        #endregion

        #region check whether the market data belongs to our deals
        private void MarketdataReceived(MarketRecord obj) {
            var relatedTrades = AllTradeData.Where(x => x.SecurityID == obj.Instrument &&
                                   x.Price == obj.Price &&
                                   x.QtyInLots == obj.QtyInLots &&
                                   x.TrdExecDateTime == obj.ExecutedSGT &&
                                   x.TrdStatus==obj.Status).ToList();
            if (relatedTrades.Count() > 0) {
                //current focus is "cleared" trades
                obj.DealID = string.Join("; ", relatedTrades.Select(x => x.DealID));
                obj.IsProductRelated = true;
            }
        }
        #endregion

        #region Titan OTC API (data receive)
        private void TO_OnSecurities(ThmSecurityListRsp securRsp) {
            var securList = securRsp.Securities.ToList();
            foreach (var security in securList) {                
                if (security.SecurityDesc.ToLower().Contains(" put")) {//put options
                    InstrumentMapping.Add(security.SecurityID + "_P", security.SecurityDesc);
                }
                else if (security.SecurityDesc.ToLower().Contains(" call")) {//call options
                    InstrumentMapping.Add(security.SecurityID + "_C", security.SecurityDesc);
                }
                else {//future
                    InstrumentMapping.Add(security.SecurityID, security.SecurityDesc);
                }
            }
        }

        private void TO_OnPartyListRequest(ThmPartyDetailsRsp partyRsp) {
            foreach (var party in partyRsp.Parties) {
                foreach (var subID in party.PartySubs) {
                    if (subID.PartySubIDType == 5) {
                        PartyFullnameMapping.Add(party.PartyID, subID.PartySubID);
                    }
                    else if (subID.PartySubIDType == 29) {
                        PartyEligibleCounterpartyMapping.Add(party.PartyID, subID.PartySubID);
                    }
                }
            }
        }

        private void TO_OnTradeCaptureReport(ThmTradeCaptureReportRsp tradeRsp) {
            //string selectedProduct = SelectProductWin.SelectedProduct == null ? "" : SelectProductWin.SelectedProduct;
            List<string> selectedProducts = SelectProductWin.SelectedProducts;
            List<string> selectedAccounts = SelectProductWin.SelectedAccounts;

            bool isRightProduct = false;
            foreach(var selectedProduct in selectedProducts) {
                if (tradeRsp.Symbol.StartsWith(selectedProduct)) {
                    isRightProduct = true;
                    break;
                }
            }
            if (!isRightProduct) {
                return;
            }
           
            if (!AllTradeData.Any(x => x.TradeID == tradeRsp.TradeID)) {
                var tr = new TradeRecord() {
                    TradeID = tradeRsp.TradeID,
                    DealID = tradeRsp.ExecID,
                    MarketSegmentID = tradeRsp.MarketSegmentID,
                    SecurityID = tradeRsp.Symbol,
                    Price = tradeRsp.LastPx,
                    QtyInLots = tradeRsp.LastQty,
                    ClearedDate = (tradeRsp.ClearingBusinessDate == null)
                                 ? null
                                 : DateTime.ParseExact(tradeRsp.ClearingBusinessDate, "yyyyMMdd", CultureInfo.InvariantCulture),
                    QtyInUnits = (int)tradeRsp.LastQty * 100,
                    QtyType = "MT",
                    TrdExecDateTime = tradeRsp.TransactTime != DateTime.MinValue
                                    ? tradeRsp.TransactTime.ToLocalTime()
                                    : (!string.IsNullOrEmpty(tradeRsp.TradeDate) ? Convert.ToDateTime(tradeRsp.TradeDate) : DateTime.Now),
                    NewlyReceived = true
                };
                string instrument = tr.SecurityID;
                string expiry;
                string month;
                if (tradeRsp.Symbol.Contains("_")) {//option
                    int underscoreIndex = instrument.IndexOf("_");
                    string optionDirection = instrument.Substring(underscoreIndex + 1, 1).ToUpper();// C:call, P: put
                    tr.Strike = Convert.ToDecimal(instrument.Substring(underscoreIndex + 2, instrument.Length - underscoreIndex - 2));
                    tr.Instrument = InstrumentMapping[instrument.Substring(0, underscoreIndex - 3) + instrument.Substring(underscoreIndex, 2)];
                    month = instrument.Substring(underscoreIndex - 3, 1);
                    if (contractMonthCodeMapping[month] < 10) {
                        expiry = "01/0" + contractMonthCodeMapping[month]
                            + "/20" + instrument.Substring(underscoreIndex - 2, 2);
                    }
                    else {
                        expiry = "01/" + contractMonthCodeMapping[month]
                            + "/20" + instrument.Substring(underscoreIndex - 2, 2);
                    }
                }
                else {//future
                    tr.Instrument = InstrumentMapping[tradeRsp.Symbol.Substring(0, instrument.Length - 3)];
                    month = instrument.Substring(instrument.Length - 3, 1);
                    if (contractMonthCodeMapping[month] < 10) {
                        expiry = "01/0" + contractMonthCodeMapping[month]
                            + "/20" + instrument.Substring(instrument.Length - 2, 2);
                    }
                    else {
                        expiry = "01/" + contractMonthCodeMapping[month]
                            + "/20" + instrument.Substring(instrument.Length - 2, 2);
                    }
                }

                tr.ExpireDate = DateTime.ParseExact(expiry, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                switch (tradeRsp.TrdType) {
                    case 1:
                        tr.TrdType = "NLT";
                        break;
                    case 2:
                        tr.TrdType = "EFP";
                        break;
                    case 12:
                        tr.TrdType = "EFS";
                        break;
                    case 54:
                        tr.TrdType = "OTC trade";
                        break;
                    default:
                        break;
                }

                switch (tradeRsp.ExecType) {
                    case 'F':
                        tr.TrdStatus = "Cleared";
                        break;
                    case '0':
                        tr.TrdStatus = "New";
                        break;
                    case '4':
                        tr.TrdStatus = "Canceled";
                        break;
                    case '6':
                        tr.TrdStatus = "Pending Cancel";
                        break;
                    case '8':
                        tr.TrdStatus = "Rejected";
                        break;
                    case 'A':
                        tr.TrdStatus = "Pending New";
                        break;
                    case 'J':
                        tr.TrdStatus = "Trade in a clearing hold";
                        tr.ApprovalDateTime = tradeRsp.TransactTime.ToLocalTime();
                        break;
                    case 'K':
                        tr.TrdStatus = "Trade has been released to clearing";
                        tr.TradeRegistrationDateTime = tradeRsp.TransactTime.ToLocalTime();
                        break;
                    default:
                        break;
                }

                foreach (var rspSide in tradeRsp.Sides) {
                    if (rspSide.Side == '1') { //buy
                        tr.BuyerAccount = rspSide.Account;
                        tr.BuyerSecurID = rspSide.SideExecID;
                        tr.BuyerInternalTrdID = rspSide.SideTradeReportID;
                        tr.BuyerComment = rspSide.Text;
                        tr.BuySource = rspSide.TradeInputSource;
                        foreach (var party in rspSide.Parties) {
                            switch (party.PartyRole) {
                                case 1:
                                    tr.BuyerTrdCom = PartyFullnameMapping[party.PartyID];
                                    break;
                                case 4:
                                    tr.BuyerCM = PartyFullnameMapping[party.PartyID];
                                    break;
                                case 7:
                                    tr.BuyerTradeRegistrant = PartyFullnameMapping[party.PartyID];
                                    break;
                                case 12:
                                    tr.BuyerTrader = party.PartyID;
                                    break;
                                case 30:
                                    tr.BuyerBroker = PartyFullnameMapping[party.PartyID];
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else if (rspSide.Side == '2') { //sell
                        tr.SellerAccount = rspSide.Account;
                        tr.SellerSecurID = rspSide.SideExecID;
                        tr.SellerInternalTrdID = rspSide.SideTradeReportID;
                        tr.SellerComment = rspSide.Text;
                        tr.SellSource = rspSide.TradeInputSource;
                        foreach (var party in rspSide.Parties) {
                            switch (party.PartyRole) {
                                case 1:
                                    tr.SellerTrdCom = PartyFullnameMapping[party.PartyID];
                                    break;
                                case 4:
                                    tr.SellerCM = PartyFullnameMapping[party.PartyID];
                                    break;
                                case 7:
                                    tr.SellerTradeRegistrant = PartyFullnameMapping[party.PartyID];
                                    break;
                                case 12:
                                    tr.SellerTrader = party.PartyID;
                                    break;
                                case 30:
                                    tr.SellerBroker = PartyFullnameMapping[party.PartyID];
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    if (rspSide.Account != null) {
                        tr.TradingSessionSubID = rspSide.TradingSessionSubID == null ? "T" : (rspSide.TradingSessionSubID == "3" ? "T" : "T+1");
                    }
                }
                if (selectedAccounts.Contains(tr.BuyerAccount) || selectedAccounts.Contains(tr.SellerAccount)) {
                    AllTradeData.Insert(0, tr);
                }
            }
            else {//status update
                TradeRecord originalRecord = AllTradeData.First(x => x.TradeID == tradeRsp.TradeID);
                if (AllTradeData.Remove(originalRecord)) {
                    originalRecord.ClearedDate = (tradeRsp.ClearingBusinessDate == null)
                                     ? null
                                     : DateTime.ParseExact(tradeRsp.ClearingBusinessDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    originalRecord.TrdExecDateTime = tradeRsp.TransactTime != DateTime.MinValue
                                    ? tradeRsp.TransactTime.ToLocalTime()
                                    : (!string.IsNullOrEmpty(tradeRsp.TradeDate) ? Convert.ToDateTime(tradeRsp.TradeDate) : DateTime.Now);
                    switch (tradeRsp.ExecType) {
                        case 'F':
                            originalRecord.TrdStatus = "Cleared";
                            break;
                        case '0':
                            originalRecord.TrdStatus = "New";
                            break;
                        case '4':
                            originalRecord.TrdStatus = "Canceled";
                            break;
                        case '6':
                            originalRecord.TrdStatus = "Pending Cancel";
                            break;
                        case '8':
                            originalRecord.TrdStatus = "Rejected";
                            break;
                        case 'A':
                            originalRecord.TrdStatus = "Pending New";
                            break;
                        case 'J':
                            originalRecord.TrdStatus = "Trade in a clearing hold";
                            originalRecord.ApprovalDateTime = tradeRsp.TransactTime.ToLocalTime();
                            break;
                        case 'K':
                            originalRecord.TrdStatus = "Trade has been released to clearing";
                            originalRecord.TradeRegistrationDateTime = tradeRsp.TransactTime.ToLocalTime();
                            break;
                        default:
                            break;
                    }
                    foreach (var rspSide in tradeRsp.Sides) {
                        if (rspSide.Side == '1') { //buy
                            originalRecord.BuyerAccount = rspSide.Account;
                            originalRecord.BuyerSecurID = rspSide.SideExecID;
                            originalRecord.BuyerInternalTrdID = rspSide.SideTradeReportID;
                            originalRecord.BuyerComment = rspSide.Text;
                            originalRecord.BuySource = rspSide.TradeInputSource;
                            foreach (var party in rspSide.Parties) {
                                switch (party.PartyRole) {
                                    case 1:
                                        originalRecord.BuyerTrdCom = PartyFullnameMapping[party.PartyID];
                                        break;
                                    case 4:
                                        originalRecord.BuyerCM = PartyFullnameMapping[party.PartyID];
                                        break;
                                    case 7:
                                        originalRecord.BuyerTradeRegistrant = PartyFullnameMapping[party.PartyID];
                                        break;
                                    case 12:
                                        originalRecord.BuyerTrader = party.PartyID;
                                        break;
                                    case 30:
                                        originalRecord.BuyerBroker = PartyFullnameMapping[party.PartyID];
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else if (rspSide.Side == '2') { //sell
                            originalRecord.SellerAccount = rspSide.Account;
                            originalRecord.SellerSecurID = rspSide.SideExecID;
                            originalRecord.SellerInternalTrdID = rspSide.SideTradeReportID;
                            originalRecord.SellerComment = rspSide.Text;
                            originalRecord.SellSource = rspSide.TradeInputSource;
                            foreach (var party in rspSide.Parties) {
                                switch (party.PartyRole) {
                                    case 1:
                                        originalRecord.SellerTrdCom = PartyFullnameMapping[party.PartyID];
                                        break;
                                    case 4:
                                        originalRecord.SellerCM = PartyFullnameMapping[party.PartyID];
                                        break;
                                    case 7:
                                        originalRecord.SellerTradeRegistrant = PartyFullnameMapping[party.PartyID];
                                        break;
                                    case 12:
                                        originalRecord.SellerTrader = party.PartyID;
                                        break;
                                    case 30:
                                        originalRecord.SellerBroker = PartyFullnameMapping[party.PartyID];
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    AllTradeData.Insert(0, originalRecord);
                }
            }
        }
        #endregion

        #region Display User's Filter Selections
        public ObservableCollection<FilterObj> FilterLabel { get; } = new();
        public Dictionary<string, ObservableCollection<FilterObj>> FilterDic { get; } = new();

        internal void AddDisplayLabel(string title, string value) {
            string displayLabel = PropertyColumnMapping[title];
            if (value.Length > 16)
            {
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

        #region export to csv: not working due to memory stream
        private void ExportToExcel(DataGrid dg) {
            DataGrid dataGrid = dg;
            if (dataGrid.Items.Count > 0) {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.FileName = "Trade Report_" + DateTime.Today.ToString("yyyyMMdd") + ".csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK) {
                    if (File.Exists(sfd.FileName)) {
                        try {
                            File.Delete(sfd.FileName);//override previous file(with same name)
                        }
                        catch (IOException ex) {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError) {
                        try {
                            dataGrid.SelectAllCells();
                            ApplicationCommands.Copy.Execute(null, dataGrid);
                            dataGrid.UnselectAllCells();
                            //string result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                            var result = Clipboard.GetData(DataFormats.CommaSeparatedValue).ToString();

                            int columnCount = dataGrid.Columns.Count;
                            string columnNames = "";
                            for (int i = 0; i < columnCount; i++) {
                                DockPanel header = dataGrid.Columns[i].Header as DockPanel;
                                foreach (var item in header.Children) {
                                    if (item is TextBlock tb && !string.IsNullOrEmpty(tb.Text)) {
                                        columnNames += tb.Text + ",";
                                        break;
                                    }
                                }
                            }

                            StreamWriter sw = new StreamWriter(sfd.FileName);
                            sw.WriteLine(columnNames);
                            sw.WriteLine(result);
                            sw.Close();

                            MessageBox.Show("Trade Data Exported Successfully !!!", "Info");

                            //int columnCount = dataGrid.Columns.Count;
                            //string columnNames = "";
                            //string[] outputCsv = new string[dataGrid.Items.Count + 1];
                            //for (int i = 0; i < columnCount; i++) {
                            //    DockPanel header = dataGrid.Columns[i].Header as DockPanel;
                            //    foreach (var item in header.Children) {
                            //        if (item is TextBlock tb && !string.IsNullOrEmpty(tb.Text)) {
                            //            columnNames += tb.Text + ",";
                            //            break;
                            //        }
                            //    }
                            //}
                            //outputCsv[0] += columnNames;

                            //for (int i = 0; i < dataGrid.Items.Count; i++) {
                            //    for (int j = 0; j < columnCount; j++) {
                            //        TextBlock value = dataGrid.Columns[j].GetCellContent(dataGrid.Items[i]) as TextBlock;
                            //        if (value != null) {
                            //            outputCsv[i + 1] += value.Text + ",";
                            //        }
                            //    }
                            //}

                            //File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                            //MessageBox.Show("Trade Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex) {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }
        #endregion

        public void Dispose() {
            OtcHelper.Client.OnPartyListRequest -= TO_OnPartyListRequest;
            OtcHelper.Client.OnSecurities -= TO_OnSecurities;
            OtcHelper.Client.OnTradeCaptureReport -= TO_OnTradeCaptureReport;
        }
    }
}
