using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Models;
using Moq;
namespace CRUD.Test;
using StackExchange.Redis;

public class Tests
{
    private Mock<IDatabase> mockRedis;
    private helloworldController _controller;
    private IDatabase _redisDatabase;

    [SetUp]
    public void Setup() //在每個測試方法執行之前執行
    {
        var _mockMultiplexer = new Mock<IConnectionMultiplexer>();
        mockRedis = new Mock<IDatabase>(); // 創建模擬的 Redis 數據庫 
       // mockRedis =_mockMultiplexer.Object.GetDatabase();    
        // 設置對 IConnectionMultiplexer 的模擬
       // _mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockRedis.Object);    
        
        
        // 設置模擬的 Redis 數據庫的行為
        mockRedis.Setup(db => db.HashGetAllAsync(It.Is<RedisKey>(x => x=="TodoItems:999"), It.IsAny<CommandFlags>()))
                .ReturnsAsync(new HashEntry[]{}); 

        //mockRedis.Setup(db => db.HashGetAllAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
        //        .ReturnsAsync(new HashEntry[] { /* 模擬返回的數據 */ });
        
        // 設置模擬的 Redis 數據庫的行為，當刪除指定鍵時，返回 true 表示成功
        mockRedis.Setup(db => db.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

        
        // 添加兩筆假資料到模擬的資料庫
        mockRedis.Setup(db => db.HashGetAllAsync(It.Is<RedisKey>(x => x=="TodoItems:1"), It.IsAny<CommandFlags>()))
                .ReturnsAsync(new HashEntry[]{
                 new HashEntry("Id", "1"),
                 new HashEntry("Name", "Test Todo 1"),
                 new HashEntry("IsComplete", "True")
                }); 
        mockRedis.Setup(db => db.HashGetAllAsync(It.Is<RedisKey>(x => x=="TodoItems:2"), It.IsAny<CommandFlags>()))
                .ReturnsAsync(new HashEntry[]{
                 new HashEntry("Id", "2"),
                 new HashEntry("Name", "Test Todo 2"),
                 new HashEntry("IsComplete", "True")
                }); 
        // Arrange
        var expectedItems = new List<TodoItem>
        {
            new TodoItem { Id = 1, Name = "TodoItem 1", IsComplete = true },
            new TodoItem { Id = 2, Name = "TodoItem 2", IsComplete = false },
            // Add more expected items as needed
        };

        // Set up mock behavior
        var mockServer = new Mock<IServer>();
        mockServer.Setup(s => s.Keys(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
        .Returns(new RedisKey[] { "TodoItems:1", "TodoItems:2" });
        //mockServer.Setup(m => m.Keys(It.IsAny<int>(),It.IsAny<RedisValue>())).Returns();
        _mockMultiplexer.Setup(m => m.GetServer(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>())).Returns(mockServer.Object);

        // 設置 Database 的行為
        mockRedis.Setup(db => db.Multiplexer)
            .Returns(_mockMultiplexer.Object);
/*
       mockRedis.Setup(db => db.HashSetAsync(It.Is<RedisKey>(x => x=="TodoItems:1")),  HashEntry[] hashFields,It.IsAny<CommandFlags>()))
                .ReturnsAsync(new HashEntry[]{
                 new HashEntry("Id", "1"),
                 new HashEntry("Name", "Test Todo 1"),
                 new HashEntry("IsComplete", "True")
                }); */

        // 設置對 IConnectionMultiplexer 的模擬
        //_mockMultiplexer.Setup(m => m.GetServer(It.IsAny<string>(), It.IsAny<object>())).Returns(mockRedis.Object);  
        _mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockRedis.Object);  
       
       

        _redisDatabase = mockRedis.Object; // 使用模擬的 Redis 數據庫
        _controller = new helloworldController(_mockMultiplexer.Object); // 將模擬的 Redis 數據庫注入到控制器中
    }

    [TearDown]
    public void TearDown() //在每個測試方法執行之後執行。
    {
       mockRedis= null;
       _controller = null;
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
    public async Task PutTodoItem_WithValidId_UpdatesTodoItem()
    {
        long id=1;
        var todoItem = new TodoItem { Id = id, Name = "Updated Todo", IsComplete = false };

        // Act
        var result = await _controller.PutTodoItem(id, todoItem);

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.NoContentResult>());
    }

    [Test]
    public async Task PutTodoItem_WithInvalidId_ReturnsNotFoundResult()
    {
        long id=999;
        var todoItem = new TodoItem { Id = id, Name = "Updated TodoItem", IsComplete = true };

        
        var result = await _controller.PutTodoItem(id, todoItem);

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.NotFoundResult>()); //抓999會返回NotFound
    }

    [Test]
    public void PostTodoItem_CreatesNewTodoItem()
    {
        var newTodo = new helloworldController.TodoItemNoId { Name = "New Todo", IsComplete = false };

        var result = _controller.PostTodoItem(newTodo).Result;

        var createdResult = (Microsoft.AspNetCore.Mvc.CreatedAtActionResult)result.Result;  //當成功創建新資源時，API通常返回的一種ActionResult類型 包含有關新資源的詳細資訊，例如狀態碼、位置和新資源本身。
        var todo = (TodoItem)createdResult.Value;

        ///Assert.That(todo.Id, Is.EqualTo(3)); 
        Assert.That(todo.Name, Is.EqualTo("New Todo"));
    }

    [Test]
    public async Task DeleteTodoItem_WithValidId_DeletesTodoItem()
    {
        long id=1;

        var result = await _controller.DeleteTodoItem(id);

        //Assert.That(result, Is.Null);
        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.NoContentResult>()); //抓對會返回NoContentResult
    }

    [Test]
    public void DeleteTodoItem_WithInvalidId_ReturnsNotFound()
    {
        var result = _controller.DeleteTodoItem(999).Result;

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.NotFoundResult>()); //抓999會返回NotFound
    }


}