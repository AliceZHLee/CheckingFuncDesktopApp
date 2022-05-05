//-----------------------------------------------------------------------------
// File Name   : FixUtils
// Author      : junlei
// Date        : 6/5/2020 9:18:59 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;

namespace FixAdapter {
    public static class FixUtils {
        public static string GenerateID() {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssffffff");
        }

        public static string GenerateGUID() {
            return Guid.NewGuid().ToString();
        }
    }
}
