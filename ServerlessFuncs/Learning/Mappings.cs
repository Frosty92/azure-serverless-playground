using AzureFunctionsTodo.TableStorage;
using ServerlessFuncs.TableStorage;

public static class Mappings
{
    public static TodoTableEntity ToTableEntity(this Todo todo)
    {
        return new TodoTableEntity()
        {
            PartitionKey = "TODO",
            RowKey = todo.Id,
            CreatedTime = todo.CreatedTime,
            IsCompleted = todo.IsCompleted,
            TaskDescription = todo.TaskDescription
        };
    }

    public static Todo ToTodo(this TodoTableEntity todo)
    {
        return new Todo()
        {
            Id = todo.RowKey,
            CreatedTime = todo.CreatedTime,
            IsCompleted = todo.IsCompleted,
            TaskDescription = todo.TaskDescription
        };
    }

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

}
