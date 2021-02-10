# AzureGems SpendOps - TableStorage Charge Tracker
TODO: Add instructions

## Persist SpendOps SpendTests data in Azure TableStorage

```csharp
services    
    // enable SpendOps tracking
    .AddSpendOps()
    // enable SpendOps data to persist in Azure Table Storage
    .TrackSpendTests<CosmosDbChargedResponse, TableStorageSpendTestChargeTracker>()
    .AddCosmosDb(builder =>
    {
        builder.WithContainerConfig(c =>
            {
                ...
            });
    })
    .AddCosmosContext<DemoCosmosContext>();
```
