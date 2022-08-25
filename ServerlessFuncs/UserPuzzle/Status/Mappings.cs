using System;
using System.Collections.Generic;
using ServerlessFuncs.Puzzles;
using ServerlessFuncs.UserProgress;

namespace ServerlessFuncs.UserPuzzle.Progress
{
    public static class Mappings
    {
        public static UserPuzzleStatus ToUserPuzzleStatus(this UserPuzzleStatusEntity puzzleEntity)
        {
            return new UserPuzzleStatus()
            {
                LevelNum = puzzleEntity.LevelNum,
                LoopNum = puzzleEntity.LoopNum,
                UserRating = puzzleEntity.UserRating,
                LastCompletedPuzzleIndex = puzzleEntity.LastCompletedPuzzleIndex,
                PaginationToken = puzzleEntity.PaginationToken
            };
        }


        public static UserPuzzleStatusEntity ToUserPuzzleStatusEntity(this UserPuzzleStatus puzzleProgress, string userID)
        {
            return new UserPuzzleStatusEntity()
            {
                PartitionKey = userID,
                RowKey = userID,
                LastCompletedPuzzleIndex = puzzleProgress.LastCompletedPuzzleIndex,
                LevelNum = puzzleProgress.LevelNum,
                UserRating = puzzleProgress.UserRating,
                PaginationToken = puzzleProgress.PaginationToken,
                LoopNum = puzzleProgress.LoopNum
            };
        }
    }
}

