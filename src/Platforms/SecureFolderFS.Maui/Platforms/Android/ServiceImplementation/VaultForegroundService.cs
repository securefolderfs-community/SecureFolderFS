using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Platform;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    [Service(
        ForegroundServiceType = ForegroundService.TypeDataSync,
        Exported = false)]
    public class VaultForegroundService : Service, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private const string CHANNEL_ID = "securefolderfs_vault_foreground_service";
        private const int NOTIFICATION_ID = 4949;
        public const string LockAll = "securefolderfs.action.LOCK_ALL";

        private static TaskCompletionSource<VaultForegroundService> InstanceTcs { get; } = new();

        public List<IVaultModel> UnlockedVaults { get; } = new();

        public static async Task<VaultForegroundService> GetInstanceAsync()
        {
            return await InstanceTcs.Task;
        }

        /// <inheritdoc/>
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }

        /// <inheritdoc/>
        public override void OnCreate()
        {
            InstanceTcs.TrySetResult(this);

            base.OnCreate();
            EnsureNotificationChannel();
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
        }

        /// <inheritdoc/>
        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == LockAll)
            {
                foreach (var vault in UnlockedVaults.ToList())
                    WeakReferenceMessenger.Default.Send(new VaultLockRequestedMessage(vault));

                return StartCommandResult.Sticky;
            }

            var notification = BuildNotification();
            if (notification is null)
                return StartCommandResult.NotSticky;

            StartForeground(NOTIFICATION_ID, notification, ForegroundService.TypeDataSync);
            return StartCommandResult.Sticky;
        }

        /// <inheritdoc/>
        public override void OnDestroy()
        {
            WeakReferenceMessenger.Default.Unregister<VaultLockedMessage>(this);
            WeakReferenceMessenger.Default.Unregister<VaultUnlockedMessage>(this);
            base.OnDestroy();
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            UnlockedVaults.Add(message.VaultModel);
            UpdateNotification();
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            UnlockedVaults.Remove(message.VaultModel);
            UpdateNotification();
        }

        private void UpdateNotification()
        {
            var manager = NotificationManagerCompat.From(this);
            if (manager is null)
                return;

            var notification = BuildNotification();
            manager.Notify(NOTIFICATION_ID, notification);
        }

        private Notification? BuildNotification()
        {
            var count = UnlockedVaults.Count;
            var title = count switch
            {
                0 => "VaultUnlocked".ToLocalized(),
                1 => "OneVaultIsUnlocked".ToLocalized(),
                _ => "MultipleVaultsAreUnlocked".ToLocalized(count)
            };

            // Tapping the notification brings the app back to foreground
            var tapIntent = new Intent(this, typeof(MainActivity));
            tapIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            var tapPendingIntent = PendingIntent.GetActivity(
                this, 0, tapIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            // "Lock all" action
            var lockAllIntent = new Intent(this, typeof(VaultForegroundService));
            lockAllIntent.SetAction(LockAll);
            var lockAllPendingIntent = PendingIntent.GetService(
                this, 0, lockAllIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var iconRid = MauiApplication.Current.GetDrawableId("app_icon.png");
            if (iconRid == 0)
                iconRid = 0x0108002f; // ic_lock_lock

            return new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle(title)
                ?.SetContentText("TapToLockAll".ToLocalized())
                ?.SetSmallIcon(iconRid)
                ?.SetContentIntent(tapPendingIntent)
                ?.AddAction(0, "LockAll".ToLocalized(), lockAllPendingIntent)
                ?.SetOngoing(true)
                ?.SetOnlyAlertOnce(true)
                ?.Build();
        }

        private void EnsureNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Vault Status",
                    NotificationImportance.Low) // Low = no sound, no heads-up
                {
                    Description = "Shows which vaults are currently unlocked"
                };

            var manager = GetSystemService(NotificationService) as NotificationManager;
            manager?.CreateNotificationChannel(channel);
        }
    }
}
