$nugetKey = "oy2nmxtu4u4ockonb64q4iyblzpoiqfuvfvokjpvdnohoe";
$nugetSource = "https://api.nuget.org/v3/index.json"
$currentVersion = "1.0.0";
$configuration = "Release";
$libraries = (
    "AzureGems.Cosmos"
    #"AzureGems.MediatR.Extensions"
    #"AzureGems.Repository.Abstractions"
    #"AzureGems.Repository.CosmosDB"
    #"AzureGems.SpendOps.Abstractions"
    #"AzureGems.SpendOps.CosmosDB"
    #"AzureGems.SpendOps.CosmosDB.ChargeTrackers.File"
    #"AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage"
    );

foreach ($lib in $libraries) {
    Write-Host "Pushing $lib"
    dotnet nuget push ./$lib/bin/$configuration/$lib.$currentVersion.nupkg -k $nugetKey -s $nugetSource
    Write-Host
}

    