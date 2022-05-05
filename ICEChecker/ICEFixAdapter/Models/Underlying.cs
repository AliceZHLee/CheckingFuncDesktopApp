//-----------------------------------------------------------------------------
// File Name   :eUnderlying
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
    public class Underlying {
        //the idea is when receiveing sequenced data, do not directly process and convert to corresponding type,
        //first receive, then process at viewmodel level

        public int UnderlyingSymbol { get; set; }
        //public string UnderlyingSymbol { get; set; }
        public string UnderlyingSecurityID { get; set; }
        public int UnderlyingSecurityIDSource { get; set; }

        public List<UnderlyingSecurityAlt> UnderlyingSecurityAltIDs { get; } = new List<UnderlyingSecurityAlt>();

        public string UnderlyingCFICode { get; set; }
        public string UnderlyingSecuritySubType { get; set; }
        public string UnderlyingSecurityDesc { get; set; }
        public string UnderlyingSecurityExchange { get; set; }
        //public DateTime UnderlyingMaturityDate { get; set; }
        public string UnderlyingMaturityDate { get; set; }
        public decimal UnderlyingCouponRate { get; set; }
        public decimal UnderlyingContractMultiplier { get; set; }
        public string UnderlyingDatedDate { get; set; }
        public string UnderlyingInterestAccrualDate { get; set; }
        public string UnderlyingIssueDate { get; set; }
        public decimal UnderlyingRepurchaseRate { get; set; }
        public decimal UnderlyingFactor { get; set; }
        public string UnderlyingCreditRating { get; set; }
        public string UnderingInstrRegistry { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string UnderlyingUnitOfMeasure { get; set; }
        public int UnderlyingPutOrCall { get; set; }
        public decimal UnderlyingStrikePrice { get; set; }
        public int SecurityTradingStatus { get; set; }
        public string UnderlyingCurrency { get; set; }
        public char UnderlyingSettlMethod { get; set; }
        public int StrikeExerciseStyle { get; set; }
        public decimal IncrementQty { get; set; }
        public decimal IncrementPrice { get; set; }
        public decimal ScreenTickValue { get; set; }
        public decimal TickValue { get; set; }
        public decimal LegacyTickValue { get; set; }
        public decimal BlockTickValue { get; set; }
        public decimal OffExchangeIncrementPrice { get; set; }
        public decimal OffExchangeIncrementQty { get; set; }
        public decimal LotSize { get; set; }
        public string ProductName { get; set; }
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
        public int AONAllowed { get; set; }
        public int HubID { get; set; }
        public char ImpliedType { get; set; }
        public decimal NumOfDecimalPrice { get; set; }
        public decimal NumOfDecimalQty { get; set; }
        public decimal NumOfDecimalStrikePrice { get; set; }
        public decimal LotSizeMultiplier { get; set; }
        public decimal IncrementStrike { get; set; }
        public decimal MinStrike { get; set; }
        public decimal MaxStrike { get; set; }
        public decimal UnderlyingAccruedPremiumAmt { get; set; }
        public decimal UnderlyingEventPaymentAmt { get; set; }
        public decimal UnderlyingAlignmentInterestRate { get; set; }
        public decimal UnderlyingInterpolationFactor { get; set; }
        public string Granularity { get; set; }
        public string HomeExchange { get; set; }
        public string ClearedAlias { get; set; }
        public string ProductType { get; set; }
        public string PriceDenomination { get; set; }
        public string PriceUnit { get; set; }
        public string HubName { get; set; }
        public string HubAlias { get; set; }
        public string PhysicalCode { get; set; }
        public string UnderlyingRepurchaseDate { get; set; }
        public string ProductGroup { get; set; }
        public int BaseNumLots { get; set; }
        public int IsDividendAdjusted { get; set; }
        public int OverrideBlockMin { get; set; }
        public int TestMarketIndicator { get; set; }
        public int UDSAllowed { get; set; }
        public int MarketTransparencyType { get; set; }
        public int NonCommoditizedMarket { get; set; }
        public List<BlockDetail> BlockDetails { get; } = new List<BlockDetail>();
    }
}
