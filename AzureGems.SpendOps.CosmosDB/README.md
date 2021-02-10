# AzureGems SpendOps - CosmosDB Extensions
TODO: Add instructions

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

TODO: Document TContext.ForFeature() extension (CosmosContextExtensions.cs) - tags all repos with the same feature - quickfire method

TODO: Document GetContainerForFeature() (see CosmosDbClientExtensions.cs) - tag an individual container with a feature

TODO: Document ICosmosDbContainer.ForFeature() extension (see CosmosDbContainerExtensions.cs) - set the feature for a container diretly

TODO: Document IRepository.Tag() extension (see RepositoryExtensions.cs)