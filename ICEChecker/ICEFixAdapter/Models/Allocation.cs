using System;
using System.Collections.Generic;

namespace ICEFixAdapter.Models {
    public class Allocation {
        public string AllocAccount { get; set; }
        public decimal AllocShares { get; set; }
        public decimal AllocPrice { get; set; }
        public int SecondaryIndividualAllocID { get; set; }
        public char IndividualAllocType { get; set; }
        public int AllocCustomerCapacity { get; set; }
        public char AllocPositionEffect { get; set; }
        public string AllocText { get; set; }
        public string SettlementAccountCode { get; set; }
        public DateTime AcceptanceTime { get; set; }
        public List<AllocInfo> AllocInfos { get; } = new List<AllocInfo>();
        public List<IceParty> NestedPartys { get; } = new List<IceParty>();
    }
}
