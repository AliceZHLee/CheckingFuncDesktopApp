using System;
using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceDefinedStrategyRsp {//35=UDS
        public string SecurityResponseID { get; set; }
        public int SecurityResponseType { get; set; }//1 = Accept security proposal as is
                                                     //4 = List of securities returned per request
                                                     //5 = Security proposal is rejected
                                                     //10 = Security proposal already defined
        public string SecurityReqID { get; set; }
        public string ExchangeSilo { get; set; }//Indicates the Exchange Silo of the Market.
                                                //Supported value(s):
                                                //0 = ICE
                                                //1 = ENDEX
                                                //2 = LIFFE
        public string SecurityID { get; set; }
        public string SecurityType { get; set; }
        public string SecurityIDSource { get; set; }
        public string SecurityDesc { get; set; }
        public int SecurityTradingStatus { get; set; }
        public string SecurityExchange { get; set; }
        public string StrategySecurityID { get; set; }
        public string SecuritySubType { get; set; }
        public string UnitOfMeasure { get; set; }
        public int Symbol { get; set; }//not string, is UDS Market ID
        public int UnderlyingStrategySymbol { get; set; }//Market ID of underlying strategy
        public DateTime TransactTime { get; set; }
        public string ProductName { get; set; }
        public int MarketTypeID { get; set; }
        public string MaturityDate { get; set; }
        public List<BlockDetail> BlockDetails { get; }=new List<BlockDetail>(){ };
        public decimal ScreenTickValue { get; set; }
        public decimal BlockTickValue { get; set; }
        public List<UnderlyingSecurityAlt> UnderlyingSecurityAltIDs { get; } = new List<UnderlyingSecurityAlt>();
        public List<IceLeg> Legs { get; }=new List<IceLeg>();
        public int StrikeExerciseStyle { get; set; }
        public decimal IncrementQty { get; set; }
        public decimal IncrementPrice { get; set; }
        public decimal TickValue { get; set; }
        public decimal LegacyTickValue { get; set; }
        public decimal OffExchangeIncrementPrice { get; set; }
        public decimal OffExchangeIncrementQty { get; set; }
        public decimal LotSize { get; set; }
        public string ProductDesc { get; set; }
        public int ProductID { get; set; }
        public bool Clearable { get; set; }
        public int HedgeProductID { get; set; }
        public int HedgeMarketID { get; set; }
        public int HedgeOnly { get; set; }
        public int Denominator { get; set; }
        public int InitialMargin { get; set; }
        public int PrimaryLegSymbol { get; set; }
        public int SecondaryLegSymbol { get; set; }
        public int StripType { get; set; }
        public int StripID { get; set; }
        public string StripName { get; set; }
        public int BlockOnly { get; set; }
        public int FlexAllowed { get; set; }
        public int GTAllowed { get; set; }
        public int MiFIDRegulatedMarket { get; set; }
        public int LegDealsSuppressed { get; set; }
        public int AONAllowed { get; set; }
        public int HubID { get; set; }
        public char ImpliedType { get; set; }
        public decimal NumOfDecimalPrice { get; set; }
        public decimal NumOfDecimalQty { get; set; }
        public decimal NumOfDecimalStrikePrice { get; set; }
        public int NumberOfCycles { get; set; }
        public decimal LotSizeMultiplier { get; set; }
        public decimal IncrementStrike { get; set; }
        public decimal MinStrike { get; set; }
        public decimal MaxStrike { get; set; }
        public string Granularity { get; set; }
        public string HomeExchange { get; set; }
        public string ClearedAlias { get; set; }
        public string ProductType { get; set; }
        public string PriceDenomination { get; set; }
        public string PriceUnit { get; set; }
        public string HubName { get; set; }
        public string HubAlias { get; set; }
        public string PhysicalCode { get; set; }
        public string ProductGroup { get; set; }
        public int BaseNumLots { get; set; }
        public int IsDividendAdjusted { get; set; }
        public int OverrideBlockMin { get; set; }
        public int TestMarketIndicator { get; set; }
        public int UDSAllowed { get; set; }
        public int MarketTransparencyType { get; set; }
        public int NonCommoditizedMarket { get; set; }
        public string Text { get; set; }
    }
}
