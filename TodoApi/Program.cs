using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// 添加身份驗證服務
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   //用戶未經驗證時將被導向的路徑
        options.AccessDeniedPath = "/Account/AccessDenied";  //拒絕訪問路徑是用戶權限不足時將被導向的路徑
    });

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

app.UseHttpsRedirection();//HTTPS 重新導向中介軟體    會擋下HTTPS外的所有請求

//靜態檔案中介軟體  啟用靜態檔案存取   /images/test.png
app.UseStaticFiles();

//Cookie原則中介軟體 
app.UseCookiePolicy(new CookiePolicyOptions
{
    // 將 Secure 設為 Always，以便在生產環境中要求安全連接
    Secure = CookieSecurePolicy.Always,  // Cookie 只能在安全連接（HTTPS）中傳遞
    // 設定 Cookie 名稱
    HttpOnly = HttpOnlyPolicy.Always,  //防止客戶端腳本訪問 Cookie。
    // 設定 Cookie 的 SameSite 屬性
    MinimumSameSitePolicy = SameSiteMode.Strict  //強制要求瀏覽器在跨站請求時不要發送 Cookie。
});

app.UseRouting();//路由中介軟體 啟用路由
app.UseCors(MyAllowSpecificOrigins);//跨源资源共享中介軟體  默認情況下允許來自任何來源的任何請求
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

