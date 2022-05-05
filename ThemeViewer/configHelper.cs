using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace ThemeViewer {
    public class ConfigHelper {
        public static List<string> LoadProductData(string filePath) {
            JObject o1 = JObject.Parse(File.ReadAllText(filePath));
            var productOptions = o1["Products"];
            List<string> str = new List<string>();
            if (productOptions != null) {
                foreach (var item in productOptions) {
                    str.Add(item.ToString());
                }
            }
            return str;
        }

        public static List<string> LoadAccountData(string filePath) {
            JObject o1 = JObject.Parse(File.ReadAllText(filePath));
            var accountOptions = o1["Accounts"];
            List<string> str = new List<string>();
            if (accountOptions != null) {
                foreach (var item in accountOptions) {
                    str.Add(item.ToString());
                }
            }
            return str;
        }

        public static JToken LoadAlertSetting(string filePath) {
            JObject o1 = JObject.Parse(File.ReadAllText(filePath));
            var alertSetting = o1["AlertSetting"];
            return alertSetting;
        }

        public static void UpdateAlertSetting(string filePath, bool alertOnOff, int newThreshold,bool isAlertExcludeOurTrades) {
            JObject o1 = JObject.Parse(File.ReadAllText(filePath));
            var alertSettings = o1["AlertSetting"];
            if (alertSettings != null) {
                alertSettings["AlertOn"]= alertOnOff;
                alertSettings["Threshold"]=newThreshold;
                alertSettings["ExcludeAlertForOurTrades"] = isAlertExcludeOurTrades;
            }
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(o1, Newtonsoft.Json.Formatting.Indented);
            File.Move(filePath, filePath.Replace(".json","")+"_origin_"+ DateTime.Now.ToString("yyyyMMddhhmmss")+".json");
            File.WriteAllText(filePath, output);
        }
    }
}
