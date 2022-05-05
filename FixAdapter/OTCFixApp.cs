//-----------------------------------------------------------------------------
// File Name   : OTCFixApp
// Author      : junlei
// Date        : 5/26/2020 2:19:35 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Linq;
using FixAdapter.Models;
using FixAdapter.Models.Request;
using FixAdapter.Models.Response;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX50SP2;
using QuickFix.Transport;
using Message = QuickFix.Message;

namespace FixAdapter {
    internal class OTCFixApp : MessageCracker, IApplication, IDisposable {
        private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();

        private event Action<ThmErrorRsp> OnErrorRsp;  // general error, eg. logout...
        private event Action<ThmLogonReqRsp> OnLogonRsp;

        private event Action<ThmSecurityListRsp> OnSecurities;
        private event Action<ThmSecurityDefinitionRsp> OnSecurityDefinition;
        private event Action<ThmPartyDetailsRsp> OnPartyListRequest;

        private event Action<ThmTradeCaptureReportRsp> OnTradeCaptureReport;
        private event Action<ThmTradeCaptureReportAckRsp> OnTradeCaptureReportAck;
        private event Action<ThmTradeCaptureReportRequestAckRsp> OnTradeCaptureReportRequestAck;

        private event Action<ThmMarketDataSnapshotRsp> OnMarketDataSnapshot;
        private event Action<ThmMarketDataIncrementalRsp> OnMarketDataIncremental;

        private event Action<ThmExecutionReportRsp> OnExecutionReport;

        private event Action<ThmOrderMassCancelReportRsp> OnOrderMassCancel;

        private readonly SocketInitiator _initiator;
        private Session _session;

        private readonly ThmLogonReqRsp _loginInfo;
        internal OTCFixApp(string newPassword = null) {
            var settings = new SessionSettings("config/fix.cfg");
            _loginInfo = new ThmLogonReqRsp {
                UserName = settings.Get().GetString("Username"),
                Password = settings.Get().GetString("Password"),
                NewPassword = newPassword,  // null if not change password
            };

            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            _initiator = new SocketInitiator(this, storeFactory, settings, logFactory);
        }

