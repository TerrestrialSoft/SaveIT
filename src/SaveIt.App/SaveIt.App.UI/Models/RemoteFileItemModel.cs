﻿namespace SaveIt.App.UI.Models;
public class RemoteFileItemModel : NamedModel
{
    public required string? Id { get; set; }
    public required string ParentId { get; set; }
    public bool IsDirectory { get; set; }
}