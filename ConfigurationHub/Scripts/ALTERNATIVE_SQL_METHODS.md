# Alternative SQL Execution Methods

Since SSMS isn't working, you can try these alternatives:

## Method 1: Using sqlcmd (Command Line)

Open PowerShell and run:

```powershell
# Connect to SQL Server
sqlcmd -S localhost -d InfraBaseDb -E

# Then type queries and GO to execute:
SELECT COUNT(*) FROM Assets WHERE CompanyName = 'RUA AlHaram AlMakki';
GO

# Type EXIT to quit
```

## Method 2: Using EF Core Migrations to Check Data

In your project directory:

```powershell
cd "C:\Source Codes\Partners Hub Services\PartnersHub.Services\InfraBase\PartnersHub.InfraBase.Infrastructure"

# Check if database exists
dotnet ef database list --startup-project ..\PartnersHub.InfraBase.Apis

# Execute custom SQL through migration (create a temporary query migration)
```

## Method 3: Use Visual Studio SQL Server Object Explorer

1. In Visual Studio, go to **View** ? **SQL Server Object Explorer**
2. Connect to your server
3. Right-click database ? **New Query**
4. Execute your queries there

## Method 4: PowerShell with SqlServer Module

```powershell
# Install module if needed
Install-Module -Name SqlServer -AllowClobber -Force

# Execute query
Invoke-Sqlcmd -ServerInstance "localhost" -Database "InfraBaseDb" -Query "SELECT COUNT(*) FROM Assets"
```

## Method 5: Create a Simple C# Console App

```csharp
using Microsoft.Data.SqlClient;

var connectionString = "Server=localhost;Database=InfraBaseDb;Trusted_Connection=True;TrustServerCertificate=True;";
using var connection = new SqlConnection(connectionString);
connection.Open();

using var command = new SqlCommand("SELECT COUNT(*) FROM Assets WHERE CompanyName = 'RUA AlHaram AlMakki'", connection);
var count = (int)command.ExecuteScalar();
Console.WriteLine($"RUA AlHaram Assets: {count}");
```

Choose whichever method is most convenient for you!
