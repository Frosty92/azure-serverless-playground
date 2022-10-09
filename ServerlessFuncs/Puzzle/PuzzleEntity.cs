using System;
using ServerlessFuncs.Database;

namespace ServerlessFuncs.Puzzles
{
    public class PuzzleEntity : BaseTableEntity
    {
        public string Fen { get; set; }
        public string Moves { get; set; }
        public int Rating { get; set; }
        public string URL { get; set; }
    }
}

