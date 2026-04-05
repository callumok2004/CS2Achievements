using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CS2Achievements;

public class AchievementDisplayItem
{
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	public string CategoryName { get; set; } = "";
	public int Progress { get; set; }
	public int MaxProgress { get; set; }
	public bool Complete { get; set; }
	public BitmapSource? Icon { get; set; }
	public string ProgressText => MaxProgress > 0 ? $"{Progress} / {MaxProgress}" : "";
	public bool HasProgress => MaxProgress > 0;
	public double IconOpacity => (Progress > 0 || Complete) ? 1.0 : 0.5;
}

public partial class MainWindow : Window
{
	private ICollectionView? _logView;
	private ICollectionView? _achievementView;
	private readonly List<AchievementDisplayItem> _achievementItems = [];
	private readonly DispatcherTimer _refreshTimer;
	private readonly DispatcherTimer _statusTimer;

	public MainWindow() {
		InitializeComponent();
		_refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
		_refreshTimer.Tick += (_, _) => RefreshAchievements();
		_statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
		_statusTimer.Tick += (_, _) => UpdateStatus();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e) {
		_logView = CollectionViewSource.GetDefaultView(AppLogger.Instance.Entries);
		_logView.Filter = LogFilter;
		LogList.ItemsSource = _logView;

		AppLogger.Instance.Entries.CollectionChanged += OnLogEntriesChanged;

		InitConfigControls();

		RefreshAchievements();
		_refreshTimer.Start();
		_statusTimer.Start();

		Task.Run(GameService.Start);
	}

	private void InitConfigControls() {
		PopupAlways.IsChecked = AppConfig.Instance.ProgressPopups == PopupMode.Always;
		PopupRoundEnd.IsChecked = AppConfig.Instance.ProgressPopups == PopupMode.OnRoundEnd;
		PopupMilestones.IsChecked = AppConfig.Instance.ProgressPopups == PopupMode.AtMilestones;
		ShowUnlockCheck.IsChecked = AppConfig.Instance.ShowUnlockPopups;
	}

	private void Tab_Checked(object sender, RoutedEventArgs e) {
		if (LogsPanel == null) return;
		LogsPanel.Visibility = TabLogs.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
		ConfigPanel.Visibility = TabConfig.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
		AchievementsPanel.Visibility = TabAchievements.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
	}

	private void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
	private void MaximizeClick(object sender, RoutedEventArgs e) =>
		WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
	private void CloseClick(object sender, RoutedEventArgs e) => Close();

	protected override void OnStateChanged(EventArgs e) {
		base.OnStateChanged(e);
		MaxBtn.Content = WindowState == WindowState.Maximized ? "❐" : "□";
		OuterBorder.Padding = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
	}

	private void LogSearch_TextChanged(object sender, TextChangedEventArgs e) => _logView?.Refresh();
	private void LogFilterChanged(object sender, RoutedEventArgs e) => _logView?.Refresh();

	private bool LogFilter(object obj) {
		if (obj is not LogEntry entry) return false;

		bool levelOk = entry.Level switch {
			LogLevel.Verbose => FilterVerbose.IsChecked == true,
			LogLevel.Debug => FilterDebug.IsChecked == true,
			LogLevel.Information => FilterInfo.IsChecked == true,
			LogLevel.Warning => FilterWarn.IsChecked == true,
			LogLevel.Error => FilterError.IsChecked == true,
			LogLevel.Fatal => FilterFatal.IsChecked == true,
			_ => true
		};
		if (!levelOk) return false;

		if (!string.IsNullOrWhiteSpace(LogSearch.Text) &&
			!entry.Message.Contains(LogSearch.Text, StringComparison.OrdinalIgnoreCase))
			return false;

		return true;
	}

