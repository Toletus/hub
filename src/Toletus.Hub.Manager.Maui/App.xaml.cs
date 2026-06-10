namespace Toletus.Hub.Manager.Maui;

public partial class App : Application
{
    private const int InitialWindowWidth = 1350;
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
            },
            Width = InitialWindowWidth,
            Height = InitialWindowHeight
        };

        return window;
    }
}
