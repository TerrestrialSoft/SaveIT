using SaveIt.App.Domain;

namespace SaveIt.App.UI;
internal class ApplicationContext : IApplicationContext
{
    public string DatabasePath => Path.Combine(FileSystem.Current.AppDataDirectory, "data.db");
}
