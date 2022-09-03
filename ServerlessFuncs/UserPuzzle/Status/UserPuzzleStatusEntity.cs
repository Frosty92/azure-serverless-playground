using System;
using System.Collections.Generic;
using AzureFunctionsTodo.TableStorage;
using ServerlessFuncs.Utils;

namespace ServerlessFuncs.UserPuzzle.Progress
{
    public class UserPuzzleStatusEntity : BaseTableEntity
    {
        public int LevelNum { get; set; }
        public int LoopNum { get; set; }
        public int LastCompletedPuzzleIndex { get; set; }
        public string NextPageToken { get; set; }
        public string CurrentPageToken { get; set; }
        public int UserRating { get; set; }
        public int TotalPuzzlesCompleted { get; set; }
        public Dictionary<int, int> PuzzlesCompletedBylevel { get; set; } = new Dictionary<int, int>();
    }
}

