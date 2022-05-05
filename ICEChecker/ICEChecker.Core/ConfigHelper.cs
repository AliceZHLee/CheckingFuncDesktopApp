using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace ICEChecker.Core {
    public class ConfigHelper {      
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

        public static JToken LoadLoginSetting(string filePath) {
            JObject o1 = JObject.Parse(File.ReadAllText(filePath));
            var loginSetting = o1["LoginSetting"];
            return loginSetting;
        }

        public static void UpdateUserSetting(string filePath,  string userID) {
            JObject o1 = JObject.Parse(File.ReadAllText(filePath));
            var loginSetting = o1["LoginSetting"];
            if (loginSetting != null) {
                loginSetting["FirstTimeLogin"] = false;
                loginSetting["UserID"] = userID;
            }
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(o1, Newtonsoft.Json.Formatting.Indented);
            File.Move(filePath, filePath.Replace(".json", "") + "_origin_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".json");
            File.WriteAllText(filePath, output);
        }
    }
}
