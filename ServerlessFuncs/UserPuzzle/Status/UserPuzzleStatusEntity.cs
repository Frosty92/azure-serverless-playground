using System;
using System.Collections.Generic;
using AzureFunctionsTodo.TableStorage;
using ServerlessFuncs.Database;

namespace ServerlessFuncs.UserPuzzle.Progress
{
    public class UserPuzzleStatusEntity : BaseTableEntity
    {
        public int LoopNum { get; set; }
        public int LastCompletedPuzzleIndex { get; set; }
        public int LevelNum { get; set; }
        public int SubLevel { get; set; }
        public int UserRating { get; set; }
        public int TotalPuzzlesCompleted { get; set; }
        public int PuzzlesCompletedForLevel { get; set; }
        public string UserName { get; set; }
    }
}

