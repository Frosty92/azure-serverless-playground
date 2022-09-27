using System;
using System.Collections.Generic;
using ServerlessFuncs.Puzzles;
using ServerlessFuncs.TableStorage;
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
                SubLevel = puzzleEntity.SubLevel,
                PuzzlesCompletedForLevel = puzzleEntity.PuzzlesCompletedForLevel,
                TotalPuzzlesCompleted = puzzleEntity.TotalPuzzlesCompleted,
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
                SubLevel = puzzleProgress.SubLevel,
                UserRating = puzzleProgress.UserRating,
                LoopNum = puzzleProgress.LoopNum,
                PuzzlesCompletedForLevel = puzzleProgress.PuzzlesCompletedForLevel,
                TotalPuzzlesCompleted = puzzleProgress.TotalPuzzlesCompleted
            };
        }
    }
}

