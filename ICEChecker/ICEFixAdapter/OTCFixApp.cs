//-----------------------------------------------------------------------------
// File Name   : OTCFixApp
// Author      : Alice Li
// Date        : 24/2/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Diagnostics;
using ICEFixAdapter.Models;
using ICEFixAdapter.Models.Request;
using ICEFixAdapter.Models.Response;
using ICEFixAdapter.MsgProcessor;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Transport;
using Message = QuickFix.Message;

namespace ICEFixAdapter {
    internal class OTCFixApp : MessageCracker, IApplication, IDisposable {
        private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
        #region API message section
        private event Action<IceErrorRsp> OnErrorRsp;  // general error, eg. logout...
        private event Action<IceLogonReqRsp> OnLogonRsp;
        private event Action<IceSecurityDefinitionRsp> OnSecurityDefinition;
        private event Action<IceDefinedStrategyRsp> OnDefinedStrategy;
        private event Action<IceTradeCaptureReportRsp> OnTradeCaptureReport;
        private event Action<IceTradeCaptureReportRequestAckRsp> OnTradeCaptureReportRequestAck;
        private event Action<IceUserCompanyResponse> OnUserCompanyResponse;
        private event Action<IceAllocationReportRsp> OnAllocationReport;
        private event Action<IceNewsRsp> OnNews;

        private Session _session;
        private readonly SocketInitiator _initiator;
        private readonly IceLogonReqRsp _loginInfo;

        const int EXCHANGESILO_FIELD = 9064;
        const int STRATEGYPREFERENCE_FIELD = 9006;
        const int PUBLISHCLEARINGALLOCATIONS_FIELD = 9008;
        const int PUBLISHMKTCREATIONREALTIME_FIELD = 9010;
        const int TOTALNUMSECURITIES_FIELD = 393;
        const int NORPTS_FIELD = 82;
        const int LISTSEQNO_FIELD = 67;
        const int BLOCK_DETAILS_BLOCK_TYPE = 9071;
        const int BLOCK_DETAILS_TRADE_TYPE = 9072;
        const int BLOCK_DETAILS_MIN_QTY = 9073;
        const string DEFINEDSTRATEGY = "UDS";
        const string USERCOMPANYRESPONSE = "UCR";
        const string USERCOMPANYREQUEST = "UCP";
        #endregion

        #region incoming msg collision prevention
        private QueueReader queue;
        #endregion

        internal OTCFixApp() {
            var settings = new SessionSettings("config/fix.cfg");
            _loginInfo = new IceLogonReqRsp {
                UserName = settings.Get().GetString("Username"),
                Password = settings.Get().GetString("Password"),
            };

            try {
                queue = new QueueReader(this);
            }
            catch { }

            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            _initiator = new SocketInitiator(this, storeFactory, settings, logFactory);
        }

        internal void Start(Action<IceLogonReqRsp> onLogonRsp = null, Action<IceErrorRsp> onErrorRsp = null) {
            OnLogonRsp = onLogonRsp;
            OnErrorRsp = onErrorRsp;

            _initiator.Start();
        }

        internal void Stop() {
            _initiator?.Stop();
        }

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID) {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID) {
        }

        public void OnLogout(SessionID sessionID) {

        }

        public void FromAdmin(Message msg, SessionID sessionID) {
            try {
                switch (msg.Header.GetString(35)) {
                    case "0": {//heartbeat
                            break;
                        }
                    case "3": {//reject(session level)
                            var rsp = new IceErrorRsp();
                            try { rsp.Text = msg.GetString(58); }
                            catch { }
                            try { rsp.RefTagID = msg.GetInt(371); }
                            catch { }
                            try { rsp.SessionRejectReason = msg.GetInt(373); }
                            catch { }
                            try { rsp.RefMsgType = msg.GetString(35); }
                            catch { }
                            OnErrorRsp?.Invoke(rsp);
                            break;
                        }
                    case "5": {  // logout response
                            var rsp = new IceErrorRsp() {
                                RefMsgType = msg.GetString(35)
                            };
                            try { rsp.Text = msg.GetString(58); }
                            catch { }
                            OnErrorRsp?.Invoke(rsp);
                            break;
                        }
                    case "A": { // if it's a logon response
                            var rsp = new IceLogonReqRsp {
                                MsgSeqNum = msg.Header.GetInt(34), // 34
                            };

                            OnLogonRsp?.Invoke(rsp);
                            break;
                        }
                    default:
                        Logger.Warn("FromAmdin: unsupported type - " + msg.Header.GetString(35));
                        break;
                }
            }
            catch (Exception e) {
                Logger.Error("FromAdmin: " + e);
            }
        }

        public void ToAdmin(Message msg, SessionID sessionID) {
            try {
                switch (msg.Header.GetString(35)) {
                    case "A": {  // if it's a logon request                            
                            msg.SetField(new Username(_loginInfo.UserName));
                            msg.SetField(new Password(_loginInfo.Password));  // 554
                            msg.SetField(new IntField(PUBLISHMKTCREATIONREALTIME_FIELD, 1));
                            msg.SetField(new IntField(PUBLISHCLEARINGALLOCATIONS_FIELD, 1));
                            msg.SetField(new StringField(9450, "ICEChecker"));
                            msg.SetField(new StringField(9451, "0.7"));
                            msg.SetField(new StringField(9452, "Theme International Trading"));
                            //msg.SetField(new ResetSeqNumFlag(true));
                            break;
                        }
                    case "0": {
                            break;
                        }
                    case "5": {// if it is a logout request
                            break;
                        }
                    default: {
                            Logger.Warn("ToAdmin: unsupported type - " + msg.Header.GetString(35));
                            break;
                        }
                }
            }
            catch (Exception e) {
                Logger.Error("ToAdmin: " + e);
            }
        }

