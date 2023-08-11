using Android;
using Android.Content.PM;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia.MediaPlugin.Abstractions;
using Splat;


namespace Avalonia.MediaPlugin.Android
{
    [Preserve(AllMembers = true)]
    public static class Bootstrapper
    {
        const int locationPermissionsRequestCode = 1000;
        public static Activity? Context { get; private set; }

        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.Register<IMedia>(() => new MediaImplementation());
        }

        public static Avalonia.AppBuilder UseMediaPlugin(this Avalonia.AppBuilder builder, Activity activity)
        {
            Context = activity;
            //Registramos 
            Register(Locator.CurrentMutable, Locator.Current);
            onPermissions();

            return builder;
        }

        static void onPermissions()
        {

            var locationPermissions = new[]
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessLocationExtraCommands,
                Manifest.Permission.BluetoothConnect,
                Manifest.Permission.BluetoothScan,
                Manifest.Permission.BluetoothPrivileged,
                Manifest.Permission.Camera
            };

            var cameraPermissionGranted = ContextCompat.CheckSelfPermission(Context, Manifest.Permission.Camera);

            // if either is denied permission, request permission from the user
            if (cameraPermissionGranted == Permission.Denied)
            {
                ActivityCompat.RequestPermissions(Context, locationPermissions, locationPermissionsRequestCode);
            }

        }
    }
}