	private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		if (e.Action == NotifyCollectionChangedAction.Add && LogList.Items.Count > 0) {
			Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
				if (LogList.Items.Count > 0)
					LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
			});
		}
	}

	private void AchievementSearch_TextChanged(object sender, TextChangedEventArgs e) => _achievementView?.Refresh();
	private void AchievementSortChanged(object sender, RoutedEventArgs e) => ApplyAchievementSort();

	private void RefreshAchievements() {
		// Preserve scroll position
		var scrollViewer = GetScrollViewer(AchievementListBox);
		double savedOffset = scrollViewer?.VerticalOffset ?? 0;

		_achievementItems.Clear();
		int completed = 0;
		int total = Achievements.AchievementList.Count;

		foreach (var a in Achievements.AchievementList) {
			_achievementItems.Add(new AchievementDisplayItem {
				Name = a.Name,
				Description = a.Description,
				CategoryName = FormatCategoryName(a.Category),
				Progress = a.Progress,
				MaxProgress = a.MaxProgress,
				Complete = a.Complete,
				Icon = LoadWpfIcon(a.Name)
			});
			if (a.Complete) completed++;
		}

		_achievementView = CollectionViewSource.GetDefaultView(_achievementItems);
		_achievementView.Filter = AchievementFilter;
		ApplyAchievementSort();
		AchievementListBox.ItemsSource = _achievementView;
		AchievementCount.Text = $"{completed} / {total} completed";

		// Restore scroll position
		Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => {
			scrollViewer?.ScrollToVerticalOffset(savedOffset);
		});
	}

	private static ScrollViewer? GetScrollViewer(DependencyObject o) {
		if (o is ScrollViewer sv) return sv;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++) {
			var child = GetScrollViewer(VisualTreeHelper.GetChild(o, i));
			if (child != null) return child;
		}
		return null;
	}

	private bool AchievementFilter(object obj) {
		if (obj is not AchievementDisplayItem item) return false;
		if (string.IsNullOrWhiteSpace(AchievementSearch.Text)) return true;
		return item.Name.Contains(AchievementSearch.Text, StringComparison.OrdinalIgnoreCase) ||
				 item.Description.Contains(AchievementSearch.Text, StringComparison.OrdinalIgnoreCase) ||
				 item.CategoryName.Contains(AchievementSearch.Text, StringComparison.OrdinalIgnoreCase);
	}

	private void ApplyAchievementSort() {
		if (_achievementView == null) return;

		_achievementView.GroupDescriptions.Clear();
		_achievementView.SortDescriptions.Clear();

		if (SortCategory?.IsChecked == true) {
			_achievementView.GroupDescriptions.Add(new PropertyGroupDescription("CategoryName"));
			_achievementView.SortDescriptions.Add(new SortDescription("CategoryName", ListSortDirection.Ascending));
			_achievementView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		}
		else if (SortName?.IsChecked == true) {
			_achievementView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		}
		else if (SortProgress?.IsChecked == true) {
			_achievementView.SortDescriptions.Add(new SortDescription("Complete", ListSortDirection.Descending));
			_achievementView.SortDescriptions.Add(new SortDescription("Progress", ListSortDirection.Descending));
		}
	}

	private void UpdateStatus() {
		SteamIdText.Text = Global.SteamID != null ? $"SteamID: {Global.SteamID}" : "SteamID: not detected";
		ConfigSteamIdText.Text = Achievements.LoadedSteamId != null
			? $"SteamID: {Achievements.LoadedSteamId}"
			: "No Steam account loaded — waiting for Steam...";
		StatusText.Text = GameService.IsRunning ? "Listening for game events..." : "Starting...";
		StatusDot.Fill = GameService.IsRunning
			? new SolidColorBrush(Color.FromRgb(0x5C, 0x7E, 0x10))
			: new SolidColorBrush(Color.FromRgb(0xD4, 0xA0, 0x17));
	}

	private void PopupMode_Changed(object sender, RoutedEventArgs e) {
		if (PopupAlways?.IsChecked == true) AppConfig.Instance.ProgressPopups = PopupMode.Always;
		else if (PopupRoundEnd?.IsChecked == true) AppConfig.Instance.ProgressPopups = PopupMode.OnRoundEnd;
		else if (PopupMilestones?.IsChecked == true) AppConfig.Instance.ProgressPopups = PopupMode.AtMilestones;
		AppConfig.Save();
	}

	private void ShowUnlock_Changed(object sender, RoutedEventArgs e) {
		AppConfig.Instance.ShowUnlockPopups = ShowUnlockCheck.IsChecked == true;
		AppConfig.Save();
	}

	private void ResetProgress_Click(object sender, RoutedEventArgs e) {
		if (Achievements.LoadedSteamId == null) {
			MessageBox.Show("No Steam account is loaded. Nothing to reset.",
				"Reset", MessageBoxButton.OK, MessageBoxImage.Information);
			return;
		}

		var result = MessageBox.Show(
			$"Are you sure you want to reset ALL achievement progress for SteamID {Achievements.LoadedSteamId}?\n\nThis cannot be undone.",
			"Confirm Reset",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning);

		if (result != MessageBoxResult.Yes) return;

		var confirm2 = MessageBox.Show(
			"Really? All progress will be permanently lost.",
			"Final Confirmation",
			MessageBoxButton.YesNo,
			MessageBoxImage.Exclamation);

		if (confirm2 != MessageBoxResult.Yes) return;

		Achievements.ResetAllProgress();
		RefreshAchievements();
	}

	static readonly Dictionary<string, BitmapSource?> _wpfIconCache = [];

	static BitmapSource? LoadWpfIcon(string name) {
		if (_wpfIconCache.TryGetValue(name, out var cached))
			return cached;

		string? normalized = Achievements.NormalizeNameForIcon(name);
		if (normalized == null) { _wpfIconCache[name] = null; return null; }

		string resourcePath = $"CS2Achievements.Images.{normalized}.png";
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
		if (stream == null) { _wpfIconCache[name] = null; return null; }

		try {
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.StreamSource = stream;
			bitmap.EndInit();
			bitmap.Freeze();
			_wpfIconCache[name] = bitmap;
			return bitmap;
		}
		catch {
			_wpfIconCache[name] = null;
			return null;
		}
	}

	static string FormatCategoryName(Category c) => c switch {
		Category.TeamTactics => "Team Tactics",
		Category.CombatSkills => "Combat Skills",
		Category.WeaponSpecialist => "Weapon Specialist",
		Category.GlobalExpertise => "Global Expertise",
		Category.ArmsRaceDemolition => "Arms Race & Demolition",
		Category.CutContent => "Cut Content",
		_ => c.ToString()
	};
}


