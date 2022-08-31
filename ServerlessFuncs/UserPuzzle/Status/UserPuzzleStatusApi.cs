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
using Microsoft.WindowsAzure.Storage.Table;
using System.Reflection.Emit;
using ServerlessFuncs.Puzzles;
using Azure;
using AzureFunctionsTodo.TableStorage;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ServerlessFuncs.PuzzleNS;
using ServerlessFuncs.UserPuzzle.Status;

namespace ServerlessFuncs.UserProgress
{
    public static class UserPuzzleStatusApi
    {
        private const string Route = "userPuzzleStatus";
        private const string UserPuzzleStatusTable = "userPuzzleStatus";
        private const string PuzzlesTable = "puzzles";
        private const string UserPuzzleHistoryTable = "userPuzzleHistory";


        [FunctionName("GetUserPuzzleStatus")]
        public static async Task<IActionResult> GetUserPuzzleStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserPuzzleStatusTable, "{userID}","{userID}", Connection = "AzureWebJobsStorage")] UserPuzzleStatusEntity progressEntity,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            string userID
            )
        {
            UserPuzzleStatus puzzleStatus;
            if (progressEntity == null)
            {
                log.LogInformation("Running puzzle ctr");
                puzzleStatus = new UserPuzzleStatus()
                {
                    LoopNum = 1,
                    LevelNum = 1,
                    LastCompletedPuzzleIndex = -1,
                    UserRating = 1200,
                    IsNewUser = true
                };

            } else
            {
                puzzleStatus = progressEntity.ToUserPuzzleStatus();
            }

            await UpdatePuzzleStatusWithPuzzleSet(puzzlesTable, puzzleStatus);


            return new OkObjectResult(puzzleStatus);
        }

        [FunctionName("CreateUserPuzzleStatus")]
        public static async Task<IActionResult> CreateuserPuzzleStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserPuzzleStatusTable, Connection = "AzureWebJobsStorage")] IAsyncCollector<UserPuzzleStatusEntity> progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            string userID
            )
        {
            try
            {
                string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
                UserPuzzleStatus puzzStatus = JsonConvert.DeserializeObject<UserPuzzleStatus>(reqBody);

                await progressTable.AddAsync(puzzStatus.ToUserPuzzleStatusEntity(userID));
                if (puzzStatus.GetNextPuzzleSet)
                {
                    var nextPuzzleSet = await GetNextPuzzleSet(puzzlesTable, puzzStatus.LevelNum, puzzStatus.NextPageToken);
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
            [Table(UserPuzzleStatusTable, "{userID}", Connection = "AzureWebJobsStorage")] TableClient progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            string userID
            )
        {

            try
            {
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
                existingRow.LoopNum = updatedEntity.LoopNum;
                existingRow.CurrentPageToken = updatedEntity.CurrentPageToken;
                existingRow.NextPageToken = updatedEntity.NextPageToken;
                existingRow.UserRating = updatedEntity.UserRating;

                await progressTable.UpdateEntityAsync(existingRow, existingRow.ETag, TableUpdateMode.Replace);

                if (updatedEntity.GetNextPuzzleSet)
                {
                    var nextPuzzleSet = await GetNextPuzzleSet(puzzlesTable, updatedEntity.LevelNum, updatedEntity.NextPageToken);
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



        private static async Task<PuzzleSet> GetNextPuzzleSet(TableClient puzzlesTable, int levelNum, string nextPageToken)
        {
            var puzzleSetFetcher = new PuzzleSetFetcher(puzzlesTable);
            var puzzleSet = await puzzleSetFetcher.FetchPuzzleSet(
                levelNum,
                PuzzleSetFetcher.PUZZLES_PER_PAGE - 1,
                null,
                nextPageToken
                );

            return puzzleSet;
        }

        private static async Task UpdatePuzzleStatusWithPuzzleSet(TableClient puzzlesTable, UserPuzzleStatus userStatus)
        {
            var puzzleSetFetcher = new PuzzleSetFetcher(puzzlesTable);
            var puzzleSet = await puzzleSetFetcher.FetchPuzzleSet(
                userStatus.LevelNum,
                userStatus.LastCompletedPuzzleIndex,
                userStatus.CurrentPageToken,
                userStatus.NextPageToken
                );

            userStatus.Puzzles = puzzleSet.Puzzles;
            userStatus.CurrentPageToken = puzzleSet.CurrentPageToken;
            userStatus.NextPageToken = puzzleSet.NextPageToken;
            userStatus.LevelNum = puzzleSet.LevelNum;
            userStatus.LastCompletedPuzzleIndex = puzzleSet.LastCompletedPuzzleIndex;          
        }
    }
}
