using System;

namespace AzureGems.CosmosDB;

public class ContainerDefinitionAlreadyExistsException(ContainerDefinition containerDefinition) : Exception($"Container Definition [{containerDefinition.ContainerId}] for type [{containerDefinition.EntityType}] already exists!");