public class LogLevelToColorConverter : IValueConverter
{
	static readonly Brush VerboseBrush = Frozen(new SolidColorBrush(Color.FromRgb(0x55, 0x66, 0x77)));
	static readonly Brush DebugBrush = Frozen(new SolidColorBrush(Color.FromRgb(0x8F, 0x98, 0xA0)));
	static readonly Brush InfoBrush = Frozen(new SolidColorBrush(Color.FromRgb(0x66, 0xC0, 0xF4)));
	static readonly Brush WarnBrush = Frozen(new SolidColorBrush(Color.FromRgb(0xD4, 0xA0, 0x17)));
	static readonly Brush ErrorBrush = Frozen(new SolidColorBrush(Color.FromRgb(0xCD, 0x3B, 0x3B)));
	static readonly Brush FatalBrush = Frozen(new SolidColorBrush(Colors.Red));
	static Brush Frozen(SolidColorBrush b) { b.Freeze(); return b; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		return value is LogLevel level ? level switch {
			LogLevel.Verbose => VerboseBrush,
			LogLevel.Debug => DebugBrush,
			LogLevel.Information => InfoBrush,
			LogLevel.Warning => WarnBrush,
			LogLevel.Error => ErrorBrush,
			LogLevel.Fatal => FatalBrush,
			_ => Brushes.White
		} : Brushes.White;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

public class ProgressToColorConverter : IValueConverter
{
	static readonly Brush GreenBrush = Frozen(new SolidColorBrush(Color.FromRgb(0x5C, 0x7E, 0x10)));
	static readonly Brush GoldBrush = Frozen(new SolidColorBrush(Color.FromRgb(0xC3, 0x93, 0x1F)));
	static Brush Frozen(SolidColorBrush b) { b.Freeze(); return b; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is true ? GoldBrush : GreenBrush;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
