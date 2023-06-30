using SecureFolderFS.Shared.Utils;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels.Database
{
    /// <inheritdoc cref="BaseDatabaseModel{TDictionaryValue}"/>
    public abstract class ObservableDatabaseModel<TDictionaryValue> : BaseDatabaseModel<TDictionaryValue>
    {
        private readonly INotifyCollectionChanged _notifyCollectionChanged;

        protected ObservableDatabaseModel(INotifyCollectionChanged notifyCollectionChanged, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
            _notifyCollectionChanged = notifyCollectionChanged;
            _notifyCollectionChanged.CollectionChanged += Settings_CollectionChanged;
        }

        private async void Settings_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            await OnCollectionChangedAsync(e);
        }

        /// <summary>
        /// Captures the recent changes of the database storage.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> class which represents the change that occurred.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual async Task OnCollectionChangedAsync(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Replace)
                return;

            if (e.NewItems?[0] is not string changedItem)
                return;

            await ProcessChangeAsync(changedItem);
        }

        /// <summary>
        /// Updates the state of this database model based on the recent storage changes.
        /// </summary>
        /// <param name="changedItem">The item that was changed.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected abstract Task ProcessChangeAsync(string changedItem);

        /// <inheritdoc/>
        public override void Dispose()
        {
            _notifyCollectionChanged.CollectionChanged -= Settings_CollectionChanged;
        }
    }
}