        private int getNum = 0;
        public void FromApp(Message msg, SessionID sessionID) {
            try {
                if (!queue.Offer(msg)) {
                    var rsp = new IceErrorRsp();
                    rsp.Text = string.Format("Processing queue is full, message skipped: {}", msg);
                    OnErrorRsp?.Invoke(rsp);
                }
            }
            catch { }
        }
        public void Process(Message msg, SessionID sessionID = null) {
            try {
                string msgType = msg.Header.GetString(Tags.MsgType);
                switch (msgType) {
                    case MsgType.SECURITY_DEFINITION://d
                        ProcessSecurityDefinition((SecurityDefinition)msg, sessionID);
                        break;
                    case DEFINEDSTRATEGY://UDS
                        ProcessDefinedStrategy(msg, sessionID);
                        break;
                    case MsgType.NEWS://B
                        ProcessNews((News)msg, sessionID);
                        break;
                    case USERCOMPANYRESPONSE://UCR
                        ProcessUserCompanyResponse(msg, sessionID);
                        break;
                    case MsgType.TRADECAPTUREREPORTREQUESTACK://AQ
                        ProcessTCReportRequestAck((TradeCaptureReportRequestAck)msg, sessionID);
                        break;
                    case MsgType.TRADECAPTUREREPORT://AE
                        ProcessTCReport((TradeCaptureReport)msg, sessionID);
                        break;
                    case MsgType.ALLOCATIONREPORT://AS
                        ProcessAllocReport((AllocationReport)msg, sessionID);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e) {
                Logger.Error("==Cracker exception==: " + e);
            }
        }

        public void ToApp(Message msg, SessionID sessionID) {
            try {
                bool possDupFlag = false;
                if (msg.Header.IsSetField(Tags.PossDupFlag)) //
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(msg.Header.GetString(Tags.PossDupFlag)); // FIXME
                }

                if (possDupFlag) {
                    throw new DoNotSend();
                }
            }
            catch (Exception ex) {
                Logger.Error($"ToApp: {msg} error: {ex.Message}");
            }
        }
        #endregion     

        #region Security Definition Request and response
        // 35=c
        internal void SendSecurityDefinitionRequest(IceSecurityDefinitionReq thmReq, Action<IceSecurityDefinitionRsp> onSecurityDefinition = null, Action<IceDefinedStrategyRsp> onDefinedStrategy = null, Action<IceNewsRsp> onNews = null) {
            if (thmReq.SecurityRequestType != 3 && thmReq.SecurityRequestType != 101) {
                string err = $"SecurityRequestType '{thmReq.SecurityRequestType}' must be 3 or 101 in ICE OTC.";
                throw new Exception(err);
            }
            if (thmReq.SecurityRequestType == 3) {
                if (!string.IsNullOrEmpty(thmReq.CFICode) && thmReq.CFICode != "FXXXXX" && thmReq.CFICode != "OXXXXX" && thmReq.CFICode != "OXXFXX") {
                    string err = $"CFICode '{thmReq.CFICode}' must be one of 'FXXXXX','OXXXXX','OXXFXX' when SecurityRequestType equals 3.";
                    throw new Exception(err);
                }
                else if (string.IsNullOrEmpty(thmReq.CFICode)) {
                    thmReq.CFICode = "FXXXXX";//DEFAULT
                }
            }

            var req = new SecurityDefinitionRequest() {
                SecurityReqID = new SecurityReqID(thmReq.SecurityReqID),
                SecurityRequestType = new SecurityRequestType(thmReq.SecurityRequestType),
                SecurityID = new SecurityID(thmReq.SecurityID),//2=Oil, 165=Oil Americas
            };
            if (!string.IsNullOrEmpty(thmReq.CFICode)) {
                req.CFICode = new CFICode(thmReq.CFICode);
            }
            OnSecurityDefinition = onSecurityDefinition;
            OnDefinedStrategy = onDefinedStrategy;
            OnNews = onNews;
            _session.Send(req);
        }

