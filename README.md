# Freight Quote Web

ASP.NET Core MVC web application for a Thailand-based freight forwarding company.
The project is designed for **SQL Server 2014** and includes quotation dashboard, quotation CRUD, charge lines, VAT handling, profit summary, and sample database script.

## Main Features

- Dashboard with quote count, sales, and profit summary
- Quotation list with search and status filter
- Create / edit / view quotation
- Freight-specific fields: mode, service type, incoterm, origin, destination, carrier/agent, commodity, weight, CBM
- Charge line editor with cost, sell, and profit
- VAT and discount summary
- Quick status update: Draft, Sent, Approved, Rejected, Expired, Booked
- SQL Server 2014 database script with sample data

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Microsoft.Data.SqlClient
- SQL Server 2014
- Bootstrap 5 CDN

## Folder Structure

- `Controllers` MVC controllers
- `Data` SQL Server connection factory and repository
- `Models` view models
- `Views` Razor pages
- `wwwroot` CSS and JavaScript
- `database/Create_FreightQuotationDB_SQLServer2014.sql` database script

## How to Run

1. Open SQL Server Management Studio.
2. Run `database/Create_FreightQuotationDB_SQLServer2014.sql`
3. Open `appsettings.json`
4. Change the connection string:

```json
"DefaultConnection": "Server=YOUR_SQL_SERVER;Database=FreightQuotationDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

Example for Windows Authentication:

```json
"DefaultConnection": "Server=YOUR_SQL_SERVER;Database=FreightQuotationDB;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

5. Restore packages:

```bash
dotnet restore
```

6. Run the application:

```bash
dotnet run
```

7. Browse to the local URL shown by ASP.NET Core.

## Recommended Next Steps

- Add authentication and role-based access
- Add PDF quotation export
- Add customer master CRUD
- Add quote-to-booking conversion
- Add shipment tracking and invoice module
- Connect to your existing ERP / WMS / FMS database if needed

## Notes

- The database script is written for SQL Server 2014 compatibility.
- The app uses direct SQL via `Microsoft.Data.SqlClient` to keep deployment simple.
- In this environment, I was able to generate the project files and SQL script, but I could not compile the app here because the .NET SDK is not installed in the container.
