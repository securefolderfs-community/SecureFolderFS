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

            // TODO: For now, load all items up until that index.
            // Indexes are synchronized (+ 2 to account for count -- not index, and next item):
            var itemsToLoad = _dataSource.Items.Take(CurrentIndex + 2);

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

                _ = (previewer as IAsyncInitialize)?.InitAsync();
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
                TypeHint.Media => new VideoPreviewerViewModel(file),
                _ => null
            };
        }
    }
}