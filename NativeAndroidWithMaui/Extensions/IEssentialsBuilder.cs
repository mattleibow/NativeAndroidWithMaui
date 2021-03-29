using System;
using Xamarin.Essentials;

namespace NativeAndroidWithMaui
{
	/// <summary>
	/// This interface represents the app-level configuration of Xamarin.Essentials.
	/// In most cases, the library author will create this and provide a set of
	/// extensions methods to get started.
	/// </summary>
	public interface IEssentialsBuilder
	{
		IEssentialsBuilder AddAppAction(AppAction appAction);

		IEssentialsBuilder OnAppAction(Action<AppAction> action);

		IEssentialsBuilder TrackVersion();

		IEssentialsBuilder UseLegacySecureStorage();
	}
}