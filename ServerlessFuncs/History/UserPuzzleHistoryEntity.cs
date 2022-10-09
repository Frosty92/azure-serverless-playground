using System;
using ServerlessFuncs.Database;

namespace ServerlessFuncs.History
{
    public class UserPuzzleHistoryEntity : BaseTableEntity
    {
        public int RatingBefore { get; set; }
        public int RatingAfter { get; set; }
        public int PuzzleCompletionTime { get; set; }
        public string PuzzleID { get; set; }
        public string PuzzlePartitionKey { get; set; }
        public bool Success { get; set; }
    }

}

