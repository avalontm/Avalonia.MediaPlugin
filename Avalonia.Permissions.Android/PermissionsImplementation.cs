﻿using Android;
using Android.Content;
#if __ANDROID_29__
using AndroidX.Core.App;
using AndroidX.Core.Content;
#else
using Android.Support.V4.App;
using Android.Support.V4.Content;
#endif
using System;
using Avalonia.Permissions.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App = Android.App.Application;
using Activity = Android.App.Activity;
using Android.OS;
using PM = Android.Content.PM;
using Uri = Android.Net.Uri;
using Provider = Android.Provider;

[assembly: LinkerSafe]
namespace Avalonia.Permissions.Android
{
	/// <summary>
	/// Implementation for Feature
	/// </summary>
	public class PermissionsImplementation : IPermissions
	{

		object locker = new object();
		TaskCompletionSource<Dictionary<Permission, PermissionStatus>> tcs;
		Dictionary<Permission, PermissionStatus> results;
		IList<string> requestedPermissions;

		/// <summary>
		/// Current Permissions Implementation
		/// </summary>
		public static PermissionsImplementation Current =>
			(PermissionsImplementation)CrossPermissions.Current;

		/// <summary>
		/// Request to see if you should show a rationale for requesting permission
		/// Only on Android
		/// </summary>
		/// <returns>True or false to show rationale</returns>
		/// <param name="permission">Permission to check.</param>
		public Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission)
		{
			var activity = Bootstrapper.Context ?? (Activity)App.Context;

			if (activity == null)
			{
                System.Diagnostics.Debug.WriteLine("Unable to detect current Activity. Please ensure Xamarin.Essentials is installed in your Android project and is initialized.");
				return Task.FromResult(false);
			}

			var names = GetManifestNames(permission);

			//if isn't an android specific group then go ahead and return false;
			if (names == null)
			{
				System.Diagnostics.Debug.WriteLine("No android specific permissions needed for: " + permission);
				return Task.FromResult(false);
			}

			if (names.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine("No permissions found in manifest for: " + permission + " no need to show request rationale");
				return Task.FromResult(false);
			}

			foreach (var name in names)
			{
				if (ActivityCompat.ShouldShowRequestPermissionRationale(activity, name))
					return Task.FromResult(true);
			}

			return Task.FromResult(false);

		}

