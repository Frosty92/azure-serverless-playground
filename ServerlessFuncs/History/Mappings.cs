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
                PRating = entity.PRating,
                ID = entity.RowKey,
                CompletedOn = entity.Timestamp,
                PFen = entity.PFen
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
                PSubLevel = history.PSubLevel,
                PRating = history.PRating,
                PFen = history.PFen
            };
        }
    }
}

