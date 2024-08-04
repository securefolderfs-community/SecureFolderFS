using System.ComponentModel;

namespace SecureFolderFS.Maui.Extensions
{
    internal static class NavigationExtensions
    {
        public static IDictionary<string, object> ToViewModelParameter(this INotifyPropertyChanged viewModel)
        {
            return new Dictionary<string, object>()
            {
                { "ViewModel", viewModel }
            };
        }

        public static TViewModel? ToViewModel<TViewModel>(this IDictionary<string, object> query)
            where TViewModel : class, INotifyPropertyChanged
        {
            if (query.TryGetValue("ViewModel", out var viewModel) && viewModel is TViewModel typedViewModel)
                return typedViewModel;

            return null;
        }

        public static async Task GoBackAsync(this Shell shell, int amountOfLevels = 1)
        {
            var destination = string.Empty;
            for (var i = 0; i < amountOfLevels; i++)
                destination += "../";

            await shell.GoToAsync(destination);
        }
    }
}
