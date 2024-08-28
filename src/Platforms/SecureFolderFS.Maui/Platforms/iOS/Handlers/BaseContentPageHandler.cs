using Microsoft.Maui.Handlers;
using ContentView = Microsoft.Maui.Platform.ContentView;

namespace SecureFolderFS.Maui.Handlers
{
    public abstract class BaseContentPageHandler : PageHandler
    {
        protected ContentView? PlatformView { get; private set; }

        protected ContentPage? ThisPage => VirtualView as ContentPage;
        
        protected override void ConnectHandler(ContentView platformView)
        {
            base.ConnectHandler(platformView);
            if (ThisPage is null)
                return;
            
            ThisPage.Loaded += ContentPage_Loaded;
            ThisPage.Appearing += ContentPage_Appearing;
            App.Instance.AppResumed += App_Resumed;

            PlatformView = platformView;
        }

        protected override void DisconnectHandler(ContentView platformView)
        {
            ThisPage!.Loaded -= ContentPage_Loaded;
            ThisPage!.Appearing -= ContentPage_Appearing;
            App.Instance.AppResumed -= App_Resumed;
            base.DisconnectHandler(platformView);
        }

        private void ContentPage_Unloaded(object? sender, EventArgs e)
        {
            ThisPage!.NavigatedTo -= ContentPage_NavigatedTo;
            ThisPage!.Unloaded -= ContentPage_Unloaded;
            ThisPage.ToolbarItems.Clear();
        }

        protected abstract void ApplyHandler(IPlatformViewHandler viewHandler);

        private void ApplyHandler()
        {
            if (this is not IPlatformViewHandler viewHandler)
                return;
            
            ApplyHandler(viewHandler);
        }
        
        private async void ContentPage_Loaded(object? sender, EventArgs e)
        {
            ThisPage!.NavigatedTo += ContentPage_NavigatedTo;
            ThisPage!.Unloaded += ContentPage_Unloaded;
            
            // Await a small delay for the UI to load
            await Task.Delay(10);
            ApplyHandler();
        }

        private void App_Resumed(object? sender, EventArgs e)
        {
            // When app is resumed, UI is reevaluated
            ApplyHandler();
        }
        
        private void ContentPage_Appearing(object? sender, EventArgs e)
        {
            ApplyHandler();
        }

        private void ContentPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
        {
            ApplyHandler();
        }
    }
}
