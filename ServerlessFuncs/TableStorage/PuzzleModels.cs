using System;
using AzureFunctionsTodo.TableStorage;

namespace ServerlessFuncs.TableStorage
{
    public class Puzzle
    {
        public string Id { get; set; }
        public string Fen { get; set; }
        public string Moves { get; set; }
        public string Tags { get; set; }

    }


    public class PuzzleEntity : BaseTableEntity
    {
        public string Fen { get; set; }
        public string Moves { get; set; }
        public string Tags { get; set; }
    }
}

