using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed partial class CarouselPreviewerViewModel : BasePreviewerViewModel, IDisposable
    {
        private readonly ImmutableList<FileViewModel> _dataSource;

        [ObservableProperty] private int _CurrentIndex;
        [ObservableProperty] private ObservableCollection<BasePreviewerViewModel> _Slides;

        public CarouselPreviewerViewModel(IEnumerable<FileViewModel> dataSource, FileViewModel? fileViewModel = null)
        {
            _dataSource = dataSource.ToImmutableList();
            Slides = new();
            CurrentIndex = _dataSource.IndexOf(fileViewModel ?? _dataSource.First());
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in _dataSource)
            {
                var previewer = GetPreviewer(item.Inner);
                if (previewer is null)
                    continue;

                Slides.Add(previewer);
            }

            return Task.CompletedTask;
        }

        private static BasePreviewerViewModel? GetPreviewer(IStorable storable)
        {
            if (storable is not IFile file)
                return null;

            return FileTypeHelper.GetTypeHint(storable) switch
            {
                TypeHint.Image => new ImagePreviewerViewModel(file),
                TypeHint.Media => new VideoPreviewerViewModel(file, true),
                _ => null
            };
        }

        partial void OnCurrentIndexChanged(int value)
        {
            var item = Slides.ElementAtOrDefault(value);
            if (item is not null)
                Title = item.Title;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Slides.DisposeAll();
            Slides.Clear();
        }
    }
}