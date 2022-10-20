using System;
using ServerlessFuncs.Database;

namespace ServerlessFuncs.History
{
    public class UserPuzzleHistoryEntity : BaseTableEntity
    {
        public string ID { get; set; }
        public int RatingDiff { get; set; }
        public bool Success { get; set; }
        public int? PCompSeconds { get; set; }
        public string PID { get; set; }
        public int PLevel { get; set; }
        public int PRating { get; set; }
        public string PFen { get; set; }
        public string PMoves { get; set; }

    };
};

