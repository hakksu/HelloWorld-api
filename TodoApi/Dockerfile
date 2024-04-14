# 使用官方的 ASP.NET Core 映像檔作為基礎映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# 設定初始工作目錄為 /app
WORKDIR /app

# 在 /app 目錄下建立一個名為 src 的子目錄
RUN mkdir src

# 進入 /app/src 目錄，並設定為新的工作目錄
WORKDIR /app/src

# 複製檔案到 /app/src 目錄
COPY . .

#創建一個初始的資料庫遷移，用於定義您的資料庫模型的初始狀態。
RUN dotnet ef migrations add InitialCreate
# 執行 dotnet ef 命令來執行資料庫遷移
RUN dotnet ef database update

# 對外暴露端口
EXPOSE 8080

# 預設指令，當容器啟動時運行的命令
CMD ["dotnet", "TodoApi.dll"]
