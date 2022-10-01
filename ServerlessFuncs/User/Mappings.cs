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
                LastCompletedPuzzleIndex = userProfile.LastCompletedPuzzleIndex
            };
        }


        public static UserProfile ToUserProfile(this UserProfileEntity userProfileEntity)
        {
            return new UserProfile()
            {
                LoopNum = userProfileEntity.LoopNum,
                LevelNum = userProfileEntity.LevelNum,
                SubLevel = userProfileEntity.SubLevel,
                UserName = userProfileEntity.UserName,
                UserRating = userProfileEntity.UserRating,
                TotalPuzzlesCompleted = userProfileEntity.TotalPuzzlesCompleted,
                PuzzlesCompletedForLevel = userProfileEntity.PuzzlesCompletedForLevel,
                LastCompletedPuzzleIndex = userProfileEntity.LastCompletedPuzzleIndex
            };
        }
    }
}
