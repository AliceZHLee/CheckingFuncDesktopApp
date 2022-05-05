using System;
using System.Collections.ObjectModel;

namespace ICEChecker.ModuleCheckerOutput.Models {
    public class TimeFltOptions : ObservableCollection<string> {
        public TimeFltOptions() {
            DateTime start = new DateTime();
            int startDay = start.AddDays(1).Day;
            do {
                Add(start.ToString("HH:mm"));
                start = start.AddMinutes(15);
            } while (start.Day != startDay);
        }
    }
}
