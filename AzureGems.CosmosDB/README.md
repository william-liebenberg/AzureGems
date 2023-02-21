# AzureGems.CosmosDb

Easy to use CosmosDb library.

The purpose of this library was firstly to learn the Cosmos Db SDK, but then evolved into a helper library to help deal with the rapid changes being made to the SDK. 

It also serves as an example of applying [#SpendOps](https://azuregems.io/spendops-with-azure-cosmos-db/) to your Cosmos DB usage so that you can be armed with good metrics to predict your budget and track the cost of code changes before releasing the code into Production.

The accompanying library called [AzureGems.SpendOps.CosmosDB](https://github.com/william-liebenberg/AzureGems/tree/master/AzureGems.SpendOps.CosmosDB) goes into more detail for adding [#SpendOps](https://azuregems.io/spendops-with-azure-cosmos-db/) to your own CosmosDb application.

## Getting Started

To get started using the AzureGems.CosmosDB library, add the `AzureGems.CosmosDb` NuGet package to your project. 

### Application Configuration

The CosmosDB connection settings can be specified via `appsettings.json` instead of being hard-coded. Add the `cosmosDbConnection` section to your `appsettings.json` file and include the following values:

```json
{
  "cosmosDbConnection": {
    "endpoint": "<YOUR COSMOS DB ENDPOINT>",
    "authkey": "<YOUR COSMOSDB AUTHKEY>",
    "databaseId": "<YOUR COSMOSDB DATABASE ID/NAME>",
    "sharedThroughput": 5000
  }
}
```

**TODO**: Allow `ContainerConfig` to be specified via `appsettings.json`

### Adding AzureGems.CosmosDb

The following code can be added to `ConfigureServices()` in your `Startup.cs` file to set up your connection and datatypes to use with CosmosDb:

```csharp
services.AddCosmosDb(builder =>
{
	builder
		.Connect(endPoint: "<YOUR COSMOS DB ENDPOINT>", authKey: "<YOUR COSMOSDB AUTHKEY>")
		.UseDatabase(databaseId: "MyDatabase")
		.WithSharedThroughput(4000)
		.WithContainerConfig(c =>
		{
			c.AddContainer<Vehicle>(containerId: "Cars", partitionKeyPath: "/brand", queryByDiscriminator: false, throughput: 5000);
			c.AddContainer<Receipt>(containerId: "Receipts", partitionKeyPath: "/id");
			c.AddContainer<Accessory>(containerId: "Accessories", partitionKeyPath: "/category");
		});
});
```

> The example above specifies all the details required to connect to the CosmosDb instance and which database to use.

```csharp
services.AddCosmosDb(builder =>
{
	builder
		.WithContainerConfig(c =>
		{
			c.AddContainer<Vehicle>(containerId: "Cars", partitionKeyPath: "/brand", queryByDiscriminator: false, throughput: 5000);
			c.AddContainer<Receipt>(containerId: "Receipts", partitionKeyPath: "/id");
			c.AddContainer<Accessory>(containerId: "Accessories", partitionKeyPath: "/category");
		});
});
```

> The example above only specifies the container configuration to use. The connection details for which CosmosDb instance and database to use are specified via the `appsettings.json` file.

The code above builds a `CosmosDbClient` that uses the specified connection settings, database name, shared throughput, and container configuration.

The `ICosmosDbClient` will ensure that:

1. a connection is established with the specified CosmosDb `databaseId` using the given `endPoint` and `authKey`
2. the database is created using the specified shared throughput (NOTE: When specifying `null` you have to explicitly configure throughput for each container individually)
3. the containers are created according to the specified configuration that takes into account the `containerId`, `partitionKeyPath`, `queryByDiscriminator` and `throughput`

> **NOTE:** if the database/containers already exists, they remain untouched. This configuration will only ensure creation if the resources don't already exist.

The `ICosmosDbClient` is automatically added to the .NET Core `ServiceCollection` that allows you to inject it into your controllers/services.

```csharp
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
	protected readonly ICosmosDbClient _client;

	public VehiclesController(ICosmosDbClient client)
	{
		// keep a reference to injected client
		_client = client;
	}

	//
	// Use the ICosmosDbClient (this._client) in your actions
	//
}
```

## Creating Models

Models in CosmosDb can be as simple or complex as you need them to be, as long as it is serializable to and from a JSON document.
 
At the root level of every model an `Id` field of type `string` is required so that it can be looked up/retrieved directly.

Optionally, a `Discriminator` field can be specified as well to help with co-locating different model types in a single CosmosDb container.

```csharp
public class Vehicle
{
	// required
	[JsonProperty("id")]
	public string Id { get; set; }

	// optional
	public string Discriminator { get; set; }

	public string Brand { get; set; }
	public string Color { get; set; }
	public string Model { get; set; }
	public int Cylinders { get; set; }
	public decimal PurchasePrice { get;set; }
}
```

An abstract base class can be used to easily add `Id` and `Discriminator` to all models in your domain.

```csharp
public abstract class BaseEntity
{
	[JsonProperty("id")]
	public string Id { get; set; }
	public string Discriminator { get; set; }
}

public class Vehicle : BaseEntity
{
	public string Brand { get; set; }
	public string Color { get; set; }
	public string Model { get; set; }
	public int Cylinders { get; set; }
	public decimal PurchasePrice { get;set; }
}
```

Currently you need to specify your own ID and Discriminator values. *This will change in a future update.*

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

All the CRUD methods for the CosmosDb Containers are strongly typed and return a `CosmosDbResponse<T>` object. The resulting value can be accessed via the `.Result` property.  

> NOTE: Anonymous types are not supported.

### Add()

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

### Get()

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

See the [Cosmos Db Cheatsheets](https://docs.microsoft.com/en-us/azure/cosmos-db/query-cheat-sheet) for more CoreSQL Query examples.

### LINQ Query

You can build up LINQ queries (just like EFCore) instead of writing CoreSQL.

Use the `GetByLinq()` method to obtain an `IQueryable<T>` from the container to start building up your query. 

Once you have your LINQ query prepared, call the `Resolve<T>()` method to obtain your results.

```csharp
IQueryable<Vehicle> query = container.GetByLinq<Vehicle>("Camaro")
	.Where(v => v.Color == "Black" && v.PurchasePrice > 50000)
	.OrderBy(v => v.PurchasePrice)

// Resolve the LINQ Query to get the results
CosmosDbResponse<IEnumerable<Vehicle>> cars = await container.Resolve(query);
```

See [Supported LINQ operators](https://docs.microsoft.com/en-us/azure/cosmos-db/sql-query-linq-to-sql#SupportedLinqOperators) for a full list of supported LINQ operators.

> NOTE: Some Linq operators may not be supported by Cosmos Db. Although more operators are constantly being added by the Cosmos Db team.
> NOTE: `Resolve<T>()` is required because `ToList()` and `ToArray()` are currently returning `null` .

### Update()

```csharp
TODO: add .Update() example
```

###  Delete()

Deleting an entity using `.Delete()` requires both the `PartitionKey` and the entity `Id` values to be specified.

```csharp
CosmosDbResponse<Vehicle> deletedCamaro = await container.Delete<Vehicle>("Camaro", "<ID VALUE>");
```

## Future additions

* [ ] Apply ID and Discriminator automatically at container level
* [ ] Automatically resolve partition key `value` from ContainerDefinition's `PartitionKeyPath`
* [ ] Sample applications
* [ ] Custom RequestHandlers (extra logging, schema validation, throttling)
* [ ] Streaming
* [ ] Transactions
* [ ] Container Migrations
* [ ] Simple Relationships
* [x] Migrate to .NET 7

## Thank you

Feel free to reach out or add GitHub issues for anything you think could be useful in this library.

If you made it this far, Thank you!
