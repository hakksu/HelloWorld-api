# HelloWorld-api

## 下載Docker

1. https://docs.docker.com/desktop/release-notes/
2. 依照電腦系統安裝對應程式後執行

## 使用專案

1. 下載此專案。
2. 使用 Visual Studio Code 開啟下載的資料夾位址。
3. 在TodoApi專案位置下執行終端機後，執行指令:  docker compose up
4. -p後面可自行設定名稱，可用docker ps 指令確認容器狀態

# 使用docker compose 應可架設資料庫server 下面是手動的方式

## 建立PostgreSQL server

1. 在終端機中執行指令 docker run --name MyPostgres -d -p 5432:5432 -v ./Postgres:/var/lib/postgresql/data -e POSTGRES_DB=mydatabase -e POSTGRES_USER=user -e POSTGRES_PASSWORD='12345' postgres:latest
2. 可用docker ps 指令確認容器狀態
3. dotnet ef migrations add InitialCreate 創建一個初始的資料庫遷移，用於定義您的資料庫模型的初始狀態。
4. dotnet ef database update              應用新創建的遷移，並創建與您的資料模型相對應的資料庫表格。