        // 35=d
        internal void ProcessSecurityDefinition(SecurityDefinition msg, SessionID sessionID) {
            var rlt = new IceSecurityDefinitionRsp() {
                SecurityResponseID = msg.SecurityResponseID.getValue(),
                SecurityResponseType = msg.SecurityResponseType.getValue(),
                SecurityReqID = msg.SecurityReqID.getValue()
            };

            try { rlt.MarketTypeID = msg.SecurityID.getValue(); }
            catch { }
            try { rlt.Symbol = msg.GetInt(55); }//symbol is a int instead of string in ICE doc
            catch {
            }
            try {
                switch (msg.GetInt(EXCHANGESILO_FIELD)) {
                    case 0:
                        rlt.ExchangeSilo = "ICE";
                        break;
                    case 1:
                        rlt.ExchangeSilo = "ENDEX";
                        break;
                    case 2:
                        rlt.ExchangeSilo = "LIFFE";
                        break;
                    default: break;
                }
            }
            catch { }

            try { rlt.TotalNumSecurity = msg.GetInt(TOTALNUMSECURITIES_FIELD); }
            catch { }
            try { rlt.NoRpts = msg.GetInt(NORPTS_FIELD); }
            catch { }
            try { rlt.ListSeqNo = msg.GetInt(LISTSEQNO_FIELD); }
            catch { }
            try { rlt.Text = msg.Text.getValue(); }
            catch { }

            try {
                var noUnderlyings = msg.NoUnderlyings.getValue();
                for (int i = 1; i <= noUnderlyings; ++i) {
                    var grp = new SecurityDefinition.NoUnderlyingsGroup();
                    msg.GetGroup(i, grp);//group index start from 1

                    var underlying = new Underlying {
                        UnderlyingSymbol = grp.GetInt(311),
                        IncrementQty = grp.GetDecimal(9014),
                        IncrementPrice = grp.GetDecimal(9013),
                        LotSize = grp.GetDecimal(9017),
                        NumOfDecimalPrice = grp.GetDecimal(9083),

                    };
                    try { underlying.UnderlyingSecurityID = grp.UnderlyingSecurityID.getValue(); }
                    catch { }
                    try { underlying.UnderlyingSecurityIDSource = Convert.ToInt32(grp.UnderlyingSecurityIDSource.getValue()); }
                    catch { }
                    try { underlying.UnderlyingCFICode = grp.UnderlyingCFICode.getValue(); }
                    catch { }
                    try { underlying.UnderlyingSecuritySubType = grp.UnderlyingSecuritySubType.getValue(); }
                    catch { }
                    try { underlying.UnderlyingSecurityDesc = grp.UnderlyingSecurityDesc.getValue(); }
                    catch { }
                    try { underlying.UnderlyingSecurityExchange = grp.UnderlyingSecurityExchange.getValue(); }
                    catch { }
                    try { underlying.UnderlyingMaturityDate = grp.UnderlyingMaturityDate.getValue(); }
                    catch { }
                    try { underlying.UnderlyingCouponRate = grp.UnderlyingCouponRate.getValue(); }
                    catch { }
                    try { underlying.UnderlyingContractMultiplier = grp.UnderlyingContractMultiplier.getValue(); }
                    catch { }
                    try { underlying.UnderlyingDatedDate = grp.GetString(2041); }
                    catch { }
                    try { underlying.UnderlyingInterestAccrualDate = grp.GetString(2042); }
                    catch { }
                    try { underlying.UnderlyingIssueDate = grp.UnderlyingIssueDate.getValue(); }
                    catch { }
                    try { underlying.UnderlyingRepurchaseRate = grp.UnderlyingRepurchaseRate.getValue(); }
                    catch { }
                    try { underlying.UnderlyingFactor = grp.UnderlyingFactor.getValue(); }
                    catch { }
                    try { underlying.UnderlyingCreditRating = grp.UnderlyingCreditRating.getValue(); }
                    catch { }
                    try { underlying.UnderingInstrRegistry = grp.UnderlyingInstrRegistry.getValue(); }
                    catch { }
                    try { underlying.StartDate = grp.GetString(916); }
                    catch { }
                    try { underlying.EndDate = grp.GetString(917); }
                    catch { }
                    try { underlying.UnderlyingUnitOfMeasure = grp.GetString(998); }
                    catch { }
                    try { underlying.UnderlyingPutOrCall = grp.UnderlyingPutOrCall.getValue(); }
                    catch { }
                    try { underlying.UnderlyingStrikePrice = grp.UnderlyingStrikePrice.getValue(); }
                    catch { }
                    try { underlying.SecurityTradingStatus = grp.GetInt(326); }
                    catch { }
                    try { underlying.UnderlyingCurrency = grp.UnderlyingCurrency.getValue(); }
                    catch { }
                    try { underlying.UnderlyingSettlMethod = grp.GetChar(1039); }
                    catch { }
                    try { underlying.StrikeExerciseStyle = grp.GetInt(1304); }
                    catch { }
                    try { underlying.ScreenTickValue = grp.GetDecimal(9133); }
                    catch { }
                    try { underlying.BlockTickValue = grp.GetDecimal(9134); }
                    catch { }
                    try { underlying.OffExchangeIncrementPrice = grp.GetDecimal(9040); }
                    catch { }
                    try { underlying.OffExchangeIncrementQty = grp.GetDecimal(9041); }
                    catch { }
                    try { underlying.ProductName = grp.GetString(9062); }
                    catch { }
                    try { underlying.ProductDesc = grp.GetString(9063); }
                    catch { }
                    try { underlying.NumOfDecimalQty = grp.GetDecimal(9084); }
                    catch { }
                    try { underlying.Granularity = grp.GetString(9085); }
                    catch { }
                    try { underlying.BaseNumLots = grp.GetInt(9030); }
                    catch { }
                    try { underlying.TickValue = grp.GetDecimal(9032); }
                    catch { }
                    try { underlying.LegacyTickValue = grp.GetDecimal(9132); }
                    catch { }
                    try { underlying.ProductID = grp.GetInt(9061); }
                    catch { }
                    try { underlying.Clearable = grp.GetBoolean(9025); }
                    catch { }
                    try { underlying.HedgeProductID = grp.GetInt(9026); }
                    catch { }
                    try { underlying.HedgeMarketID = grp.GetInt(9027); }
                    catch { }
                    try { underlying.HedgeOnly = grp.GetInt(9033); }
                    catch { }
                    try { underlying.HomeExchange = grp.GetString(9042); }
                    catch { }
                    try { underlying.IsDividendAdjusted = grp.GetInt(9043); }
                    catch { }
                    try { underlying.ClearedAlias = grp.GetString(9091); }
                    catch { }
                    try { underlying.Denominator = grp.GetInt(9092); }
                    catch { }
                    try { underlying.InitialMargin = grp.GetInt(9093); }
                    catch { }
                    try { underlying.ProductType = grp.GetString(9095); }
                    catch { }
                    try { underlying.ImpliedType = grp.GetChar(9002); }
                    catch { }
                    try { underlying.PrimaryLegSymbol = grp.GetInt(9004); }
                    catch { }
                    try { underlying.SecondaryLegSymbol = grp.GetInt(9005); }
                    catch { }
                    try { underlying.PriceDenomination = grp.GetString(9100); }
                    catch { }
                    try { underlying.PriceUnit = grp.GetString(9101); }
                    catch { }
                    try { underlying.NumOfDecimalStrikePrice = grp.GetDecimal(9185); }
                    catch { }
                    try { underlying.LotSizeMultiplier = grp.GetDecimal(9024); }
                    catch { }
                    try { underlying.StripType = grp.GetInt(9200); }
                    catch { }
                    try { underlying.StripID = grp.GetInt(9201); }
                    catch { }
                    try { underlying.StripName = grp.GetString(9202); }
                    catch { }
                    try { underlying.BlockOnly = grp.GetInt(9203); }
                    catch { }
                    try { underlying.FlexAllowed = grp.GetInt(9204); }
                    catch { }
                    try { underlying.GTAllowed = grp.GetInt(9205); }
                    catch { }
                    try { underlying.MiFIDRegulatedMarket = grp.GetInt(9215); }
                    catch { }
                    try { underlying.AONAllowed = grp.GetInt(9216); }
                    catch { }
                    try { underlying.HubID = grp.GetInt(9300); }
                    catch { }
                    try { underlying.HubName = grp.GetString(9301); }
                    catch { }
                    try { underlying.HubAlias = grp.GetString(9302); }
                    catch { }
                    try { underlying.PhysicalCode = grp.GetString(9303); }
                    catch { }
                    try { underlying.IncrementStrike = grp.GetDecimal(9400); }
                    catch { }
                    try { underlying.MinStrike = grp.GetDecimal(9401); }
                    catch { }
                    try { underlying.MaxStrike = grp.GetDecimal(9402); }
                    catch { }
                    try { underlying.UnderlyingAccruedPremiumAmt = grp.GetDecimal(9900); }
                    catch { }
                    try { underlying.UnderlyingEventPaymentAmt = grp.GetDecimal(9901); }
                    catch { }
                    try { underlying.UnderlyingAlignmentInterestRate = grp.GetDecimal(9902); }
                    catch { }
                    try { underlying.UnderlyingRepurchaseDate = grp.GetString(9903); }
                    catch { }
                    try { underlying.UnderlyingInterpolationFactor = grp.GetDecimal(9904); }
                    catch { }
                    try { underlying.OverrideBlockMin = grp.GetInt(9034); }
                    catch { }
                    #region block
                    //try {
                    //    var noBlockDetails = grp.GetInt(9070);
                    //    for (int j = 1; j <= noBlockDetails; j++) {
                    //        var AltGrp = new SecurityDefinition.NoUnderlyingsGroup.NoBlockDetails();
                    //        grp.GetGroup(j, AltGrp);
                    //        var underlyingSecurityAlt = new underblock();
                    //        try { underlyingSecurityAlt.UnderlyingSecurityAltID = AltGrp.UnderlyingSecurityAltID.getValue(); }
                    //        catch { }
                    //        try { underlyingSecurityAlt.UnderlyingSecurityAltIDSource = AltGrp.UnderlyingSecurityAltIDSource.getValue(); }
                    //        catch { }
                    //        underlying.UnderlyingSecurityAltIDs.Add(underlyingSecurityAlt);
                    //    }
                    //}
                    //catch { }
                    #endregion
                    try { underlying.TestMarketIndicator = grp.GetInt(9217); }
                    catch { }
                    try { underlying.UDSAllowed = grp.GetInt(9049); }
                    catch { }
                    try { underlying.MarketTransparencyType = grp.GetInt(9074); }
                    catch { }
                    try { underlying.ProductGroup = grp.GetString(9075); }
                    catch { }
                    try { underlying.NonCommoditizedMarket = grp.GetInt(9425); }
                    catch { }
                    try {
                        var noUnderlyingSecurityAlts = grp.NoUnderlyingSecurityAltID.getValue();
                        for (int j = 1; j <= noUnderlyingSecurityAlts; j++) {
                            var AltGrp = new SecurityDefinition.NoUnderlyingsGroup.NoUnderlyingSecurityAltIDGroup();
                            grp.GetGroup(j, AltGrp);
                            var underlyingSecurityAlt = new UnderlyingSecurityAlt();
                            try { underlyingSecurityAlt.UnderlyingSecurityAltID = AltGrp.UnderlyingSecurityAltID.getValue(); }
                            catch { }
                            try { underlyingSecurityAlt.UnderlyingSecurityAltIDSource = AltGrp.UnderlyingSecurityAltIDSource.getValue(); }
                            catch { }
                            underlying.UnderlyingSecurityAltIDs.Add(underlyingSecurityAlt);
                        }
                    }
                    catch { }
                    rlt.Underlyings.Add(underlying);
                }
            }
            catch { }

            OnSecurityDefinition?.Invoke(rlt);
        }

