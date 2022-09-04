using System;
using System.Collections.Generic;
using ServerlessFuncs.Puzzles;

namespace ServerlessFuncs.PuzzleNS
{
    public class PuzzleSet
    {
        public List<Puzzle> Puzzles = new List<Puzzle>();
        public string CurrentPageToken { get; set; }
        public string NextPageToken { get; set; }
        public int LevelNum { get; set; }
        public int LastCompletedPuzzleIndex { get; set; }
        public int PuzzlesNum { get; set; }
        public int LevelPuzzleCount { get; set; }
    }
}

