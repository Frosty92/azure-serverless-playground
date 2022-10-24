using System;
using Microsoft.Graph;

namespace ServerlessFuncs.User
{
    public static class Mappings
    {
        public static UserProfileEntity ToUserProfileEntity(this UserProfile userProfile, string userID)
        {
            return new UserProfileEntity()
            {
                RowKey = userID,
                PartitionKey = userID,
                LoopNum = userProfile.LoopNum,
                LevelNum = userProfile.LevelNum,
                SubLevel = userProfile.SubLevel,
                UserName = userProfile.UserName,
                UserRating = userProfile.UserRating,
                TotalPuzzlesCompleted  = userProfile.TotalPuzzlesCompleted,
                PuzzlesCompletedForLevel = userProfile.PuzzlesCompletedForLevel,
                LastCompletedPuzzleIndex = userProfile.LastCompletedPuzzleIndex,
                PuzzlePoints = userProfile.PuzzlePoints
            };
        }


        public static UserProfile ToUserProfile(this UserProfileEntity entity)
        {
            return new UserProfile()
            {
                LoopNum = entity.LoopNum,
                LevelNum = entity.LevelNum,
                SubLevel = entity.SubLevel,
                UserName = entity.UserName,
                UserRating = entity.UserRating,
                TotalPuzzlesCompleted = entity.TotalPuzzlesCompleted,
                PuzzlesCompletedForLevel = entity.PuzzlesCompletedForLevel,
                LastCompletedPuzzleIndex = entity.LastCompletedPuzzleIndex,
                PuzzlePoints = entity.PuzzlePoints
            };
        }
    }
}