        // 35=UDS, Defined Strategy is response to security definition request with 321='101'
        internal void ProcessDefinedStrategy(Message msg, SessionID sessionID) {
            var rlt = new IceDefinedStrategyRsp() {
                SecurityResponseID = msg.GetString(322),
                SecurityResponseType = msg.GetInt(323),
                SecurityReqID = msg.GetString(320)
            };

            try { rlt.MarketTypeID = msg.GetInt(9052); }
            catch { }
            try { rlt.Symbol = msg.GetInt(55); }//symbol is a int instead of string in ICE doc
            catch { }
            try { rlt.SecurityID = msg.GetString(48); }
            catch { }
            try { rlt.SecurityIDSource = msg.GetString(22); }
            catch { }
            try { rlt.SecurityExchange = msg.GetString(207); }
            catch { }
            try { rlt.StrategySecurityID = msg.GetString(9048); }
            catch { }
            try { rlt.UnderlyingStrategySymbol = msg.GetInt(9055); }
            catch { }
            try { rlt.SecurityType = msg.GetString(167); }
            catch { }
            try { rlt.MaturityDate = msg.GetString(541); }
            catch { }
            try { rlt.SecurityDesc = msg.GetString(107); }
            catch { }
            try { rlt.SecurityTradingStatus = msg.GetInt(326); }
            catch { }
            try { rlt.SecuritySubType = msg.GetString(762); }
            catch { }
            try { rlt.UnitOfMeasure = msg.GetString(996); }
            catch { }
            try {
                switch (msg.GetInt(EXCHANGESILO_FIELD)) {
                    case 0:
                        rlt.ExchangeSilo = "ICE";
                        break;
                    case 1:
                        rlt.ExchangeSilo = "ENDEX";
                        break;
                    case 2:
                        rlt.ExchangeSilo = "LIFFE";
                        break;
                    default: break;
                }
            }
            catch { }
            try { rlt.TransactTime = msg.GetDateTime(60); }
            catch { }
            try { rlt.IncrementPrice = msg.GetDecimal(9013); }
            catch { }
            try { rlt.IncrementQty = msg.GetDecimal(9014); }
            catch { }
            try { rlt.NumOfDecimalPrice = msg.GetDecimal(9083); }
            catch { }
            try { rlt.NumOfDecimalQty = msg.GetDecimal(9084); }
            catch { }
            try { rlt.ProductID = msg.GetInt(9061); }
            catch { }
            try { rlt.BaseNumLots = msg.GetInt(9030); }
            catch { }
            try { rlt.ClearedAlias = msg.GetString(9091); }
            catch { }
            try { rlt.Denominator = msg.GetInt(9092); }
            catch { }
            try { rlt.ImpliedType = msg.GetChar(9002); }
            catch { }
            try { rlt.OffExchangeIncrementPrice = msg.GetDecimal(9040); }
            catch { }
            try { rlt.OffExchangeIncrementQty = msg.GetDecimal(9041); }
            catch { }
            try { rlt.PriceDenomination = msg.GetString(9100); }
            catch { }
            try { rlt.PriceUnit = msg.GetString(9101); }
            catch { }
            try { rlt.NumOfDecimalStrikePrice = msg.GetDecimal(9185); }
            catch { }
            try { rlt.NumberOfCycles = msg.GetInt(9022); }
            catch { }
            try { rlt.LotSizeMultiplier = msg.GetDecimal(9024); }
            catch { }
            try { rlt.BlockOnly = msg.GetInt(9203); }
            catch { }
            try { rlt.FlexAllowed = msg.GetInt(9204); }
            catch { }
            try { rlt.GTAllowed = msg.GetInt(9205); }
            catch { }
            try { rlt.MiFIDRegulatedMarket = msg.GetInt(9215); }
            catch { }
            try { rlt.LegDealsSuppressed = msg.GetInt(9011); }
            catch { }
            try { rlt.ProductName = msg.GetString(9062); }
            catch { }
            try { rlt.StripType = msg.GetInt(9200); }
            catch { }
            try { rlt.StripName = msg.GetString(9202); }
            catch { }
            try { rlt.HubID = msg.GetInt(9300); }
            catch { }
            try { rlt.HubName = msg.GetString(9301); }
            catch { }
            try { rlt.HubAlias = msg.GetString(9302); }
            catch { }
            try { rlt.PhysicalCode = msg.GetString(9303); }
            catch { }
            try { rlt.OverrideBlockMin = msg.GetInt(9034); }
            catch { }
            try { rlt.TestMarketIndicator = msg.GetInt(9217); }
            catch { }
            try { rlt.ScreenTickValue = msg.GetDecimal(9133); }
            catch { }
            try { rlt.BlockTickValue = msg.GetDecimal(9134); }
            catch { }
            try { rlt.Text = msg.GetString(58); }
            catch { }

            try {
                var noBlockDetails = msg.GetInt(9070);
                for (int j = 1; j <= noBlockDetails; j++) {
                    var grp = msg.GetGroup(j, BLOCK_DETAILS_BLOCK_TYPE);
                    var blockDetail = new BlockDetail();
                    try { blockDetail.BlockDetailsBlockType = grp.GetInt(9071); }
                    catch { }
                    try { blockDetail.BlockDetailsTradeType = grp.GetString(9072); }
                    catch { }
                    try { blockDetail.BlockDetailsMinQty = grp.GetDecimal(9073); }
                    catch { }
                    rlt.BlockDetails.Add(blockDetail);
                }
            }
            catch { }

            try {
                var noUnderlyingSecurityAlts = msg.GetInt(457);
                for (int j = 1; j <= noUnderlyingSecurityAlts; j++) {
                    var AltGrp = new SecurityDefinition.NoUnderlyingsGroup.NoUnderlyingSecurityAltIDGroup();
                    msg.GetGroup(j, AltGrp);
                    var underlyingSecurityAlt = new UnderlyingSecurityAlt();
                    try { underlyingSecurityAlt.UnderlyingSecurityAltID = AltGrp.UnderlyingSecurityAltID.getValue(); }
                    catch { }
                    try { underlyingSecurityAlt.UnderlyingSecurityAltIDSource = AltGrp.UnderlyingSecurityAltIDSource.getValue(); }
                    catch { }
                    rlt.UnderlyingSecurityAltIDs.Add(underlyingSecurityAlt);
                }
            }
            catch { }
            try {
                var noLegs = msg.GetInt(555);
                for (int i = 1; i <= noLegs; i++) {
                    var legGrp = new SecurityDefinition.NoLegsGroup();
                    msg.GetGroup(i, legGrp);
                    var iceleg = new IceLeg() {
                        LegSymbol = legGrp.GetInt(600),
                        LegSecurityType = legGrp.LegSecurityType.getValue(),
                        LegSide = legGrp.LegSide.getValue(),
                        LegQty = legGrp.GetDecimal(623),
                        LegPrice = legGrp.GetDecimal(566),
                        LegSecuritySubType = legGrp.LegSecuritySubType.getValue(),
                        LegOptionDelta = legGrp.GetInt(1017),
                        LegRatioQtyDenominator = legGrp.GetInt(9623),
                        LegRatioQtyNumerator = legGrp.GetInt(9624),
                        LegRatioPriceDenominator = legGrp.GetInt(9566),
                        LegRatioPriceNumerator = legGrp.GetInt(9567)
                    };
                    rlt.Legs.Add(iceleg);
                }
            }
            catch { }
            OnDefinedStrategy?.Invoke(rlt);
        }
        #endregion

