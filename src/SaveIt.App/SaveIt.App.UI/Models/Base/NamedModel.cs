namespace SaveIt.App.UI.Models;
public class NamedModel
{
    public required string Name { get; set; }

    public virtual string FullPath => Name; 
}
