using System.Reflection;
using Plugin.Maui.BottomSheet;
using Plugin.Maui.BottomSheet.Navigation;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Sheets
{
    internal sealed class ViewOptionsSheetFragment : IOverlayControl
    {
        private readonly TaskCompletionSource<IResult> _tcs;
        private ViewOptionsViewModel? _viewModel;

        public ViewOptionsSheetFragment()
        {
            _tcs = new();
        }
        
        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (_viewModel is null)
                return Result.Failure(null);
            
            var sheetNavigationService = DI.Service<IBottomSheetNavigationService>();
            await sheetNavigationService.NavigateToAsync(nameof(ViewOptionsSheet), new BottomSheetNavigationParameters()
            {
                { "ViewModel", _viewModel }
            });

            var implementation = (BottomSheetNavigationService)sheetNavigationService;
            var sheetNavigationServiceType = typeof(BottomSheetNavigationService);
            var fieldInfo = sheetNavigationServiceType.GetField("_bottomSheetStack", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo is null)
            {
                _tcs.SetResult(Result.Failure(null));
                return await _tcs.Task;
            }

            var objStack = fieldInfo.GetValue(implementation);
            var stack = (Stack<IBottomSheet>?)objStack;
            if (stack?.FirstOrDefault() is not BottomSheet sheet)
            {
                _tcs.SetResult(Result.Failure(null));
                return await _tcs.Task;
            }

            if (sheet is not ViewOptionsSheet viewOptionsSheet)
                return await _tcs.Task;

            viewOptionsSheet.TaskCompletion = _tcs;
            return await _tcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            _viewModel = (ViewOptionsViewModel)viewable;
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
        public TaskCompletionSource<IResult>? TaskCompletion { get; set; }
        
        public ViewOptionsSheet()
        {
            InitializeComponent();
        }
        
        private async void Button_Clicked(object? sender, EventArgs e)
        {
            var sheetNavigationService = DI.Service<IBottomSheetNavigationService>();
            await sheetNavigationService.ClearBottomSheetStackAsync();
            TaskCompletion?.TrySetResult(Result.Success);
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<ViewOptionsViewModel>();
        }

        public ViewOptionsViewModel? ViewModel
        {
            get => (ViewOptionsViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(ViewOptionsViewModel), typeof(ViewOptionsSheet), null);

        private void VisualElement_OnLoaded(object? sender, EventArgs e)
        {
            if (sender is not Picker picker)
                return;

            _ = picker.BindingContext;
        }
    }
}

