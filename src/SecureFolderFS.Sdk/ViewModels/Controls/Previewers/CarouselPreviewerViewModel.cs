using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed partial class CarouselPreviewerViewModel : BasePreviewerViewModel<ObservableCollection<IViewable>>
    {
        private readonly FolderViewModel _dataSource;

        [ObservableProperty] private ObservableCollection<IViewable> _Slides;
        [ObservableProperty] private int _CurrentIndex;

        public CarouselPreviewerViewModel(FolderViewModel dataSource, BrowserItemViewModel? itemViewModel = null)
        {
            _dataSource = dataSource;
            Slides = new();
            CurrentIndex = dataSource.Items.IndexOf(itemViewModel ?? dataSource.Items.First());
        }

        partial void OnCurrentIndexChanged(int oldValue, int newValue)
        {
            Title = Slides.ElementAtOrDefault(newValue)?.Title;
            return;
            if (Slides.IsEmpty())
                return;

            var isForward = newValue > oldValue;
            if (isForward)
            {
                // Dispose previous item
                var oldPreviousIndex = Math.Max(oldValue - 1, 0);
                (Slides.ElementAtOrDefault(oldPreviousIndex) as IDisposable)?.Dispose();

                // Prepare and initialize new next item
                var newNextIndex = Math.Min(newValue + 1, _dataSource.Items.Count - 1);
                var newNextItem = _dataSource.Items[newNextIndex];

                if (Slides.FirstOrDefault(x => (x as IWrapper<IFile>)?.Inner.Id == newNextItem.Inner.Id) is { } viewable)
                {
                    // Don't add duplicate previewers, only initialize the next view
                    _ = (viewable as IAsyncInitialize)?.InitAsync();
                    return;
                }

                var previewer = GetPreviewer(newNextItem.Inner);
                if (previewer is null)
                    return;

                _ = (previewer as IAsyncInitialize)?.InitAsync();
                Slides.Insert(newNextIndex, previewer);
            }
            else
            {
                // Dispose next item
                var oldNextIndex = Math.Min(oldValue + 1, _dataSource.Items.Count - 1);
                (Slides.ElementAtOrDefault(oldNextIndex) as IDisposable)?.Dispose();

                // Prepare and initialize new previous item
                var newPreviousIndex = Math.Max(newValue - 1, 0);
                var newPreviousItem = _dataSource.Items[newPreviousIndex];

                if (Slides.FirstOrDefault(x => (x as IWrapper<IFile>)?.Inner.Id == newPreviousItem.Inner.Id) is { } viewable)
                {
                    // Don't add duplicate previewers, only initialize the next view
                    _ = (viewable as IAsyncInitialize)?.InitAsync();
                    return;
                }

                var previewer = GetPreviewer(newPreviousItem.Inner);
                if (previewer is null)
                    return;

                _ = (previewer as IAsyncInitialize)?.InitAsync();
                Slides.Insert(newPreviousIndex, previewer);
            }
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Separate indexes (CarouselView - public, _internalIndex - private)
            // This is because when loading a non-first item, the index in file list might be greater than in collection view.
            // To fix this we either load all Slides up until that index (which costs performance if the user
            // opens an item at the very end) or separate the two indexes so that the collection view follows a different
            // index from the internal index in file list

            // TODO: MOST IMPORTANT!!! (IGNORE OTHER TODOs)
            // Actually, write an in-house carousel control based on gesture recognizers
            // The MAUI's CarouselView does not work well with pinch and pan class and also fixing
            // the item loading issue (index issue) is not worth the effort.
            // I imagine the user control would either:
            //
            // 1. Take a collection of items and have current item (the previous and next items would be managed
            //      by the control itself) be displayed and animated when the user swipes (less ideal)
            //
            // 2. Take previous, current, and next item as separate properties, animate the view change on swipe, and somehow
            //      notify the view model to swap out the new items (more ideal)
            //


            // TODO: For now, load all items up until that index.
            // Indexes are synchronized (+ 2 to account for count -- not index, and next item):
            var itemsToLoad = _dataSource.Items;//.Take(CurrentIndex + 2);

            // // Get 2 previous items and 2 next items (4 in total)
            // var startIndex = Math.Max(0, _dataSource.Items.IndexOf(_currentItem) - 2);
            // var endIndex = Math.Min(startIndex + 4, _dataSource.Items.Count - 1);
            //
            // // Take items between start and end indexes
            // var itemsToLoad = _dataSource.Items.Take(new Range(startIndex, endIndex)).ToArray();
            foreach (var item in itemsToLoad)
            {
                var previewer = GetPreviewer(item.Inner);
                if (previewer is null)
                    continue;

                Slides.Add(previewer);
            }

            return Task.CompletedTask;
        }

        private static IViewable? GetPreviewer(IStorable storable)
        {
            if (storable is not IFile file)
                return null;

            var classification = FileTypeHelper.GetClassification(storable);
            return classification.TypeHint switch
            {
                TypeHint.Image => new ImagePreviewerViewModel(file),
                TypeHint.Media => new VideoPreviewerViewModel(file, true),
                _ => null
            };
        }
    }
}