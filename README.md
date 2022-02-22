![](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)

# eShop

Web shop REST API backend. Manage items, items stock, items category, user carts and orders.

# Setup:

* Create appsettings.json file in eShop folder 
* Copy the following into it and then replace RANDOM_STRING (required for jwt tokens) and YOUR_DB_CONNETION_STRING.


```
{
  "AppSettings": {
    "Secret": "RANDOM_STRING"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "data": "YOUR_DB_CONNETION_STRING"
  }

```

* RANDOM_STRING: Example: http://www.unit-conversion.info/texttools/random-string-generator/ set to 160 and paste the result
* YOUR_DB_CONNETION_STRING: Add MS SQL connection string, example: 
`"Data Source=$YourDatabaseName$;Database=eShop;Trusted_Connection=True;MultipleActiveResultSets=True;`

# Running:
Make sure that you have [.NET 5 SDK installed](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)

Navigate to project folder and run:
```
dotnet run
```

To build without running run:
```
dotnet build
```

# Database scheme
![](https://github.com/helenaJovanovic/eShop/blob/master/dbschema.png)
