using System.Windows;

namespace CS2Achievements;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		AppLogger.Instance.SetDispatcher(Dispatcher);
		base.OnStartup(e);
	}

	protected override void OnExit(ExitEventArgs e)
	{
		GameService.Stop();
		base.OnExit(e);
	}
}
