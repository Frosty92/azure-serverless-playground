using System;
using System.Globalization;
using Microsoft.Graph;
using Microsoft.Graph.ExternalConnectors;

namespace ServerlessFuncs.History
{
    public static class Mappings
    {
        public static UserPuzzleHistory ToUserPuzzleHistry(this UserPuzzleHistoryEntity entity)
        {
            return new UserPuzzleHistory()
            {
                RatingAfter = entity.RatingAfter,
                RatingBefore = entity.RatingBefore,
                PCompSeconds = entity.PCompSeconds,
                Success = entity.Success,
                PID = entity.PID,
                PLevel = entity.PLevel,
                PSubLevel = entity.PSubLevel,
                ID = entity.RowKey
            };
        }

        public static UserPuzzleHistoryEntity ToUserPuzzleHistoryEntity(this UserPuzzleHistory history, string userID)
        {
            return new UserPuzzleHistoryEntity()
            {
                RowKey = history.ID,
                PartitionKey = userID,
                RatingAfter = history.RatingAfter,
                RatingBefore = history.RatingBefore,
                PCompSeconds = history.PCompSeconds,
                Success = history.Success,
                PID = history.PID,
                PLevel = history.PLevel,
                PSubLevel = history.PSubLevel
            };
        }


        private static string GetRowKey()
        {
            var inverseTimeKey = DateTime
                            .MaxValue
                            .Subtract(DateTime.UtcNow)
                            .TotalMilliseconds
                            .ToString(CultureInfo.InvariantCulture);
            return string.Format("{0}_{1}",
                                    inverseTimeKey,
                                    Guid.NewGuid().ToString().Substring(0, 5));
        }

    }
}

