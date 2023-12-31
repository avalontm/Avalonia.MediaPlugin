﻿using Avalonia.MediaPlugin.Abstractions;
using Splat;


namespace Avalonia.MediaPlugin.iOS
{
    [Preserve(AllMembers = true)]
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.Register<IMedia>(() => new MediaImplementation());
        }

        public static Avalonia.AppBuilder UseMediaPlugin(this Avalonia.AppBuilder builder)
        {
            //Registramos 
            Register(Locator.CurrentMutable, Locator.Current);
            onPermissions();

            return builder;
        }

        static void onPermissions()
        {

           

        }
    }
}