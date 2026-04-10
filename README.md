# SomeBoard
Small anonymous board for text-only posts.<br/>
*Supports running in docker containers.*

## Install
- Install Docker on your server [(link to official Docker wiki)](https://docs.docker.com/get-started/get-docker/)
- Download docker-compose.yml and .env files (also download https.yml file if you want to use HTTPS on backend and frontend)
- Change docker-compose.yml and .env files if you need (enable HTTPS, use .pgpass, set path to some files, etc.)
- **Change password in .env file** (*Server will not work if password is not set*)
- Create folders /backend and /frontend in the same directory as docker-compose.yml
- Copy frontend-appsettings-base.json and backend-appsettings-base.json to created folders
- Rename both files to config.json
- Change this files if you need (add more boards, override name and description, etc., you can check ...-appsettings-example for more info)
- Compose (`docker compose up -d`)

## Build and edit
This project is .Net Core C# solution. 
You can just download repo and use dotnet restore to download all dependencies.
