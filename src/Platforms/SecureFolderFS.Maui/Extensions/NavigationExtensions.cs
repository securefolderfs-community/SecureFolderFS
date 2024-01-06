using System.ComponentModel;

namespace SecureFolderFS.Maui.Extensions
{
    internal static class NavigationExtensions
    {
        public static IDictionary<string, object> ViewModelParameter(this INotifyPropertyChanged viewModel)
        {
            return new Dictionary<string, object>()
            {
                { "ViewModel", viewModel }
            };
        }

        public static TViewModel? ViewModelParameter<TViewModel>(this IDictionary<string, object> query)
            where TViewModel : class, INotifyPropertyChanged
        {
            if (query.TryGetValue("ViewModel", out var viewModel) && viewModel is TViewModel typedViewModel)
                return typedViewModel;

            return null;
        }
    }
}
