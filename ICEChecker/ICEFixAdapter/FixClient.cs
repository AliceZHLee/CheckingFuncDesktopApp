//-----------------------------------------------------------------------------
// File Name   : FixClient
// Author      : Alice Li
// Date        : 24/2/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using ICEFixAdapter.Models.Request;
using ICEFixAdapter.Models.Response;

namespace ICEFixAdapter {
    public class FixClient : IDisposable {
        /// <summary>
        /// general error, eg. logout...
        /// </summary>
        public event Action<IceErrorRsp> OnErrorRsp;
        public event Action<IceLogonReqRsp> OnLogonRsp;

        public event Action<IceSecurityDefinitionRsp> OnSecurityDefinition;//d
        public event Action<IceDefinedStrategyRsp> OnDefinedStrategy;//d
        public event Action<IceNewsRsp> OnNews;//d

        public event Action<IceTradeCaptureReportRsp> OnTradeCaptureReport; // AE
        public event Action<IceTradeCaptureReportRequestAckRsp> OnTradeCaptureReportRequestAck; // AQ
        public event Action<IceUserCompanyResponse> OnUserCompanyResponse;//UCR
        public event Action<IceAllocationReportRsp> OnAllocationReport;//AS

        private readonly OTCFixApp _client;
        public FixClient() {
            _client = new OTCFixApp();
        }

        public void Start() {
            _client.Start(OnLogonRsp, OnErrorRsp);
        }

        // c: d,UDS
        public void SendSecurityDefinitionRequest(IceSecurityDefinitionReq thmReq) {
            _client.SendSecurityDefinitionRequest(thmReq, OnSecurityDefinition, OnDefinedStrategy, OnNews);
        }

        // AD: AE, AQ
        public void SendTradeCaptureReportRequest(IceTradeSubscriptionReq thmReq) {
            _client.SendTradeCaptureReportRequest(thmReq, OnTradeCaptureReport, OnTradeCaptureReportRequestAck, OnAllocationReport);
        }

        //UCP:UCR
        public void SendUserCompanyRequest() {
            _client.SendUserCompanyRequest(OnUserCompanyResponse);
        }

        public void Dispose() {
            _client.Stop();
            _client.Dispose();
        }
    }
}