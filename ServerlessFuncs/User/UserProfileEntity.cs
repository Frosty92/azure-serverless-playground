using System;
using System.Collections.Generic;
using ServerlessFuncs.Database;

namespace ServerlessFuncs.User
{
    public class UserProfileEntity : BaseTableEntity
    {
        public int LoopNum { get; set; }
        public int LastCompletedPuzzleIndex { get; set; }
        public int LevelNum { get; set; }
        public int SubLevel { get; set; }
        public int UserRating { get; set; }
        public int TotalPuzzlesCompleted { get; set; }
        public int PuzzlesCompletedForLevel { get; set; }
        public string UserName { get; set; }
        public int LevelPuzzleCount { get; set; }
    }
}

