using System.Collections.Concurrent;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SecureFolderFS.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", Exported = true, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity? Instance { get; private set; }

        private static readonly ConcurrentDictionary<string, MainActivityIntermediateTask> ActivityPendingTasks = new();

        private bool _launched;
        private Intent? _actualIntent;
        private string? _guid;
        private int _requestCode;

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
            Instance = this;

            var extras = savedInstanceState ?? Intent?.Extras;

            // read the values
            _launched = extras?.GetBoolean("launched", false) ?? false;


#pragma warning disable 618 // TODO: one day use the API 33+ version: https://developer.android.com/reference/android/os/Bundle#getParcelable(java.lang.String,%20java.lang.Class%3CT%3E)
#pragma warning disable CA1422 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            _actualIntent = extras?.GetParcelable("actual_intent") as Intent;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore 618


            _guid = extras?.GetString("guid");
            _requestCode = extras?.GetInt("request_code", -1) ?? -1;

            if (GetIntermediateTask(_guid) is MainActivityIntermediateTask task)
            {
                task.OnCreate?.Invoke(_actualIntent!);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            // make sure we mark this activity as launched
            outState.PutBoolean("launched", true);

            // save the values
            outState.PutParcelable("actual_intent", _actualIntent);
            outState.PutString("guid", _guid);
            outState.PutInt("request_code", _requestCode);

            base.OnSaveInstanceState(outState);
        }

        /// <inheritdoc/>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // we have a valid GUID, so handle the task
            if (GetIntermediateTask(_guid, true) is MainActivityIntermediateTask task)
            {
                if (resultCode == Result.Canceled)
                {
                    task.TaskCompletionSource.TrySetCanceled();
                }
                else
                {
                    try
                    {
                        data ??= new Intent();

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

            // create the intermediate intent, and add the real intent to it
            var intermediateIntent = new Intent(this, typeof(MainActivity));
            intermediateIntent.PutExtra("actual_intent", intent);
            intermediateIntent.PutExtra("guid", data.Id);
            intermediateIntent.PutExtra("request_code", requestCode);

            // start the intermediate activity
            StartActivityForResult(intermediateIntent, requestCode);

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
