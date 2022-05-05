//-----------------------------------------------------------------------------
// File Name   : IceTradeCaptureReport
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceTradeCaptureReportRsp {
        //Y field
        public string TradeReportID { get; set; }
        public int TradeReportTransType { get; set; }
        public string ExecID { get; set; }//deal id of the trade
        public bool PreviouslyReported { get; set; }
        public int Symbol { get; set; }//market ID
        //public string Symbol { get; set; }//market ID
        public string CFICode { get; set; }
        public decimal LastQty { get; set; }
        public decimal LastPx { get; set; }
        public int NumberOfCycles { get; set; }//number of days in a flow contract
        public string TradeDate { get; set; }
        public DateTime TransactTime { get; set; }
        public int ClientAppType { get; set; }//Used to indicate the source application on the request. Can be 
                                              // used to differentiate WebICE deals from FIXOS deals.
                                              // Supported value(s):
                                              // 0 = WebICE
                                              // 1 = FIX OS
                                              // 2 = FIXml
                                              // 3 = ICEBlock
                                              // 4 = Other
                                              // 5 = FPML
                                              // 6 = UPS
                                              // 7 = Mobile
                                              // 8 = FIX POF
                                              // 9 = YJ ISV
                                              // 11 = IOA
        public List<IceTradeSide> Sides { get; } = new List<IceTradeSide>();//count always 1


        // C field
        public char ExecType { get; set; }//Please note that ExecType (Tag 150) will not be sent on snapshot Trade Capture Reports.Duplicate Trade Capture
                                          //Reports may be sent with ExecType(Tag 150) = ‘5’ to indicate a change on either side of the trade.
                                          //Supported value(s): 5 = Replace,F = Trade(partial fill or fill),H = Trade Cancel
        public string SecurityIDSource { get; set; }
        public decimal StrikePrice { get; set; }

        //Tag 8013,9700,9701,9702,9703,9704,9705,9706,9707,9036 are for MiFID(Europe market), will not be used for our oil S2F(America market)

        public int DealAdjustIndicator { get; set; }//0=No,1=Yes


        // O field
        public int TradeReportType { get; set; }
        public string TradeRequestID { get; set; }
        public string TrdType { get; set; } //0 = Regular Trade
                                            //2 = ICE EFRP
                                            //3 = ICEBLK
                                            //4 = Basis Trade
                                            //5 = Guaranteed Cross
                                            //6 = Volatility Contingent Trade
                                            //7 = Stock Contingent Trade
                                            //9 = CCX EFP Trade
                                            //A = Other Clearing Venue
                                            //D = N2EX
                                            //E = EFP Trade/Against Actual
                                            //G = EEX
                                            //F = EFS / EFP Contra Trade
                                            //I = EFM Trade
                                            //J = EFR Trade
                                            //K = Block Trade
                                            //O = NG EFP/EFS Trade
                                            //Q = EOO Trade
                                            //S = EFS Trade
                                            //T = Contra Trade
                                            //U = CPBLK
                                            //V = Bilateral Off-Exchange Trade
                                            //Y = Cross Contra Trade
                                            //AA = Asset Allocation
        public int OrigTradeID { get; set; }//adapted from FIX 5.0
        public int TradeLinkID { get; set; }
        public string GroupIndicator { get; set; }//ID used to group components of a block trade submitted via ICE Block or the Trade Registration API.
        public int TradeLinkMktID { get; set; }
        public char OrdStatus { get; set; }// 2=Filled, 4=Canceled
        public string SecurityID { get; set; }//Contract symbol
        public int OptionsSymbol { get; set; }
        public string SecurityExchange { get; set; }
        public int ExchangeSilo { get; set; }
        public string StartDate { get; set; }//Format: YYYYMMDD. Present on custom strips for bilateral trades.
        public string EndDate { get; set; }//Format: YYYYMMDD. Present on custom strips for bilateral trades.
        public DateTime DeliveryStartDate { get; set; }
        public DateTime DeliveryEndDate { get; set; }
        public string LocationCode { get; set; }
        public string MeterNumber { get; set; }
        public int LeadTime { get; set; }
        public string ReasonCode { get; set; }
        public decimal LastParPx { get; set; }
        public decimal NumOfLots { get; set; }//Trade quantity represented in number of lots. Please note that the value should be ignored for strategy trades.
        public string SecuritySubType { get; set; }
        public int SequenceWithinMills { get; set; }
        public string Currency { get; set; }
        public string Contract { get; set; }//Company short name (abbreviation) whose contract terms are agreed for the deal
        public string GeneralTerms { get; set; }
        public List<CombiDefinition> CombiDefinitions { get; } = new List<CombiDefinition>();
        public string TermsQualityComments { get; set; }
        public string WaiverIndicator { get; set; }
        public int SelfMatchPreventionID { get; set; }
        public char SelfMatchPreventionInstruction { get; set; }
        public int QuantityMax { get; set; }
        public decimal OptoIMinQuantity { get; set; }
        public decimal OptoIMaxQuantity { get; set; }
        public string OptolPriceBasis { get; set; }
        public string OptolPriceBasisPeriod { get; set; }
        public string OptolPriceBasisSubLevel { get; set; }
        public decimal OptolPrice { get; set; }
        public List<IceLeg> Legs { get; }=new List<IceLeg>();
    }
}
