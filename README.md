# HelloWorld-api

## 下載Docker

1. https://docs.docker.com/desktop/release-notes/
2. 依照電腦系統安裝對應程式後執行

## 使用專案

1. 下載此專案。
2. 使用 Visual Studio Code 開啟下載的資料夾位址。
3. 在TodoApi專案位置下執行終端機後，執行指令: docker compose -p redis-server-project up 
4. -p後面可自行設定名稱，可用docker ps 指令確認容器狀態


# 使用docker compose 應可架設資料庫server 下面是手動的方式

## 建立redis server

1. 在終端機中執行指令 docker run --name my-redis -d -p 6379:6379 redis 以開啟 Redis，並將容器的 6379 端口映射到主機的 6379 端口上。
2. 可用docker ps 指令確認容器狀態
3. 可在docker中使用redis-cli後輸入KEYS *查看現有資料，HGETALL 鍵名可查看資料內容。