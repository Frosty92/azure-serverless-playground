using System;
using ServerlessFuncs.Puzzles;

namespace ServerlessFuncs.History
{
    public class UserPuzzleHistory
    {
        public string ID { get; set; }
        public int RatingBefore { get; set; }
        public int RatingAfter { get; set; }
        public bool Success { get; set; }
        public int? PCompSeconds { get; set; }
        public DateTimeOffset? CompletedOn { get; set; }
        public Puzzle Puzzle { get; set; }
    }
}

