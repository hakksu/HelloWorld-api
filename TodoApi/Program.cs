using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
//using Microsoft.OpenApi.Models;
//using Microsoft.AspNetCore.Mvc;
using System.Reflection;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

     options.OperationFilter<RemoveIdFromRequestBodyFilter>();
});


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

public class RemoveIdFromRequestBodyFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        Console.WriteLine($"Applying RemoveIdFromRequestBodyFilter to operation: {operation.OperationId}");
        if (operation.RequestBody != null && operation.RequestBody.Content.ContainsKey("application/json"))
        {
            var schema = operation.RequestBody.Content["application/json"].Schema;
            if (schema.Properties.ContainsKey("id"))
            {
                schema.Properties.Remove("id");
            }
        }
    }
}

