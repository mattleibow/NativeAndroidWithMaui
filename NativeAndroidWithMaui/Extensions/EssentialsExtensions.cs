using Android.App;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace NativeAndroidWithMaui
{
	/// <summary>
	/// These extension methods can be used to setup and configure the library.
	/// In most cases, the library author will create this.
	/// </summary>
	public static class EssentialsExtensions
	{
		public static IAppHostBuilder UseEssentials(this IAppHostBuilder builder, Application application) =>
			builder.UseEssentials(() => Platform.Init(application));

		public static IAppHostBuilder UseEssentials(this IAppHostBuilder builder, Activity activity, Bundle bundle) =>
			builder.UseEssentials(() => Platform.Init(activity, bundle));

		private static IAppHostBuilder UseEssentials(this IAppHostBuilder builder, Action initDelegate)
		{
			builder.ConfigureLifecycleEvents((ctx, life) =>
			{
				initDelegate();

				life.AddAndroid(android => android
					.OnRequestPermissionsResult((activity, requestCode, permissions, grantResults) =>
					{
						Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
					})
					.OnNewIntent((activity, intent) =>
					{
						Platform.OnNewIntent(intent);
					})
					.OnResume((activity) =>
					{
						Platform.OnResume();
					}));
			});

			return builder;
		}

		public static IAppHostBuilder ConfigureEssentials(this IAppHostBuilder builder, Action<HostBuilderContext, IEssentialsBuilder> configureDelegate)
		{
			builder.ConfigureServices<EssentialsBuilder>(configureDelegate);
			return builder;
		}

		public static IEssentialsBuilder AddAppAction(this IEssentialsBuilder essentials, string id, string title, string subtitle = null, string icon = null) =>
			essentials.AddAppAction(new AppAction(id, title, subtitle, icon));

		/// <summary>
		/// This class is an internal mechanism to hook up ny services, events or properties for any given library.
		/// </summary>
		private class EssentialsBuilder : IEssentialsBuilder, IServiceCollectionBuilder
		{
			readonly List<AppAction> _appActions = new List<AppAction>();
			Action<AppAction> _appActionHandlers;
			bool _trackVersions;
			private bool _useLegaceSecureStorage;

			public IEssentialsBuilder AddAppAction(AppAction appAction)
			{
				_appActions.Add(appAction);
				return this;
			}

			public IEssentialsBuilder OnAppAction(Action<AppAction> action)
			{
				_appActionHandlers += action;
				return this;
			}

			public IEssentialsBuilder TrackVersion()
			{
				_trackVersions = true;
				return this;
			}

			public IEssentialsBuilder UseLegacySecureStorage()
			{
				_useLegaceSecureStorage = true;
				return this;
			}

			public void Build(IServiceCollection services)
			{
			}

			public async void Configure(IServiceProvider services)
			{
				AppActions.OnAppAction += HandleOnAppAction;

				await AppActions.SetAsync(_appActions);

				if (_trackVersions)
					VersionTracking.Track();

				if (_useLegaceSecureStorage)
					SecureStorage.LegacyKeyHashFallback = _useLegaceSecureStorage;
			}

			private void HandleOnAppAction(object sender, AppActionEventArgs e)
			{
				_appActionHandlers?.Invoke(e.AppAction);
			}
		}
	}
}