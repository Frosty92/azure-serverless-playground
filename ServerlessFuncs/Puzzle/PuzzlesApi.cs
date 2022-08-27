using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunctionsTodo.TableStorage;
using Microsoft.AspNetCore.Routing;
using Azure.Data.Tables;
using Azure;
using ServerlessFuncs.TableStorage;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ServerlessFuncs.UserPuzzle.Status;

namespace ServerlessFuncs.Puzzles
{
    public static class PuzzlesApi
    {
        private const string TableName = "puzzles";
        private const string Route = "puzzles";

 
        [FunctionName("Table_GetPuzzlesForLevel")]
        public static async Task<IActionResult> GetPuzzlesForLevel(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{level}")] HttpRequest req,
            [Table(TableName,"{level}", Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            int level)
        {
            string currentPageToken = req.Query["currentPageToken"];
            string nextPageToken = req.Query["nextPageToken"];

            var puzzleSetFetcher = new PuzzleSetFetcher(puzzlesTable);
            var puzzleSet = await puzzleSetFetcher.FetchPuzzleSet(
                level,
                0,
                currentPageToken,
                nextPageToken
            );

            return new OkObjectResult(puzzleSet);
        }
    }
}

