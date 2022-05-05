//-----------------------------------------------------------------------------
// File Name   : IceSecurityDefinitionRsp
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceSecurityDefinitionRsp {//35=d
        public string SecurityResponseID { get; set; }
        public int SecurityResponseType { get; set; }//Supported value(s):
                                                    //4 = List of securities returned per request
                                                    //5 = Security proposal is rejected
                                                    //6 = Cannot match selection criteria
        public string SecurityReqID { get; set; }
        public string MarketTypeID { get; set; }//mapped to message's SecurityID(TAG 48)
        public int Symbol { get; set; }
        public string ExchangeSilo { get; set; }//Indicates the Exchange Silo of the Market.
                                            //Supported value(s):
                                            //0 = ICE
                                            //1 = ENDEX
                                            //2 = LIFFE

        //public string SecurityType { get; set; }

        public int TotalNumSecurity { get; set; }
        public int NoRpts { get; set; }
        public int ListSeqNo { get; set; }
        public string Text { get; set; }
        public List<Underlying> Underlyings { get; }=new List<Underlying>();
        //public string ProductComplex { get; set; }

        //public List<IceLeg> Legs { get; } = new List<IceLeg>();
        //public List<IceMarketSegment> MarketSegments { get; } = new List<IceMarketSegment>();
    }
}
