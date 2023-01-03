using System;
using ServerlessFuncs.Database;
using ServerlessFuncs.Puzzles;

namespace ServerlessFuncs.History
{
    public class UserPuzzleHistoryEntity : BaseTableEntity
    {
        public string ID { get; set; }
        public int RatingBefore { get; set; }
        public int RatingAfter { get; set; }
        public bool Success { get; set; }
        public int? PCompSeconds { get; set; }
        public Puzzle Puzzle { get; set; }

    };
};

