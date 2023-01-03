using System;
using System.Collections.Generic;

namespace ServerlessFuncs.History
{
    public class UserPuzzleHistoryList
    {
        public List<UserPuzzleHistory> History = new List<UserPuzzleHistory>();
        public string PaginationToken { get; set; }
    }
}

