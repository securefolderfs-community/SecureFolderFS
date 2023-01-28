using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Animation;

namespace SecureFolderFS.AvaloniaUI.Animations
{
    internal sealed class Animation : Avalonia.Animation.Animation
    {
        public required Animatable Target { get; set; }

        public Task PlayAsync()
        {
            return RunAsync(Target, null);
        }

        public static Task PlayAsync(IEnumerable<Animation> animations)
        {
            return Task.WhenAll(animations.Select(x => x.PlayAsync()));
        }
    }
}