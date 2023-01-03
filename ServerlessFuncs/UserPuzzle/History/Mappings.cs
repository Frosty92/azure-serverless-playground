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

                PCompSeconds = entity.PCompSeconds,
                Success = entity.Success,
                ID = entity.RowKey,
                CompletedOn = entity.Timestamp,
                Puzzle = entity.Puzzle,
                RatingAfter = entity.RatingAfter,
                RatingBefore = entity.RatingBefore
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
                Puzzle = history.Puzzle
            };
        }
    }
}

