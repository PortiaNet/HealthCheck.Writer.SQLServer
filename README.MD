<img src="https://user-images.githubusercontent.com/11803924/159634426-45568864-0568-441c-9d0b-a28756f42f9f.png" width="400">


[![Nuget](https://img.shields.io/nuget/v/PortiaNet.HealthCheck.Writer.SQLServer?label=NuGet)](https://www.nuget.org/packages/PortiaNet.HealthCheck.Reporter/)

This library is the SQL writer for the [PortiaNet.HealthCheck.Reporter](https://github.com/PortiaNet/HealthCheck.Reporter) and saves all reported logs to the configured Microsoft SQL Server Database.

### Installation

You can install this tool from [Nuget](https://www.nuget.org/packages/PortiaNet.HealthCheck.Writer.SQLServer) using Package Manager Console:

```
PM> Install-Package PortiaNet.HealthCheck.Writer.SQLServer
```

### How do I get started?

After adding the [HealthCheck](https://github.com/PortiaNet/HealthCheck.Reporter/blob/master/PortiaNet.HealthCheck.Reporter/HealthCheckMiddleware.cs) middleware to the application pipeline, you need to add this library to DI and configure it as you prefer.

``` C#
using PortiaNet.HealthCheck.Writer;

var builder = WebApplication.CreateBuilder(args);
...
builder.Services.AddSQLServerWriter(options =>
{
    options.TableName = "RequestTracks"; // Default Table Name
    options.MuteOnError = false; // To suppress/not suppress the internal errors like SQL Connection or privilege exceptions
    options.NodeName = "Main Node"; // When having different instances of one service, this parameter will help you to distinguish between their logs in the database 
    options.ConnectionString = "..."; // MS SQL Server Connection String to the database which you want to save the logs in it
});
...
```

### How it works
By configuring the **PortiaNet.HealthCheck.Writer.SQLServer** library in DI, it adds a singletone instance of [IHealthCheckReportService](https://github.com/PortiaNet/HealthCheck.Reporter/blob/master/PortiaNet.HealthCheck.Reporter/IHealthCheckReportService.cs) to the dependency injection. When the initializer of the injected class gets called, it first checks if the database which is mentioned in the connection string exists or not. If not then it will try to create a new one, **Then the provided SQL Server credential should have _dbcreator_ privilege** for this process.
After ensuring the database, the instance checks the table existence which is mentioned in the configuration. As with the database creation, it will create the table if it does not exist, then the provided SQL credential should have access to create a new table in the same database.

To decrease the handshake, connection closing, and reopening overheads due to the number of API calls, this service keeps the SQL connection open till its work gets done.