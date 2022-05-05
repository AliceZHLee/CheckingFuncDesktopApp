using ICEFixAdapter;
using ICEFixAdapter.Models;
using ICEFixAdapter.Models.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICEChecker.ModuleICEData {
    public static class OtcHelper {
        public static FixClient Client { get; } = new FixClient();

        public static Dictionary<string, string> MarketTypeNameNumberMapping = new() {
            ["Oil"] = "2",
            ["Oil Americas"] = "165"
        };
        static OtcHelper() {
            Client.Start();
            Task.Delay(600).Wait();
        }

        public static void Release() {
            Client.Dispose();
        }

        public static void StartTradeData() {
            //StartTradeData("Oil");
            StartTradeData("Oil Americas");
        }
        public static void StartTradeData(string MarketType) {
            #region//check yesterday
            //IceTradeSubscriptionReq req = new() {
            //    TradeReqID = Guid.NewGuid().ToString(),
            //    TradeRequestType = 0,//569
            //    SubscriptionRequestType = 0,//263
            //    PublishClearingAllocations = 1
            //    //SecurityID = MarketTypeNameNumberMapping[MarketType]//2=Oil, 165=Oil Americas
            //};
            //if (DateTime.Now.DayOfWeek == DayOfWeek.Monday) {
            //    req.Dates.Add(new TradeTime() {
            //        TradeDate = DateTime.Now.AddDays(-3).Date,
            //        TransactTime = DateTime.Now.AddDays(-3).Date.ToUniversalTime()
            //    });
            //}
            //else {
            //    req.Dates.Add(new TradeTime() {
            //        TradeDate = DateTime.Now.AddDays(-2).Date,
            //        TransactTime = DateTime.Now.AddDays(-2).Date.ToUniversalTime()
            //    });
            //}

            ////req.Dates.Add(new TradeTime() {
            ////    TradeDate = DateTime.Now.Date,
            ////    TransactTime = DateTime.Now.Date.ToUniversalTime()
            ////});
            //req.Dates.Add(new TradeTime() {
            //    TradeDate = DateTime.Now.AddDays(-1).Date,
            //    TransactTime = DateTime.Now.AddDays(-1).Date.ToUniversalTime()
            //});
            #endregion

            #region//check today
            IceTradeSubscriptionReq req = new() {
                TradeReqID = Guid.NewGuid().ToString(),
                TradeRequestType = 0,//569
                SubscriptionRequestType = 1,//263
                PublishClearingAllocations = 1
                //SecurityID = MarketTypeNameNumberMapping[MarketType]//2=Oil, 165=Oil Americas
            };


            req.Dates.Add(new TradeTime() {
                TradeDate = DateTime.Now.Date,
                TransactTime = DateTime.Now.Date.ToUniversalTime()
            });
            #endregion

            Client.SendTradeCaptureReportRequest(req);
            Task.Delay(600).Wait();
        }

        public static void StartSecurityDefinitionData(int sendType) {//total 8 request to fetch all securities/UDS
            if (sendType == 1) {
                IceSecurityDefinitionReq req = new() {
                    SecurityReqID = Guid.NewGuid().ToString(),
                    SecurityID = "165",//2,165
                    SecurityRequestType = 3,
                    CFICode = "FXXXXX"//FXXXXX,OXXXXX,OXXFXX
                };
                Client.SendSecurityDefinitionRequest(req);
                Task.Delay(600).Wait();
            }
            else {
                IceSecurityDefinitionReq req = new() {
                    SecurityReqID = Guid.NewGuid().ToString(),
                    SecurityID = "165",//2,165
                    SecurityRequestType = 101
                };
                Client.SendSecurityDefinitionRequest(req);
                Task.Delay(600).Wait();
            }
        }

        public static void StartUserCompanyRequest() {
            Client.SendUserCompanyRequest();
        }
    }
}