        #region Trade Capture Report (DualSide / SingleSide / Trading Subsciption) //35=AD

        //35=AD
        internal void SendTradeCaptureReportRequest(IceTradeSubscriptionReq thmReq, Action<IceTradeCaptureReportRsp> onTradeCaptureReport, Action<IceTradeCaptureReportRequestAckRsp> onTradeCaptureReportRequestAck,
                                                    Action<IceAllocationReportRsp> onAllocationReport) {
            var req = new TradeCaptureReportRequest() {
                TradeRequestID = new TradeRequestID(thmReq.TradeReqID),  //568
                TradeRequestType = new TradeRequestType(thmReq.TradeRequestType), // 569: 0 ,1
            };
            req.SetField(new IntField(263, thmReq.SubscriptionRequestType));
            if (!string.IsNullOrEmpty(thmReq.SecurityID)) {
                req.SecurityID = new SecurityID(thmReq.SecurityID);
            }

            if (thmReq.Parties.Count > 0) {
                req.NoPartyIDs = new NoPartyIDs(thmReq.Dates.Count);
                foreach (var party in thmReq.Parties) {
                    var partyGrp = new TradeCaptureReportRequest.NoPartyIDsGroup {
                        PartyID = new PartyID(party.PartyID),
                        PartyIDSource = new PartyIDSource(party.PartyIDSource),
                        PartyRole = new PartyRole(party.PartyRole)
                    };
                    req.AddGroup(partyGrp);
                }
            }
            if (thmReq.Underlyings.Count > 0) {
                req.NoUnderlyings = new NoUnderlyings(thmReq.Underlyings.Count);
                foreach (var underlying in thmReq.Underlyings) {
                    var underlyingGrp = new TradeCaptureReportRequest.NoUnderlyingsGroup {
                        UnderlyingSymbol = new UnderlyingSymbol(underlying.UnderlyingSymbol.ToString())
                    };
                    req.AddGroup(underlyingGrp);
                }
            }
            if (thmReq.Dates.Count > 0) {
                req.NoDates = new NoDates(thmReq.Dates.Count);
                foreach (var date in thmReq.Dates) {
                    var dateGrp = new TradeCaptureReportRequest.NoDatesGroup {
                        TradeDate = new TradeDate(date.TradeDate.ToString("yyyyMMdd")),
                        TransactTime = new TransactTime(date.TransactTime)
                    };
                    req.AddGroup(dateGrp);
                }
            }
            OnTradeCaptureReport = onTradeCaptureReport;
            OnTradeCaptureReportRequestAck = onTradeCaptureReportRequestAck;
            OnAllocationReport = onAllocationReport;
            _session.Send(req);
        }

