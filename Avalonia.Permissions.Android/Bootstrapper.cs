using Android;
using Android.Content.PM;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia.Permissions.Abstractions;
using Avalonia.Permissions.Android;
using Splat;


namespace Avalonia.Permissions.Android
{
    [Preserve(AllMembers = true)]
    public static class Bootstrapper
    {
        public static Activity? Context { get; private set; }

        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.Register<IPermissions>(() => new PermissionsImplementation());
        }

        public static Avalonia.AppBuilder UsePermissions(this Avalonia.AppBuilder builder, Activity activity)
        {
            Context = activity;
            //Registramos 
            Register(Locator.CurrentMutable, Locator.Current);
            return builder;
        }
    }
}