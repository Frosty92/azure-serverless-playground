using System;
using System.Collections.Generic;
using ServerlessFuncs.History;
using ServerlessFuncs.Puzzles;

namespace ServerlessFuncs.User
{
    public class UserProfile
    {
        public int LoopNum { get; set; }
        public int LastCompletedPuzzleIndex { get; set; }
        public int LevelNum { get; set; }
        public int SubLevel { get; set; }
        public int UserRating { get; set; }
        public int TotalPuzzlesCompleted { get; set; }
        public int PuzzlesCompletedForLevel { get; set; }
        public string UserName { get; set; }
        public int LevelPuzzleCount { get; set; }
        public bool GetNextPuzzleSet { get; set; }
        public List<Puzzle> Puzzles = new List<Puzzle>();
        public List<UserPuzzleHistory> History = new List<UserPuzzleHistory>();
        public bool IsNewUser { get; set; }

    }
}

