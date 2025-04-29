using System;

namespace AzureGems.CosmosDB;

public class ContainerDefinitionNotFoundException : Exception
{
    public ContainerDefinitionNotFoundException(string containerId) : base($"Container Definition [{containerId}] not found!")
    {
    }

    public ContainerDefinitionNotFoundException(Type entityType) : base($"Container Definition for type [{entityType}] not found!")
    {
    }

    public ContainerDefinitionNotFoundException(ContainerDefinition containerDefinition) : base($"Container Definition [{containerDefinition.ContainerId}] for type [{containerDefinition.EntityType}] not found!")
    {
    }
}