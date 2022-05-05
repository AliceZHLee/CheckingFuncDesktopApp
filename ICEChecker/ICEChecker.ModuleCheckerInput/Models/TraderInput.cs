using System;
using System.Reflection;

namespace ICEChecker.ModuleCheckerInput.Models {
    public class TraderInput {
        //Con Mon Price Qty Shop Time  P* Q Sprd? Full?	Bro? Remark  Con Mon Qty B_Leg   S_Leg B_Q S_Q
        public string Contract { get; set; }//mapped to "Hub" in ICEBlock
        public string Month { get; set; }
        public double? Price { get; set; }//double
        public double? Qty { get; set; }//double
        public string Shop { get; set; }
        public DateTime? Time { get; set; }//datetime
        public string PxQ { get; set; }
        //public string IsSpreadValue { get; set; }
        public bool IsSpread { get; set; }
        public string Trader { get; set; }// can map to the login account(clearing account)
        public DateTime? TradeDate { get; set; }
        public bool IsConsistent { get; set; }
        
        public override bool Equals(object obj) {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
                return false;
            }
            else {
                Type sourceType = this.GetType();
                Type destinationType = obj.GetType();

                if (sourceType == destinationType) {
                    PropertyInfo[] sourceProperties = sourceType.GetProperties();
                    foreach (PropertyInfo pi in sourceProperties) {
                        if (pi.Name != "IsConsistent") {//this field has some update during checking, therefore no check for this field, otherwise, always cannot find duplicate even if there is
                            if (sourceType.GetProperty(pi.Name).GetValue(this, null) == null && destinationType.GetProperty(pi.Name).GetValue(obj, null) == null) {
                                // if both are null, don't try to compare  (throws exception)
                                continue;
                            }
                            else if (sourceType.GetProperty(pi.Name).GetValue(this, null) == null && destinationType.GetProperty(pi.Name).GetValue(obj, null) != null ) {
                                return false;
                            }
                            else if (sourceType.GetProperty(pi.Name).GetValue(this, null) != null && destinationType.GetProperty(pi.Name).GetValue(obj, null) == null) {
                                return false;
                            }
                            else if (!(sourceType.GetProperty(pi.Name).GetValue(this, null).ToString().ToLower() == destinationType.GetProperty(pi.Name).GetValue(obj, null).ToString().ToLower())) {
                                // only need one property to be different to fail Equals.
                                return false;
                            }
                        }
                    }
                }
                else {
                    return false;
                }

                return true;
            }
        }
    }
}
