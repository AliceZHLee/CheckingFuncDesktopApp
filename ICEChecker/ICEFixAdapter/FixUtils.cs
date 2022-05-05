//-----------------------------------------------------------------------------
// File Name   : FixUtils
// Author      : Alice Li
// Date        : 24/2/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;

namespace ICEFixAdapter {
    public static class FixUtils {
        public static string GenerateID() {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssffffff");
        }

        public static string GenerateGUID() {
            return Guid.NewGuid().ToString();
        }
    }
}
