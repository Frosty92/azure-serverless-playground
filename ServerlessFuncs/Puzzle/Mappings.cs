using System;
namespace ServerlessFuncs.Puzzles
{
    public static class Mappings
    {
        public static PuzzleEntity ToPuzzleEntity(this Puzzle puzzle, string partitionKey)
        {
            return new PuzzleEntity()
            {
                PartitionKey = partitionKey,
                RowKey = puzzle.Id,
                Fen = puzzle.Fen,
                Moves = puzzle.Moves,
                Rating = puzzle.Rating,
                URL = puzzle.URL
                
            };
        }

        public static Puzzle ToPuzzle(this PuzzleEntity puzzleEntity)
        {
            return new Puzzle()
            {
                Id = puzzleEntity.RowKey,
                Fen = puzzleEntity.Fen,
                Moves = puzzleEntity.Moves,
                Rating = puzzleEntity.Rating,
                URL = puzzleEntity.URL
            };
        }


    }
}

