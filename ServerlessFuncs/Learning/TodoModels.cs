using System;
using System.Collections.Generic;

namespace ServerlessFuncs.TableStorage
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }
    }

    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class ReturnModel
    {
        public string paginationToken { get; set; }
        public List<Todo> Todos  = new List<Todo>();
    }

}

