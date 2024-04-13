using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using System.Text.Json.Serialization;

//using Swashbuckle.AspNetCore.Annotations;
//using Swashbuckle.AspNetCore.Filters;

using Microsoft.OpenApi.Models;

using StackExchange.Redis;


namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class helloworldController : ControllerBase
    {
        //private readonly TodoContext _context;
        private readonly IDatabase _redisDatabase;

        //public helloworldController(TodoContext context)
        public helloworldController( IConnectionMultiplexer redisConnection)
        {
           // _context = context;
           _redisDatabase = redisConnection.GetDatabase();
        }

        // GET: api/TodoItems

        /// <remarks>
        /// 取得目前資料庫內所有資料
        /// </remarks>
        [HttpGet]  
        //[SwaggerOperation(Summary = "取得目前資料庫內所有資料")]      
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            //var todoItems = new List<TodoItem>();
            try{
                Console.WriteLine("keys"); 
                var keys = _redisDatabase.Multiplexer.GetServer("localhost", 6379)
                .Keys(pattern: "TodoItems:*")
                .Where(key => key != "TodoItems:NextId")
                .OrderBy(key => long.Parse(key.ToString().Substring("TodoItems:".Length)));
                
                var todoItems = new List<TodoItem>();

                Console.WriteLine(keys); 

                foreach (var key in keys)
                {
                    var hashEntries = await _redisDatabase.HashGetAllAsync(key);
                    
                    //var keyType = await _redisDatabase.KeyTypeAsync(key);
                    //if (keyType == RedisType.Hash)
                    //{
                        var todoItem = new TodoItem();
                        todoItem.Id=1;
                        foreach (var hashEntry in hashEntries)
                        {
                            if (hashEntry.Name == "Id")
                            {
                                todoItem.Id = long.Parse(hashEntry.Value.ToString());
                            }
                            if (hashEntry.Name == "Name")
                            {
                                todoItem.Name = hashEntry.Value.ToString();
                            }
                            else if (hashEntry.Name == "IsComplete")
                            {
                                todoItem.IsComplete =Convert.ToBoolean(hashEntry.Value.ToString());
                            }
                        }

                        todoItems.Add(todoItem);
                   // }
                }

                return todoItems;

                //return await _context.TodoItems.ToListAsync();
            }
            catch (Exception ex)
            {
                // 處理錯誤
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/TodoItems/5

        /// <remarks>
        /// 取得目前資料庫內指定資料
        /// </remarks>
        /// <param name="id">資料的 ID</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            try{
                // 檢查要取得的 TodoItem 是否存在
                var hashItem = await _redisDatabase.HashGetAllAsync($"TodoItems:{id}");
                if (hashItem== null || hashItem.Length == 0)
                {
                    return NotFound();
                }
                // 將 Hash 轉換為 TodoItem 對象
                var todoItem = new TodoItem
                {
                    Id = id,
                    Name = hashItem.SingleOrDefault(f => f.Name == "Name").Value,
                    IsComplete = bool.Parse(hashItem.SingleOrDefault(f => f.Name == "IsComplete").Value)
                };

                return todoItem;
            }
            catch (Exception ex)
            {
                // 處理錯誤
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        // PUT: api/TodoItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        
        /// <remarks>
        /// 修改資料庫內指定id處資料
        /// </remarks>
        /// <param name="id">資料的 ID</param>
        /// <response code="204">請求成功，但客戶端不需要更新目前的頁面</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
        {
            try{
                // 檢查要取得的 TodoItem 是否存在
                var hashItem = await _redisDatabase.HashGetAllAsync($"TodoItems:{id}");
                if (hashItem== null || hashItem.Length == 0 )
                {
                    return NotFound();
                }

                // 更新 TodoItem 的屬性
                var hashFields = new HashEntry[]
                {
                    new HashEntry("Id", id),
                    new HashEntry("Name", todoItem.Name),
                    new HashEntry("IsComplete", todoItem.IsComplete.ToString())
                };
                await _redisDatabase.HashSetAsync($"TodoItems:{id}", hashFields);

                return NoContent();
            }
            catch (Exception ex)
            {
                // 處理錯誤
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        public class TodoItemNoId
        {
            [JsonIgnore]
            public long Id { get; set; }
            public string? Name { get; set; }
            public bool IsComplete { get; set; }
        }


        // POST: api/TodoItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <remarks>
        /// 在資料庫內新增資料
        /// 
        ///
        ///     POST 
        ///     {
        ///        "name": "Item",
        ///        "isComplete": true
        ///     }
        ///
        /// </remarks>
        /// <response code="201" type="TodoItemNoId">請求成功且新的資源成功被創建</response>
        [HttpPost]
        //[SwaggerRequestExample(typeof(TodoItemNoId), typeof(CreateTodoItemNoIdExample))]
        [ProducesResponseType(typeof(TodoItemNoId),StatusCodes.Status201Created)]
        public async Task<ActionResult<TodoItemNoId>> PostTodoItem(TodoItemNoId todoItem1)
        {
            long a = await _redisDatabase.StringIncrementAsync("TodoItems:NextId");// 自動遞增 ID
            var todoItem = new TodoItem(){Id  =a, Name = todoItem1.Name, IsComplete = todoItem1.IsComplete };
            try
            {
                // 將 TodoItem 存儲為 Hash
                var hashFields = new HashEntry[]
                {
                    new HashEntry("Id", todoItem.Id),
                    new HashEntry("Name", todoItem.Name),
                    new HashEntry("IsComplete", todoItem.IsComplete.ToString())
                };
                await _redisDatabase.HashSetAsync($"TodoItems:{todoItem.Id}", hashFields);

                return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
            }
            catch (Exception ex)
            {
                // 處理錯誤
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
            
        }//*/



        // DELETE: api/TodoItems/5
        /// <remarks>
        /// 刪除資料庫內指定id處資料
        /// </remarks>
        /// <param name="id">資料的 ID</param>
        /// <response code="204">請求成功，但客戶端不需要更新目前的頁面</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            try
                {
                    // 檢查要刪除的 TodoItem 是否存在
                    var existingItem = await _redisDatabase.HashGetAllAsync($"TodoItems:{id}");
                    Console.WriteLine("existingItem");
                    Console.WriteLine(existingItem ); 
                    Console.WriteLine(existingItem.Length ); 
                    if (existingItem == null || existingItem.Length == 0 )
                    {
                        Console.WriteLine("existingItem12" ); 
                        return NotFound();                        
                    }

                    // 刪除指定 ID 的 TodoItem
                    await _redisDatabase.KeyDeleteAsync($"TodoItems:{id}");

                    // 返回成功的 HTTPS 回應，不帶任何內容
                    return NoContent();
                }
                catch (Exception ex)
                {
                    // 處理錯誤
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
                }
        }
    }
}
