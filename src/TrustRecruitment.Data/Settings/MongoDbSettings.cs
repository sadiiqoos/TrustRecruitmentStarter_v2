namespace TrustRecruitment.Data.Settings;

public class MongoDbSettings
{
    public string DatabaseName { get; set; } = null!;
    public string JobsCollectionName { get; set; } = null!;
    public string ApplicationsCollectionName { get; set; } = null!;
    public string UsersCollectionName { get; set; } = null!;
}