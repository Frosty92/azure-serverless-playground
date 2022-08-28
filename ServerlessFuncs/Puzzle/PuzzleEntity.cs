﻿using System;
using ServerlessFuncs.Utils;

namespace ServerlessFuncs.Puzzles
{
    public class PuzzleEntity : BaseTableEntity
    {
        public string Fen { get; set; }
        public string Moves { get; set; }
        public string Tags { get; set; }
        public int Rating { get; set; }
        public string URL { get; set; }
    }
}

