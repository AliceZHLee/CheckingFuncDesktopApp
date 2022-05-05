//-----------------------------------------------------------------------------
// File Name   : ThmParty
// Author      : junlei
// Date        : 6/10/2020 6:22:52 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace FixAdapter.Models {
    public class ThmParty {
        public string PartyID { get; set; } // 448
        public char? PartyIDSource { get; set; } //447
        public int PartyRole { get; set; } // 452

        // NoPartySubIDs: 802
        public List<ThmPartySub> PartySubs { get; } = new List<ThmPartySub>();
        public List<ThmRelatedParty> RelatedParties { get; } = new List<ThmRelatedParty>();
    }

    public class ThmPartySub {
        public string PartySubID { get; set; } // 523
        public int PartySubIDType { get; set; } // 803
    }

    public class ThmRelatedParty {
        public string RelatedPartyID { get; set; } // 1563
        public char? RelatedPartyIDSource { get; set; } // 1564
        public int RelatedPartyRole { get; set; } //1565

        public List<ThmRelatedSubParty> RelatedSubParties { get; } = new List<ThmRelatedSubParty>();
        public List<ThmPartyRelationShip> PartyRelationShips { get; } = new List<ThmPartyRelationShip>();
    }

    public class ThmRelatedSubParty {
        public string RelatedPartySubID { get; set; }
        public int RelatedPartySubIDType { get; set; }
    }

    public class ThmPartyRelationShip {
        public int? PartyRelationship { get; set; }
    }

    // -----------
    public class ThmTargetParty {
        public string TargetPartyID { get; set; } // 1462
        public char? TargetPartyIDSource { get; set; } // 1463
        public int TargetPartyRole { get; set; }  // // 1464

        public List<ThmTargetPartySub> TargetPartySubs { get; } = new List<ThmTargetPartySub>();
    }

    // -----------
    public class ThmTargetPartySub {
        public string TargetPartySubID { get; set; } // 2434        
        public int TargetPartySubIDType { get; set; }  // 2435
    }

    public class ThmValueCheck {
        public int ValueCheckType { get; set; }    // 1869
        public int ValueCheckAction { get; set; }  // 1870
    }
}
