using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFuncs.TableStorage;

namespace AzureFunctionsTodo.TableStorage;

public static class TodoApiTableStorage
{
    private const string Route = "todo";
    private const string TableName = "todos";
    private const string PartitionKey = "TODO";


    /*
    

    [FunctionName("Table_CreateTodo")]
    public static async Task<IActionResult> CreateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
        [Table(TableName, Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
        ILogger log)
    {
        try
        {
            log.LogInformation("Creating a new todo list item \n \n");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            log.LogError("This is a test error 123 \n");
            log.LogCritical("This is a test critical 666");
            var todo = new Todo() { TaskDescription = input.TaskDescription };
            await todoTable.AddAsync(todo.ToTableEntity());
            return new OkObjectResult(todo);
        } catch (Exception ex)
        {
            log.LogError(ex.ToString() + "\n");
            return new BadRequestObjectResult(ex.Message);
        }
        
    }

    [FunctionName("Table_GetTodos")]
    public static async Task<IActionResult> GetTodos(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
        [Table(TableName, Connection = "AzureWebJobsStorage")] TableClient todoTable,
        ILogger log)
    {
        string paginationToken = req.Query["paginationToken"];
        log.LogInformation("Getting todo list items");
        List<Todo> page1 = new List<Todo>();

        var returnObj = new ReturnModel();
        
        await foreach (Page<TodoTableEntity> page in todoTable.QueryAsync<TodoTableEntity>().AsPages(paginationToken, 5))
        {
            var pages = page.Values.ToList();
            returnObj.paginationToken = page.ContinuationToken;
            foreach (var p in pages)
            {          
                page1.Add(p.ToTodo());
            }
            break;
        }

        page1.OrderByDescending(r => r.CreatedTime);
        returnObj.Todos = page1;

        return new OkObjectResult(returnObj);
    }



    [FunctionName("Table_GetTodoById")]
    public static IActionResult GetTodoById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequest req,
        [Table(TableName,"TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todo,
        ILogger log, string id)
    {
        log.LogInformation("Getting todo item by id");
        if (todo == null)
        {
            log.LogInformation($"Item {id} not found");
            return new NotFoundResult();
        }
        return new OkObjectResult(todo.ToTodo());
    }

    [FunctionName("Table_UpdateTodo")]
    public static async Task<IActionResult> UpdateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequest req,
        [Table(TableName, Connection = "AzureWebJobsStorage")] TableClient todoTable,
        ILogger log, string id)
    {

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
        TodoTableEntity existingRow;
        try
        {
            var findResult = await todoTable.GetEntityAsync<TodoTableEntity>(PartitionKey, id);
            existingRow = findResult.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return new NotFoundResult();
        }

        existingRow.IsCompleted = updated.IsCompleted;
        if (!string.IsNullOrEmpty(updated.TaskDescription))
        {
            existingRow.TaskDescription = updated.TaskDescription;
        }

        await todoTable.UpdateEntityAsync(existingRow, existingRow.ETag, TableUpdateMode.Replace);

        return new OkObjectResult(existingRow.ToTodo());
    }

    [FunctionName("Table_DeleteTodo")]
    public static async Task<IActionResult> DeleteTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequest req,
        [Table(TableName, Connection = "AzureWebJobsStorage")] TableClient todoTable,
        ILogger log, string id)
    {
        try
        {
            await todoTable.DeleteEntityAsync(PartitionKey, id, ETag.All);
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return new NotFoundResult();
        }
        return new OkResult();
    }
    */
}
