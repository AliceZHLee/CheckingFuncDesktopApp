using Prism.Mvvm;
using System;

namespace ICEChecker.ModuleCheckerOutput.Models {
    public class CheckerOutput:BindableBase {
        //Con Mon Price Qty Shop Time    P* Q Sprd? Full?	Bro? Remark  Con Mon Qty B_Leg   S_Leg B_Q S_Q
        public string Trader { get; set; }
        public DateTime? TradeDate { get; set; }
        public string Contract { get; set; }
        public string Month { get; set; }
        public double? Price { get; set; }
        public double? Qty { get; set; }
        public string Shop { get; set; }
        public DateTime? Time { get; set; }
        private string _trdStatus;
        public string TrdStatus {
            get => _trdStatus;
            set => SetProperty(ref _trdStatus, value); 
        }
        public string BRK { get; set; }

        //inspector for which API record is mapped to the selected checked output
        //public bool IsSelected { get; set; }

        public bool IsConsistent { get; set; }

    }
}
