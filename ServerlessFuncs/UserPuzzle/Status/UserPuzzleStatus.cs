using System;
using System.Collections.Generic;
using ServerlessFuncs.History;
using ServerlessFuncs.PuzzleNS;
using ServerlessFuncs.Puzzles;

namespace ServerlessFuncs.UserProgress
{
    public class UserPuzzleStatus
    {
        public int LevelNum { get; set; }
        public int SubLevel { get; set; }
        public int LevelPuzzleCount { get; set; }
        public int LoopNum { get; set; }
        public int LastCompletedPuzzleIndex { get; set; }
        public int UserRating { get; set; }
        public List<Puzzle> Puzzles = new List<Puzzle>();
        public PuzzleSet NextPuzzleSet = null;
        public List<UserPuzzleHistory> CompletedPuzzles = new List<UserPuzzleHistory>();
        public bool GetNextPuzzleSet { get; set; }
        public int TotalPuzzlesCompleted { get; set; }
        public int PuzzlesCompletedForLevel { get; set; }
    }
}

