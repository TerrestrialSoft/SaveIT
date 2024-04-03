using Microsoft.AspNetCore.Components;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class SearchBar
{
    private string searchText = "";

    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    private async Task OnSearchTextChanged(ChangeEventArgs e)
    {
        searchText = e.Value?.ToString() ?? "";
        await OnSearch.InvokeAsync(searchText);
    }
}
