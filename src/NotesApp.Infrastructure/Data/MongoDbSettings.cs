
namespace NotesApp.Infrastructure.Data
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UsersCollectionName { get; set; }
        public string NotesCollectionName { get; set; }
    }
}
