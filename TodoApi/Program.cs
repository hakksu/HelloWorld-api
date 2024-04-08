using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Microsoft.AspNetCore.StaticFiles;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential 
    // cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;

    options.MinimumSameSitePolicy = SameSiteMode.None;
});

var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";  //https://learn.microsoft.com/zh-tw/aspnet/core/security/cors?view=aspnetcore-8.0
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://example.com",
                                              "http://www.contoso.com");
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //在開發環境中執行時
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();//開發人員例外狀況頁面中介軟體
}
else//在生產環境中執行時
{
    app.UseExceptionHandler("/Error");//例外狀況處理常式中介軟體
    app.UseHsts();//HTTP 靜態傳輸安全性通訊協定 (HSTS) 中介軟體
}

app.UseHttpsRedirection();//HTTPS 重新導向中介軟體

//靜態檔案中介軟體  啟用靜態檔案存取   /images/test1.png
app.UseStaticFiles();

app.UseCookiePolicy();//Cookie原則中介軟體 
app.UseRouting();//路由中介軟體 啟用路由
app.UseCors(MyAllowSpecificOrigins);//跨源资源共享中介軟體
app.UseAuthentication();//驗證中介軟體 身分驗證
app.UseAuthorization();//授權中介軟體  身分授權
//app.UseSession();//工作階段中介軟體
//註冊 Middleware 的方法
app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("1st Middleware in. \r\n");
                await next.Invoke();//跳至下一個Middleware的方法
                await context.Response.WriteAsync("1st Middleware out. \r\n");
            });
//app.UseResponseCompression();
//app.UseResponseCaching();

//app.MapRazorPages();

app.Run();