		/// <summary>
		/// Determines whether this instance has permission the specified permission.
		/// </summary>
		/// <returns><c>true</c> if this instance has permission the specified permission; otherwise, <c>false</c>.</returns>
		/// <param name="permission">Permission to check.</param>
		public Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission)
		{
			var names = GetManifestNames(permission);

			//if isn't an android specific group then go ahead and return true;
			if (names == null)
			{
				System.Diagnostics.Debug.WriteLine("No android specific permissions needed for: " + permission);
				return Task.FromResult(PermissionStatus.Granted);
			}

			//if no permissions were found then there is an issue and persmission is not set in Android manifest
			if (names.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine("No permissions found in manifest for: " + permission);
				return Task.FromResult(PermissionStatus.Unknown);
			}

			var context = GetContext();
			if (context == null)
				return Task.FromResult(PermissionStatus.Unknown);

			var targetsMOrHigher = context.ApplicationInfo.TargetSdkVersion >= BuildVersionCodes.M;

			foreach (var name in names)
			{
				if (targetsMOrHigher)
				{
					if (ContextCompat.CheckSelfPermission(context, name) != PM.Permission.Granted)
						return Task.FromResult(PermissionStatus.Denied);
				}
				else
				{
					if (PermissionChecker.CheckSelfPermission(context, name) != PermissionChecker.PermissionGranted)
						return Task.FromResult(PermissionStatus.Denied);
				}
			}
			return Task.FromResult(PermissionStatus.Granted);
		}

		/// <summary>
		/// Requests the permissions from the users
		/// </summary>
		/// <returns>The permissions and their status.</returns>
		/// <param name="permissions">Permissions to request.</param>
		public async Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions)
		{
			if (tcs != null && !tcs.Task.IsCompleted)
			{
				tcs.SetCanceled();
				tcs = null;
			}
			lock (locker)
			{
				results = new Dictionary<Permission, PermissionStatus>();
			}

            var activity = Bootstrapper.Context ?? (Activity)App.Context;

            if (activity == null)
			{
                System.Diagnostics.Debug.WriteLine("Unable to detect current Activity. Please ensure Xamarin.Essentials is installed in your Android project and is initialized.");
				foreach (var permission in permissions)
				{
					lock (locker)
					{
						if (!results.ContainsKey(permission))
							results.Add(permission, PermissionStatus.Unknown);
					}
				}

				return results;
			}
			var permissionsToRequest = new List<string>();
			foreach (var permission in permissions)
			{
				var result = await CheckPermissionStatusAsync(permission);
				if (result != PermissionStatus.Granted)
				{
					var names = GetManifestNames(permission);
					//check to see if we can find manifest names
					//if we can't add as unknown and continue
					if ((names?.Count ?? 0) == 0)
					{
						lock (locker)
						{
							if (!results.ContainsKey(permission))
								results.Add(permission, PermissionStatus.Unknown);
						}
						continue;
					}

					permissionsToRequest.AddRange(names);
				}
				else
				{
					//if we are granted you are good!
					lock (locker)
					{
						if (!results.ContainsKey(permission))
							results.Add(permission, PermissionStatus.Granted);
					}
				}
			}

			if (permissionsToRequest.Count == 0)
				return results;

			tcs = new TaskCompletionSource<Dictionary<Permission, PermissionStatus>>();

			ActivityCompat.RequestPermissions(activity, permissionsToRequest.ToArray(), permissioncode);

			return await tcs.Task;
		}

		const int permissioncode = 25;
		/// <summary>
		/// Callback that must be set when request permissions has finished
		/// </summary>
		/// <param name="requestCode"></param>
		/// <param name="permissions"></param>
		/// <param name="grantResults"></param>
		public void OnRequestPermissionsResult(int requestCode, string[] permissions, PM.Permission[] grantResults)
		{
			if (requestCode != permissioncode)
				return;

			if (tcs == null)
				return;

			for (var i = 0; i < permissions.Length; i++)
			{
				if (tcs.Task.Status == TaskStatus.Canceled)
					return;

				var permission = GetPermissionForManifestName(permissions[i]);
				if (permission == Permission.Unknown)
					continue;

				lock (locker)
				{
					if (permission == Permission.Microphone)
					{
						if (!results.ContainsKey(Permission.Speech))
							results.Add(Permission.Speech, grantResults[i] == PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
					}
					else if (permission == Permission.Location)
					{
						if (!results.ContainsKey(Permission.LocationAlways))
							results.Add(Permission.LocationAlways, grantResults[i] == PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);

						if (!results.ContainsKey(Permission.LocationWhenInUse))
							results.Add(Permission.LocationWhenInUse, grantResults[i] == PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
					}

					if (!results.ContainsKey(permission))
						results.Add(permission, grantResults[i] == PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
				}
			}

			tcs.TrySetResult(results);
		}

		static Permission GetPermissionForManifestName(string permission)
		{
			switch (permission)
			{
				case Manifest.Permission.ReadCalendar:
				case Manifest.Permission.WriteCalendar:
					return Permission.Calendar;
				case Manifest.Permission.Camera:
					return Permission.Camera;
				case Manifest.Permission.ReadContacts:
				case Manifest.Permission.WriteContacts:
				case Manifest.Permission.GetAccounts:
					return Permission.Contacts;
				case Manifest.Permission.AccessCoarseLocation:
				case Manifest.Permission.AccessFineLocation:
#if __ANDROID_29__
				case Manifest.Permission.AccessBackgroundLocation:
#endif
					return Permission.Location;
				case Manifest.Permission.RecordAudio:
					return Permission.Microphone;
				case Manifest.Permission.ReadPhoneState:
				case Manifest.Permission.CallPhone:
				case Manifest.Permission.ReadCallLog:
				case Manifest.Permission.WriteCallLog:
				case Manifest.Permission.AddVoicemail:
				case Manifest.Permission.UseSip:
				case Manifest.Permission.ProcessOutgoingCalls:
					return Permission.Phone;
				case Manifest.Permission.BodySensors:
					return Permission.Sensors;
				case Manifest.Permission.SendSms:
				case Manifest.Permission.ReceiveSms:
				case Manifest.Permission.ReadSms:
				case Manifest.Permission.ReceiveWapPush:
				case Manifest.Permission.ReceiveMms:
					return Permission.Sms;
				case Manifest.Permission.ReadExternalStorage:
				case Manifest.Permission.WriteExternalStorage:
					return Permission.Storage;
			}

			return Permission.Unknown;
		}

		List<string> GetManifestNames(Permission permission)
		{
			var permissionNames = new List<string>();
			switch (permission)
			{
				case Permission.Calendar:
					{
						if (HasPermissionInManifest(Manifest.Permission.ReadCalendar))
							permissionNames.Add(Manifest.Permission.ReadCalendar);
						if (HasPermissionInManifest(Manifest.Permission.WriteCalendar))
							permissionNames.Add(Manifest.Permission.WriteCalendar);
					}
					break;
				case Permission.Camera:
					{
						if (HasPermissionInManifest(Manifest.Permission.Camera))
							permissionNames.Add(Manifest.Permission.Camera);
					}
					break;
				case Permission.Contacts:
					{
						if (HasPermissionInManifest(Manifest.Permission.ReadContacts))
							permissionNames.Add(Manifest.Permission.ReadContacts);

						if (HasPermissionInManifest(Manifest.Permission.WriteContacts))
							permissionNames.Add(Manifest.Permission.WriteContacts);

						if (HasPermissionInManifest(Manifest.Permission.GetAccounts))
							permissionNames.Add(Manifest.Permission.GetAccounts);
					}
					break;
				case Permission.LocationAlways:
					{
						if (HasPermissionInManifest(Manifest.Permission.AccessCoarseLocation))
							permissionNames.Add(Manifest.Permission.AccessCoarseLocation);


						if (HasPermissionInManifest(Manifest.Permission.AccessFineLocation))
							permissionNames.Add(Manifest.Permission.AccessFineLocation);

#if __ANDROID_29__
						var context = GetContext();
						if (context == null)
							break;

						var targetsQ = context.ApplicationInfo.TargetSdkVersion >= BuildVersionCodes.Q;
						var runningQ = (int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.Q;
						if (targetsQ && runningQ)
						{
							if (HasPermissionInManifest(Manifest.Permission.AccessBackgroundLocation))
								permissionNames.Add(Manifest.Permission.AccessBackgroundLocation);
						}
#endif
					}
					break;
				case Permission.LocationWhenInUse:
				case Permission.Location:
					{
						if (HasPermissionInManifest(Manifest.Permission.AccessCoarseLocation))
							permissionNames.Add(Manifest.Permission.AccessCoarseLocation);


						if (HasPermissionInManifest(Manifest.Permission.AccessFineLocation))
							permissionNames.Add(Manifest.Permission.AccessFineLocation);
					}
					break;
				case Permission.Speech:
				case Permission.Microphone:
					{
						if (HasPermissionInManifest(Manifest.Permission.RecordAudio))
							permissionNames.Add(Manifest.Permission.RecordAudio);

					}
					break;
				case Permission.Phone:
					{
						if (HasPermissionInManifest(Manifest.Permission.ReadPhoneState))
							permissionNames.Add(Manifest.Permission.ReadPhoneState);

						if (HasPermissionInManifest(Manifest.Permission.CallPhone))
							permissionNames.Add(Manifest.Permission.CallPhone);

						if (HasPermissionInManifest(Manifest.Permission.ReadCallLog))
							permissionNames.Add(Manifest.Permission.ReadCallLog);

						if (HasPermissionInManifest(Manifest.Permission.WriteCallLog))
							permissionNames.Add(Manifest.Permission.WriteCallLog);

						if (HasPermissionInManifest(Manifest.Permission.AddVoicemail))
							permissionNames.Add(Manifest.Permission.AddVoicemail);

						if (HasPermissionInManifest(Manifest.Permission.UseSip))
							permissionNames.Add(Manifest.Permission.UseSip);

						if (HasPermissionInManifest(Manifest.Permission.ProcessOutgoingCalls))
							permissionNames.Add(Manifest.Permission.ProcessOutgoingCalls);
					}
					break;
				case Permission.Sensors:
					{
						if (HasPermissionInManifest(Manifest.Permission.BodySensors))
							permissionNames.Add(Manifest.Permission.BodySensors);
					}
					break;
				case Permission.Sms:
					{
						if (HasPermissionInManifest(Manifest.Permission.SendSms))
							permissionNames.Add(Manifest.Permission.SendSms);

						if (HasPermissionInManifest(Manifest.Permission.ReceiveSms))
							permissionNames.Add(Manifest.Permission.ReceiveSms);

						if (HasPermissionInManifest(Manifest.Permission.ReadSms))
							permissionNames.Add(Manifest.Permission.ReadSms);

						if (HasPermissionInManifest(Manifest.Permission.ReceiveWapPush))
							permissionNames.Add(Manifest.Permission.ReceiveWapPush);

						if (HasPermissionInManifest(Manifest.Permission.ReceiveMms))
							permissionNames.Add(Manifest.Permission.ReceiveMms);
					}
					break;
				case Permission.Storage:
					{
						if (HasPermissionInManifest(Manifest.Permission.ReadExternalStorage))
							permissionNames.Add(Manifest.Permission.ReadExternalStorage);

						if (HasPermissionInManifest(Manifest.Permission.WriteExternalStorage))
							permissionNames.Add(Manifest.Permission.WriteExternalStorage);
					}
					break;
				default:
					return null;
			}

			return permissionNames;
		}

		Context GetContext()
		{
            var context = Bootstrapper.Context ?? (Activity)App.Context;

            if (context == null)
			{
                System.Diagnostics.Debug.WriteLine("Unable to detect current Activity or App Context. Please ensure Xamarin.Essentials is installed in your Android project initialized.");				
			}

			return context;
		}

		bool HasPermissionInManifest(string permission)
		{
			try
			{
				if (requestedPermissions != null)
					return requestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));

				//try to use current activity else application context
				var context = GetContext();

				if (context == null)
					return false;

				var info = context.PackageManager.GetPackageInfo(context.PackageName, PM.PackageInfoFlags.Permissions);

				if (info == null)
				{
                    System.Diagnostics.Debug.WriteLine("Unable to get Package info, will not be able to determine permissions to request.");
					return false;
				}

				requestedPermissions = info.RequestedPermissions;

				if (requestedPermissions == null)
				{
					System.Diagnostics.Debug.WriteLine("There are no requested permissions, please check to ensure you have marked permissions you want to request.");
					return false;
				}

				return requestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));
			}
			catch (Exception ex)
			{
				Console.Write("Unable to check manifest for permission: " + ex);
			}
			return false;
		}

		/// <summary>
		/// Opens settings to app page
		/// </summary>
		/// <returns>true if could open.</returns>
		public bool OpenAppSettings()
		{

			var context = GetContext();
			if (context == null)
				return false;

			try
			{
				var settingsIntent = new Intent();
				settingsIntent.SetAction(Provider.Settings.ActionAppNotificationSettings);
				settingsIntent.AddCategory(Intent.CategoryDefault);
				settingsIntent.SetData(Uri.Parse("package:" + context.PackageName));
				settingsIntent.AddFlags(ActivityFlags.NewTask);
				settingsIntent.AddFlags(ActivityFlags.NoHistory);
				settingsIntent.AddFlags(ActivityFlags.ExcludeFromRecents);
				context.StartActivity(settingsIntent);
				return true;
			}
			catch
			{
				return false;
			}


		}

		public Task<PermissionStatus> CheckPermissionStatusAsync<T>() where T : BasePermission, new() =>
			new T().CheckPermissionStatusAsync();

		public Task<PermissionStatus> RequestPermissionAsync<T>() where T : BasePermission, new() =>
			new T().RequestPermissionAsync();
	}
}