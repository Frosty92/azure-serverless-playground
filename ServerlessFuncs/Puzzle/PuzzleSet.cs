using System;
using System.Collections.Generic;
using ServerlessFuncs.Puzzles;

namespace ServerlessFuncs.PuzzleNS
{
    public class PuzzleSet
    {
        public string PaginationToken { get; set; }
        public List<Puzzle> Puzzles = new List<Puzzle>();
        public int LevelNum { get; set; }
    }
}

