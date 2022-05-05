using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICEFixAdapter.Models {
    public class Execution {
        public decimal LastShares { get; set; }
        public string ExecID { get; set; }
        public decimal LastPx { get; set; }
        public string TradeID { get; set; }
        public int ClientAppType { get; set; }
    }
}
