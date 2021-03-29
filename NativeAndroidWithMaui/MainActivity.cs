using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System;
using System.Threading;
using Xamarin.Essentials;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace NativeAndroidWithMaui
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, IDialogInterfaceOnCancelListener
	{
		private CancellationTokenSource _cancellationTokenSource;

		public IServiceProvider Services { get; private set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// The app host builder can be run anywhere, even inside the activity.
			// In most cases, the builder can be done once in the Application, and
			// then just accessed by all the activities.
			var host = new AppHostBuilder()
				.ConfigureEssentials((_, essentials) => essentials
					.AddAppAction("action", "Action Title")
					.TrackVersion()
					.OnAppAction(action => Console.WriteLine($"App action was triggered: {action.Title} ({action.Id})")))
				.UseEssentials(this, savedInstanceState)
				.ConfigureServices((_, services) => services
					.AddSingleton<ISnackbarMessageService, SnackbarMessageService>())
				.Build();

			// We can get access to all the services registered.
			Services = host.Services;

			// Normal app code.
			SetupAppUI();
		}

		private void FabOnClick(object sender, EventArgs eventArgs)
		{
			var view = (View)sender;

			// Since we have the app host, which supports services, we can just access
			// all of the registered services and use them.
			var messages = Services.GetRequiredService<ISnackbarMessageService>();

			Snackbar
				.Make(view, messages.GetMessage(), Snackbar.LengthLong)
				.SetAction("Action", (View.IOnClickListener)null)
				.Show();
		}

		private void SetupAppUI()
		{
			SetContentView(Resource.Layout.activity_main);

			var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
			fab.Click += FabOnClick;

			var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
			drawer.AddDrawerListener(toggle);
			toggle.SyncState();

			var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
			navigationView.SetNavigationItemSelectedListener(this);
		}

		public override void OnBackPressed()
		{
			var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			if (drawer.IsDrawerOpen(GravityCompat.Start))
				drawer.CloseDrawer(GravityCompat.Start);
			else
				base.OnBackPressed();
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu_main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			var id = item.ItemId;
			if (id == Resource.Id.action_settings)
			{
				LoadLocationAsync();
				return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		private async void LoadLocationAsync()
		{
			var dialog = new AlertDialog.Builder(this)
				.SetTitle("Loading location...")
				.SetMessage("We are currently trying to obtain your location for no reason.")
				.SetOnCancelListener(this)
				.Create();

			try
			{
				_cancellationTokenSource = new CancellationTokenSource();

				dialog.Show();

				var location = await Geolocation.GetLocationAsync(
					new GeolocationRequest(GeolocationAccuracy.Lowest),
					_cancellationTokenSource.Token);

				dialog.SetTitle("Location loaded!");
				dialog.SetMessage($"We found wehere you are! You are at {location.Latitude}, {location.Longitude}!");
			}
			catch (Exception ex)
			{
				dialog.SetTitle("Error!");
				dialog.SetMessage($"Something bad happened and we don't know what! The last message we recieved was: '{ex.Message}'");
			}
		}

		public void OnCancel(IDialogInterface dialog)
		{
			_cancellationTokenSource.Cancel();
		}

		public bool OnNavigationItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.nav_camera:
					break;
				case Resource.Id.nav_gallery:
					break;
				case Resource.Id.nav_slideshow:
					break;
				case Resource.Id.nav_manage:
					break;
				case Resource.Id.nav_share:
					break;
				case Resource.Id.nav_send:
					break;
			}

			var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			drawer.CloseDrawer(GravityCompat.Start);

			return true;
		}
	}
}