        //35=AQ
        public void ProcessTCReportRequestAck(TradeCaptureReportRequestAck msg, SessionID sessionID) {
            var rlt = new IceTradeCaptureReportRequestAckRsp() {
                TradeRequestID = msg.TradeRequestID.getValue(),
                TradeRequestType = msg.TradeRequestType.getValue(),
                TradeRequestResult = msg.TradeRequestResult.getValue(), // 749
                TradeRequestStatus = msg.TradeRequestStatus.getValue(), // 750               
            };
            try { rlt.CFICode = msg.CFICode.getValue(); }
            catch { }
            try { rlt.SecurityID = msg.SecurityID.getValue(); }
            catch { }
            try { rlt.Text = msg.Text.getValue(); }
            catch { }
            try {
                var noUnderlyings = msg.NoUnderlyings.getValue();
                for (int j = 1; j <= noUnderlyings; ++j) {
                    var partyGrp = new TradeCaptureReport.NoUnderlyingsGroup();
                    msg.GetGroup(j, partyGrp);
                    rlt.Underlyings.Add(new Underlying() { UnderlyingSymbol = partyGrp.GetInt(311) }); ;
                }
            }
            catch { }
            OnTradeCaptureReportRequestAck?.Invoke(rlt);
        }

        //35=AE
        public void ProcessTCReport(TradeCaptureReport msg, SessionID sessionID) {
            var rlt = new IceTradeCaptureReportRsp {
                TradeReportID = msg.TradeReportID.getValue(),
                TradeReportTransType = msg.TradeReportTransType.getValue(),
                ExecID = msg.ExecID.getValue(),
                PreviouslyReported = msg.PreviouslyReported.getValue(),
                Symbol = msg.GetInt(55),
                LastQty = msg.LastQty.getValue(),
                LastPx = msg.LastPx.getValue(),
                CFICode = msg.CFICode.getValue(),
                NumberOfCycles = msg.GetInt(9022),
                TradeDate = msg.TradeDate.getValue(),
                TransactTime = msg.TransactTime.getValue(),
                ClientAppType = msg.GetInt(9413)
            };

            try {
                var noSides = msg.NoSides.getValue();
                for (int i = 1; i <= noSides; ++i) {
                    var sideGrp = new TradeCaptureReport.NoSidesGroup();
                    msg.GetGroup(i, sideGrp);

                    var side = new IceTradeSide {
                        Side = sideGrp.Side.getValue(),
                        OrderID = Convert.ToInt32(sideGrp.OrderID.getValue()),//ask quickfix/n for meta data
                    };

                    try {
                        var noParties = sideGrp.NoPartyIDs.getValue();
                        for (int j = 1; j <= noParties; ++j) {
                            var partyGrp = new TradeCaptureReport.NoSidesGroup.NoPartyIDsGroup();
                            sideGrp.GetGroup(j, partyGrp);

                            var party = new IceParty {
                                PartyID = partyGrp.PartyID.getValue(),
                                PartyIDSource = partyGrp.PartyIDSource.getValue(),
                                PartyRole = partyGrp.PartyRole.getValue()
                            };

                            side.Parties.Add(party);
                        }
                    }
                    catch { }
                    try {
                        var noAllocs = sideGrp.NoAllocs.getValue();
                        for (int j = 1; j <= noAllocs; ++j) {
                            var allocGrp = new TradeCaptureReport.NoSidesGroup.NoAllocsGroup();
                            sideGrp.GetGroup(j, allocGrp);

                            var alloc = new Allocation {
                                AllocAccount = allocGrp.AllocAccount.getValue()
                            };

                            try {
                                var noNested2Parties = allocGrp.NoNested2PartyIDs.getValue();
                                for (int k = 1; k <= noNested2Parties; ++k) {
                                    var partyGrp = new TradeCaptureReport.NoSidesGroup.NoAllocsGroup.NoNested2PartyIDsGroup();
                                    allocGrp.GetGroup(k, partyGrp);

                                    var nested2Party = new IceParty {
                                        PartyID = partyGrp.Nested2PartyID.getValue(),
                                        PartyIDSource = partyGrp.Nested2PartyIDSource.getValue(),
                                        PartyRole = partyGrp.Nested2PartyRole.getValue()
                                    };

                                    alloc.NestedPartys.Add(nested2Party);
                                }
                            }
                            catch { }
                            side.Allocs.Add(alloc);
                        }
                    }
                    catch { }
                    try { side.Text = sideGrp.Text.getValue(); }
                    catch { }
                    try { side.ComplianceID = sideGrp.ComplianceID.getValue(); }
                    catch { }
                    try { side.PositionEffect = sideGrp.PositionEffect.getValue(); }
                    catch { }
                    try { side.TransactDetails = sideGrp.GetString(9123); }
                    catch { }
                    try { side.MemoField = sideGrp.GetString(9121); }
                    catch { }
                    try { side.CustOrderHandlingInst = sideGrp.GetString(1031); }
                    catch { }

                    //we dont have to retrieve all the data because we dont use that data, actually
                    rlt.Sides.Add(side);
                }
            }
            catch { }
            try { rlt.TradeReportType = msg.TradeReportType.getValue(); }
            catch { }
            try { rlt.TradeRequestID = msg.TradeRequestID.getValue(); }
            catch { }
            try { rlt.TrdType = msg.TrdType.getValue().ToString(); }
            catch { }
            try { rlt.OrdStatus = msg.OrdStatus.getValue(); }
            catch { }
            try { rlt.SecurityID = msg.SecurityID.getValue(); }//CONTRACT SYMBOL
            catch { }
            try { rlt.ExecType = msg.ExecType.getValue(); }
            catch {
                rlt.ExecType = '0';
            }
            try {
                var noLegs = msg.NoLegs.getValue();
                for (int i = 1; i <= noLegs; i++) {
                    var legGrp = new TradeCaptureReport.NoLegsGroup();
                    msg.GetGroup(i, legGrp);
                    var iceleg = new IceLeg() {
                        LegSymbol = legGrp.GetInt(600),
                        LegCFICode = legGrp.LegCFICode.getValue(),
                        LegSide = legGrp.LegSide.getValue(),
                        LegLastPx = legGrp.LegLastPx.getValue(),
                        LegRefID = Convert.ToInt32(legGrp.LegRefID.getValue()),
                        LegNumOfCycles = legGrp.GetInt(9023),
                        LegStartDate = legGrp.GetString(9020),
                        LegEndDate = legGrp.GetString(9021),
                    };
                    try {
                        var noNestedParties = legGrp.NoNestedPartyIDs.getValue();
                        for (int j = 1; j <= noNestedParties; ++j) {
                            var partyGrp = new TradeCaptureReport.NoSidesGroup.NoPartyIDsGroup();
                            legGrp.GetGroup(j, partyGrp);

                            var party = new IceParty {
                                PartyID = partyGrp.PartyID.getValue(),
                                PartyIDSource = partyGrp.PartyIDSource.getValue(),
                                PartyRole = partyGrp.PartyRole.getValue()
                            };
                            iceleg.NestedParties.Add(party);
                        }
                    }
                    catch { }
                    try { iceleg.LegComplianceID = legGrp.GetString(9376); }
                    catch { }
                    try { iceleg.LinkExecID = legGrp.GetInt(9527); }
                    catch { }
                    try { iceleg.LegNumOfLots = legGrp.GetInt(9019); }
                    catch { }
                    try { iceleg.LegQty = legGrp.LegQty.getValue(); }
                    catch { }
                    try { iceleg.LegSecurityID = legGrp.LegSecurityID.getValue(); }
                    catch { }
                    try { iceleg.LegSecurityIDSource = legGrp.LegSecurityIDSource.getValue(); }
                    catch { }
                    try { iceleg.LegStrikePrice = legGrp.LegStrikePrice.getValue(); }
                    catch { }
                    try { iceleg.LegOptionSymbol = legGrp.GetInt(9404); }
                    catch { }
                    try { iceleg.LegSecurityExchange = legGrp.LegSecurityExchange.getValue(); }
                    catch { }
                    try { iceleg.LegOptionDelta = legGrp.GetInt(1017); }
                    catch { }
                    try { iceleg.LegMemoField = legGrp.GetString(9122); }
                    catch { }
                    try { iceleg.LegCustOrderHandlingInst = legGrp.GetString(9122); }
                    catch { }
                    rlt.Legs.Add(iceleg);
                }
            }
            catch { }
            try { rlt.OrigTradeID = msg.GetInt(1126); }
            catch { }
            try { rlt.TradeLinkID = msg.GetInt(820); }
            catch { }
            try { rlt.GroupIndicator = msg.GetString(9820); }
            catch { }
            try { rlt.TradeLinkMktID = msg.GetInt(9414); }
            catch { }
            try { rlt.SecurityIDSource = msg.SecurityIDSource.getValue(); }
            catch { }
            try { rlt.StrikePrice = msg.StrikePrice.getValue(); }
            catch { }
            try { rlt.OptionsSymbol = msg.GetInt(9403); }
            catch { }
            try { rlt.SecurityExchange = msg.SecurityExchange.getValue(); }
            catch { }
            try { rlt.ExchangeSilo = msg.GetInt(9064); }
            catch { }
            try { rlt.StartDate = msg.StartDate.getValue(); }
            catch { }
            try { rlt.EndDate = msg.EndDate.getValue(); }
            catch { }
            try { rlt.DeliveryStartDate = msg.GetDateTime(9520); }
            catch { }
            try { rlt.DeliveryEndDate = msg.GetDateTime(9521); }
            catch { }
            try { rlt.LocationCode = msg.GetString(9522); }
            catch { }
            try { rlt.LeadTime = msg.GetInt(9524); }
            catch { }
            try { rlt.ReasonCode = msg.GetString(9525); }
            catch { }
            try { rlt.MeterNumber = msg.GetString(9523); }
            catch { }
            try { rlt.LastParPx = msg.LastParPx.getValue(); }
            catch { }
            try { rlt.NumOfLots = msg.GetDecimal(9018); }
            catch { }
            try { rlt.GeneralTerms = msg.GetString(9098); }
            catch { }
            try { rlt.Contract = msg.GetString(9097); }
            catch { }
            try { rlt.Currency = msg.GetString(15); }
            catch { }
            try { rlt.SecuritySubType = msg.SecuritySubType.getValue(); }
            catch { }
            try { rlt.DealAdjustIndicator = msg.GetInt(9124); }
            catch { }
            try { rlt.SelfMatchPreventionID = msg.GetInt(9821); }
            catch { }
            try { rlt.SelfMatchPreventionInstruction = msg.GetChar(9822); }
            catch { }
            try { rlt.QuantityMax = msg.GetInt(9512); }
            catch { }
            try { rlt.OptoIMinQuantity = msg.GetDecimal(9513); }
            catch { }
            try { rlt.OptoIMaxQuantity = msg.GetDecimal(9514); }
            catch { }
            try { rlt.OptolPriceBasis = msg.GetString(9515); }
            catch { }
            try { rlt.OptolPriceBasisPeriod = msg.GetString(9516); }
            catch { }
            try { rlt.OptolPriceBasisSubLevel = msg.GetString(9518); }
            catch { }
            try { rlt.OptolPrice = msg.GetDecimal(9517); }
            catch { }
            try { rlt.WaiverIndicator = msg.GetString(8013); }
            catch { }
            OnTradeCaptureReport?.Invoke(rlt);
        }
        #endregion

