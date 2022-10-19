namespace SaveIT.CloudStorage.Models;

public record GoogleMetadataModel (string Name, string MimeType, IEnumerable<string> Parents);
