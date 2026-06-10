namespace Toletus.Hub.Manager.Maui;

public partial class App : Application
{
    private const int InitialWindowWidth = 1285;
    private const int InitialWindowHeight = 980;
    private const string AppTitle = "LiteNet Manager";
    private const string AppIcon = @"Resources\AppIcon\appicon.ico";

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage())
        {
            Title = AppTitle,
            TitleBar = new TitleBar
            {
                Icon = AppIcon,
                Title = AppTitle
            }
        };

#if WINDOWS
        window.Created += (_, _) =>
        {
            if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
                return;

            nativeWindow.Title = AppTitle;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Title = AppTitle;

            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromPoint(
                new Windows.Graphics.PointInt32(0, 0),
                Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;
            var x = workArea.X + (workArea.Width - InitialWindowWidth) / 2;
            var y = workArea.Y + (workArea.Height - InitialWindowHeight) / 2;
            appWindow.MoveAndResize(new Windows.Graphics.RectInt32(
                x,
                y,
                InitialWindowWidth,
                InitialWindowHeight));
        };
#endif

        return window;
    }
}
