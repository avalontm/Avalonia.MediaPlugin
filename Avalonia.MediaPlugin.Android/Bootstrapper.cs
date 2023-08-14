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
            Register(Locator.CurrentMutable, Locator.Current);

            return builder;
        }
    }
}