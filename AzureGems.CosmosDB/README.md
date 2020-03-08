# AzureGems.CosmosDb
Easy to use CosmosDB library.


## Getting Started

To get started using the AzureGems.CosmosDB library, add the `AzureGems.CosmosDb` NuGet to your project. 

Use the following code in your `StartUp.cs` file to set up your connection and datatypes to use with CosmosDb:

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
			c.AddContainer<Widget>(containerId: "Gadgets", partitionKeyPath: "/style");
		});
});
```

The code above builds a `CosmosDbClient` that uses the specified connection settings, database name, shared throughput, and container configuration. 
 
The `ICosmosDbClient` will ensure that:
1. a connection is established with the specified CosmosDb instance using the specified settings
2. the database is created using the specified shared throughput (NOTE: Specifying null means you have to specify throughput for each container individually)
3. the containers are created according to the configuration specified
> **NOTE:** If the database/containers are created from scratch, then the new settings will be applied. However if the containers already exists, they remain untouched.

The `ICosmosDbClient` is automatically added to the Dependency Injection (DI) framework that allows you to inject it into your services.

## App Configuration

The CosmosDB connection settings can be specified via `appsettings.json` instead of being hard-coded. Add the `cosmosDbConnection` section to your `appsettings.json` file and include the following values:

```json
{
  "cosmosDbConnection": {
    "endpoint": "<YOUR COSMOS DB ENDPOINT>",
    "authkey": "<YOUR COSMOSDB AUTHKEY>",
    "databaseId": "<YOUR COSMOSDB DATABASE ID/NAME>",
    "sharedThroughput": 20000
  }
}
```

**TODO**: Allow `ContainerConfig` to be specified via `appsettings.json`

## Accessing the CosmosDb Containers

Obtaining a reference to a CosmosDb Container, call the async `GetContainer<T>()` method with the generic type it contains:  

```csharp
ICosmosDbContainer container = await cosmosDbClient.GetContainer<Vehicle>();
```

Alternatively, you can obtain a reference to a CosmosDb Container by specifying the `ContainerId` (without generic type) when calling `GetContainer()`:

```csharp
ICosmosDbContainer container = await cosmosDbClient.GetContainer("Cars");
```

## Using the CosmosDb Containers

All the CRUD methods for the CosmosDb Containers return a `CosmosDbResponse<T>` object. The resulting value can be accessed via the `.Result` property.  

> NOTE: Anonymous types are not supported.


### Add

```csharp
CosmosDbResponse<Vehicle> addResponse = await container.Add(
	"Camaro",
	new Vehicle
	{
		Id = Guid.NewGuid().ToString(),
		Brand = "Camaro",
		Color = "Black",
		Model = "ZL1",
		Cylinders = 8,
		PurchasePrice = 74000,

		// The Discriminator property is useful when colocating multiple types in a single container.
		// For this example the discriminator allows the container to filter on the entity type
		Discriminator = typeof(Vehicle).FullName,

	});
```

### Get

Getting an item from a container can be done with or without specifying the primary key value.

By specifying the primary key reduces the number RU's charged to fulfill your query, it also has a performance benefit because CosmosDb can avoid having to scan through the entire container to find your requested item.

```csharp
CosmosDbResponse<Vehicle> getResponse = await container.Get<Vehicle>("Camaro", "<ID VALUE>");

// access the result
Vehicle mycar = getResponse.Result;
```

### Core SQL Query

You are able to write and execute Core SQL Queries by using the `GetByQuery()` method.

```csharp
string query = "SELECT * FROM Cars c WHERE c.Color = 'Black'";
CosmosDbResponse<IEnumerable<Vehicle>> blackCamaros = await container.GetByQuery<Vehicle>("Camaro", query);
```

See the [Cosmos Db Cheatsheets](https://docs.microsoft.com/en-us/azure/cosmos-db/query-cheat-sheet) for more CoreSQL Query examples

### LINQ Query

You can build up LINQ queries (just like EFCore) instead of writing CoreSQL. 

Use the `GetByLinq()` method to obtain an `IQueryable<T>` from the container to start building up your query. 

Once you have your LINQ prepared, call the `Resolve<T>()` method to obtain your results.
 
```csharp
IQueryable<Vehicle> query = container.GetByLinq<Vehicle>("Camaro")
	.Where(v => v.Color == "Black" && v.PurchasePrice > 50000)
	.OrderBy(v => v.PurchasePrice)

// Resolve the LINQ Query to get the results
CosmosDbResponse<IEnumerable<Vehicle>> cars = await container.Resolve(query);
```

See [Supported LINQ operators](https://docs.microsoft.com/en-us/azure/cosmos-db/sql-query-linq-to-sql#SupportedLinqOperators) for a full list of supported LINQ operators.

> NOTE: Some Linq operators may not be supported by Cosmos Db. Although more operators are constantly being added by the Cosmos Db team.

> NOTE: `Resolve<T>()` is required because `ToList()` and `ToArray()` aren't implemented on the CosmosDb LINQ provider. 

### Update

```csharp
TODO
```

###  Delete

Deleting an entity using `.Delete()` requires both the `PartitionKey` and the entity `Id` values to be specified.

```csharp
CosmosDbResponse<Vehicle> deletedCamaro = await container.Delete<Vehicle>("Camaro", "<ID VALUE>");
```

## Thank you

Feel free to reach out or add GitHub issues for anything you think could be useful in this library.

If you made it this far, Thank you!