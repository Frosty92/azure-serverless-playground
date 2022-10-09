using Azure;
using Azure.Data.Tables;
using ServerlessFuncs.Database;
using ServerlessFuncs.TableStorage;
using System;

namespace AzureFunctionsTodo.TableStorage
{
    public class TodoTableEntity : BaseTableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

}