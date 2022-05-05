using System.Collections.ObjectModel;

namespace ThemeViewer.Models {
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
