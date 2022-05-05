//-----------------------------------------------------------------------------
// File Name   : IceParty
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace ICEFixAdapter.Models {
    public class IceParty {
        public string PartyID { get; set; } // 448
        public char PartyIDSource { get; set; } //447
        public int PartyRole { get; set; } // 452

        // NoPartySubIDs: 802
        public List<IcePartySub> PartySubs { get; } = new List<IcePartySub>();
        public List<IceRelatedParty> RelatedParties { get; } = new List<IceRelatedParty>();
        public string Text { get; set; }
    }

    public class IcePartySub {
        public string PartySubID { get; set; } // 523
        public int PartySubIDType { get; set; } // 803
    }

    public class IceRelatedParty {
        public string RelatedPartyID { get; set; } // 1563
        public char? RelatedPartyIDSource { get; set; } // 1564
        public int RelatedPartyRole { get; set; } //1565

        public List<IceRelatedSubParty> RelatedSubParties { get; } = new List<IceRelatedSubParty>();
        public List<IcePartyRelationShip> PartyRelationShips { get; } = new List<IcePartyRelationShip>();
    }

    public class IceRelatedSubParty {
        public string RelatedPartySubID { get; set; }
        public int RelatedPartySubIDType { get; set; }
    }

    public class IcePartyRelationShip {
        public int? PartyRelationship { get; set; }
    }

    // -----------
    public class IceTargetParty {
        public string TargetPartyID { get; set; } // 1462
        public char? TargetPartyIDSource { get; set; } // 1463
        public int TargetPartyRole { get; set; }  // // 1464

        public List<IceTargetPartySub> TargetPartySubs { get; } = new List<IceTargetPartySub>();
    }

    // -----------
    public class IceTargetPartySub {
        public string TargetPartySubID { get; set; } // 2434        
        public int TargetPartySubIDType { get; set; }  // 2435
    }

    public class IceValueCheck {
        public int ValueCheckType { get; set; }    // 1869
        public int ValueCheckAction { get; set; }  // 1870
    }
}