        internal void Start(Action<ThmLogonReqRsp> onLogonRsp = null, Action<ThmErrorRsp> onErrorRsp = null) {
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
                    case "0": {
                        break;
                    }
                    case "3": {
                        var rsp = new ThmErrorRsp {
                            Text = msg.GetString(58),
                            Message = msg.ToString(),
                        };
                        OnErrorRsp?.Invoke(rsp);
                        break;
                    }
                    case "5": {  // logout
                        var rsp = new ThmErrorRsp() {
                            Message = msg.ToString(),
                        };
                        try { rsp.SessionStatus = msg.GetString(1409); }  //5
                        catch { }
                        try { rsp.Text = msg.GetString(58); }
                        catch { }

                        OnErrorRsp?.Invoke(rsp);
                        break;
                    }
                    case "A": { // if it's a logon
                        var rsp = new ThmLogonReqRsp {
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
                    case "A": {  // if it's a logon
                        msg.RemoveField(141);
                        msg.SetField(new MsgSeqNum(1)); // always set to 1 
                        msg.SetField(new Username(_loginInfo.UserName));
                        msg.SetField(new Password(_loginInfo.Password));  // 554

                        if (!string.IsNullOrEmpty(_loginInfo.NewPassword)) {
                            msg.SetField(new NewPassword(_loginInfo.NewPassword));
                        }
                        break;
                    }
                    case "0": {
                        break;
                    }
                    case "5": {
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

        public void FromApp(Message msg, SessionID sessionID) {
            try {
                //Logger.Info("FromApp: " + msg.ToString());
                Crack(msg, sessionID);
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

                //msg.Header.RemoveField(Tags.SenderSubID);
                if (possDupFlag) {
                    throw new DoNotSend();
                }
            }
            catch (Exception ex) {
                Logger.Error($"ToApp: {msg} error: {ex.Message}");
            }
        }
        #endregion

        #region Security list request
        //  35=x
        internal void SendSecurityListRequest(ThmSecurityListReq thmReq, Action<ThmSecurityListRsp> onSecurities) {
            var req = new SecurityListRequest() {
                SecurityReqID = new SecurityReqID(thmReq.SecurityReqId),
                SecurityListRequestType = new SecurityListRequestType(thmReq.SecurityListRequestType),
            };

            OnSecurities = onSecurities;
            _session.Send(req);
        }

        // 35=y
        public void OnMessage(SecurityList msg, SessionID sessionID) {
            var rlt = new ThmSecurityListRsp();
            try { rlt.SecurityReqID = msg.SecurityReqID.getValue(); }
            catch { };
            try { rlt.SecurityResponseID = msg.SecurityResponseID.getValue(); }
            catch { };
            try { rlt.SecurityRequestResult = msg.SecurityRequestResult.getValue(); }
            catch { };

            int count = msg.NoRelatedSym.getValue();  //Number of securities in one msg
            for (int i = 1; i <= count; ++i) {
                var tmp = new SecurityList.NoRelatedSymGroup();
                msg.GetGroup(i, tmp);

                var sec = new ThmSecurity() {
                    SecurityID = tmp.SecurityID.getValue(),
                    SecurityIDSource = tmp.SecurityIDSource.getValue(),
                    SecurityDesc = tmp.SecurityDesc.getValue(),

                    //Currency = tmp.Currency.getValue(),
                    //Exchange = tmp.SecurityExchange.getValue(),
                    //MaturityDay = tmp.MaturityDate.getValue(),
                    //ContractMultiplier = tmp.ContractMultiplier.getValue(),
                    //MaturityMonthYear = tmp.MaturityMonthYear.getValue()
                };
                try { sec.Symbol = tmp.Symbol.getValue(); }
                catch { }

                rlt.Securities.Add(sec);
            }

            OnSecurities?.Invoke(rlt);
        }

        #endregion

        #region Security Definition Request // 35=c
        internal void SendSecurityDefinitionRequest(ThmSecurityDefinitionReq thmReq, Action<ThmSecurityDefinitionRsp> onSecurityDefinition) {
            if (thmReq.RequestType != 0 && thmReq.RequestType != 3) {
                string err = $"SecurityRequestType '{thmReq.RequestType}' must be 0 or 3 in TitanOTC.";
                throw new Exception(err);
            }

            var req = new SecurityDefinitionRequest() {
                SecurityReqID = new SecurityReqID(thmReq.RequestID),
                SecurityRequestType = new SecurityRequestType(thmReq.RequestType),
                //SecurityExchange = new SecurityExchange("SGX"),                
            };

            if (!string.IsNullOrEmpty(thmReq.Symbol)) {
                req.Symbol = new Symbol(thmReq.Symbol);
            }

            if (!string.IsNullOrEmpty(thmReq.SecurityID)) {
                req.SecurityID = new SecurityID(thmReq.SecurityID);
            }

            if (!string.IsNullOrEmpty(thmReq.SecurityType)) {
                req.SecurityType = new SecurityType(thmReq.SecurityType); // "FUT", "OPT", "FORWARD", etc.
            }

            if (thmReq.Legs.Any()) {
                req.NoLegs = new NoLegs(thmReq.Legs.Count);
                foreach (var leg in thmReq.Legs) {
                    var grp = new SecurityDefinitionRequest.NoLegsGroup() {
                        LegSymbol = new LegSymbol(leg.LegSymbol),
                        LegSecurityID = new LegSecurityID(leg.LegSecurityID),
                    };

                    if (!string.IsNullOrEmpty(leg.LegSecurityType)) {
                        grp.LegSecurityType = new LegSecurityType(leg.LegSecurityType);
                    }
                    if (leg.LegPutOrCall != null) {
                        grp.LegPutOrCall = new LegPutOrCall(leg.LegPutOrCall.Value);
                    }

                    req.AddGroup(grp);
                }
            }

            OnSecurityDefinition = onSecurityDefinition;
            _session.Send(req);
        }

        // 35=d
        public void OnMessage(SecurityDefinition msg, SessionID sessionID) {
            var rlt = new ThmSecurityDefinitionRsp();

            try { rlt.SecurityReqID = msg.SecurityReqID.getValue(); }
            catch { }
            try { rlt.SecurityResponseType = msg.SecurityResponseType.getValue(); }
            catch { }
            try { rlt.Symbol = msg.Symbol.getValue(); }
            catch { }
            try { rlt.SecurityType = msg.SecurityType.getValue(); }
            catch { }
            try { rlt.ProductComplex = msg.ProductComplex.getValue(); }
            catch { }
            try { rlt.Text = msg.Text.getValue(); }
            catch { }

            try {
                var noLeg = msg.NoLegs.getValue();
                for (int i = 1; i <= noLeg; ++i) {
                    var grp = new SecurityDefinition.NoLegsGroup();
                    msg.GetGroup(i, grp);

                    var leg = new ThmLeg {
                        LegSymbol = grp.LegSecurityID.getValue(),
                        LegSecurityID = grp.LegSecurityID.getValue(),
                    };
                    try { leg.LegSecurityIDSource = grp.LegSecurityIDSource.getValue(); }
                    catch { }
                    try { leg.LegSecurityType = grp.LegSecurityType.getValue(); }
                    catch { }
                    try { leg.LegMaturityMonthYear = grp.LegMaturityMonthYear.getValue(); }
                    catch { }
                    try { leg.LegStrikePrice = grp.LegStrikePrice.getValue(); }
                    catch { }
                    try { leg.LegRatioQty = grp.LegRatioQty.getValue(); }
                    catch { }
                    try { leg.LegPutOrCall = grp.LegPutOrCall.getValue(); }
                    catch { }

                    try { leg.LegExerciseStyle = grp.LegExerciseStyle.getValue(); }
                    catch { }

                    rlt.Legs.Add(leg);
                }
            }
            catch { }

            try {
                var noMarket = msg.NoMarketSegments.getValue();
                for (int i = 1; i <= noMarket; ++i) {
                    var grp = new SecurityDefinition.NoMarketSegmentsGroup();
                    msg.GetGroup(i, grp);

                    var mktSeg = new ThmMarketSegment {
                        MarketSegmentID = grp.MarketSegmentID.getValue(),
                        LowLimitPrice = grp.LowLimitPrice.getValue(),
                        HighLimitPrice = grp.HighLimitPrice.getValue(),
                        TradingReferencePrice = grp.TradingReferencePrice.getValue(),
                        MultilegModel = grp.MultilegModel.getValue()
                    };

                    rlt.MarketSegments.Add(mktSeg);
                }
            }
            catch { }

            OnSecurityDefinition?.Invoke(rlt);
        }

        #endregion

        #region Trade Capture Report (DualSide / SingleSide / Trading Subsciption) //35=AD

        //35=AD
        internal void SendTradeCaptureReportRequest(ThmTradeSubscriptionReq thmReq,
            Action<ThmTradeCaptureReportRsp> onTradeCaptureReport,
            Action<ThmTradeCaptureReportRequestAckRsp> onTradeCaptureReportRequestAck) {
            var req = new TradeCaptureReportRequest() {
                TradeRequestID = new TradeRequestID(thmReq.TradeReqID),  //568
                TradeRequestType = new TradeRequestType(thmReq.TradeRequestType), // 569: 0 ,1
                SubscriptionRequestType = new SubscriptionRequestType(thmReq.SubscriptionRequestType.Value), // 263: 0 , 1,
            };

            if (!string.IsNullOrEmpty(thmReq.TradeID)) {
                req.TradeID = new TradeID(thmReq.TradeID);  //TradeID
            }

            if (thmReq.Dates.Any()) {
                req.NoDates = new NoDates(thmReq.Dates.Count);

                foreach (var date in thmReq.Dates) {
                    var dateGrp = new TradeCaptureReportRequest.NoDatesGroup {
                        TransactTime = new TransactTime(date)
                    };
                    req.AddGroup(dateGrp);
                }
            }

            OnTradeCaptureReport = onTradeCaptureReport;
            OnTradeCaptureReportRequestAck = onTradeCaptureReportRequestAck;

            _session.Send(req);
        }

        //35=AE
        internal void SendTradeCaptureReport(ThmTradeCaptureReportReq thmReq, Action<ThmTradeCaptureReportRsp> onTradeCaptureReport, Action<ThmTradeCaptureReportAckRsp> onTradeCaptureReportAck) {
            if (thmReq.TradeReportTransType != 0 && thmReq.TradeReportTransType != 1 && thmReq.TradeReportTransType != 2) {
                string err = $"TradeReportTransType '{thmReq.TradeReportTransType}' must be 0,1,2 in TitanOTC.";
                throw new Exception(err);
            }

            var req = new TradeCaptureReport() {
                Symbol = new Symbol(thmReq.Symbol),    // 55
                LastQty = new LastQty(thmReq.LastQty), // 32
                LastPx = new LastPx(thmReq.LastPx),    // 31: =0.85
                QtyType = new QtyType(thmReq.QtyType), // 854: 1-CONTRACTS
                TradeReportTransType = new TradeReportTransType(thmReq.TradeReportTransType), // 487
                TrdType = new TrdType(thmReq.TrdType), // 828 :1;2;12;54
            };

            if (!string.IsNullOrEmpty(thmReq.SecurityID)) {
                req.SecurityID = new SecurityID(thmReq.SecurityID);
            }
            if (!string.IsNullOrEmpty(thmReq.SecurityIDSource)) {
                req.SecurityIDSource = new SecurityIDSource(thmReq.SecurityIDSource);
            }
            if (!string.IsNullOrEmpty(thmReq.SecurityType)) {
                req.SecurityType = new SecurityType(thmReq.SecurityType);
            }
            if (!string.IsNullOrEmpty(thmReq.MaturityMonthYear)) {
                req.MaturityMonthYear = new MaturityMonthYear(thmReq.MaturityMonthYear); // 200
            }

            if (!string.IsNullOrEmpty(thmReq.TradeReportID)) {
                req.TradeReportID = new TradeReportID(thmReq.TradeReportID);  // 571
            }
            if (thmReq.TradeReportType != null) {
                req.TradeReportType = new TradeReportType(thmReq.TradeReportType.Value); // 856
            }
            if (!string.IsNullOrEmpty(thmReq.TradeRequestID)) {
                req.TradeRequestID = new TradeRequestID(thmReq.TradeRequestID);
            }
            if (thmReq.ExecType != null) {
                req.ExecType = new ExecType(thmReq.ExecType.Value); // 150
            }
            if (thmReq.TotNumTradeReports != null) {
                req.TotNumTradeReports = new TotNumTradeReports(thmReq.TotNumTradeReports.Value);
            }
            if (thmReq.LastRptRequested != null) {
                req.LastRptRequested = new LastRptRequested(thmReq.LastRptRequested.Value);
            }
            if (!string.IsNullOrEmpty(thmReq.ExecID)) {
                req.ExecID = new ExecID(thmReq.ExecID);
            }
            if (thmReq.PriceType != null) {
                req.PriceType = new PriceType(thmReq.PriceType.Value); // 423: 2-per_unit      
            }

            if (thmReq.TransactTime != null) {
                req.TransactTime = new TransactTime(thmReq.TransactTime.Value);
            }
            else if (!string.IsNullOrEmpty(thmReq.TradeDate)) {
                req.TradeDate = new TradeDate(thmReq.TradeDate); //"20200610"                
            }

            if (!string.IsNullOrEmpty(thmReq.ClearingBusinessDate)) {
                req.ClearingBusinessDate = new ClearingBusinessDate(thmReq.ClearingBusinessDate);
            }
            if (!string.IsNullOrEmpty(thmReq.TradeID)) {
                req.TradeID = new TradeID(thmReq.TradeID); //1003
            }
            if (!string.IsNullOrEmpty(thmReq.Currency)) {
                req.Currency = new Currency(thmReq.Currency);
            }
            if (!string.IsNullOrEmpty(thmReq.MarketSegmentID)) {
                req.MarketSegmentID = new MarketSegmentID(thmReq.MarketSegmentID);
            }

            if (thmReq.Sides.Any()) {
                req.NoSides = new NoSides(thmReq.Sides.Count);  // 552

                foreach (var side in thmReq.Sides) {
                    var sideGrp = new TradeCaptureReport.NoSidesGroup() {
                        Side = new Side(side.Side.Value),     // 54: buy                        
                    };

                    if (!string.IsNullOrEmpty(side.Account)) {
                        sideGrp.Account = new Account(side.Account);  // 1
                    }
                    if (!string.IsNullOrEmpty(side.Text)) {
                        sideGrp.Text = new Text(side.Text);
                    }
                    if (!string.IsNullOrEmpty(side.SideTradeReportID)) {
                        sideGrp.SideTradeReportID = new SideTradeReportID(side.SideTradeReportID); //1105
                    }
                    if (!string.IsNullOrEmpty(side.ExchangeSpecialInstructions)) {
                        sideGrp.ExchangeSpecialInstructions = new ExchangeSpecialInstructions(side.ExchangeSpecialInstructions);  // 1139
                    }
                    if (side.AggressorIndicator != null) {
                        sideGrp.AggressorIndicator = new AggressorIndicator(side.AggressorIndicator.Value);
                    }
                    if (!string.IsNullOrEmpty(side.TradeInputSource)) {
                        sideGrp.TradeInputSource = new TradeInputSource(side.TradeInputSource);
                    }
                    if (!string.IsNullOrEmpty(side.SideExecID)) {
                        sideGrp.SideExecID = new SideExecID(side.SideExecID);
                    }
                    if (!string.IsNullOrEmpty(side.TradingSessionID)) {
                        sideGrp.TradingSessionID = new TradingSessionID(side.TradingSessionID);
                    }
                    if (!string.IsNullOrEmpty(side.TradingSessionSubID)) {
                        sideGrp.TradingSessionSubID = new TradingSessionSubID(side.TradingSessionSubID);
                    }

                    if (side.Parties.Any()) {
                        sideGrp.NoPartyIDs = new NoPartyIDs(side.Parties.Count); // 453

                        foreach (var party in side.Parties) {
                            var partIDGrp = new TradeCaptureReport.NoSidesGroup.NoPartyIDsGroup() {
                                PartyID = new PartyID(party.PartyID), // 448
                                PartyIDSource = new PartyIDSource(party.PartyIDSource.Value), // 447
                                PartyRole = new PartyRole(party.PartyRole),  // 452
                            };
                            sideGrp.AddGroup(partIDGrp);
                        }
                    }

                    req.AddGroup(sideGrp);
                }
            }

            if (thmReq.MultiLegReportingType != null) { // 1: single security by default
                req.MultiLegReportingType = new MultiLegReportingType(thmReq.MultiLegReportingType.Value); // 442: 3
            }

            if (thmReq.Legs.Any()) {
                req.NoLegs = new NoLegs(thmReq.Legs.Count); // 555
                foreach (var leg in thmReq.Legs) {
                    var grp = new TradeCaptureReport.NoLegsGroup();
                    if (!string.IsNullOrEmpty(leg.LegSymbol)) {
                        grp.LegSymbol = new LegSymbol(leg.LegSymbol); // 600
                    }
                    if (leg.LegLastPx != null) {
                        grp.LegLastPx = new LegLastPx(leg.LegLastPx.Value);
                    }
                    if (leg.LegLastQty != null) {
                        grp.LegLastQty = new LegLastQty(leg.LegLastQty.Value);
                    }
                    if (leg.LegRatioQty != null) {
                        grp.LegRatioQty = new LegRatioQty(leg.LegRatioQty.Value);
                    }

                    if (!string.IsNullOrEmpty(leg.LegSecurityID)) {
                        grp.LegSecurityID = new LegSecurityID(leg.LegSecurityID);
                    }
                    if (!string.IsNullOrEmpty(leg.LegSecurityIDSource)) {
                        grp.LegSecurityIDSource = new LegSecurityIDSource(leg.LegSecurityIDSource);
                    }
                    if (!string.IsNullOrEmpty(leg.LegSecurityType)) {
                        grp.LegSecurityType = new LegSecurityType(leg.LegSecurityType);
                    }
                    if (!string.IsNullOrEmpty(leg.LegMaturityMonthYear)) {
                        grp.LegMaturityMonthYear = new LegMaturityMonthYear(leg.LegMaturityMonthYear);
                    }

                    req.AddGroup(grp);
                }
            }

            OnTradeCaptureReport = onTradeCaptureReport;
            OnTradeCaptureReportAck = onTradeCaptureReportAck;

            _session.Send(req);
        }

        //35=AQ
        public void OnMessage(TradeCaptureReportRequestAck msg, SessionID sessionID) {
            var rlt = new ThmTradeCaptureReportRequestAckRsp() {
                TradeRequestID = msg.TradeRequestID.getValue(),
                TradeRequestType = msg.TradeRequestType.getValue(),
                TradeRequestResult = msg.TradeRequestResult.getValue(), // 749
                TradeRequestStatus = msg.TradeRequestStatus.getValue(), // 750
            };

            OnTradeCaptureReportRequestAck?.Invoke(rlt);
        }

        //35=AR
        public void OnMessage(TradeCaptureReportAck msg, SessionID sessionID) {
            var rlt = new ThmTradeCaptureReportAckRsp() {
                LastQty = msg.LastQty.getValue(),
                LastPx = msg.LastPx.getValue(),
            };

            try { rlt.TradeReportID = msg.TradeReportID?.getValue(); }
            catch { }
            try { rlt.TradeReportTransType = msg.TradeReportTransType?.getValue(); }
            catch { }
            try { rlt.TradeReportType = msg.TradeReportType?.getValue(); }
            catch { }
            try { rlt.ExecType = msg.ExecType?.getValue(); }
            catch { }
            try { rlt.TrdRptStatus = msg.TrdRptStatus?.getValue(); }
            catch { }
            try { rlt.Text = msg.Text?.getValue(); }
            catch { }
            try { rlt.TradeID = msg.TradeID?.getValue(); }
            catch { }
            try { rlt.TradeReportRejectReason = msg.TradeReportRejectReason?.getValue(); }
            catch { }
            try { rlt.ExecID = msg.ExecID?.getValue(); }
            catch { }
            try { rlt.Symbol = msg.Symbol?.getValue(); }
            catch { }
            try { rlt.PriceType = msg.PriceType?.getValue(); }
            catch { }
            try { rlt.QtyType = msg.QtyType.getValue(); }
            catch { }
            try { rlt.MatchStatus = msg.MatchStatus?.getValue(); }
            catch { }

            try {
                var noSides = msg.NoSides?.getValue();
                for (int i = 1; i <= noSides; ++i) {
                    var grp = new TradeCaptureReportAck.NoSidesGroup();
                    msg.GetGroup(i, grp);

                    var side = new ThmTradeSide {
                        Side = grp.Side.getValue(),
                    };
                    try { side.TradingSessionID = grp.TradingSessionID?.getValue(); }
                    catch { }
                    try { side.TradingSessionSubID = grp.TradingSessionSubID?.getValue(); }
                    catch { }
                    try { side.SideTradeReportID = grp.SideTradeReportID?.getValue(); }
                    catch { }

                    rlt.Sides.Add(side);
                }
            }
            catch { }

            var noLegs = msg.NoLegs?.getValue();
            for (int i = 1; i <= noLegs; ++i) {
                var grp = new TradeCaptureReportAck.NoLegsGroup();
                msg.GetGroup(i, grp);
                var leg = new ThmLeg();

                try { leg.LegSymbol = grp.LegSymbol?.getValue(); }
                catch { }
                try { leg.LegRatioQty = grp.LegRatioQty?.getValue(); }
                catch { }
                try { leg.LegLastPx = grp.LegLastPx?.getValue(); }
                catch { }
                try { leg.LegReportID = grp.LegReportID?.getValue(); }
                catch { }
                try { leg.LegLastQty = grp.LegLastQty?.getValue(); }
                catch { }

                rlt.Legs.Add(leg);
            }

            OnTradeCaptureReportAck?.Invoke(rlt);
        }

        //35=AE
        public void OnMessage(TradeCaptureReport msg, SessionID sessionID) {
            var rlt = new ThmTradeCaptureReportRsp {
                Symbol = msg.Symbol.getValue(),
                LastQty = msg.LastQty.getValue(),
                LastPx = msg.LastPx.getValue(),
                QtyType = msg.QtyType.getValue(),
            };

            try { rlt.TradeReportID = msg.TradeReportID.getValue(); }
            catch { }
            try { rlt.TradeReportTransType = msg.TradeReportTransType.getValue(); }
            catch { }
            try { rlt.TradeReportType = msg.TradeReportType.getValue(); }
            catch { }
            try { rlt.TradeRequestID = msg.TradeRequestID.getValue(); }
            catch { }
            try { rlt.TrdType = msg.TrdType.getValue(); }
            catch { }
            try { rlt.PriceType = msg.PriceType.getValue(); }
            catch { }
            try { rlt.TradeDate = msg.TradeDate.getValue(); }
            catch { }
            try { rlt.ClearingBusinessDate = msg.ClearingBusinessDate.getValue(); }
            catch { }
            try { rlt.TransactTime = msg.TransactTime.getValue(); }
            catch { }
            try { rlt.SecurityID = msg.SecurityID.getValue(); }
            catch { }
            try { rlt.TradeID = msg.TradeID.getValue(); }
            catch { }
            try { rlt.ExecID = msg.ExecID.getValue(); }
            catch { }
            try { rlt.ExecType = msg.ExecType.getValue(); }
            catch { }
            try { rlt.TotNumTradeReports = msg.TotNumTradeReports.getValue(); } // 748
            catch { }
            try { rlt.LastRptRequested = msg.LastRptRequested.getValue(); } // 912
            catch { }

            try {
                var noSides = msg.NoSides.getValue();
                for (int i = 1; i <= noSides; ++i) {
                    var sideGrp = new TradeCaptureReport.NoSidesGroup();
                    msg.GetGroup(i, sideGrp);

                    var side = new ThmTradeSide {
                        Side = sideGrp.Side.getValue(),
                    };

                    try {
                        var noParties = sideGrp.NoPartyIDs.getValue();
                        for (int j = 1; j <= noParties; ++j) {
                            var partyGrp = new TradeCaptureReport.NoSidesGroup.NoPartyIDsGroup();
                            sideGrp.GetGroup(j, partyGrp);

                            var party = new ThmParty {
                                PartyID = partyGrp.PartyID.getValue(),
                                PartyIDSource = partyGrp.PartyIDSource.getValue(),
                                PartyRole = partyGrp.PartyRole.getValue()
                            };

                            side.Parties.Add(party);
                        }
                    }
                    catch { }

                    try { side.Account = sideGrp.Account.getValue(); }
                    catch { }
                    try { side.Text = sideGrp.Text.getValue(); }
                    catch { }
                    try { side.SideTradeReportID = sideGrp.SideTradeReportID.getValue(); }
                    catch { }
                    try { side.AggressorIndicator = sideGrp.AggressorIndicator.getValue(); }
                    catch { }
                    try { side.TradeInputSource = sideGrp.TradeInputSource.getValue(); }
                    catch { }
                    try { side.TradingSessionID = sideGrp.TradingSessionID.getValue(); }
                    catch { }
                    try { side.TradingSessionSubID = sideGrp.TradingSessionSubID.getValue(); }
                    catch { }

                    try { side.SideRiskLimitCheckStatus = sideGrp.SideRiskLimitCheckStatus.getValue(); } // 2344
                    catch { }

                    try { side.SideExecID = sideGrp.SideExecID.getValue(); } // 1427
                    catch { }

                    //TradeCaptureReport.NoSidesGroup.

                    rlt.Sides.Add(side);
                }
            }
            catch { }

            try { rlt.Currency = msg.Currency.getValue(); }
            catch { }
            try { rlt.MarketSegmentID = msg.MarketSegmentID.getValue(); } // 1300
            catch { }
            try { rlt.RiskLimitCheckStatus = msg.RiskLimitCheckStatus.getValue(); } // 2343
            catch { }

            OnTradeCaptureReport?.Invoke(rlt);
        }
        #endregion

        #region Party Details Request  // 35=CF
        internal void SendPartyDetailsListRequest(ThmPartyDetailsReq thmReq, Action<ThmPartyDetailsRsp> onPartyDetailsReq) {
            var req = new PartyDetailsListRequest() {
                PartyDetailsListRequestID = new PartyDetailsListRequestID(thmReq.ReqID),
            };

            if (thmReq.PartyListResponseTypes.Count > 0) {
                req.NoPartyListResponseTypes = new NoPartyListResponseTypes(thmReq.PartyListResponseTypes.Count);
                foreach (var rspType in thmReq.PartyListResponseTypes) {
                    var grp = new PartyDetailsListRequest.NoPartyListResponseTypesGroup() {
                        PartyListResponseType = new PartyListResponseType(rspType),
                    };
                    req.AddGroup(grp);
                }
            }

            OnPartyListRequest = onPartyDetailsReq;
            _session.Send(req);
        }

        // 35=CG
        public void OnMessage(PartyDetailsListReport msg, SessionID sessionID) {
            var rlt = new ThmPartyDetailsRsp();
            try { rlt.PartyDetailsListReportID = msg.PartyDetailsListReportID.getValue(); }
            catch { }
            try { rlt.PartyDetailsListReqeustID = msg.PartyDetailsListRequestID?.getValue(); }
            catch { }
            try { rlt.PartyDetailsRequestResult = msg.PartyDetailsRequestResult?.getValue(); }
            catch { }

            var noParties = msg.NoPartyList.getValue();
            for (int i = 1; i <= noParties; ++i) {
                var grp = new PartyDetailsListReport.NoPartyListGroup();
                msg.GetGroup(i, grp);

                var party = new ThmParty() {
                    PartyID = grp.PartyID.getValue(),
                    PartyIDSource = grp.PartyIDSource.getValue(),
                    PartyRole = grp.PartyRole.getValue(),
                };

                try {
                    var noSubIDs = grp.NoPartySubIDs?.getValue();  // should be  == 1
                    for (int j = 1; j <= noSubIDs; ++j) {
                        var subGrp = new PartyDetailsListReport.NoPartyListGroup.NoPartySubIDsGroup();
                        grp.GetGroup(j, subGrp);

                        party.PartySubs.Add(new ThmPartySub() {
                            PartySubID = subGrp.PartySubID.getValue(),
                            PartySubIDType = subGrp.PartySubIDType.getValue(),
                        });
                    }
                }
                catch { }

                try {
                    var noRelatedPartyIDs = grp.NoRelatedPartyIDs?.getValue();

                    for (int j = 1; j <= noRelatedPartyIDs; ++j) {
                        var relatedGrp = new PartyDetailsListReport.NoPartyListGroup.NoRelatedPartyIDsGroup();
                        grp.GetGroup(j, relatedGrp);
                        var relatedParty = new ThmRelatedParty() {
                            RelatedPartyID = relatedGrp.RelatedPartyID.getValue(),
                            RelatedPartyIDSource = relatedGrp.RelatedPartyIDSource.getValue(),
                            RelatedPartyRole = relatedGrp.RelatedPartyRole.getValue(),
                        };
                        party.RelatedParties.Add(relatedParty);

                        try {
                            var noRelatedParty = relatedGrp.NoRelatedPartySubIDs?.getValue();  // should be  == 1
                            for (int k = 1; k <= noRelatedParty; ++k) {
                                var relatedParytySubID = new PartyDetailsListReport.NoPartyListGroup.NoRelatedPartyIDsGroup.NoRelatedPartySubIDsGroup();
                                relatedGrp.GetGroup(k, relatedParytySubID);
                                relatedParty.RelatedSubParties.Add(new ThmRelatedSubParty() {
                                    RelatedPartySubID = relatedParytySubID.RelatedPartySubID.getValue(),
                                    RelatedPartySubIDType = relatedParytySubID.RelatedPartySubIDType.getValue(),
                                });
                            }
                        }
                        catch { }

                        try {
                            var noRelationship = relatedGrp.NoPartyRelationships?.getValue();
                            for (int k = 1; k <= noRelationship; ++k) {
                                var relationshipGrp = new PartyDetailsListReport.NoPartyListGroup.NoRelatedPartyIDsGroup.NoPartyRelationshipsGroup();
                                relatedGrp.GetGroup(k, relationshipGrp);
                                relatedParty.PartyRelationShips.Add(new ThmPartyRelationShip {
                                    PartyRelationship = relationshipGrp.PartyRelationship?.getValue(),
                                });
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                rlt.Parties.Add(party);
            }

            OnPartyListRequest?.Invoke(rlt);
        }

        #endregion

        #region Market Data Request 
        // 35=V
        internal void SendMarketDataRequest(ThmMarketDataReq thmReq,
            Action<ThmMarketDataSnapshotRsp> onMarketDataSnapshot,
            Action<ThmMarketDataIncrementalRsp> onMarketDataIncremental) {
            var req = new MarketDataRequest {
                MDReqID = new MDReqID(thmReq.MdReqId),
                SubscriptionRequestType = new SubscriptionRequestType(thmReq.SubscriptionRequestType.Value),  // 263
                MarketDepth = new MarketDepth(thmReq.MarketDepth),  // 264: 0
                NoMDEntryTypes = new NoMDEntryTypes(thmReq.MDEntries.Count),  //267: should be 1
                NoRelatedSym = new NoRelatedSym(thmReq.RelatedSymbols.Count), //269: should be 0
            };

            foreach (var entry in thmReq.MDEntries) {
                var grp = new MarketDataRequest.NoMDEntryTypesGroup() {
                    MDEntryType = new MDEntryType(entry.EntryType.Value), // 2 = Trade
                };
                req.AddGroup(grp);
            }

            foreach (var sym in thmReq.RelatedSymbols) {
                var grp = new MarketDataRequest.NoRelatedSymGroup {
                    Symbol = new Symbol(sym.Symbol)
                };
                req.AddGroup(grp);
            }

            OnMarketDataSnapshot = onMarketDataSnapshot;
            OnMarketDataIncremental = onMarketDataIncremental;
            _session.Send(req);
        }

        // 35=W
        public void OnMessage(MarketDataSnapshotFullRefresh msg, SessionID sessionID) {
            var rlt = new ThmMarketDataSnapshotRsp {
                Symbol = msg.Symbol.getValue()
            };

            try { rlt.MDReqID = msg.MDReqID?.getValue(); }
            catch { }

            var noMDEntries = msg.NoMDEntries.getValue();
            if (noMDEntries > 0) {
                var grp = new MarketDataSnapshotFullRefresh.NoMDEntriesGroup();
                for (int i = 1; i <= noMDEntries; i++) {
                    msg.GetGroup(i, grp);

                    var entry = new ThmMarketDataEntry {
                        MDEntryType = grp.MDEntryType.getValue(),
                    };

                    try { entry.MDEntryPx = grp.MDEntryPx.getValue(); }
                    catch { }
                    try { entry.Currency = grp.Currency.getValue(); }
                    catch { }
                    try { entry.MDEntrySize = grp.MDEntrySize.getValue(); }
                    catch { }
                    try { entry.MDEntryDate = grp.MDEntryDate.getValue(); }
                    catch { }
                    try { entry.MDEntryTime = grp.MDEntryTime.getValue(); }
                    catch { }
                    try { entry.TradingSessionID = grp.TradingSessionID.getValue(); }
                    catch { }
                    try { entry.TradingSessionSubID = grp.TradingSessionSubID.getValue(); }
                    catch { }
                    try { entry.MDEntryOriginator = grp.MDEntryOriginator.getValue(); }
                    catch { }
                    try { entry.TransactTime = grp.TransactTime.getValue(); }
                    catch { }
                    try { entry.SettlDate = grp.SettlDate.getValue(); }
                    catch { }

                    rlt.MDEntries.Add(entry);
                }
            }

            OnMarketDataSnapshot?.Invoke(rlt);
        }

        // 35=X
        public void OnMessage(MarketDataIncrementalRefresh msg, SessionID sessionID) {
            var rlt = new ThmMarketDataIncrementalRsp();

            var noMDEntries = msg.NoMDEntries.getValue();
            if (noMDEntries > 0) {
                var grp = new MarketDataIncrementalRefresh.NoMDEntriesGroup();
                for (int i = 1; i <= noMDEntries; i++) {
                    msg.GetGroup(i, grp);

                    var entry = new ThmMarketDataEntry {
                        MDUpdateAction = grp.MDUpdateAction.getValue(),
                        Symbol = grp.Symbol.getValue(),
                    };

                    try { entry.MDEntryType = grp.MDEntryType.getValue(); } catch { }

                    try { entry.MDEntryPx = grp.MDEntryPx.getValue(); }
                    catch { }
                    try { entry.Currency = grp.Currency.getValue(); }
                    catch { }
                    try { entry.MDEntrySize = grp.MDEntrySize.getValue(); }
                    catch { }
                    try { entry.MDEntryDate = grp.MDEntryDate.getValue(); }
                    catch { }
                    try { entry.MDEntryTime = grp.MDEntryTime.getValue(); }
                    catch { }
                    try { entry.TradingSessionID = grp.TradingSessionID.getValue(); }
                    catch { }
                    try { entry.TradingSessionSubID = grp.TradingSessionSubID.getValue(); }
                    catch { }
                    try { entry.MDEntryOriginator = grp.MDEntryOriginator.getValue(); }
                    catch { }
                    try { entry.TransactTime = grp.TransactTime.getValue(); }
                    catch { }
                    try { entry.SettlDate = grp.SettlDate.getValue(); }
                    catch { }

                    rlt.MDEntries.Add(entry);
                }
            }

            OnMarketDataIncremental?.Invoke(rlt);
        }

        //35=Y
        public void OnMessage(MarketDataRequestReject msg, SessionID sessionID) {
            var rejectionReason = msg.MDReqRejReason.getValue();

            var rlt = new ThmMarketDataRsp {
                MDReqID = msg.MDReqID.getValue(),
                //MDUpdateAction = rsp.MD
                FailedResult = msg.Text.getValue()
            };
            //OnMarketData?.Invoke(rlt);
        }

        #endregion

        #region Order Management

        // 35=D
        internal void SendOrderNewRequest(ThmOrderReq thmReq, Action<ThmExecutionReportRsp> onExecutionReport) {
            var msg = new NewOrderSingle {
                Account = new Account(thmReq.Account),
                ClOrdID = new ClOrdID(thmReq.OrderID),
                Symbol = new Symbol(thmReq.Symbol),
                Side = new Side(thmReq.Side.Value),
                OrdType = new OrdType(thmReq.OrdType.Value),
                OrderQty = new OrderQty(thmReq.OrderQty),
                Price = new Price(thmReq.Price),
                TransactTime = new TransactTime(thmReq.TransactTime),
            };

            if (null != thmReq.TimeInForce) {  // 59
                msg.TimeInForce = new TimeInForce(thmReq.TimeInForce.Value);
            }
            if (!string.IsNullOrEmpty(thmReq.ExecInst)) { // 18
                msg.ExecInst = new ExecInst(thmReq.ExecInst);
            }

            msg.NoPartyIDs = new NoPartyIDs(thmReq.Parties.Count);  // 453
            foreach (var party in thmReq.Parties) {
                var grp = new NewOrderSingle.NoPartyIDsGroup {
                    PartyID = new PartyID(party.PartyID),
                    PartyIDSource = new PartyIDSource(party.PartyIDSource.Value),
                    PartyRole = new PartyRole(party.PartyRole)
                };

                msg.AddGroup(grp);
            }

            if (thmReq.TargetParties.Any()) {
                msg.NoTargetPartyIDs = new NoTargetPartyIDs(thmReq.TargetParties.Count);  // 1461
                foreach (var targetParty in thmReq.TargetParties) {
                    var grp = new NewOrderSingle.NoTargetPartyIDsGroup {
                        TargetPartyID = new TargetPartyID(targetParty.TargetPartyID),
                        TargetPartyIDSource = new TargetPartyIDSource(targetParty.TargetPartyIDSource.Value),
                        TargetPartyRole = new TargetPartyRole(targetParty.TargetPartyRole),
                    };

                    msg.AddGroup(grp);
                }
            }

            if (thmReq.ValueChecks.Any()) {
                msg.NoValueChecks = new NoValueChecks(thmReq.ValueChecks.Count);  // 1868
                foreach (var vc in thmReq.ValueChecks) {
                    var grp = new NewOrderSingle.NoValueChecksGroup {
                        ValueCheckType = new ValueCheckType(vc.ValueCheckType),  // 1869
                        ValueCheckAction = new ValueCheckAction(vc.ValueCheckAction) // 1870
                    };

                    msg.AddGroup(grp);
                }
            }

            if (thmReq.ExposureDuration != null) {
                msg.ExposureDuration = new ExposureDuration(thmReq.ExposureDuration.Value);
            }
            if (!string.IsNullOrEmpty(thmReq.RefOrderID)) {
                msg.RefOrderID = new RefOrderID(thmReq.RefOrderID);
            }
            if (thmReq.RefOrderIDSource != null) {
                msg.RefOrderIDSource = new RefOrderIDSource(thmReq.RefOrderIDSource.Value);
            }

            OnExecutionReport = onExecutionReport;
            _session.Send(msg);
        }

        //35=F
        internal void SendOrderCancelRequest(ThmOrderReq thmReq, Action<ThmExecutionReportRsp> onExecutionReport) {
            var msg = new OrderCancelRequest {
                OrderID = new OrderID(thmReq.OrderID),
                ClOrdID = new ClOrdID(thmReq.ClOrdID),
                Symbol = new Symbol(thmReq.Symbol),
                Side = new Side(thmReq.Side.Value),
                TransactTime = new TransactTime(thmReq.TransactTime),
                OrderQty = new OrderQty(thmReq.OrderQty)
            };

            OnExecutionReport = onExecutionReport;
            _session.Send(msg);
        }

        //35=G
        internal void SendOrderCancelReplaceRequest(ThmOrderReq thmReq, Action<ThmExecutionReportRsp> onExecutionReport) {
            var msg = new OrderCancelReplaceRequest {
                OrderID = new OrderID(thmReq.OrderID),
                ClOrdID = new ClOrdID(thmReq.ClOrdID),
                Symbol = new Symbol(thmReq.Symbol),
                Side = new Side(thmReq.Side.Value),
                TransactTime = new TransactTime(thmReq.TransactTime),
                OrderQty = new OrderQty(thmReq.OrderQty),
                OrdType = new OrdType(thmReq.OrdType.Value),
                Price = new Price(thmReq.Price),

                Text = new Text(thmReq.Text),
            };

            //if (thmReq.TargetParties.Any()) {
            //    msg.NoTargetPartyIDs = new NoTargetPartyIDs(thmReq.TargetParties.Count);  // 1461
            //    foreach (var targetParty in thmReq.TargetParties) {
            //        var grp = new NewOrderSingle.NoTargetPartyIDsGroup {
            //            TargetPartyID = new TargetPartyID(targetParty.TargetPartyID),
            //            TargetPartyIDSource = new TargetPartyIDSource(targetParty.TargetPartyIDSource.Value),
            //            TargetPartyRole = new TargetPartyRole(targetParty.TargetPartyRole),
            //        };

            //        msg.AddGroup(grp);
            //    }
            //}

            if (!string.IsNullOrEmpty(thmReq.OrigClOrdID)) {
                msg.OrigClOrdID = new OrigClOrdID(thmReq.OrigClOrdID);
            }
            if (!string.IsNullOrEmpty(thmReq.ExecInst)) {
                msg.OrigClOrdID = new OrigClOrdID(thmReq.ExecInst);
            }

            //msg.MatchGroup = new OrderCancelReplaceRequest.MatchGroup {
            msg.MatchInst = new MatchInst(thmReq.MatchGroup.MatchInst); // 1625
            msg.MatchAttribTagID = new MatchAttribTagID(thmReq.MatchGroup.MatchAttribTagID); // 1626
            msg.MatchAttribValue = new MatchAttribValue(thmReq.MatchGroup.MatchAttribValue); // 1627
            //}

            OnExecutionReport = onExecutionReport;
            _session.Send(msg);
        }

        // 35=H
        internal void SendOrderStatusRequest(ThmOrderReq thmReq, Action<ThmExecutionReportRsp> onExecutionReport) {
            var req = new OrderStatusRequest {
                OrderID = new OrderID(thmReq.OrderID),
                //Account = new Account(thmReq.Account),
                Symbol = new Symbol(thmReq.Symbol),
                Side = new Side(thmReq.Side.Value),
            };

            if (!string.IsNullOrEmpty(thmReq.OrdStatusReqID)) {
                req.OrdStatusReqID = new OrdStatusReqID(thmReq.OrdStatusReqID);
            }

            OnExecutionReport = onExecutionReport;
            _session.Send(req);
        }

        // 35=q
        internal void SendOrderMassCancelRequest(ThmOrderReq thmReq, Action<ThmOrderMassCancelReportRsp> onOrderMassCancel) {
            var req = new OrderMassCancelRequest {
                ClOrdID = new ClOrdID(thmReq.ClOrdID),
                MassCancelRequestType = new MassCancelRequestType(thmReq.MassCancelRequestType.Value),
                TransactTime = new TransactTime(thmReq.TransactTime),
            };

            OnOrderMassCancel = onOrderMassCancel;
            _session.Send(req);
        }

        // 35=r
        public void OnMessage(OrderMassCancelReport msg, SessionID sessionID) {
            var rlt = new ThmOrderMassCancelReportRsp {
                MassCancelRequestType = msg.MassCancelRequestType.getValue(),
            };

            try { rlt.ClOrdID = msg.ClOrdID?.getValue(); }
            catch { }
            try { rlt.MassCancelResponse = msg.MassCancelResponse?.getValue(); }
            catch { }
            try { rlt.MassCancelRejectReason = msg.MassCancelRejectReason?.getValue(); }
            catch { }
            try { rlt.Text = msg.Text?.getValue(); } //Rejection message 
            catch { }
            try { rlt.MassActionReportID = msg.MassActionReportID?.getValue(); }
            catch { }

            OnOrderMassCancel?.Invoke(rlt);
        }

        //35=8 : execution report
        public void OnMessage(ExecutionReport msg, SessionID sessionID) {
            var rlt = new ThmExecutionReportRsp {
                OrderID = msg.OrderID.getValue(),
                ClOrdID = msg.ClOrdID.getValue(),
                ExecType = msg.ExecType.getValue(), // 150
                Symbol = msg.Symbol.getValue(),
                OrdStatus = msg.OrdStatus.getValue(), // 39
                Account = msg.Account.getValue(),
            };

            try { rlt.SecondaryOrderID = msg.SecondaryOrderID.getValue(); }
            catch { }
            try { rlt.SecondaryExecID = msg.SecondaryExecID.getValue(); }
            catch { }
            try { rlt.ExecID = msg.ExecID.getValue(); }
            catch { }
            try { rlt.OrigClOrdID = msg.OrigClOrdID.getValue(); }
            catch { }
            try { rlt.OrderCategory = msg.OrderCategory.getValue(); }
            catch { }
            try { rlt.ExecInst = msg.ExecInst.getValue(); }
            catch { }
            try { rlt.Side = msg.Side.getValue(); }
            catch { }
            try { rlt.OrderQty = msg.OrderQty.getValue(); }
            catch { }
            try { rlt.OrdType = msg.OrdType.getValue(); }
            catch { }
            try { rlt.Price = msg.Price.getValue(); }
            catch { }
            try { rlt.TimeInForce = msg.TimeInForce.getValue(); }
            catch { }
            try { rlt.ExposureDuration = msg.ExposureDuration.getValue(); } //1629            
            catch { }
            try { rlt.LeavesQty = msg.LeavesQty.getValue(); }
            catch { }
            try { rlt.CumQty = msg.CumQty.getValue(); }
            catch { }
            try { rlt.LastPx = msg.LastPx.getValue(); }
            catch { }
            try { rlt.TransactTime = msg.TransactTime.getValue(); }
            catch { }
            try { rlt.Text = msg.Text.getValue(); }
            catch { }

            try {
                var noParty = msg.NoPartyIDs.getValue();
                for (int i = 1; i <= noParty; ++i) {
                    var grp = new ExecutionReport.NoPartyIDsGroup();
                    msg.GetGroup(i, grp);
                    var party = new ThmParty {
                        PartyID = grp.PartyID.getValue(),
                        PartyIDSource = grp.PartyIDSource.getValue(),
                        PartyRole = grp.PartyRole.getValue(),
                    };

                    try {
                        var noPartySub = grp.NoPartySubIDs?.getValue();
                        for (int j = 1; j <= noPartySub; ++j) {
                            var subGrp = new ExecutionReport.NoPartyIDsGroup.NoPartySubIDsGroup();
                            grp.GetGroup(j, subGrp);
                            var subParty = new ThmPartySub {
                                PartySubIDType = subGrp.PartySubIDType.getValue(),
                            };
                            try { subParty.PartySubID = subGrp.PartySubID?.getValue(); }
                            catch { }

                            party.PartySubs.Add(subParty);
                        }
                    }
                    catch { }

                    rlt.Parties.Add(party);
                }
            }
            catch { }

            try {
                var noTargetParty = msg.NoTargetPartyIDs.getValue();
                for (int i = 1; i <= noTargetParty; ++i) {
                    var grp = new ExecutionReport.NoTargetPartyIDsGroup();
                    msg.GetGroup(i, grp);

                    var targetParty = new ThmTargetParty {
                        TargetPartyID = grp.TargetPartyID.getValue(),
                        TargetPartyIDSource = grp.TargetPartyIDSource.getValue(),
                        TargetPartyRole = grp.TargetPartyRole.getValue()
                    };

                    try {
                        var noTargetPartySub = grp.NoTargetPartySubIDs.getValue();
                        for (int j = 1; j <= noTargetPartySub; ++j) {
                            var subGrp = new ExecutionReport.NoTargetPartyIDsGroup.NoTargetPartySubIDsGroup();
                            grp.GetGroup(j, subGrp);

                            var targetPartySub = new ThmTargetPartySub {
                                TargetPartySubID = subGrp.TargetPartySubID.getValue(),
                                TargetPartySubIDType = subGrp.TargetPartySubIDType.getValue()
                            };

                            targetParty.TargetPartySubs.Add(targetPartySub);
                        }
                    }
                    catch { }

                    rlt.TargetParties.Add(targetParty);
                }
            }
            catch { }

            try {
                var noValueChecks = msg.NoValueChecks.getValue(); // 1868
                for (int i = 1; i <= noValueChecks; ++i) {
                    var grp = new ExecutionReport.NoValueChecksGroup();
                    msg.GetGroup(i, grp);
                    rlt.ValueChecks.Add(new ThmValueCheck {
                        ValueCheckType = grp.ValueCheckType.getValue(),
                        ValueCheckAction = grp.ValueCheckAction.getValue(),
                    });
                }
            }
            catch { }

            OnExecutionReport?.Invoke(rlt);
        }

        #endregion

        public void Dispose() {
            _initiator.Dispose();
            _session.Dispose();
        }
    }
}

