# AzureGems Repository - Abstractions

This library provides a CRUD interface for a Generic Repository. The interface also allows you to write LINQ expressions via the `Query()` method.

## `IRepository<T>`

The `IRepository` interface does not provide methods that return an `IQueryable` as it is generally considered to be an anti-pattern.

Instead, the interface provides a `Query()` method so that you can write Expressions against a provided `IQuerable` parameter.

For example:

```csharp

private readonly IRepository<Widget> _widgets;

public IEnumerable<Widget> GetWidgetsBelow(double price)
{
	return _widgets.Query(q => q.Where(w => w.SalePrice < price));
}
```
In the code above, `q` is of type `IQueryable<Widget>` and `w` is of type `Widget`.

### Returning custom models

Each repository is strongly typed and most of the CRUD methods will only return a single/enumerable of the same strong type.

However when using the LINQ Expression with the `Query()` method we are able to return custom models (view models) using the LINQ `.Select()` operator.

For example: 

```csharp
IEnumerable<WidgetViewModel> spinners = await widgetsRepo.Query(q => q
	.Where(w => w.Type == "Spinner")
	.OrderBy(w => w.Style)
	.Select(s => new WidgetViewModel()
	{
		DisplayName = s.Name + " Spinner",
		Price = s.SalePrice
	})
);
```

> NOTE: Returning Anonymous types aren't supported!

## `BaseType`

This type is used as a generic constraint for the `IRepository<T>` interface. This means any model of type `T` must inherit from `BaseEntity`

It provides the required `Id` field and the `Discriminator` field to be used with all models.

TODO: Can `BaseType` be moved to the AzureGems.Repository.CosmosDb implementation?