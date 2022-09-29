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
using ServerlessFuncs.UserPuzzle.Status;
using System.Security.Claims;
using System.Diagnostics;
using System.Security.Principal;


namespace ServerlessFuncs.UserProgress
{
    public static class UserPuzzleStatusApi
    {
        private const string Route = "userPuzzleStatus";
        private const string UserPuzzleStatusTable = "userPuzzleStatus";
        private const string PuzzlesTable = "puzzles";


        [FunctionName("GetUserPuzzleStatus")]
        public static async Task<IActionResult> GetUserPuzzleStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserPuzzleStatusTable, "{userID}","{userID}", Connection = "AzureWebJobsStorage")] UserPuzzleStatusEntity progressEntity,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
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

                foreach (Claim claim in principal.Claims)
                {
                    Debug.WriteLine(claim.Type + " : " + claim.Value + "\n");
                    log.LogInformation(claim.Type + " : " + claim.Value + "\n");
                }
                
                UserPuzzleStatus puzzleStatus;
                if (progressEntity == null)
                {
                    puzzleStatus = new UserPuzzleStatus()
                    {
                        LoopNum = 1,
                        LevelNum = 1,
                        LastCompletedPuzzleIndex = -1,
                        UserRating = 1200,
                        IsNewUser = true,
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

        [FunctionName("CreateUserPuzzleStatus")]
        public static async Task<IActionResult> CreateuserPuzzleStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserPuzzleStatusTable, Connection = "AzureWebJobsStorage")] IAsyncCollector<UserPuzzleStatusEntity> progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
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
            [Table(UserPuzzleStatusTable, "{userID}", Connection = "AzureWebJobsStorage")] TableClient progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
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
                Debug.WriteLine(claim.Type + " : " + claim.Value + "\n");
            }
            return false;
        }
    }
}
