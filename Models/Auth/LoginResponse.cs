using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace codetest.Models.Auth
{
    public class LoginResponse
    {
        public string token { get; set; }
        public DateTime expiration { get; set; }
        public bool success { get; set; }
        public Claim[] claims { get; set; }
        public bool RememberMe { get; set; }
    }
}
