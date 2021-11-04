using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ToDoFuncApi.Models;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.Storage.Blob;

namespace ToDoFuncApi
{
    public static class ToDoApi
    {
        [FunctionName("Create")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todoitems", Connection = "AzureWebJobsStorage")] IAsyncCollector<ItemTableEntity> todoTable,
            ILogger log)
        {
            log.LogInformation("Create new Todo item");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createTodo = JsonConvert.DeserializeObject<CreateItemDTO>(requestBody);

            if (createTodo is null || string.IsNullOrWhiteSpace(createTodo?.Text)) return new BadRequestResult();

            var item = new Item { Text = createTodo.Text };

            await todoTable.AddAsync(item.ToTableEntity());

            return new OkObjectResult(item);
        }   
        
        [FunctionName("Put")]
        public static async Task<IActionResult> Put(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todoitems", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string id,
            ILogger log)
        {
            log.LogInformation("Update Todo item");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateTodo = JsonConvert.DeserializeObject<Item>(requestBody);

            if (updateTodo is null || updateTodo.Id != id) return new BadRequestResult();

            var itemTable = updateTodo.ToTableEntity();
            itemTable.ETag = "*";
            var operation = TableOperation.Replace(itemTable);
            await todoTable.ExecuteAsync(operation);

            return new NoContentResult();
        } 

        
        [FunctionName("Get")]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todoitems", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Get all Todo item");

            var query = new TableQuery<ItemTableEntity>();
            var result = await todoTable.ExecuteQuerySegmentedAsync(query, null);

            var response = new TodoItems { Items = result.Select(Mapper.ToItem).ToList() };

            return new OkObjectResult(response);
        } 
        
        [FunctionName("Delete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            [Table("todoitems", "Todo", "{id}", Connection = "AzureWebJobsStorage")] ItemTableEntity itemToRemove,
            [Table("todoitems", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string id,
            ILogger log)
        {
            log.LogInformation("Delete Todo item");

            if (string.IsNullOrWhiteSpace(id)) return new BadRequestResult();
            if (itemToRemove is null) return new NotFoundResult();


            //var itemTableToDelete = new ItemTableEntity
            //{
            //    PartitionKey = "Todo",
            //    RowKey = id,
            //    ETag = "*"
            //};

            var operation = TableOperation.Delete(itemToRemove);
            var res = await todoTable.ExecuteAsync(operation);

            return new NoContentResult();
        } 
        
        //[FunctionName("RemoveCompleted")]
        //public static async Task RemoveCompleted(
        //    [TimerTrigger("0 */1 * * * *")] TimerInfo timer,
        //    [Table("todoitems", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
        //    [Queue("todoqueue", Connection = "AzureWebJobsStorage")] IAsyncCollector<Item> queueItem,
        //    ILogger log)
        //{
        //    log.LogInformation("RemoveCompleted started...");

        //    var query = todoTable.CreateQuery<ItemTableEntity>().Where(i => i.Completed == true).AsTableQuery();

        //    // var query = new TableQuery<ItemTableEntity>().Where(TableQuery.GenerateFilterConditionForBool("Completed", QueryComparisons.Equal, true));

        //    var result = await todoTable.ExecuteQuerySegmentedAsync(query, null);

        //    foreach (var item in result)
        //    {
        //        await queueItem.AddAsync(item.ToItem());
        //        await todoTable.ExecuteAsync(TableOperation.Delete(item));
        //    }

        //}  
        
        //[FunctionName("GetRemovedFromQueue")]
        //public static async Task GetRemovedFromQueue(
        //    [QueueTrigger("todoqueue", Connection = "AzureWebJobsStorage")] Item item,
        //    [Blob("done", Connection = "AzureWebJobsStorage")] CloudBlobContainer blobContainer,
        //    ILogger log)
        //{
        //    log.LogInformation("Queue trigger started...");

        //    await blobContainer.CreateIfNotExistsAsync();
        //    var blob = blobContainer.GetBlockBlobReference($"{item.Id}.txt");
        //    await blob.UploadTextAsync($"{item.Text} is completed");

        //}
        
        
      


    }
}
