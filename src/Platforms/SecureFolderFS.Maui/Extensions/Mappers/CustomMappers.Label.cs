using Microsoft.Maui.Handlers;
using SecureFolderFS.Maui.UserControls.Common;

namespace SecureFolderFS.Maui.Extensions.Mappers
{
    public static partial class CustomMappers
    {
        public static void AddLabelMappers()
        {
            LabelHandler.Mapper.AppendToMapping($"{nameof(CustomMappers)}.{nameof(Label)}", (handler, view) =>
            {
                if (view is not SelectableLabel)
                    return;

#if ANDROID
                handler.PlatformView.SetTextIsSelectable(true);
#endif
            });
        }
    }
}
