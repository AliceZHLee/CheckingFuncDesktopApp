using System.Collections.ObjectModel;

namespace ICEChecker.ModuleICEData.Models {
    public class TextFltOptions: ObservableCollection<string> {     
        public TextFltOptions() {
            Add("Contains");
            Add("Equals");
            Add("Starts With");
            Add("Ends With");
            Add("Does Not Contain");
        }
    }
}
