using System;

namespace AzureGems.CosmosDB;

public class ContainerDefinitionNotDeletedException(ContainerDefinition containerDefinition) : Exception($"Container Definition [{containerDefinition.ContainerId}] for type [{containerDefinition.EntityType}] could not be deleted!");