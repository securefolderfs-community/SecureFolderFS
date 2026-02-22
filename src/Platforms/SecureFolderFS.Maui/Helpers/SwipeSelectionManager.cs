using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;

namespace SecureFolderFS.Maui.Helpers
{
    /// <summary>
    /// Manages swipe-to-select gesture state, mirroring the iOS Photos/Gallery app behavior.
    /// </summary>
    internal sealed class SwipeSelectionManager
    {
        private bool _isActive;
        private bool _selectionIntent; // true = select, false = deselect
        private readonly HashSet<string> _processedItemIds = new();
        private readonly HashSet<string> _currentlyInsideIds = new();

        /// <summary>Whether a swipe gesture is currently in progress.</summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// Call when a pan gesture BEGINS on a specific item.
        /// Determines the intent from the item's current selection state.
        /// </summary>
        /// <param name="item">The item under the finger when the gesture started.</param>
        public void Begin(BrowserItemViewModel item)
        {
            _isActive = true;
            _selectionIntent = !item.IsSelected; // flip from the current state
            _processedItemIds.Clear();
            Apply(item);
        }
        
        public void UpdateFromRectangle(IList<BrowserItemViewModel> allItems, Func<BrowserItemViewModel, bool> isInsideRect)
        {
            if (!_isActive)
                return;

            foreach (var item in allItems)
            {
                var inside = isInsideRect(item);
                var id = item.Inner.Id;

                if (inside && !_currentlyInsideIds.Contains(id))
                {
                    // Entered rectangle — apply intent
                    item.IsSelected = _selectionIntent;
                    _currentlyInsideIds.Add(id);
                }
                else if (!inside && _currentlyInsideIds.Contains(id))
                {
                    // Left rectangle — revert to original state
                    item.IsSelected = !_selectionIntent;
                    _currentlyInsideIds.Remove(id);
                }
            }
        }

        /// <summary>
        /// Call whenever the swipe moves over a new item.
        /// Applies the selection intent if the item hasn't been processed yet.
        /// </summary>
        /// <param name="item">The item currently under the finger.</param>
        public void Update(BrowserItemViewModel item)
        {
            if (!_isActive)
                return;

            Apply(item);
        }

        /// <summary>Call when the pan gesture ends or is cancelled.</summary>
        public void End()
        {
            _isActive = false;
            _processedItemIds.Clear();
            _currentlyInsideIds.Clear();
        }

        private void Apply(BrowserItemViewModel item)
        {
            var id = item.Inner.Id;
            if (_processedItemIds.Contains(id))
                return;

            item.IsSelected = _selectionIntent;
            _processedItemIds.Add(id);
        }
    }
}
