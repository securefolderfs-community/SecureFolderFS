using System;
using System.Collections.Concurrent;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Octokit;

namespace SecureFolderFS.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", Exported = true, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const string ID_GUID = "guid";
        private const string ID_LAUNCHED = "launched_state";

        private volatile bool _currentLaunched;
        private volatile string? _currentGuid;

        public static MainActivity? Instance { get; private set; }

        private static readonly ConcurrentDictionary<string, MainActivityIntermediateTask> ActivityPendingTasks = new();

        protected override void OnPostCreate(Bundle? savedInstanceState)
        {
            try
            {
                base.OnPostCreate(savedInstanceState);
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        /// <inheritdoc/>
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance ??= this;

            var extras = savedInstanceState ?? Intent?.Extras;
            _currentLaunched = extras?.GetBoolean(ID_LAUNCHED, false) ?? false;
            _currentGuid = extras?.GetString(ID_GUID);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _ = outState;
            outState.PutBoolean(ID_LAUNCHED, true);
            outState.PutString(ID_GUID, _currentGuid);

            base.OnSaveInstanceState(outState);
        }

        /// <inheritdoc/>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            //if (data is null)
            //    return;

            //var extras = data.Extras;
            //var guid = extras.GetString("guid");
            if (GetIntermediateTask(_currentGuid, true) is { } task)
            {
                if (resultCode == Result.Canceled)
                    task.TaskCompletionSource.TrySetCanceled();
                else
                {
                    try
                    {
                        data ??= new();
                        task.OnResult?.Invoke(data);
                        task.TaskCompletionSource.TrySetResult(data);
                    }
                    catch (Exception ex)
                    {
                        task.TaskCompletionSource.TrySetException(ex);
                    }
                }
            }

            // Close the activity
            Finish();
        }

        public Task<Intent> StartAsync(Intent intent, int requestCode, Action<Intent>? onCreate = null, Action<Intent>? onResult = null)
        {
            // create a new task
            var data = new MainActivityIntermediateTask(onCreate, onResult);
            ActivityPendingTasks[data.Id] = data;

            intent.PutExtra("guid", data.Id);
            intent.SetClass(this, typeof(MainActivity)); // TODO: this causes crash in OnPostCreate

            // start the intermediate activity
            StartActivityForResult(intent, requestCode);

            return data.TaskCompletionSource.Task;
        }

        private static MainActivityIntermediateTask? GetIntermediateTask(string? guid, bool remove = false)
        {
            if (string.IsNullOrEmpty(guid))
                return null;

            if (remove)
            {
                ActivityPendingTasks.TryRemove(guid, out var removedTask);
                return removedTask;
            }

            ActivityPendingTasks.TryGetValue(guid, out var task);
            return task;
        }

        private sealed class MainActivityIntermediateTask
        {
            public string Id { get; }

            public TaskCompletionSource<Intent> TaskCompletionSource { get; }

            public Action<Intent>? OnCreate { get; }

            public Action<Intent>? OnResult { get; }

            public MainActivityIntermediateTask(Action<Intent>? onCreate, Action<Intent>? onResult)
            {
                Id = Guid.NewGuid().ToString();
                TaskCompletionSource = new TaskCompletionSource<Intent>();

                OnCreate = onCreate;
                OnResult = onResult;
            }
        }
    }
}
