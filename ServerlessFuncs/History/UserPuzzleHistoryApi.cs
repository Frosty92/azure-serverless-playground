using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using System.Security.Claims;
using ServerlessFuncs.Auth;
using Azure;
using ServerlessFuncs.Puzzles;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ServerlessFuncs.History
{
    public static class UserPuzzleHistoryApi
    {

        public const string UserPuzzleHistoryTable = "userPuzzleHistory";

        private const string Route = "userPuzzleHistory";
        private const int ENTITIES_PER_PAGE = 50;

        [FunctionName("GetUserPuzzleHistory")]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserPuzzleHistoryTable, "{userID}", Connection = "AzureWebJobsStorage")] TableClient HistoryTable,
            ClaimsPrincipal principal,
            string userID,
            ILogger log)
        {

            try
            {
                string paginationToken = req.Query["paginationToken"];



                bool isValid = ClaimsPrincipleValidator.Validate(principal, userID, req.Headers);
                if (isValid == false)
                {
                    return new UnauthorizedResult();
                }

                var historyList = new UserPuzzleHistoryList();

                await foreach (Page<UserPuzzleHistoryEntity> page
                    in HistoryTable.QueryAsync<UserPuzzleHistoryEntity>()
                    .AsPages(paginationToken, ENTITIES_PER_PAGE))
                {
                    var entities = page.Values.ToList();
                    foreach (var e in entities)
                    {
                        historyList.History.Add(e.ToUserPuzzleHistry());
                    }

                    historyList.PaginationToken = page.ContinuationToken;
                }

                return new OkObjectResult(historyList);

            } catch (Exception ex)
            {
                Trace.WriteLine($"Err is: {ex.ToString()}");
                log.LogError(ex.ToString());
                return new BadRequestObjectResult(ex.ToString());
            }

        }
    }
}

