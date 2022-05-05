//-----------------------------------------------------------------------------
// File Name   : TitanOtcHelper
// Author      : junlei
// Date        : 8/17/2020 3:50:40 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FixAdapter;
  using FixAdapter.Models;
using FixAdapter.Models.Request;

namespace ThemeViewer {
    public static class OtcHelper {
        public static FixClient Client { get; } = new FixClient();

        static OtcHelper() {
            Client.Start();
            Task.Delay(600).Wait();
        }

        public static void Release() {
            Client.Dispose();
        }

        public static void StartMarketData() {
            ThmMarketDataReq req = new() {
                MdReqId = DateTime.Now.ToString(),
                SubscriptionRequestType = '1',
                MarketDepth = 0,
                Exchange = "SGX"
            };
            //Alice commented on 2021/09/20
            //as stated in Titan OTC API doc, the market data request, does not require the Symbol as input(page 47)

            //var thmSymbols = new List<ThmSymbol>();
            //foreach (var sym in symbols) {
            //    thmSymbols.Add(new ThmSymbol { Symbol = sym });
            //}

            //req.RelatedSymbols.AddRange(thmSymbols);

            req.MDEntries.AddRange(new List<ThmMDEntry>() {
                new ThmMDEntry() { EntryType = '2'}
            });

            Client.SendMarketDataRequest(req);
        }

        public static void StartTradeData() {
            ThmTradeSubscriptionReq req = new() {
                TradeReqID = "123123",
                TradeRequestType = 0,//569
                SubscriptionRequestType = '1'//263
            };

            Client.SendTradeCaptureReportRequest(req);
        }

        public static void StartPartyData() {
            ThmPartyDetailsReq req = new() {
                ReqID = DateTime.Now.ToString()
            };
            req.PartyListResponseTypes.Add(1);

            Client.SendPartyDetailsListRequest(req);
        }

        public static void StartSecurityData() {
            ThmSecurityListReq req = new() {
                SecurityReqId = DateTime.Now.ToString(),
                SecurityListRequestType = 4
            };

            Client.SendSecurityListRequest(req);
        }

        //public static void StartSecurityDefinitionData(string product) {
        //    ThmSecurityDefinitionReq req = new() {
        //        RequestID = DateTime.Now.ToString(),
        //        RequestType = 3,
        //        Symbol = "[N/A]",
        //        SecurityType = "FUT",

        //    };

        //    for (int i = 0; i < 12; i++) {
        //        req.Legs.Add(new ThmLeg() {
        //            LegSymbol = "[N/A]",
        //            LegSecurityID = product
        //        });
        //    }
        //    // NoLegs=12,
        //    //LegSymbol="[N/A]",
        //    //LegSecurityID= product

        //    Client.SendSecurityDefinitionRequest(req);
        //}

        internal static void StartOrderStatus() {
            
        }

    }
}
