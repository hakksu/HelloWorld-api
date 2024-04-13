using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
//using Microsoft.OpenApi.Models;
//using Microsoft.AspNetCore.Mvc;
using System.Reflection;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var redisOptions = new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }, // Redis 伺服器地址和端口號
    Password = "your_redis_password", // 如果有密碼的話
    ConnectTimeout = 5000, // 連接超時時間（毫秒）
    SyncTimeout = 5000 // 同步操作超時時間（毫秒）
};

var redisConnection = ConnectionMultiplexer.Connect(redisOptions);

builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);  //將你的 Redis 連接註冊為服務，以便在整個應用程式中使用


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



