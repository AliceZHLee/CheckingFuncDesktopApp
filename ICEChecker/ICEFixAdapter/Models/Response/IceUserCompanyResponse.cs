using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceUserCompanyResponse {
        public List<IceParty> Parties = new List<IceParty>();   
        public string Text { get; set; }
    }
}
