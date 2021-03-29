namespace NativeAndroidWithMaui
{
	public class SnackbarMessageService : ISnackbarMessageService
	{
		public string GetMessage()
		{
			return $"This is from the service.";
		}
	}
}