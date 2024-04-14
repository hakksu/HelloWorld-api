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

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class helloworldController : ControllerBase
    {
        private readonly TodoContext _context;

        public helloworldController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/TodoItems
        //[SwaggerOperation(Summary = "取得目前資料庫內所有資料")]    

        /// <remarks>
        /// 取得目前資料庫內所有資料
        /// </remarks>
        [HttpGet]           
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/TodoItems/5

        /// <remarks>
        /// 取得目前資料庫內指定資料
        /// </remarks>
        /// <param name="id">資料的 ID</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
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
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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
            long a=0;
            //var todoItemNoId = new { Name = todoItem.Name, IsComplete = todoItem.IsComplete };
            var todoItem = new TodoItem(){Id  =a, Name = todoItem1.Name, IsComplete = todoItem1.IsComplete };
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            //return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
            //return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItemNoId);
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem1);
        }



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
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }//*/
    }
}