using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceNewsRsp {
        public string Headline { get; set; }
        public int Urgency { get; set; }
        public string UserName { get; set; }
        public int LinesOfText { get; set; }
        public List<string> Texts = new List<string>();
    }
}
