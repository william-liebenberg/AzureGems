# AzureGems

Summary of all the [AzureGems](https://azuregems.io) libraries. For more detailed information check out the libraries individually.

  - [AzureGems.CosmosDB](#azuregemscosmosdb)
  - [AzureGems.Repository.Abstractions](#azuregemsrepositoryabstractions)
  - [AzureGems.Repository.CosmosDB](#azuregemsrepositorycosmosdb)
  - [AzureGems.SpendOps.Abstractions](#azuregemsspendopsabstractions)
  - [AzureGems.SpendOps.CosmosDB](#azuregemsspendopscosmosdb)
  - [AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage](#azuregemsspendopscosmosdbchargetrackerstablestorage)
  - [AzureGems.SpendOps.CosmosDB.ChargeTrackers.CosmosDB](#azuregemsspendopscosmosdbchargetrackerscosmosdb)
  - [AzureGems.MediatR.Extensions](#azuregemsmediatrextensions)

## AzureGems.CosmosDB

[View Code](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.CosmosDB)

Library for getting started with Cosmos DB quickly and easily.

* Simple connection config
* Define DB Container properties:
  * model type
  * container name
  * throughput
  * partition key path
  * and more...

For example the following code can be added to your .NET/ASP.NET Core `Startup.cs` file to connect to Cosmos DB and ensure that the required containers are created/exists in the database:

```csharp
services.AddCosmosDb(builder =>
{
	builder
		.UseConnection(endPoint: "<YOUR COSMOS DB ENDPOINT>", authKey: "<YOUR COSMOSDB AUTHKEY>")
		.UseDatabase(databaseId: "MyDatabase")
		.WithSharedThroughput(10000);
		.WithContainerConfig(c =>
		{
			c.AddContainer<Vehicle>(containerId: "Cars", partitionKeyPath: "/brand", queryByDiscriminator: false, throughput: 20000);
			c.AddContainer<Receipt>(containerId: "Receipts", partitionKeyPath: "/id");
			c.AddContainer<Accessory>(containerId: "Accessories", partitionKeyPath: "/category");
		});
});
```

## AzureGems.Repository.Abstractions

[View Code](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.Repository.Abstractions)

Interface for a generic [Repository Pattern](https://deviq.com/repository-pattern/).

Sample implementation: [AzureGems.Repository.CosmosDB](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.Repository.CosmosDB)

## AzureGems.Repository.CosmosDB

[View Code](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.Repository.CosmosDB)

Cosmos DB specific implementation of the generic [Repository Pattern](https://deviq.com/repository-pattern/). `CosmosDbContainerRepository.cs` implements the `IRepository<T>` interface.

This library also aims to provide EFCore-like `DbContext`'s by simply declaring one or more `IRepository<T>` (`DbSet` in EFCore) inside a `CosmosDbContext` class.

For example:

```csharp
public class LittleNorthwindDbContext : DbContext
{
    public IRepository<Customer> Customers { get; set; }
    public IRepository<Order> Orders { get; set; }
    public IRepository<Invoice> Invoices { get; set; }
}
```

The `CosmosDbContainer` implementation already contains the definition of each container (see [AzureGems.CosmosDB](#AzureGemsCosmosDB)) and can instantiate the `IRepository<T>` instances via reflection when the `DbContext` is resolved from the `ServiceCollection`.

The instance of your `DbContext` can now be used in your application's services or controllers by constructor injection.

## AzureGems.SpendOps.Abstractions

[View Code](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.SpendOps.Abstractions)

Interfaces necessary for implementing [**#SpendOps**](https://azuregems.io/spendops-with-azure-cosmos-db/) into your ORM/Data Layers and Test Libraries.

Sample implementation using Cosmos DB: [AzureGems.SpendOps.CosmosDB](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.SpendOps.CosmosDB)

## AzureGems.SpendOps.CosmosDB

[View Code](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.SpendOps.CosmosDB)

Cosmos DB implementations for `IChargeTracker` and `ISpendTestChargeTracker`.

The `TrackedCosmosDbContainer` collects `RequestCharge` values from all the `CosmosDbResponse<T>` objects returned from the CRUD operations performed by the `CosmosDbContainer` implementation.

Implementations of `IChargeTracker` and `ISpendTestChargeTracker` are configured in the **Composition Root** for your **Application** or **Test Library**.

For **applications** an implementation of `IChargeTracker` is used.

For **unit/integration/spend** tests an implementation of `ISpendTestChargeTracker` is configured and registered.

## AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage

[View Code](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage)

Charge Tracker using [Azure Table Storage](https://azure.microsoft.com/en-us/services/storage/tables/) for persisting [**#SpendOps**](https://azuregems.io/spendops-with-azure-cosmos-db/) data.

## AzureGems.SpendOps.CosmosDB.ChargeTrackers.CosmosDB

[View Code](https://github.com/william-liebenberg/AzureGems/)

Charge Tracker using [Azure Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/) for persisting [**#SpendOps**](https://azuregems.io/spendops-with-azure-cosmos-db/) data.

TODO: Finish Code

## AzureGems.MediatR.Extensions

[View Code](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.MediatR.Extensions)

Simple but useful extensions for [MediatR](https://github.com/jbogard/MediatR)

- Specializes `IRequest` into `IQuery` and `ICommand`
- Specializes `IRequestHandler` into `IQueryHandler` and `ICommandHandler`
