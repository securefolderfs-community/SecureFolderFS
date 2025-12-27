using System.Collections.Generic;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls;
using static SecureFolderFS.Storage.Constants.Sizes;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class RecycleBinHelpers
    {
        public static IEnumerable<PickerOptionViewModel> GetSizeOptions(long totalFreeSpace)
        {
            // Always return "No size limit" option
            yield return new("-1", "NoSizeLimit".ToLocalized());

            // Return in tranches

            // 1GB
            if (totalFreeSpace >= GIGABYTE)
                yield return new PickerOptionViewModel(GIGABYTE.ToString(), "1GB");

            // 2GB
            if (totalFreeSpace >= GIGABYTE * 2L)
                yield return new PickerOptionViewModel((GIGABYTE * 2L).ToString(), "2GB");

            // 5GB
            if (totalFreeSpace >= GIGABYTE * 5L)
                yield return new PickerOptionViewModel((GIGABYTE * 5L).ToString(), "5GB");

            // 10GB
            if (totalFreeSpace >= GIGABYTE * 10L)
                yield return new PickerOptionViewModel((GIGABYTE * 10L).ToString(), "10GB");

            // 20GB
            if (totalFreeSpace >= GIGABYTE * 20L)
                yield return new PickerOptionViewModel((GIGABYTE * 20L).ToString(), "20GB");

            // 50GB
            if (totalFreeSpace >= GIGABYTE * 50L)
                yield return new PickerOptionViewModel((GIGABYTE * 50L).ToString(), "50GB");
        }
    }
}
