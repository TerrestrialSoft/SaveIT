namespace SaveIt.App.UI.Models;
public class SelectedItemViewModel<T>
{
    public T Item { get; set; }
    public bool IsSelected { get; set; }

    public SelectedItemViewModel(T item)
    {
        Item = item;
    }

    public SelectedItemViewModel(T item, bool isSelected)
    {
        Item = item;
        IsSelected = isSelected;
    }
}
