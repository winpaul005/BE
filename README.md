# Tasque Manager (Server Application)
This is a BackEnd server application for the Tasque Manager web app (Frontend availible here: https://github.com/winpaul005/qwerty)
# Requirements
- .NET 9.0
- ASP.NET Framework
- PostgreSQL
- Redis
# Usage
1. Install Redis (and configure it to the port 9090 if not set by default) and PostgreSQL if not installed
2. Clone the repository
3. Change the connection string (The DefaultConnection property on line 9) in appsettings.json file to the following format:
   > SERVER={SERVERADDRESS};DATABASE={TARGETDATABASE};UID={ADMINUSERNAME;PWD={ADMINPASSWD};
4. Apply migrations:
   > dotnet ef migrations add Initial
   > dotnet ef database update
5. Run server:
   > dotnet run dev
   Run frontend server
