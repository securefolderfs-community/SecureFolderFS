using Plugin.Maui.BottomSheet;
using Plugin.Maui.BottomSheet.Navigation;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Sheets
{
    internal sealed class ViewOptionsSheetFragment : IOverlayControl
    {
        private readonly TaskCompletionSource<IResult> _tcs;
        
        public LayoutsViewModel? ViewModel { get; private set; }
        
        public static ViewOptionsSheetFragment? EarlyInstance { get; set; }

        public ViewOptionsSheetFragment()
        {
            _tcs = new();
            EarlyInstance = this;
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Result.Failure(null);

            var sheetNavigationService = DI.Service<IBottomSheetNavigationService>();
            await sheetNavigationService.NavigateToAsync(nameof(ViewOptionsSheet), new BottomSheetNavigationParameters()
            {
                { "ViewModel", ViewModel },
                { "TaskCompletion", _tcs }
            });

            return await _tcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (LayoutsViewModel)viewable;
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            var sheetNavigationService = DI.Service<IBottomSheetNavigationService>();
            await sheetNavigationService.ClearBottomSheetStackAsync();
        }
    }

    public partial class ViewOptionsSheet : BottomSheet, IQueryAttributable
    {
        private TaskCompletionSource<IResult>? _tcs;

        public ViewOptionsSheet()
        {
            ViewModel ??= ViewOptionsSheetFragment.EarlyInstance?.ViewModel;
            Closed += Sheet_Closed;
            
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<LayoutsViewModel>();
            _tcs = query.Get("TaskCompletion") as TaskCompletionSource<IResult>;
        }

        private void Sheet_Closed(object? sender, EventArgs e)
        {
            _tcs?.TrySetResult(Result.Success);
            ViewOptionsSheetFragment.EarlyInstance = null;
        }

        public LayoutsViewModel? ViewModel
        {
            get => (LayoutsViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(LayoutsViewModel), typeof(ViewOptionsSheet));
    }
}

