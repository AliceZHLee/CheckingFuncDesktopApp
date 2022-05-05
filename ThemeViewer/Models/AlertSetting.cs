namespace ThemeViewer.Models {
    public class AlertSetting {
        public bool IsAlertOn { get; set; }
        public string Threshold { get; set; }
        public bool IsOursExcluded { get; set; }//true=disable ours
    }
}
