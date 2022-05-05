//-----------------------------------------------------------------------------
// File Name   : Program
// Author      : junlei
// Date        : 5/21/2020 6:04:04 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Threading;
using FixAdapter.Models.Request;

namespace FixAdapter {
    class Program {
        static void Main() {
            var client = new FixClient();
            try {
                client.Start();
                while (true) {
                    bool quit = false;
                    _ = int.TryParse(Console.ReadLine().Trim().ToLower(), out int input);
                    switch (input) {
                        case -1:
                            quit = true;
                            break;
                        case 1: {
                            var req = new ThmSecurityListReq {
                                SecurityReqId = FixUtils.GenerateID(),
                                SecurityListRequestType = 4,
                            };

                            client.SendSecurityListRequest(req);  // done
                        }
                        break;
                        default:
                            break;
                    }

                    if (quit) {
                        break;
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            finally {
                client.Dispose();
            }
        }
    }
}
