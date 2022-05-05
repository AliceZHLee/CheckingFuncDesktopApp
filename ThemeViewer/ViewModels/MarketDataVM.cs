using FixAdapter.Models.Response;
using NAudio.Wave;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using ThemeViewer.Models;
using ThemeViewer.Views;

namespace ThemeViewer.ViewModels {
    public class MarketDataVM : BindableBase, IDisposable {
        public Dictionary<string, string> MarketTypeMapping { get; } = new Dictionary<string, string> {//key:product,value=instrument
            ["ZAPN"] = "Equities",
            ["ZAXS"] = "Equities",
            ["ZBAF"] = "Equities",
            ["ZBHA"] = "Equities",
            ["ACF"] = "Commodities",
            ["M42F"] = "Commodities",
            ["EF"] = "Commodities",
            ["CWF"] = "Commodities",
            ["HWF"] = "Commodities",
            ["PVF"] = "Commodities",
            ["PWF"] = "Commodities",
            ["SWF"] = "Commodities",
            ["FNAXJ"] = "Equities",
            ["CN"] = "Equities",
            ["FNMY"] = "Equities",
            ["TWN"] = "Equities",
            ["FNTH"] = "Equities",
            ["ZHCL"] = "Equities",
            ["ZHDB"] = "Equities",
            ["ZHND"] = "Equities",
            ["ZHDF"] = "Equities",
            ["ZICI"] = "Equities",
            ["ZIHF"] = "Equities",
            ["ZINF"] = "Equities",
            ["IU"] = "FX",
            ["M65F"] = "Commodities",
            ["LPF"] = "Commodities",
            ["FEF"] = "Commodities",
            ["FE"] = "Commodities",
            ["ZKMB"] = "Equities",
            ["KU"] = "FX",
            ["ZMSI"] = "Equities",
            ["IN"] = "Equities",
            ["INB"] = "Equities",
            ["NK"] = "Equities",
            ["ND"] = "Equities",
            ["GO"] = "Commodities",
            ["MF5F"] = "Commodities",
            ["BZF"] = "Commodities",
            ["BZ"] = "Commodities",
            ["BZNF"] = "Commodities",
            ["BZN"] = "Commodities",
            ["MEG"] = "Commodities",
            ["SMC"] = "Commodities",
            ["PXN"] = "Commodities",
            ["PXF"] = "Commodities",
            ["PX"] = "Commodities",
            ["ZRIL"] = "Equities",
            ["TF"] = "Commodities",
            ["FNEMK"] = "Equities",
            ["EJRT"] = "Equities",
            ["ZSBI"] = "Equities",
            ["ZTCS"] = "Equities",
            ["ZTAT"] = "Equities",
            ["ZTEC"] = "Equities",
            ["ZUTC"] = "Equities",
            ["UC"] = "FX",
            ["ZWPR"] = "Equities",
            ["EJP"] = "Equities",
            ["EJRT"] = "Equities"
        };
        public Dictionary<string, string> PropertyColumnMapping { get; } = new Dictionary<string, string> {
            ["Market"] = "Market",
            ["Instrument"] = "Instrument",
            ["Product"] = "Product",
            ["Contract"] = "Contract",
            ["ComboType"] = "Combo Type",
            ["DealID"] = "Deal ID",
            ["Legs"] = "Legs",
            ["Price"] = "Price/Premium",
            ["QtyInLots"] = "Quantity (Lots)",
            ["Strike"] = "Strike Price",
            ["Amount"] = "Amount",
            ["Unit"] = "Unit",
            ["ExecutedSGT"] = "Executed (SGT)",
            ["SubmittedSGT"] = "Submitted (SGT)",
            ["ClearedSGT"] = "Cleared (SGT)",
            ["Session"] = "Session",
            ["DeletedSGT"] = "Deleted (SGT)",
            ["ClearedDate"] = "Cleared Date",
            ["Status"] = "Status"
        };
        public Dictionary<string, int> MonthCodeMapping { get; } = new Dictionary<string, int> {
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

        private readonly object _lock = new();
        public ObservableCollection<MarketRecord> AllMarketData { get; } = new();
        public CollectionViewSource MarketDataViewS { get; } = new();
        public List<FilterObj> MarketColumns { get; } = new();
        public int QtyInLotsThreshold { get; set; }
        public bool ThresholdOn { get; set; }
        public bool AlertExcludeOurTrds { get; set; }

        MarketRecord lastRecord = new MarketRecord();
        public HashSet<string> receivedCalendarContract = new HashSet<string>();

        private readonly IEventAggregator _ea;

        //public string SelectAllToolTip { get; } = "This option will select all values when apply the filter";

        #region constructor
        public MarketDataVM(IEventAggregator eventAggregator) {
            _ea = eventAggregator;
            _ea.GetEvent<AlertSettingUpdateEvent>().Subscribe(UpdateAlertSetting);

            BindingOperations.EnableCollectionSynchronization(AllMarketData, _lock);
            MarketDataViewS.Source = AllMarketData;

            foreach (var column in PropertyColumnMapping.Values) {
                MarketColumns.Add(new FilterObj() { Label = column.ToString(), IsChecked = true });
            }

            var alertHistorySetting = ConfigHelper.LoadAlertSetting(@"config\config.json");
            ThresholdOn = (bool)alertHistorySetting["AlertOn"] == null ? true : (bool)alertHistorySetting["AlertOn"];
            QtyInLotsThreshold = (int)alertHistorySetting["Threshold"];

            OtcHelper.Client.OnMarketDataSnapshot += Client_OnMarketDataSnapshot;
            OtcHelper.Client.OnMarketDataIncremental += Client_OnMarketDataIncremental;
        }
        #endregion

        #region save market tab sound alert setting
        public void UpdateAlertSetting(AlertSetting setting) {
            if (setting.Threshold.Length >= 2 && setting.Threshold.StartsWith("0")) {
                throw new Exception("Input string was not in a correct format.");
            }
            int result;
            if (int.TryParse(setting.Threshold, out result)) {
                QtyInLotsThreshold = result;
            }
            else {
                throw new Exception("Input string was not in a correct format.");
            }
            ThresholdOn = setting.IsAlertOn;
            AlertExcludeOurTrds = setting.IsOursExcluded;

            ConfigHelper.UpdateAlertSetting(@"config\config.json", ThresholdOn, QtyInLotsThreshold,AlertExcludeOurTrds);
        }
        #endregion

        #region Titan OTC API (data receive)
        private void Client_OnMarketDataSnapshot(ThmMarketDataSnapshotRsp mdRsp) {
            mdRsp.MDEntries.ForEach(md => {
                md.Symbol = mdRsp.Symbol;
                Update(md);
            });
        }

        private void Client_OnMarketDataIncremental(ThmMarketDataIncrementalRsp mdRsp) {
            mdRsp.MDEntries.ForEach(x => Update(x));
        }

        private void Update(ThmMarketDataEntry entryData) {
            List<string> selectedProducts = SelectProductWin.SelectedProducts;

            bool isRightProduct = false;
            foreach (var selectedProduct in selectedProducts) {
                if (entryData.Symbol.StartsWith(selectedProduct)) {
                    isRightProduct = true;
                    break;
                }
            }
            if (!isRightProduct) {
                return;
            }

            var mr = new MarketRecord() {
                Instrument = entryData.Symbol,
                Price = entryData.MDEntryPx,
                ClearedDate = DateTime.ParseExact(entryData.SettlDate, "yyyyMMdd", CultureInfo.InvariantCulture),
                QtyInLots = entryData.MDEntrySize,
                Amount = (int)(entryData.MDEntrySize * 100),
                Unit = "MT",
                Session = entryData.TradingSessionSubID == "3" ? "T" : "T+1",
                ClearedSGT = entryData.MDEntryDate.Date.Add(entryData.MDEntryTime.TimeOfDay).ToLocalTime(),
                NewlyReceived = true
            };

            string instrument = mr.Instrument;
            if (instrument.Contains("_")) { //option
                int underscoreIndex = instrument.IndexOf("_");
                string optionDirection = instrument.Substring(underscoreIndex + 1, 1);// C:call, P: put
                mr.Strike = Convert.ToDecimal(instrument.Substring(underscoreIndex + 2, instrument.Length - underscoreIndex - 2));
                mr.Contract = ConvertContractToDisplayMode(instrument.Substring(underscoreIndex - 3, 3));
                mr.Product = instrument.Substring(0, underscoreIndex - 3);
                mr.Market = MarketTypeMapping.GetValueOrDefault(mr.Product, "Equities");
            }
            else {//without "_"
                mr.Contract = ConvertContractToDisplayMode(instrument.Substring(instrument.Length - 3, 3));
                mr.Product = instrument.Substring(0, instrument.Length - 3);
                mr.Market = MarketTypeMapping.GetValueOrDefault(mr.Product, "Equities");
            }

            //combo type, leg
            //current test result is all test result is outright contract, and above code also just retrieve 3-digit from instrument as contract name
            //therefore, to ensure the logic is not following the above "3-digit for contract", I make the code independently process the instrument for the "product+contract" part
            //if test result with below situations found, the above code's logic needs to be modified 
            string productContract = instrument.Split("_")[0];
            if (productContract.Contains("-")) {//spread contract: first contract-second contract
                string[] spreadContracts = productContract.Split("-");
                string firstContract = spreadContracts[0].Replace(mr.Product, "");
                string secondContract = spreadContracts[1].Replace(mr.Product, "");
                if (firstContract.Length == secondContract.Length) {//month spread
                    mr.Legs = (Convert.ToInt32(secondContract.Substring(1, 2)) - Convert.ToInt32(firstContract.Substring(1, 2))) * 12 +
                              MonthCodeMapping[secondContract.Substring(0, 1)] - MonthCodeMapping[firstContract.Substring(0, 1)] + 1;
                    mr.ComboType = "M-o-M Spread";
                }
                else if (firstContract.Length > secondContract.Length) {
                    if (firstContract.Contains("3M")) {//quarter strip spread
                        mr.Legs = MonthCodeMapping[secondContract.Substring(0, 1)] - MonthCodeMapping[firstContract.Substring(0, 1)] + 3
                            + (Convert.ToInt32(secondContract.Substring(1, 2)) - Convert.ToInt32(firstContract.Substring(1, 2))) * 12;
                        mr.ComboType = "Q-o-Q Spread";
                    }
                    else if (firstContract.Contains("12M")) { //calendar strip spread
                        mr.Legs = (Convert.ToInt32(secondContract.Substring(1, 2)) - Convert.ToInt32(firstContract.Substring(1, 2)) + 1) * 12;
                        mr.ComboType = "Y-o-Y Spread";
                    }
                }
            }
            else if (productContract.Length == mr.Product.Length + 3) { //1-digit contract month + 2-digit contract year
                mr.ComboType = "Outright";
                mr.Legs = 1;
            }
            else if (productContract.Length == mr.Product.Length + 6) {// 6-digit DDMMYY, underlying ticker may not be 2 - digit
                mr.ComboType = "Flex Futures";
            }
            else if (productContract.Length == mr.Product.Length + 8 || productContract.Length == mr.Product.Length + 7) {// 2-digit SA/SB + 2/3-digit No. of months in strip(3M/12M) + 3-digit MYY(1st Leg Month)
                if (productContract.Contains("3M")) {
                    mr.ComboType = "Quarter Strip";
                    mr.Legs = 3;
                }
                else if (productContract.Contains("12M")) {
                    mr.ComboType = "Cal Strip";
                    mr.Legs = 12;
                }
            }

            if (entryData.MDUpdateAction == null) {//snapshot
                mr.ExecutedSGT = entryData.TransactTime.ToLocalTime();
                if (entryData.SettlDate != null) {
                    mr.Status = "Cleared";
                }
            }
            if (entryData.MDUpdateAction != null) {//incremental refresh
                if (entryData.MDUpdateAction == '1') {
                    mr.Status = "Cleared";
                    mr.ExecutedSGT = entryData.TransactTime.ToLocalTime();
                }
                else if (entryData.MDUpdateAction == '2') {
                    mr.Status = "Deleted";
                    mr.DeletedSGT = entryData.TransactTime.ToLocalTime();
                }
            }
            _ea.GetEvent<NewMarketDataEvent>().Publish(mr);
            AllMarketData.Insert(0, mr);

            #region sound alert
            //sound alert when over the qty in lot, and options will be no alert
            if (mr.Status == "Cleared" && !instrument.Contains("_") && ThresholdOn && mr.QtyInLots >= QtyInLotsThreshold) {
                if (!AlertExcludeOurTrds || !mr.IsProductRelated) {//user can decide whether to sound alert for internal trds in setting
                    PlaySound(@"config\MarketAlert_QtyThreshold.mp3");
                    AIVoiceOutAlert(mr);
                    Debug.WriteLine("Lot alert:{0} times", ++times);
                }
            }

            //sound alert for Y-O-Y and Cal strip deal      
            if (string.IsNullOrEmpty(lastRecord.Contract)) {
                lastRecord = mr;
                receivedCalendarContract.Add(mr.Contract);
            }
            else {
                if (mr.Status == "Cleared") {
                    int year = Convert.ToInt32(mr.Contract.Substring(4, 4));
                    int lastRecordYear = Convert.ToInt32(lastRecord.Contract.Substring(4, 4));

                    if (lastRecord.QtyInLots == mr.QtyInLots &&
                        DateTime.Compare((DateTime)lastRecord.ClearedSGT, (DateTime)mr.ClearedSGT) == 0 &&
                        decimal.Compare(lastRecord.Price, mr.Price) == 0 &&
                        DateTime.Compare((DateTime)lastRecord.ExecutedSGT, (DateTime)mr.ExecutedSGT) == 0 &&
                        lastRecord.Session == mr.Session) {

                        if (year == lastRecordYear) {
                            if (receivedCalendarContract.Contains(mr.Contract)) {
                                receivedCalendarContract.Clear();
                                receivedCalendarContract.Add(mr.Contract);
                            }
                            else {
                                receivedCalendarContract.Add(mr.Contract);
                            }
                        }
                        else {
                            receivedCalendarContract.Clear();
                            receivedCalendarContract.Add(mr.Contract);
                        }

                        //cal strip: 12 months from Jan to Dec
                        //Y-o-Y spread: 24 months for 2 continuous year
                        if (receivedCalendarContract.Count == 12) {//Cal Strip | Y-o-Y Spread
                            PlaySound(@"config\CalStripYoYSpreadAlert.mp3");
                            receivedCalendarContract.Clear();
                            Debug.WriteLine("Annual alert: {0} times", ++annualTimes);
                        }
                    }
                    lastRecord = null;
                    lastRecord = mr;
                }
            }
            #endregion
        }

        int times = 0;
        int annualTimes = 0;
        #endregion

        private void PlaySound(string musicFileName) {
            try {
                using (var ms = File.OpenRead(musicFileName))
                using (var rdr = new Mp3FileReader(ms))
                using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
                using (var baStream = new BlockAlignReductionStream(wavStream))
                using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback())) {
                    waveOut.Init(baStream);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing) {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        #region AI speech alert(read the record details)
        private void AIVoiceOutAlert(MarketRecord mr) {
            try {
                PromptBuilder promptBuilder = new PromptBuilder();
                PromptStyle promptStyle = new PromptStyle();
                promptStyle.Rate = PromptRate.Fast;

                promptBuilder.StartStyle(promptStyle);
                promptBuilder.AppendTextWithHint(mr.Contract, SayAs.MonthYear);
                promptBuilder.EndStyle();

                promptBuilder.AppendText(", cleared ");
                promptBuilder.StartStyle(promptStyle);
                promptBuilder.AppendTextWithHint(((int)mr.QtyInLots).ToString(), SayAs.NumberCardinal);
                promptBuilder.EndStyle();

                promptBuilder.AppendText("lots, price ");
                promptBuilder.StartStyle(promptStyle);
                promptBuilder.AppendText(mr.Price.ToString("C2"));
                promptBuilder.EndStyle();

                SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                speechSynthesizer.Speak(promptBuilder);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        public string ConvertContractToDisplayMode(string contract) {
            string contractToDisplay = "";
            string month = contract.Substring(0, 1);
            switch (month) {
                case "F":
                    contractToDisplay += "Jan ";
                    break;
                case "G":
                    contractToDisplay += "Feb ";
                    break;
                case "H":
                    contractToDisplay += "Mar ";
                    break;
                case "J":
                    contractToDisplay += "Apr ";
                    break;
                case "K":
                    contractToDisplay += "May ";
                    break;
                case "M":
                    contractToDisplay += "Jun ";
                    break;
                case "N":
                    contractToDisplay += "Jul ";
                    break;
                case "Q":
                    contractToDisplay += "Aug ";
                    break;
                case "U":
                    contractToDisplay += "Sep ";
                    break;
                case "V":
                    contractToDisplay += "Oct ";
                    break;
                case "X":
                    contractToDisplay += "Nov ";
                    break;
                case "Z":
                    contractToDisplay += "Dec ";
                    break;
                default: break;
            }
            contractToDisplay += "20" + contract.Substring(1, 2);
            return contractToDisplay;
        }

        #region Display User's Filter Selections
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

        public void Dispose() {
            OtcHelper.Client.OnMarketDataSnapshot -= Client_OnMarketDataSnapshot;
            OtcHelper.Client.OnMarketDataIncremental -= Client_OnMarketDataIncremental;
        }
    }
}
