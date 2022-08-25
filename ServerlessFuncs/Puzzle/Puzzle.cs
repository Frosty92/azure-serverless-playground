using System;
using System.Collections.Generic;

namespace ServerlessFuncs.Puzzles
{
    public class Puzzle
    {
        public string Id { get; set; }
        public string Fen { get; set; }
        public string Moves { get; set; }
        public string Tags { get; set; }
    }

    public class PuzzlesResponseObj
    {
       public string PaginationToken { get; set; }
       public List<Puzzle> Puzzles = new List<Puzzle>();
    }
        
}

