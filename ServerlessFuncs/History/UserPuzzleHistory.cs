using System;

namespace ServerlessFuncs.History
{
    public class UserPuzzleHistory
    {
        public string ID { get; set; }
        public int RatingBefore { get; set; }
        public int RatingAfter { get; set; }
        public bool Success { get; set; }
        public int? PCompSeconds { get; set; }
        public string PID { get; set; }
        public int PLevel { get; set; }
        public int PSubLevel { get; set; }
        public int PRating { get; set; }
        public DateTimeOffset? CompletedOn { get; set; }
        public string PFen { get; set; }
    }
}

