using Microsoft.Maui.Platform;

namespace SaveIt.App.UI;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

#if WINDOWS
    bool _foundWindow;

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (!_foundWindow)
        {
            var window = GetParentWindow();

            if ((window?.Handler?.PlatformView as MauiWinUIWindow)?.GetAppWindow() is var appWindow)
            {
                appWindow.Changed += AppWindow_Changed;
                _foundWindow = true;
            }
        }
    }

    private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
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
