using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Diagnostics;

namespace ServerlessFuncs.Auth
{
    public static class ClaimsPrincipleValidator
    {
        public static bool Validate(
            ClaimsPrincipal principle,
            string userID,
            IHeaderDictionary headers
        )
        {

            bool isUserIDValid = false;
            bool isClaimExpired = true;


            /**
             * Claims are not added when running in localHost so for debugging purposes simply return
             * true.
             */
            if (headers.ContainsKey("Host"))
            {
                string host = headers["Host"];
                if (host.Contains("localhost")) return true;
            }
            foreach (Claim claim in principle.Claims)
            {
                Trace.WriteLine($"Claim type is: {claim.Type}. value is: {claim.Value}");
                if (claim.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")
                {
                    string tokenUserID = claim.Value;
                    if (tokenUserID == userID)
                    {
                        isUserIDValid = true;
                    }
                }

                else if (claim.Type == "exp")
                {
                    long unixTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                    long claimExpiry = Convert.ToInt64(claim.Value);
                    isClaimExpired = claimExpiry < unixTimeStamp;
                }
            }

            bool isValid = isUserIDValid && isClaimExpired == false;
            return isValid;
        }
    }
}

