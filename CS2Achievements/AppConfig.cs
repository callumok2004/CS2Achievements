using System.Text.Json;

namespace CS2Achievements;

public enum PopupMode
{
	Always,
	OnRoundEnd,
	AtMilestones
}

public class AppConfig
{
	public static AppConfig Instance { get; private set; } = new();

	public PopupMode ProgressPopups { get; set; } = PopupMode.Always;
	public bool ShowUnlockPopups { get; set; } = true;

	static readonly string ConfigDir = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CS2Achievements");
	static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

	public static void Load() {
		if (!File.Exists(ConfigPath)) return;
		try {
			string json = File.ReadAllText(ConfigPath);
			Instance = JsonSerializer.Deserialize<AppConfig>(json) ?? new();
		}
		catch {
			Instance = new();
		}
	}

	public static void Save() {
		Directory.CreateDirectory(ConfigDir);
		string json = JsonSerializer.Serialize(Instance, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(ConfigPath, json);
	}
}
