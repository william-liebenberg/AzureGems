# AzureGems SpendOps - CosmosDB Extensions

## Log SpendOps data via Microsoft.Extensions.Logger

```csharp
services
    // enable SpendOps tracking - by default the data is logged via Microsoft.Extensions.Logger
    .AddSpendOps()
    .AddCosmosDb(builder =>
    {
        builder.WithContainerConfig(c =>
            {
                ...
            });
    })
    .AddCosmosContext<DemoCosmosContext>();
```