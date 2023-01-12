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
using ServerlessFuncs.UserPuzzle.Progress;
using Azure;
using ServerlessFuncs.PuzzleNS;
using System.Security.Claims;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;
using System.Net.Http.Headers;
using ServerlessFuncs.User;
using ServerlessFuncs.History;
using System.Collections.Generic;
using ServerlessFuncs.Auth;
using System.Linq;
using ServerlessFuncs.UserPuzzle.History;

namespace ServerlessFuncs.UserProgress
{
    public static class UserPuzzleStatusApi
    {
        private const string Route = "userPuzzleStatus";
        private const string UsetPuzzleStatusTable = "userPuzzleStatus";
        private const string PuzzlesTable = "puzzles";


        [FunctionName("GetUserPuzzleStatus")]
        public static async Task<IActionResult> GetUserPuzzleStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UsetPuzzleStatusTable, "{userID}","{userID}", Connection = "AzureWebJobsStorage")] UserPuzzleStatusEntity progressEntity,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            string userID,
            ClaimsPrincipal principal
            )
        {
            try
            {
                bool isValid = ClaimsPrincipleValidator.Validate(principal, userID, req.Headers);
                if (isValid == false)
                {
                    return new UnauthorizedResult();
                }
                UserPuzzleStatus puzzleStatus;
                if (progressEntity == null)
                {
                    puzzleStatus = new UserPuzzleStatus()
                    {
                        LoopNum = 1,
                        LevelNum = 1,
                        LastCompletedPuzzleIndex = -1,
                        UserRating = 1100,
                        SubLevel = 1,
                    };
                    puzzleStatus.PuzzlesCompletedForLevel = 0;
                    puzzleStatus.LevelPuzzleCount = PuzzleSetFetcher.PUZZLE_COUNT_LVL_1;
                }
                else
                {
                    puzzleStatus = progressEntity.ToUserPuzzleStatus();
                }

                await UpdatePuzzleStatusWithPuzzleSet(puzzlesTable, puzzleStatus);


                return new OkObjectResult(puzzleStatus);
            } catch (Exception ex)
            {
                log.LogError($"for user ID: {userID}. Excep is: {ex.ToString()}");
                return new BadRequestObjectResult(ex.ToString());
            }
        }


        [FunctionName("UpdatePuzzleLevel")]
        public static async Task<IActionResult> UpdatePuzzleLevel(
           [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route ="updateLevel/{userID}")] HttpRequest req,
           [Table(UsetPuzzleStatusTable, "{userID}", Connection = "AzureWebJobsStorage")] TableClient progressTable,
           [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
           [Table(UserPuzzleHistoryApi.UserPuzzleHistoryTable, Connection = "AzureWebJobsStorage")] TableClient historyTable,
           ILogger log,
           string userID,
           ClaimsPrincipal principal
           )
        {

            try
            {
                bool isValid = ClaimsPrincipleValidator.Validate(principal, userID, req.Headers);
                if (isValid == false)
                {
                    return new UnauthorizedResult();
                }

                UserPuzzleStatusEntity existingRow = null;
                try
                {
                    var findResult = await progressTable.GetEntityAsync<UserPuzzleStatusEntity>(userID, userID);
                    existingRow = findResult.Value;
                }
                catch (RequestFailedException e) when (e.Status == 404)
                {
                    return new NotFoundResult();
                }

                existingRow.LevelNum = Convert.ToInt16(req.Query["updatedLevel"]);
                existingRow.SubLevel = 1;
                existingRow.LastCompletedPuzzleIndex = -1;
                existingRow.PuzzlesCompletedForLevel = 0;
                

                await progressTable.UpdateEntityAsync(existingRow, existingRow.ETag, TableUpdateMode.Replace);

                var puzzleSetFetcher = new PuzzleSetFetcher(puzzlesTable);
                var puzzleSet = await puzzleSetFetcher.FetchPuzzleSet(
                    existingRow.LevelNum,
                    existingRow.SubLevel,
                    existingRow.LastCompletedPuzzleIndex //this will trigger the next sequence to load
                    );

                return new OkObjectResult(puzzleSet);


            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
        }

        [FunctionName("CreateUserPuzzleStatus")]
        public static async Task<IActionResult> CreateuserPuzzleStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UsetPuzzleStatusTable, Connection = "AzureWebJobsStorage")] IAsyncCollector<UserPuzzleStatusEntity> progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            [Table(UserPuzzleHistoryApi.UserPuzzleHistoryTable, Connection = "AzureWebJobsStorage")] TableClient historyTable,
            ILogger log,
            string userID,
            ClaimsPrincipal principal
            )
        {
            try
            {
                bool isValid = ValidateUserID(principal, userID, req.Headers);
                if (isValid == false)
                {
                    return new UnauthorizedResult();
                }

                string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
                UserPuzzleStatus puzzStatus = JsonConvert.DeserializeObject<UserPuzzleStatus>(reqBody);


                await PostCompletedPuzzleHistory(historyTable, puzzStatus.CompletedPuzzles, userID);

                await progressTable.AddAsync(puzzStatus.ToUserPuzzleStatusEntity(userID));
                if (puzzStatus.GetNextPuzzleSet)
                {
                    var nextPuzzleSet = await GetNextPuzzleSet(puzzlesTable, puzzStatus.LevelNum, puzzStatus.SubLevel);
                    return new OkObjectResult(nextPuzzleSet);
                }
                else
                {
                    return new OkResult();
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
        }


        [FunctionName("UpdateUserPuzzleStatus")]
        public static async Task<IActionResult> UpdateUserPuzzleStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Put", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UsetPuzzleStatusTable, "{userID}", Connection = "AzureWebJobsStorage")] TableClient progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            [Table(UserPuzzleHistoryApi.UserPuzzleHistoryTable, Connection = "AzureWebJobsStorage")] TableClient historyTable,
            ILogger log,
            string userID,
            ClaimsPrincipal principal
            )
        {

            try
            {
                bool isValid = ClaimsPrincipleValidator.Validate(principal, userID, req.Headers);
                if (isValid == false)
                {
                    return new UnauthorizedResult();
                }


                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedEntity = JsonConvert.DeserializeObject<UserPuzzleStatus>(requestBody);

                UserPuzzleStatusEntity existingRow = null;
                try
                {
                    var findResult = await progressTable.GetEntityAsync<UserPuzzleStatusEntity>(userID, userID);
                    existingRow = findResult.Value;
                }
                catch (RequestFailedException e) when (e.Status == 404)
                {
                    return new NotFoundResult();
                }


                existingRow.LastCompletedPuzzleIndex = updatedEntity.LastCompletedPuzzleIndex;
                existingRow.LevelNum = updatedEntity.LevelNum;
                existingRow.SubLevel = updatedEntity.SubLevel;
                existingRow.LoopNum = updatedEntity.LoopNum;
                existingRow.UserRating = updatedEntity.UserRating;
                existingRow.PuzzlesCompletedForLevel = updatedEntity.PuzzlesCompletedForLevel;
                existingRow.TotalPuzzlesCompleted = updatedEntity.TotalPuzzlesCompleted;

                await progressTable.UpdateEntityAsync(existingRow, existingRow.ETag, TableUpdateMode.Replace);

                await PostCompletedPuzzleHistory(historyTable, updatedEntity.CompletedPuzzles, userID);

                if (updatedEntity.GetNextPuzzleSet)
                {
                    var nextPuzzleSet = await GetNextPuzzleSet(puzzlesTable, updatedEntity.LevelNum, updatedEntity.SubLevel);
                    return new OkObjectResult(nextPuzzleSet);
                } else
                {
                    return new OkResult();
                }

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
        }



        private static async Task<PuzzleSet> GetNextPuzzleSet(TableClient puzzlesTable, int levelNum, int subLevel)
        {
            var puzzleSetFetcher = new PuzzleSetFetcher(puzzlesTable);
            var puzzleSet = await puzzleSetFetcher.FetchPuzzleSet(
                levelNum,
                subLevel,
                PuzzleSetFetcher.PUZZLES_PER_PAGE - 1 //this will trigger the next sequence to load
                );

            return puzzleSet;
        }

        private static async Task UpdatePuzzleStatusWithPuzzleSet(TableClient puzzlesTable, UserPuzzleStatus userStatus)
        {
            var puzzleSetFetcher = new PuzzleSetFetcher(puzzlesTable);
            var puzzleSet = await puzzleSetFetcher.FetchPuzzleSet(
                userStatus.LevelNum,
                userStatus.SubLevel,
                userStatus.LastCompletedPuzzleIndex
                );

            if (puzzleSet.Puzzles.Count <= 6)
            {
                userStatus.NextPuzzleSet = await puzzleSetFetcher.FetchPuzzleSet(
                userStatus.LevelNum,
                userStatus.SubLevel + 1,
                -1
                );
            }
            userStatus.Puzzles = puzzleSet.Puzzles;
            userStatus.SubLevel = puzzleSet.SubLevel;
            userStatus.LevelNum = puzzleSet.LevelNum;
            userStatus.LastCompletedPuzzleIndex = puzzleSet.LastCompletedPuzzleIndex;
            userStatus.LevelPuzzleCount = puzzleSet.LevelPuzzleCount;
        }



        private static bool ValidateUserID(ClaimsPrincipal principle, string userID, IHeaderDictionary headers)
        {
            if (headers.ContainsKey("Host"))
            {
                string host = headers["Host"];
                if (host.Contains("localhost")) return true;
            }
            foreach (Claim claim in principle.Claims)
            {
                if (claim.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")
                {
                   
                    string tokenUserID = claim.Value;
                    return tokenUserID == userID;
                }
            }
            return false;
        }


        private static async Task PostCompletedPuzzleHistory(TableClient historyTable, List<UserPuzzleHistory> historyList, string userID)
        {
            try
            {
                await PostCompletedPuzzleHistoryByPartition(historyTable, historyList, userID);

                await PostCompletedPuzzleHistoryByPartition(
                    historyTable,
                    historyList.Where(r => r.Success == false).ToList(),
                    UserHistoryPartitionKeys.GetForWrong(userID)
                );

                await PostCompletedPuzzleHistoryByPartition(
                    historyTable,
                    historyList.Where(r => r.Marked == true).ToList(),
                    UserHistoryPartitionKeys.GetForMarked(userID)
                );

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"exception is: {ex}");
            }
        }


        private static async Task PostCompletedPuzzleHistoryByPartition(TableClient historyTable, List<UserPuzzleHistory> historyList, string partitionKey)
        {
            try
            {
                if (historyList.Count == 0) return;

                var batchTrans = new List<TableTransactionAction>();

                foreach (var h in historyList)
                {
                    var historyEntity = (Azure.Data.Tables.ITableEntity)h.ToUserPuzzleHistoryEntity(partitionKey);
                    var transEntity = new TableTransactionAction(TableTransactionActionType.Add, historyEntity);
                    batchTrans.Add(transEntity);
                }

                await historyTable.SubmitTransactionAsync(batchTrans);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"exception is: {ex}");
            }
        }


    }
}
