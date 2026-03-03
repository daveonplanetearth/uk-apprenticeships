namespace UkApprenticeships.Functions.Configuration;

public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";

    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
}
