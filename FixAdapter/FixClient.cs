//-----------------------------------------------------------------------------
// File Name   : FixClient
// Author      : junlei
// Date        : 6/8/2020 4:27:15 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using FixAdapter.Models.Request;
using FixAdapter.Models.Response;

namespace FixAdapter {
    public class FixClient : IDisposable {
        /// <summary>
        /// general error, eg. logout...
        /// </summary>
        public event Action<ThmErrorRsp> OnErrorRsp;
        public event Action<ThmLogonReqRsp> OnLogonRsp;

        public event Action<ThmSecurityListRsp> OnSecurities;
        public event Action<ThmSecurityDefinitionRsp> OnSecurityDefinition;
        public event Action<ThmPartyDetailsRsp> OnPartyListRequest;

        public event Action<ThmTradeCaptureReportRsp> OnTradeCaptureReport; // AE
        public event Action<ThmTradeCaptureReportAckRsp> OnTradeCaptureReportAck; //AR
        public event Action<ThmTradeCaptureReportRequestAckRsp> OnTradeCaptureReportRequestAck; // AQ

        public event Action<ThmMarketDataSnapshotRsp> OnMarketDataSnapshot;
        public event Action<ThmMarketDataIncrementalRsp> OnMarketDataIncremental;

        public event Action<ThmExecutionReportRsp> OnExecutionReport;  // D: 8
        //public event Action<ThmOrderRsp> OnOrder;
        public event Action<ThmOrderMassCancelReportRsp> OnOrderMassCancel;

        private readonly OTCFixApp _client;
        public FixClient(string newPassword = null) {
            _client = new OTCFixApp(newPassword);
        }

        public void Start() {
            _client.Start(OnLogonRsp, OnErrorRsp);
        }

        //x: y
        public void SendSecurityListRequest(ThmSecurityListReq thmReq) {
            _client.SendSecurityListRequest(thmReq, OnSecurities);
        }

        //CF: CG
        public void SendPartyDetailsListRequest(ThmPartyDetailsReq thmReq) {
            _client.SendPartyDetailsListRequest(thmReq, OnPartyListRequest);
        }

        // c: d
        public void SendSecurityDefinitionRequest(ThmSecurityDefinitionReq thmReq) {
            _client.SendSecurityDefinitionRequest(thmReq, OnSecurityDefinition);
        }

        //AE: AE, AR
        public void SendTradeCaptureReport(ThmTradeCaptureReportReq thmReq) {
            _client.SendTradeCaptureReport(thmReq, OnTradeCaptureReport, OnTradeCaptureReportAck);
        }

        // AD: AE, AQ
        public void SendTradeCaptureReportRequest(ThmTradeSubscriptionReq thmReq) {
            _client.SendTradeCaptureReportRequest(thmReq, OnTradeCaptureReport, OnTradeCaptureReportRequestAck);
        }

        // V: W, X
        public void SendMarketDataRequest(ThmMarketDataReq thmReq) {
            _client.SendMarketDataRequest(thmReq, OnMarketDataSnapshot, OnMarketDataIncremental);
        }

        // D: 8
        public void SendOrderNewRequest(ThmOrderReq req) {
            _client.SendOrderNewRequest(req, OnExecutionReport);
        }

        // F: 8
        public void SendOrderCancelRequest(ThmOrderReq req) {
            _client.SendOrderCancelRequest(req, OnExecutionReport);
        }

        //G: 8
        public void SendOrderCancelReplaceRequest(ThmOrderReq req) {
            _client.SendOrderCancelReplaceRequest(req, OnExecutionReport);
        }

        //H: 8
        public void SendOrderStatusRequest(ThmOrderReq req) {
            _client.SendOrderStatusRequest(req, OnExecutionReport);
        }

        //q: r
        public void SendOrderMassCancelRequest(ThmOrderReq req) {
            _client.SendOrderMassCancelRequest(req, OnOrderMassCancel);
        }

        public void Dispose() {
            _client.Stop();
            _client.Dispose();
        }
    }
}