        #region Alloc Report
        private void ProcessAllocReport(AllocationReport msg, SessionID sessionID) {
            var rlt = new IceAllocationReportRsp() {
                AllocID = msg.AllocID.getValue(),
                AllocTransType = msg.AllocTransType.getValue(),
                ClearingBusinessDate = msg.GetString(715),
                AllocReportType = msg.AllocReportType.getValue(),
                AllocStatus = msg.AllocStatus.getValue(),
                Side = msg.Side.getValue(),
                Symbol = msg.GetInt(55),
                Shares = msg.Quantity.getValue(),
                AvgPx = msg.AvgPx.getValue(),
                TradeDate = msg.TradeDate.getValue(),
                TransactTime = msg.TransactTime.getValue(),
                TradeInputSource = msg.GetString(578),
                LiquidityIndicator = msg.GetChar(9120),
            };
            try { rlt.TradeRequestID = msg.GetString(568); }
            catch { }
            try { rlt.TrdType = msg.GetString(828); }
            catch { }
            try { rlt.SecurityType = msg.SecurityType.getValue(); }
            catch { }
            try { rlt.MaturityMonthYear = msg.MaturityMonthYear.getValue(); }
            catch { }
            try { rlt.PutOrCall = msg.PutOrCall.getValue(); }
            catch { }
            try { rlt.StrikePrice = msg.StrikePrice.getValue(); }
            catch { }
            try { rlt.OptionSymbol = msg.GetInt(9403); }
            catch { }
            try { rlt.SecurityExchange = msg.SecurityExchange.getValue(); }
            catch { }
            try { rlt.Text = msg.Text.getValue(); }
            catch { }
            try {
                int noOrders = msg.NoOrders.getValue();
                for (int i = 1; i <= noOrders; i++) {
                    var orderGrp = new AllocationReport.NoOrdersGroup();
                    msg.GetGroup(i, orderGrp);

                    rlt.CIOrdIDs.Add(orderGrp.ClOrdID.getValue());
                }
            }
            catch { }

            try {
                int noExecs = msg.NoExecs.getValue();
                for (int i = 1; i <= noExecs; i++) {
                    var execGrp = new AllocationReport.NoExecsGroup();
                    msg.GetGroup(i, execGrp);

                    var exec = new Execution() {
                        LastShares = execGrp.LastQty.getValue(),
                        ExecID = execGrp.ExecID.getValue(),
                        TradeID = execGrp.GetString(1003),
                    };
                    try { exec.LastPx = execGrp.LastPx.getValue(); }
                    catch { }
                    try { exec.ClientAppType = execGrp.GetInt(9413); }
                    catch { }
                    rlt.Execs.Add(exec);
                }
            }
            catch { }
            try {
                int noAllocs = msg.NoAllocs.getValue();
                for (int i = 1; i <= noAllocs; i++) {
                    var allocGrp = new AllocationReport.NoAllocsGroup();
                    msg.GetGroup(i, allocGrp);

                    var alloc = new Allocation() {
                        AllocShares = allocGrp.AllocQty.getValue(),
                        IndividualAllocType = allocGrp.GetChar(992),

                    };
                    try { alloc.AllocAccount = allocGrp.AllocAccount.getValue(); }
                    catch { }
                    try { alloc.AllocPrice = allocGrp.AllocPrice.getValue(); }
                    catch { }
                    try { alloc.SecondaryIndividualAllocID = allocGrp.GetInt(989); }
                    catch { }
                    try { alloc.AllocCustomerCapacity = allocGrp.GetInt(993); }
                    catch { }
                    try { alloc.AllocPositionEffect = allocGrp.GetChar(1047); }
                    catch { }
                    try { alloc.AllocText = allocGrp.AllocText.getValue(); }
                    catch { }
                    try { alloc.SettlementAccountCode = allocGrp.GetString(9194); }
                    catch { }
                    try { alloc.AcceptanceTime = allocGrp.GetDateTime(9060); }
                    catch { }
                    try {
                        var noAllocInfo = allocGrp.GetInt(9140);
                        for (int k = 1; k <= noAllocInfo; k++) {
                            var allocInfoGrp = allocGrp.GetGroup(k, 9141);

                            var allocInfo = new AllocInfo() {
                                AllocSideInfo = allocInfoGrp.GetInt(9141),
                                ClientID = allocInfoGrp.GetInt(109),
                                TMMnemonic = allocInfoGrp.GetString(9103)
                            };
                            try { allocInfo.BrokerCompID = allocInfoGrp.GetInt(9065); }
                            catch { }
                            try { allocInfo.OriginatorUserID = allocInfoGrp.GetString(9139); }
                            catch { }
                            try { allocInfo.AccountCode = allocInfoGrp.GetChar(9195); }
                            catch { }
                            try { allocInfo.RIMnemonic = allocInfoGrp.GetString(9603); }
                            catch { }
                            try { allocInfo.CMMnemonic = allocInfoGrp.GetString(9604); }
                            catch { }
                            try { allocInfo.ClrHouseCode = allocInfoGrp.GetString(9609); }
                            catch { }
                            try { allocInfo.CustomerAccountReflID = allocInfoGrp.GetString(9207); }
                            catch { }
                            try { allocInfo.CTICode = allocInfoGrp.GetInt(9208); }
                            catch { }

                            alloc.AllocInfos.Add(allocInfo);
                        }
                    }
                    catch { }
                    rlt.Allocs.Add(alloc);
                }
            }
            catch { }
            OnAllocationReport?.Invoke(rlt);
        }
        #endregion

