using Microsoft.UI.Xaml.Media.Animation;
using System.Threading.Tasks;

namespace SecureFolderFS.Uno.Extensions
{
    internal static class AnimationExtensions
    {
        public static Task BeginAsync(this Storyboard storyboard)
        {
            var tcs = new TaskCompletionSource<object?>();
            storyboard.Completed += OnCompleted!;
            storyboard.Begin();
            return tcs.Task;

            void OnCompleted(object sender, object e)
            {
                ((Storyboard)sender).Completed -= OnCompleted!;
                tcs.SetResult(null);
            }
        }
    }
}
