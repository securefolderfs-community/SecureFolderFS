using Plugin.Maui.BottomSheet;
using Plugin.Maui.BottomSheet.Navigation;
using Plugin.SegmentedControl.Maui;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
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
        private readonly FirstTimeHelper _firstTime;

        public ViewOptionsSheet()
        {
            _firstTime = new(1);
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

        private void SizeSegments_Loaded(object? sender, EventArgs e)
        {
            if (sender is not SegmentedControl sizeSegments || ViewModel is null)
                return;

            if (ViewModel.CurrentSizeOption is null)
                return;

            var index = ViewModel.SizeOptions.IndexOf(ViewModel.CurrentSizeOption);
            sizeSegments.SelectedSegment = index;
        }
        
        private void SizeSegments_SelectedIndexChanged(object? sender, SelectedIndexChangedEventArgs e)
        {
            if (_firstTime.IsFirstTime())
                return;
            
            ViewModel?.SetLayoutSizeOptionCommand.Execute(e.NewValue);
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

