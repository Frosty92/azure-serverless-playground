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
            string level)
        {
            string paginationToken = req.Query["paginationToken"];

            log.LogInformation($"LEVEL provided is: {level}");

            var responseObj = new PuzzlesResponseObj();
            var puzzles = new List<Puzzle>();
            await foreach (Page<PuzzleEntity> page in puzzlesTable.QueryAsync<PuzzleEntity>().AsPages(paginationToken, 20))
            {
                List<PuzzleEntity> pages = page.Values.ToList();
                responseObj.PaginationToken = page.ContinuationToken;
                foreach (var p in pages)
                {
                    puzzles.Add(p.ToPuzzle());
                }
                break;
            }

            responseObj.Puzzles = puzzles;

            return new OkObjectResult(responseObj);
        }
    }
}

