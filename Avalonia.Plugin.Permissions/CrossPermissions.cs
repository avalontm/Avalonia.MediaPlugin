using Avalonia.Permissions.Abstractions;
using Splat;
using System;

namespace Avalonia.Permissions
{
    /// <summary>
    /// Cross platform Permissions implemenations
    /// </summary>
    public static class CrossPermissions
    {
        static readonly Lazy<IPermissions> implementation = new Lazy<IPermissions>(CreatePermissions, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => implementation.Value != null;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IPermissions Current
        {
            get
            {
                var ret = implementation.Value;

                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }

                return ret;
            }
        }

        static IPermissions CreatePermissions()
        {
            return Locator.Current.GetService<IPermissions>();
        }

        internal static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException(
                "This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
}