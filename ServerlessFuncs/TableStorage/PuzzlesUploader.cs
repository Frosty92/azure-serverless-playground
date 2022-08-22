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
using System.Collections.Generic;

namespace ServerlessFuncs.TableStorage
{
    public static class PuzzlesUploader
    {
        [FunctionName("JSON_Read")]
        public static async Task<IActionResult> ReadJSON(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "parse")] HttpRequest req,
        [Table("puzzles", Connection = "AzureWebJobsStorage")] IAsyncCollector<PuzzleEntity> puzzleTable,
        ExecutionContext context,
        ILogger log)
        {
            try
            {
                var puzzles = new List<Puzzle>();
                var pEntities = new List<PuzzleEntity>();
                for (int level = 3; level <= 6; level++)
                {
                    puzzles = PuzzlesUploader.GetPuzzles(level);
                    foreach (var p in puzzles)
                    {
                        await puzzleTable.AddAsync(p.ToPuzzleEntity(level));
                    }
                }


                return new OkObjectResult(pEntities);

            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.ToString());
            }

        }


        private static List<Puzzle> GetPuzzles(int level)
        {

            string filePath = $"/Users/hamzahassan/Projects/spicy-chess/ServerlessFuncs/ServerlessFuncs/data/{GetFileName(level)}";
            var puzzles = new List<Puzzle>();
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                puzzles = JsonConvert.DeserializeObject<List<Puzzle>>(json);
            }

            return puzzles;

        }

        private static string GetFileName(int level)
        {
            if (level == 1)
            {
                return "levelOne.json";
            }
            else if (level == 2)
            {
                return "levelTwo.json";
            }
            else if (level == 3)
            {
                return "levelThree.json";
            }
            else if (level == 4)
            {
                return "levelFour.json";
            }
            else if (level == 5)
            {
                return "levelFive.json";
            }
            else if (level == 6)
            {
                return "levelSix.json";
            }
            else
            {
                throw new ArgumentOutOfRangeException($"level: {level} is out of range");
            }
        }
    }
}

