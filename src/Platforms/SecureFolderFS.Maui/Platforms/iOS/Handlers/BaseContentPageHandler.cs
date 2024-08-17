using Microsoft.Maui.Handlers;
using ContentView = Microsoft.Maui.Platform.ContentView;

namespace SecureFolderFS.Maui.Handlers
{
    public abstract class BaseContentPageHandler : PageHandler
    {
        protected ContentPage? ThisPage => VirtualView as ContentPage;
        
        protected override void ConnectHandler(ContentView platformView)
        {
            base.ConnectHandler(platformView);
            if (ThisPage is null)
                return;
            
            ThisPage.Loaded += ContentPage_Loaded;
            ThisPage.NavigatedTo += ContentPage_NavigatedTo;
            App.Instance.AppResumed += App_Resumed;
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
            // Await a small delay for the UI to load
            await Task.Delay(100);
            ApplyHandler();
        }

        private void ContentPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
        {
            ApplyHandler();
        }
        
        private void App_Resumed(object? sender, EventArgs e)
        {
            // When app is resumed, UI is reevaluated
            ApplyHandler();
        }
    }
}
