using System.Collections.ObjectModel;

namespace ICEChecker.LogonWindow.Models {
    public class UserOptions : ObservableCollection<string> {
        public UserOptions() {
            Add("Contains");
            Add("Equals");
            Add("Starts With");
            Add("Ends With");
            Add("Does Not Contain");
        }
    }
}
