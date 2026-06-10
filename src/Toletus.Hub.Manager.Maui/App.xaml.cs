namespace Toletus.Hub.Manager.Maui;

public partial class App : Application
{
    private const int InitialWindowWidth = 1280;
    private const int InitialWindowHeight = 980;
    private const string AppTitle = "LiteNet Manager";

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage()) { Title = AppTitle };

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
            appWindow.Resize(new Windows.Graphics.SizeInt32(InitialWindowWidth, InitialWindowHeight));
        };
#endif

        return window;
    }
}
