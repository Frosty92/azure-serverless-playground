//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Azure.Data.Tables;
//using ServerlessFuncs.UserPuzzle.Progress;
//using Microsoft.WindowsAzure.Storage.Table;
//using System.Reflection.Emit;
//using ServerlessFuncs.Puzzles;
//using Azure;
//using AzureFunctionsTodo.TableStorage;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using ServerlessFuncs.PuzzleNS;

//namespace ServerlessFuncs.UserProgress
//{
//    public static class WTF
//    {
//        private const string Route = "userPuzzleStatus";
//        private const string UserPuzzleStatusTable = "userPuzzleStatus";
//        private const string PuzzlesTable = "puzzles";
//        private const string UserPuzzleHistoryTable = "userPuzzleHistory";

//        private const int PUZZLES_PER_PAGE = 30;

         

//        [FunctionName("TestFunc")]
//        public static async Task<IActionResult> GetUserPuzzleStatus(
//            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{userID}")] HttpRequest req,
//            [Table(UserPuzzleStatusTable, "{userID}", "{userID}", Connection = "AzureWebJobsStorage")] UserPuzzleStatusEntity progressEntity,
//            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
//            ILogger log,
//            string userID
//            )
//        {
//            UserPuzzleStatus puzzleStatus;
//            if (progressEntity == null)
//            {
//                log.LogInformation("Running puzzle ctr");
//                puzzleStatus = new UserPuzzleStatus()
//                {
//                    LoopNum = 1,
//                    LevelNum = 1,
//                    LastCompletedPuzzleIndex = 0,
//                    UserRating = 1200,
//                };

//            }
//            else
//            {
//                puzzleStatus = progressEntity.ToUserPuzzleStatus();
//            }

//            var puzzleSet = await GetPuzzleSet(
//                puzzlesTable,
//                puzzleStatus.PaginationToken,
//                puzzleStatus.LastCompletedPuzzleIndex,
//                puzzleStatus.LevelNum
//              );

//            puzzleStatus.Puzzles = puzzleSet.Puzzles;
//            puzzleStatus.PaginationToken = puzzleSet.PaginationToken;

//            return new OkObjectResult(puzzleStatus);
//        }




//        private static async Task<PuzzleSet> GetPuzzleSet(
//            TableClient puzzlesTable,
//            string paginationToken,
//            int lastCompletedIndex,
//            int levelNum
//           )
//        {
//            var puzzleSet = new PuzzleSet();
//            string filter = $"partitionKey eq {levelNum}";
//            await foreach (Page<PuzzleEntity> page in puzzlesTable.QueryAsync<PuzzleEntity>(filter).AsPages(paginationToken, PUZZLES_PER_PAGE))
//            {
//                List<PuzzleEntity> puzzlesPage = page.Values.ToList();



//                puzzleSet.PaginationToken = page.ContinuationToken;
//                puzzleSet.LevelNum = levelNum;

//                for (int i = lastCompletedIndex; i < puzzlesPage.Count; i++)
//                {
//                    var puzzle = puzzlesPage[i];
//                    puzzleSet.Puzzles.Add(puzzle.ToPuzzle());
//                }

//                break;

//                if (puzzleSet.Puzzles.Count >= PUZZLES_PER_PAGE)
//                {
//                    break;
//                }
//            }
//            return puzzleSet;
//        }

//        [FunctionName("CreateUserPuzzleStatus")]
//        public static async Task<IActionResult> CreateuserPuzzleStatus(
//            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = Route + "/{userID}")] HttpRequest req,
//            [Table(UserPuzzleStatusTable, Connection = "AzureWebJobsStorage")] IAsyncCollector<UserPuzzleStatusEntity> progressTable,
//            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
//            ILogger log,
//            string userID
//            )
//        {
//            try
//            {
//                string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
//                UserPuzzleStatus puzzStatus = JsonConvert.DeserializeObject<UserPuzzleStatus>(reqBody);

//                if (puzzStatus.GetNextPuzzleSet)
//                {
//                    var puzzleSet = await GetPuzzleSet(
//                        puzzlesTable,
//                        puzzStatus.PaginationToken,
//                        puzzStatus.LastCompletedPuzzleIndex,
//                        puzzStatus.LevelNum
//                       );

//                    puzzStatus.PaginationToken = puzzleSet.PaginationToken;
//                    puzzStatus.LastCompletedPuzzleIndex = 0;
//                    puzzStatus.Puzzles = puzzleSet.Puzzles;
//                }

//                await progressTable.AddAsync(puzzStatus.ToUserPuzzleStatusEntity(userID));

//                return puzzStatus.GetNextPuzzleSet ? new OkObjectResult(puzzStatus) : new OkResult();
//            }
//            catch (Exception ex)
//            {
//                return new BadRequestObjectResult(ex.ToString());
//            }
//        }


//        [FunctionName("UpdateUserPuzzleStatus")]
//        public static async Task<IActionResult> UpdateUserPuzzleStatus(
//            [HttpTrigger(AuthorizationLevel.Anonymous, "Put", Route = Route + "/{userID}")] HttpRequest req,
//            [Table(UserPuzzleStatusTable, "{userID}", Connection = "AzureWebJobsStorage")] TableClient progressTable,
//            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
//            ILogger log,
//            string userID
//            )
//        {

//            try
//            {
//                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
//                var updatedEntity = JsonConvert.DeserializeObject<UserPuzzleStatus>(requestBody);

//                if (updatedEntity.GetNextPuzzleSet)
//                {
//                    var puzzleSet = await GetPuzzleSet(
//                        puzzlesTable,
//                        updatedEntity.PaginationToken,
//                        updatedEntity.LastCompletedPuzzleIndex,
//                        updatedEntity.LevelNum
//                       );

//                    updatedEntity.PaginationToken = puzzleSet.PaginationToken;
//                    updatedEntity.LastCompletedPuzzleIndex = 0;
//                    updatedEntity.Puzzles = puzzleSet.Puzzles;
//                }

//                UserPuzzleStatusEntity existingRow = null;
//                try
//                {
//                    var findResult = await progressTable.GetEntityAsync<UserPuzzleStatusEntity>(userID, userID);
//                    existingRow = findResult.Value;
//                }
//                catch (RequestFailedException e) when (e.Status == 404)
//                {
//                    return new NotFoundResult();
//                }

//                existingRow.LastCompletedPuzzleIndex = updatedEntity.LastCompletedPuzzleIndex;
//                existingRow.LevelNum = updatedEntity.LevelNum;
//                existingRow.LoopNum = updatedEntity.LoopNum;
//                existingRow.PaginationToken = updatedEntity.PaginationToken;
//                existingRow.UserRating = updatedEntity.UserRating;

//                await progressTable.UpdateEntityAsync(existingRow, existingRow.ETag, TableUpdateMode.Replace);


//                return updatedEntity.GetNextPuzzleSet ? new OkObjectResult(updatedEntity) : new OkResult();
//            }
//            catch (Exception ex)
//            {
//                return new BadRequestObjectResult(ex.ToString());
//            }
//        }
//    }
//}

