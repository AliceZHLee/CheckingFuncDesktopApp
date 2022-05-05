//-----------------------------------------------------------------------------
// File Name   : IceLeg
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ICEFixAdapter.Models {
    public class IceLeg {
        public int LegSymbol { get; set; }
        public string LegSecurityID { get; set; }
        public string LegSecurityIDSource { get; set; }
        public string LegSecurityExchange { get; set; }
        public string LegCFICode { get; set; }
        public string LegSecurityType { get; set; }
        public char LegSide { get; set; }
        public decimal LegPrice { get; set; }
        public decimal LegLastPx { get; set; }
        public decimal LegParPx { get; set; }
        public decimal LegQty { get; set; }
        public int LegRefID { get; set; }
        public float LinkExecID { get; set; }
        public float LegNumOfLots { get; set; }
        public int LegNumOfCycles { get; set; }
        public string LegStartDate { get; set; }
        public string LegEndDate { get; set; }
        public decimal LegStrikePrice { get; set; }
        public string LegSecuritySubType { get; set; }
        public int LegOptionDelta { get; set; }
        public int LegOptionSymbol { get; set; }
        public string LegOptionSymbolName { get; set; }
        public int LegRatioQtyDenominator { get; set; }
        public int LegRatioQtyNumerator { get; set; }
        public int LegRatioPriceDenominator { get; set; }
        public int LegRatioPriceNumerator { get; set; }
        public string LegComplianceID { get; set; }
        public string LegMemoField { get; set; }
        public string LegCustOrderHandlingInst { get; set; }

        public List<IceParty> NestedParties = new List<IceParty>();

        public string Product { get; set; }
        public string Strip { get; set; }
        public string LegB_S { get; set; }
        public double TotalQty { get; set; }
    }
}
