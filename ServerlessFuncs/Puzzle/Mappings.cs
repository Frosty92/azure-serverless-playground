using System;
namespace ServerlessFuncs.Puzzles
{
    public static class Mappings
    {
        public static PuzzleEntity ToPuzzleEntity(this Puzzle puzzle, int partitionKey)
        {
            return new PuzzleEntity()
            {
                PartitionKey = partitionKey.ToString(),
                RowKey = puzzle.Id,
                Fen = puzzle.Fen,
                Moves = puzzle.Moves,
                Tags = puzzle.Tags
            };
        }

        public static Puzzle ToPuzzle(this PuzzleEntity puzzleEntity)
        {
            return new Puzzle()
            {
                Id = puzzleEntity.RowKey,
                Fen = puzzleEntity.Fen,
                Moves = puzzleEntity.Moves,
                Tags = puzzleEntity.Tags
            };
        }


    }
}

