namespace SaveIT.CloudStorage.Models;
public record GoogleFilesListModel(string Kind, bool IncompleteSearch, IEnumerable<GoogleFileModel> Files);

