using System;
using System.Globalization;
using Microsoft.Graph;

namespace ServerlessFuncs.History
{
    public static class Mappings
    {
        public static UserPuzzleHistory ToUserPuzzleHistry(this UserPuzzleHistoryEntity entity)
        {
            return new UserPuzzleHistory()
            {
                PuzzleID = entity.PuzzleID,
                PuzzlePartitionKey = entity.PuzzlePartitionKey,
                RatingAfter = entity.RatingAfter,
                RatingBefore = entity.RatingBefore,
                PuzzleCompletionTime = entity.PuzzleCompletionTime,
                Success = entity.Success
            };
        }

        public static UserPuzzleHistoryEntity ToUserPuzzleHistoryEntity(
            this UserPuzzleHistory history,
            string partitionKey
        )
        {
            return new UserPuzzleHistoryEntity()
            {
                PartitionKey = partitionKey,
                RowKey = GetRowKey(),
                PuzzleID = history.PuzzleID,
                PuzzlePartitionKey = history.PuzzlePartitionKey,
                RatingAfter = history.RatingAfter,
                RatingBefore = history.RatingBefore,
                PuzzleCompletionTime = history.PuzzleCompletionTime,
                Success = history.Success
            };
        }


        private static string GetRowKey()
        {
            var inverseTimeKey = DateTime
                            .MaxValue
                            .Subtract(DateTime.UtcNow)
                            .TotalMilliseconds
                            .ToString(CultureInfo.InvariantCulture);
            return string.Format("{0}-{1}",
                                    inverseTimeKey,
                                    Guid.NewGuid().ToString().Substring(0, 5));
        }
    }
}