        #region User company req and rsp: NOT USEFUL FOR THE CLEARING ACCOUNT LIKE S01BPI21, IT ONLY FOR OUR COMPANY NAME
        internal void SendUserCompanyRequest(Action<IceUserCompanyResponse> onUserCompanyResponse) {
            var req = new UserCompanyRequest();
            //req.Header.SetField(new BeginString("FIX.4.4"));
            //req.Header.SetField(new MsgType("UCP"));

            OnUserCompanyResponse = onUserCompanyResponse;
            _session.Send(req);
        }

        internal void ProcessUserCompanyResponse(Message msg, SessionID sessionID) {
            var rlt = new IceUserCompanyResponse();
            var noParties = msg.GetInt(453);
            try {
                for (int j = 1; j <= noParties; ++j) {
                    var partyGrp = msg.GetGroup(j, 448);
                    var party = new IceParty {
                        PartyID = partyGrp.GetString(448),
                        PartyIDSource = partyGrp.GetChar(447),
                        PartyRole = partyGrp.GetInt(452)
                    };

                    var noSubParties = msg.GetInt(523);
                    try {
                        for (int k = 1; k <= noSubParties; ++k) {
                            var subPartyGrp = partyGrp.GetGroup(k, 523);
                            var subParty = new IcePartySub {
                                PartySubID = subPartyGrp.GetString(523),
                                PartySubIDType = subPartyGrp.GetInt(803)
                            };
                            party.PartySubs.Add(subParty);
                        }
                    }
                    catch { }
                    try { party.Text = msg.GetString(58); }
                    catch { }
                    rlt.Parties.Add(party);
                }
            }
            catch { }
            OnUserCompanyResponse?.Invoke(rlt);
        }
        #endregion

        #region News //35=B
        private void ProcessNews(News msg, SessionID sessionID) {
            var rlt = new IceNewsRsp() {
                Headline = msg.Headline.getValue(),
                Urgency = msg.Urgency.getValue()
            };
            try { rlt.UserName = msg.GetString(553); }
            catch { }
            try {
                var linesOfText = msg.LinesOfText.getValue();
                rlt.LinesOfText = linesOfText;

                for (int i = 1; i <= linesOfText; i++) {
                    var textGrp = new News.LinesOfTextGroup();
                    msg.GetGroup(i, textGrp);

                    string text = textGrp.Text.getValue();
                    rlt.Texts.Add(text);
                }
            }
            catch { }
            OnNews?.Invoke(rlt);
        }
        #endregion
        public void Dispose() {
            _initiator.Dispose();
            _session.Dispose();
        }
    }
}
