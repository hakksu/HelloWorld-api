using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Models;

namespace CRUD.Test;

public class Tests
{
    private TodoContext _context;
    private helloworldController _controller;

    [SetUp]
    public void Setup() //在每個測試方法執行之前執行
    {
        var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TodoContext(options);
            _context.TodoItems.AddRange(new List<TodoItem>
            {
                new TodoItem { Id = 1, Name = "Test Todo 1", IsComplete = false },
                new TodoItem { Id = 2, Name = "Test Todo 2", IsComplete = true }
            });
            _context.SaveChanges();

            _controller = new helloworldController(_context);
    }

    [TearDown]
    public void TearDown() //在每個測試方法執行之後執行。
    {
        _context.Dispose(); //釋放_context
    }
    

    [Test]
    public void GetTodoItems()
    {
        var result = _controller.GetTodoItems().Result.Value;

        Assert.That(result.Count(), Is.EqualTo(2));//因為Set資料只有兩筆
    }

    [Test]
    public void GetTodoItem_WithValidId_ReturnsTodoItem()
    {
        var result = _controller.GetTodoItem(1).Result.Value;

        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Test Todo 1"));
    }

    [Test]
    public void GetTodoItem_WithInvalidId_ReturnsNotFound()
    {
        var result = _controller.GetTodoItem(999).Result;

        Assert.That(result.Result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.NotFoundResult>()); //抓999會返回NotFound
    }

    [Test]
    public void PutTodoItem_WithValidId_UpdatesTodoItem()
    {
        long id=1;
        var updatedTodo = _context.TodoItems.Find(id);
        updatedTodo.Name = "Updated Todo";
        updatedTodo.IsComplete = true ;

        var result = _controller.PutTodoItem(id, updatedTodo).Result;
        
        var updatedResult = _context.TodoItems.Find(id);

        Assert.That(updatedResult.Name, Is.EqualTo("Updated Todo"));
        Assert.That(updatedResult.IsComplete, Is.True);
    }

    [Test]
    public void PutTodoItem_WithInvalidId_ReturnsBadRequestResult()
    {
        long id=1;
        var updatedTodo = _context.TodoItems.Find(id);
        updatedTodo.Name = "Updated Todo";
        updatedTodo.IsComplete = true ;

        id=999;
        var result = _controller.PutTodoItem(id, updatedTodo).Result;

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.BadRequestResult>()); //抓999會返回NotFound
    }

    [Test]
    public void PostTodoItem_CreatesNewTodoItem()
    {
        var newTodo = new helloworldController.TodoItemNoId { Name = "New Todo", IsComplete = false };

        var result = _controller.PostTodoItem(newTodo).Result;

        var createdResult = (Microsoft.AspNetCore.Mvc.CreatedAtActionResult)result.Result;  //當成功創建新資源時，API通常返回的一種ActionResult類型 包含有關新資源的詳細資訊，例如狀態碼、位置和新資源本身。
        var todo = (helloworldController.TodoItemNoId)createdResult.Value;

        ///Assert.That(todo.Id, Is.EqualTo(3)); 
        Assert.That(todo.Name, Is.EqualTo("New Todo"));
    }

    [Test]
    public void DeleteTodoItem_WithValidId_DeletesTodoItem()
    {
        long id=1;
        var result = _controller.DeleteTodoItem(id).Result;
        var deletedResult = _context.TodoItems.Find(id);

        Assert.That(deletedResult, Is.Null);
    }

    [Test]
    public void DeleteTodoItem_WithInvalidId_ReturnsNotFound()
    {
        var result = _controller.DeleteTodoItem(999).Result;

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.NotFoundResult>()); //抓999會返回NotFound
    }


}