namespace SaveIT.CloudStorage.Models;

public record GoogleMetadata (string name, string mimeType, IEnumerable<string> parents);
