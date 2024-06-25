using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;

namespace SaveIt.App.UI;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

#if WINDOWS
    private bool _foundWindow;

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (!_foundWindow)
        {
            var window = GetParentWindow();

            if ((window?.Handler?.PlatformView as MauiWinUIWindow)?.GetAppWindow() is AppWindow appWindow)
            {
                appWindow.Changed += AppWindow_Changed;
                _foundWindow = true;
            }
        }
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidPositionChange)
        {
            var width = Window.Width;

            Window.Width = width + 1;
            Window.Width = width;
        }
        else if (args.DidSizeChange)
        {
            WidthRequest = sender.Size.Width;
            HeightRequest = sender.Size.Height;
        }

    }
#endif
}
