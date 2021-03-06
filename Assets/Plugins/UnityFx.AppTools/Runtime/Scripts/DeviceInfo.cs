﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityFx.AppTools
{
	/// <summary>
	/// Provides app-related utilities (app identifiers, first launch flag etc).
	/// </summary>
	public sealed class DeviceInfo
	{
		#region data

		public const string _keyPrefix = "DeviceInfo.";
		public const string _keyDeviceId = _keyPrefix + "DeviceId";
		public const string _keyFirstLaunch = _keyPrefix + "FirstLaunch";

		#endregion

		#region interface

		/// <summary>
		/// Returns unique device identifier.
		/// </summary>
		/// <seealso cref="VendorId"/>
		/// <seealso cref="AdvertisingId"/>
		public string DeviceId { get; private set; }

		/// <summary>
		/// Returns application vendor identifier.
		/// </summary>
		/// <seealso cref="AdvertisingId"/>
		/// <seealso cref="DeviceId"/>
		public string VendorId { get; private set; }

		/// <summary>
		/// Returns the application advertising identifier. Read only.
		/// </summary>
		/// <seealso cref="IsAdvertisingTrackingEnabled"/>
		/// <seealso cref="VendorId"/>
		/// <seealso cref="DeviceId"/>
		public string AdvertisingId { get; private set; }

		/// <summary>
		/// Returns <see langword="true"/> if <see cref="AdvertisingId"/> is available; <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="AdvertisingId"/>
		public bool IsAdvertisingTrackingEnabled { get; private set; }

		/// <summary>
		/// Returns <see langword="true"/> if it is the application is started for the first time; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsFirstLaunch { get; private set; }

		/// <summary>
		/// Initiates the app info initialization.
		/// </summary>
		public static Task<DeviceInfo> CreateAsync()
		{
				var result = new DeviceInfo();

#if UNITY_IOS && !UNITY_EDITOR

				result.AdvertisingId = UnityEngine.iOS.Device.advertisingIdentifier;
				result.IsAdvertisingTrackingEnabled = UnityEngine.iOS.Device.advertisingTrackingEnabled;
				result.VendorId = UnityEngine.iOS.Device.vendorIdentifier;
				result.DeviceId = GetDeviceId(result.AdvertisingId, result.VendorId);
				result.IsFirstLaunch = GetSetFirstLaunch();

				return Task.FromResult(result);

#else

				var tcs = new TaskCompletionSource<DeviceInfo>();

				if (!Application.RequestAdvertisingIdentifierAsync((advertisingId, trackingEnabled, errorMsg) =>
				{
					if (string.IsNullOrEmpty(errorMsg))
					{
						result.AdvertisingId = advertisingId;
						result.IsAdvertisingTrackingEnabled = trackingEnabled;

						try
						{
							result.VendorId = GetVendorId();
							result.DeviceId = GetDeviceId(advertisingId, result.VendorId);
							result.IsFirstLaunch = GetSetFirstLaunch();

							tcs.TrySetResult(result);
						}
						catch (Exception e)
						{
							tcs.TrySetException(e);
						}
					}
					else
					{
						tcs.TrySetException(new Exception(errorMsg));
					}
				}))
				{
					result.VendorId = GetVendorId();
					result.DeviceId = GetDeviceId(null, result.VendorId);
					result.IsFirstLaunch = GetSetFirstLaunch();

					tcs.TrySetResult(result);
				}

				return tcs.Task;

#endif
		}

		#endregion

		#region implementation

#if UNITY_IOS && !UNITY_EDITOR

		[System.Runtime.InteropServices.DllImport("__Internal")]
		private static extern string _GetKeychainValue(string key);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		private static extern void _SetKeychainValue(string key, string value);

#endif

		private static bool GetSetFirstLaunch()
		{
			if (PlayerPrefs.HasKey(_keyFirstLaunch))
			{
				return false;
			}
			else
			{
				PlayerPrefs.SetInt(_keyFirstLaunch, 0);
				return true;
			}
		}

		private static string GetDeviceId(string advertisingId, string vendorId)
		{
			if (PlayerPrefs.HasKey(_keyDeviceId))
			{
				var result = PlayerPrefs.GetString(_keyDeviceId);

				if (string.IsNullOrEmpty(result))
				{
					result = GetDeviceId2(advertisingId, vendorId);
				}

				return result;
			}
			else
			{
				return GetDeviceId2(advertisingId, vendorId);
			}
		}

		private static string GetDeviceId2(string advertisingId, string vendorId)
		{
#if UNITY_IOS && !UNITY_EDITOR

			var result = _GetKeychainValue(_keyDeviceId);

			if (string.IsNullOrEmpty(result))
			{
				result = GetDeviceId3(advertisingId, vendorId);
			}
			else
			{
				PlayerPrefs.SetString(_keyDeviceId, result);
				PlayerPrefs.Save();
			}

			return result;

#else

			return GetDeviceId3(advertisingId, vendorId);

#endif
		}

		private static string GetDeviceId3(string advertisingId, string vendorId)
		{
			var result = string.IsNullOrEmpty(advertisingId) ? vendorId : advertisingId;

			if (string.IsNullOrEmpty(result))
			{
				result = Guid.NewGuid().ToString().Replace("-", string.Empty);
			}

			PlayerPrefs.SetString(_keyDeviceId, result);
			PlayerPrefs.Save();

#if UNITY_IOS && !UNITY_EDITOR

			_SetKeychainValue(_keyDeviceId, result);

#endif

			return result;
		}

		private static string GetVendorId()
		{
#if UNITY_ANDROID && !UNITY_EDITOR

			// NOTE: getting the SystemInfo.deviceUniqueIdentifier on Android involves accessing device IMEI information,
			// which undesirably forces checking of the READ_PHONE_STATE permission in the Android manifest
			// http://forum.unity3d.com/threads/unique-identifier-details.353256/
			var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
			var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
			var secure = new AndroidJavaClass("android.provider.Settings$Secure");
			var androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");

			return androidId;

#elif UNITY_IOS && !UNITY_EDITOR

			return UnityEngine.iOS.Device.vendorIdentifier;

#else

			return SystemInfo.deviceUniqueIdentifier;

#endif
		}

		private static bool TryGetAdvertisingId(out string advertisingId)
		{
#if UNITY_ANDROID && !UNITY_EDITOR

			// NOTE: getting advertising identifier on Android is a bit tricky
			// http://stackoverflow.com/questions/28179150/getting-the-google-advertising-id-and-limit-advertising
			var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
			var client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
			var adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

			advertisingId = adInfo.Call<string>("getId").ToString();
			return adInfo.Call<bool>("isLimitAdTrackingEnabled");

#elif UNITY_IOS && !UNITY_EDITOR

			advertisingId = UnityEngine.iOS.Device.advertisingIdentifier;
			return UnityEngine.iOS.Device.advertisingTrackingEnabled;

#endif

			advertisingId = string.Empty;
			return false;
		}

		#endregion
	}
}
