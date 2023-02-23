# AzureGems MediatR Extensions

Extension library for the popular [MediatR](https://github.com/jbogard/MediatR) library by Jimmy Bogard to add clear distinction between requests without any side-effects (aka Reads or Queries) or requests with side-effects (aka Writes or Commands).

## Queries and QueryHandlers

Create a Query class and inherit from `IQuery<out TResult>`:

```cs
public class GetRecipeByName : IQuery<Recipe>
{
    public string RecipeName {get; set;}
    ...
}
```

To send / execute the query, use the `SendQuery()` extension on the standard `IMediator` interface.

```cs
var yummySpaghetti = new GetRecipeByName()
{
    RecipeName = "Spaghetti Bolognese"
}

Recipe recipe = await _mediator.SendQuery(yummySpaghetti);
```

Create a handler for the `GetRecipe` query that executes the required business logic to obtain a result:

```cs
public class GetRecipeHandler : IQueryHandler<GetRecipe, Recipe>
{
    ...
    public async Task<Recipe> Handle(GetRecipe query, CancellationToken ct)
    {
        ...
    }
}
```

## Commands & CommandHandlers

Create a Command class and inherit from `ICommand<out TResult>`:

```cs
public class CreateRecipe : ICommand<Guid>
{
    public string Name {get; set;}
    public string Cuisine {get; set;}
    public Ingredient[] Ingredients {get; set;}
    ...
}
```

To send / execute the command, use the `SendCommand()` extension on the standard `IMediator` interface.

```cs
var yummySpaghetti = new CreateRecipe()
{
    Name = "Spaghetti Bolognese",
    Cuisine = "Italian",
    Ingredients = new[]
    {
        new Ingredient() { Name = "Spaghetti", WeightInGrams = "500"},
        new Ingredient() { Name = "Bolognese Sauce", VolumeInMilliliters = "500"},
        new Ingredient() { Name = "Parmesan cheese", WeightInGrams = "250"},
    }
}

Guid recipeId = await _mediator.SendCommand(yummySpaghetti);
```

Create a handler for the `CreateRecipe` command that executes the required business logic:

```cs
public class CreateRecipeHandler : ICommandHandler<CreateRecipe, Guid>
{
    ...
    public async Task<Guid> Handle(CreateRecipe cmd, CancellationToken ct)
    {
        ...
    }
}
```

