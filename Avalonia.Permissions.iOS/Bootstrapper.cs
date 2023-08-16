
using Avalonia.Permissions.Abstractions;
using Avalonia.Permissions.iOS;
using Splat;


namespace Avalonia.MediaPlugin.iOS
{
    [Preserve(AllMembers = true)]
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.Register<IPermissions>(() => new PermissionsImplementation());
        }

        public static Avalonia.AppBuilder UsePermissions(this Avalonia.AppBuilder builder)
        {
            //Registramos 
            Register(Locator.CurrentMutable, Locator.Current);
            return builder;
        }
    }
}