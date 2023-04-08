using System;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFuncs.PuzzleNS;
using ServerlessFuncs.UserProgress;
using ServerlessFuncs.UserPuzzle.Progress;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using ServerlessFuncs.Auth;
using ServerlessFuncs.History;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using ITableEntity = Microsoft.WindowsAzure.Storage.Table.ITableEntity;
using Microsoft.Graph;

namespace ServerlessFuncs.User
{

    public static class UserProfileApi
    {
        private const string Route = "userProfile";
        

        [FunctionName("GetUserProfile")]
        public static async Task<IActionResult> GetUserProfile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{userID}")] HttpRequest req,
            ILogger log,
            string userID,
            ClaimsPrincipal principal
            )
        {
            try
            {
                bool claimsValid = ClaimsPrincipleValidator.Validate(principal, userID, req.Headers);
                if (claimsValid == false)
                {
                    return new UnauthorizedResult();
                }

                var profileFetcher = new UserProfileFetcher();
                var userProfile = await profileFetcher.FetchUserProfile(userID);
                return new OkObjectResult(userProfile);

            } catch (Exception ex)
            {
                log.LogError($"for user ID: {userID}. Excep is: {ex.ToString()}");
                return new BadRequestObjectResult(ex.ToString());
            }

        }
    }
}

