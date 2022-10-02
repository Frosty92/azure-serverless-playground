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
using ServerlessFuncs.UserPuzzle.Status;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServerlessFuncs.User
{

    public static class UserProfileApi
    {
        private const string Route = "userProfile";
        private const string UserProfileTable = "userProfile";
        private const string PuzzlesTable = "puzzles";

        private static ILogger Logger;


        [FunctionName("GetUserProfile")]
        public static async Task<IActionResult> GetUserProfile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserProfileTable, "{userID}", "{userID}", Connection = "AzureWebJobsStorage")] UserProfileEntity progressEntity,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            string userID,
            ClaimsPrincipal principal
            )
        {
            try
            {
                Logger = log;
                bool isAuthenticated = false;

                foreach (var h in req.Headers)
                {
                    log.LogInformation($"{h.Key}: {h.Value}");
                }



                /**
                 * 
                 * This endpoint acccepts unauthenticated requests BUT if the principle 
                 * is present AND is invalid, return unauthorized result.
                 */

                if (req.Headers.ContainsKey("Authorization"))
                {
                    bool claimsValid = ValidateClaimsPrinciple(principal, userID, req.Headers);
                  if (claimsValid == false)
                  {
                    return new UnauthorizedResult();
                  }
                    else isAuthenticated = true;
                }



                UserProfile profile;
                if (isAuthenticated == false || progressEntity == null)
                {
                    profile = GetNewUserProfile();
                }
                else
                {
                    profile = progressEntity.ToUserProfile();
                }

                if (isAuthenticated && profile.UserName == null)
                {
                    profile.UserName = await new UserProfileFetcher().FetchUserName(userID);
                }

                await UpdateProfileWithPuzzleSet(puzzlesTable, profile);


                return new OkObjectResult(profile);
            }
            catch (Exception ex)
            {
                log.LogError($"for user ID: {userID}. Excep is: {ex.ToString()}");
                return new BadRequestObjectResult(ex.ToString());
            }

        }

        [FunctionName("CreateUserProfile")]
        public static async Task<IActionResult> CreateuserProfile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserProfileTable, Connection = "AzureWebJobsStorage")] IAsyncCollector<UserProfileEntity> progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            string userID,
            ClaimsPrincipal principal
            )
        {
            try
            {
                bool isValid = ValidateClaimsPrinciple(principal, userID, req.Headers);
                if (isValid == false)
                {
                    return new UnauthorizedResult();
                }

                string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
                UserProfile puzzStatus = JsonConvert.DeserializeObject<UserProfile>(reqBody);

                await progressTable.AddAsync(puzzStatus.ToUserProfileEntity(userID));
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


        [FunctionName("UpdateUserProfile")]
        public static async Task<IActionResult> UpdateUserProfile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Put", Route = Route + "/{userID}")] HttpRequest req,
            [Table(UserProfileTable, "{userID}", Connection = "AzureWebJobsStorage")] TableClient progressTable,
            [Table(PuzzlesTable, Connection = "AzureWebJobsStorage")] TableClient puzzlesTable,
            ILogger log,
            string userID,
            ClaimsPrincipal principal
            )
        {

            try
            {
                bool isValid = ValidateClaimsPrinciple(principal, userID, req.Headers);
                if (isValid == false)
                {
                    return new UnauthorizedResult();
                }


                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedEntity = JsonConvert.DeserializeObject<UserProfile>(requestBody);

                UserProfileEntity existingRow = null;
                try
                {
                    var findResult = await progressTable.GetEntityAsync<UserProfileEntity>(userID, userID);
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


        private static UserProfile GetNewUserProfile()
        {
            return new UserProfile()
            {
                LoopNum = 1,
                LevelNum = 1,
                LastCompletedPuzzleIndex = -1,
                UserRating = 1200,
                SubLevel = 1,
                PuzzlesCompletedForLevel = 0,
                LevelPuzzleCount = PuzzleSetFetcher.PUZZLE_COUNT_LVL_1
            };
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

        private static async Task UpdateProfileWithPuzzleSet(TableClient puzzlesTable, UserProfile userStatus)
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




        private static bool ValidateClaimsPrinciple(ClaimsPrincipal principle, string userID, IHeaderDictionary headers)
        {

            bool isUserIDValid = false;
            bool isClaimExpired = true;

            if (headers.ContainsKey("Host"))
            {
                string host = headers["Host"];
                if (host.Contains("localhost")) return false;
            }
            foreach (Claim claim in principle.Claims)
            {
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

                    Logger.LogInformation($"unixTimeStamo: {unixTimeStamp}. claimsExpiry: {claimExpiry}. IsExpired ? {isClaimExpired}");
                }
            }

            bool isValid = isUserIDValid && isClaimExpired == false;
            return isValid;
        }
    }
}

