global using static CS2Achievements.Global;

using CounterStrike2GSI;
using CounterStrike2GSI.EventMessages;
using CounterStrike2GSI.Nodes;

using System.Diagnostics;

namespace CS2Achievements;

public static class Global
{
	public static CounterStrike2GSI.GameState? CurrentGameState;
	public static GameMode CurrentGameMode = GameMode.Undefined;
	public static string CurrentMap = string.Empty;
	public static PlayerTeam? CurrentTeam = null;
	public static string? SteamID = null;
	public static Player? Self = null;
	public static bool IsFirstRound;
	public static bool IsLastRound;
	public static bool IsPistolRound;
	public static int CurrentRound;
	public static bool Flashed;
	public static int ConsecutiveFlashKills;
	public static DateTime LastHealthChange;
	public static RoundData CurrentRoundData = new();
	public static List<RecentKill> RecentKills = [];

	public static ILogger Logger { get; } = AppLogger.Instance;
}

public struct RecentKill
{
	public PlayerGotKill Event;
	public DateTime Timestamp;
}

public struct RoundData()
{
	public DateTime RoundStart = DateTime.Now;
	public bool HasBomb;
	public bool PlantedTheBomb;
	public DateTime LastHadBomb;
	public int Health;
	public int Kills;
	public int HeadshotKills;
	public bool Died;
	public HashSet<string> UniqueWeaponsUsed = [];
}

public static class GameService
{
	static GameStateListener? _gsl;
	static volatile bool _running;
	public static bool IsRunning => _running;

	public static void Start() {
		AppConfig.Load();

		_gsl = new GameStateListener(4000);

		if (!_gsl.GenerateGSIConfigFile("Achievements"))
			Logger.Error("Could not generate GSI configuration file.");

		try {
			SteamID = SteamIDReader.GetCurrentSteamID64String();
			Logger.Information("Current SteamID64: {SteamID}", SteamID);
			Achievements.LoadForSteamId(SteamID);
		}
		catch (Exception ex) {
			Logger.Error("Failed to get SteamID: {Message}", ex.Message);
		}

#if DEBUG
		GenerateImage();
#endif

		_gsl.NewGameState += OnNewGameState;
		_gsl.BombStateUpdated += OnBombStateUpdated;
		_gsl.PlayerGotKill += OnPlayerGotKill;
		_gsl.PlayerWeaponsPickedUp += OnPlayerWeaponsPickedUp;
		_gsl.PlayerWeaponsDropped += OnPlayerWeaponsDropped;
		_gsl.RoundStarted += OnRoundStarted;
		_gsl.RoundConcluded += OnRoundConcluded;
		_gsl.GamemodeChanged += OnGamemodeChanged;
		_gsl.MapUpdated += OnMapUpdated;
		_gsl.MatchStarted += OnMatchStarted;
		// _gsl.BombExploded += OnBombExploded;
		_gsl.PlayerHealthChanged += OnPlayerHealthChanged;
		_gsl.Gameover += OnGameover;
		// _gsl.KillFeed += OnKillFeed;
		_gsl.PlayerMoneyAmountChanged += OnPlayerMoneyAmountChanged;
		_gsl.PlayerDied += OnPlayerDied;
		_gsl.RoundPhaseUpdated += OnRoundPhaseUpdated;
		_gsl.PlayerFlashAmountChanged += OnPlayerFlashAmountChanged;
		_gsl.PlayerKillsChanged += OnPlayerKillsChanged;

		if (!_gsl.Start()) {
			Logger.Fatal("GameStateListener could not start. Try running this program as Administrator.");
			return;
		}

		_running = true;
		Logger.Information("Listening for game integration calls...");

		new Thread(() => {
			while (_running) {
				try {
					string currentSteamID = SteamIDReader.GetCurrentSteamID64String();
					if (currentSteamID != SteamID) {
						string oldId = SteamID ?? "none";
						SteamID = currentSteamID;
						Logger.Information("SteamID changed from {Old} to {New}.", oldId, SteamID);
						Achievements.LoadForSteamId(SteamID);
					}
				}
				catch (Exception ex) {
					SteamID = null;
					Logger.Error("Failed to get SteamID: {Message}", ex.Message);
				}
				Thread.Sleep(10000);
			}
		}) {
			IsBackground = true
		}.Start();
	}

	private static void OnPlayerKillsChanged(PlayerKillsChanged game_event) {
		if (game_event.New > game_event.Previous) {
			int diff = game_event.New - game_event.Previous;

			if (diff > 1) { // HACK: See comment at OnPlayerGotKill
				if (Achievements.WhileBlind().Invoke(game_event))
					Achievements.IncrementAchievementProgress("Spray and Pray", 1);
			}
		}
	}

	private static void OnPlayerFlashAmountChanged(PlayerFlashAmountChanged game_event) {
		if (game_event.Player.SteamID != SteamID) return;

		Flashed = game_event.New == 1;
		if (!Flashed)
			ConsecutiveFlashKills = 0;
	}

	private static void OnRoundPhaseUpdated(RoundPhaseUpdated game_event) {
		if (game_event.New == Phase.Live) {
			CurrentRoundData.RoundStart = DateTime.Now;
			if (Self!.Weapons.Any(w => w.Name == "weapon_c4"))
				CurrentRoundData.HasBomb = true;
		}
	}

	private static void OnPlayerDied(PlayerDied game_event) => CurrentRoundData.Died = true;

	private static void OnPlayerMoneyAmountChanged(PlayerMoneyAmountChanged game_event) {
		// Console.WriteLine($"money changed ({game_event.Previous} => {game_event.New}) ({game_event.New - game_event.Previous}) {CurrentRound}");
	}

	// private static void OnKillFeed(KillFeed game_event) {
	// 	// Console.WriteLine("got killfeed event");
	// }

	public static void Stop() {
		_running = false;
		_gsl?.Stop();
	}

	private static void OnNewGameState(CounterStrike2GSI.GameState gamestate) {
		if (SteamID == null) return;
		CurrentGameState = gamestate;

		if (gamestate.Player == null || gamestate.Player.SteamID != SteamID) return;
		Self = gamestate.Player;
		CurrentTeam = gamestate.Player.Team;
	}

	private static void OnBombStateUpdated(BombStateUpdated game_event) {
		Logger.Debug($"The bomb is now {game_event.New}.");
		// Console.WriteLine($"The bomb is now {game_event.New}.");

		if (game_event.New == BombState.Planted) {
			Logger.Debug($"firing BombPlanted event");
			Achievements.OnEvent(Event.BombPlanted, game_event);
		}
		// else if (game_event.New == BombState.Defused) {

		// }
	}

	private static void OnGamemodeChanged(GamemodeChanged game_event) {
		if (game_event.New == CurrentGameMode) return;

		CurrentGameMode = game_event.New;
		Logger.Debug($"Gamemode changed to {game_event.New}.");
	}

	private static void OnMapUpdated(MapUpdated game_event) {
		if (game_event.New.Name == CurrentMap) return;

		CurrentMap = game_event.New.Name;
		Logger.Debug($"Map changed to {game_event.New.Name}.");
	}

	private static void OnPlayerGotKill(PlayerGotKill game_event) { // FUN fact! Getting 2 kills with one bullet will only trigger this ONCE, thanks volvo
		if (game_event.Player.SteamID == SteamID) {
			Logger.Debug($"Got a kill with {game_event.Weapon.Name}");

			CurrentRoundData.Kills++;

			if (Flashed) ConsecutiveFlashKills += 1;
			if (game_event.IsHeadshot) CurrentRoundData.HeadshotKills++;

			CurrentRoundData.UniqueWeaponsUsed.Add(game_event.Weapon.Name);

			RecentKills.Add(new() { Event = game_event, Timestamp = DateTime.Now });
			if (RecentKills.Count > 10) RecentKills.RemoveAt(0);

			Achievements.OnEvent(Event.KilledPlayer, game_event);
			Achievements.AddUniqueItem("Expert Marksman", game_event.Weapon.Name);
		}
	}

	private static void OnPlayerWeaponsPickedUp(PlayerWeaponsPickedUp game_event) {
		if (game_event.Player.SteamID == SteamID) {
			if (game_event.Weapons.Any(w => w.Name == "weapon_c4"))
				CurrentRoundData.HasBomb = true;
		}
	}
	private static void OnPlayerWeaponsDropped(PlayerWeaponsDropped game_event) {
		if (game_event.Player.SteamID == SteamID) {
			if (game_event.Weapons.Any(w => w.Name == "weapon_c4")) {
				CurrentRoundData.HasBomb = false;
				CurrentRoundData.LastHadBomb = DateTime.Now;
			}
		}
	}

	private static void OnMatchStarted(MatchStarted game_event) => CurrentRoundData = new();

	private static async void OnRoundStarted(RoundStarted game_event) { // Fun fact, this is triggered as soon as a round ends, and NOT when the next freeze time starts, fun
		await Task.Delay(250);
		CurrentRoundData = new();
		CurrentRound = game_event.Round;
		IsFirstRound = game_event.IsFirstRound;
		IsLastRound = game_event.IsLastRound;
		IsPistolRound = game_event.IsFirstRound || game_event.Round == 13;
		LastHealthChange = DateTime.Now;
		AchievementTimers.DestroyAll();
	}

	private static void OnRoundConcluded(RoundConcluded game_event) {
		// Console.WriteLine("Round Concluded");
		if (game_event.WinningTeam == CurrentTeam) {
			Achievements.OnEvent(Event.RoundWon, game_event);
			if (game_event.IsLastRound)
				Achievements.OnEvent(Event.MatchWon, game_event);
		}

		Logger.Debug($"Round concluded - {game_event.RoundConclusionReason}");

		Achievements.FlushRoundEndPopups();
		AchievementTimers.DestroyAll();
	}

	private static void OnBombExploded(BombExploded game_event) {
		// Console.WriteLine($"BombExploded");
	}

	private static void OnPlayerHealthChanged(PlayerHealthChanged game_event) {
		CurrentRoundData.Health = game_event.New;
		LastHealthChange = DateTime.Now;

		if (!Achievements.OnArmsRace().Invoke(game_event))
			return;

		if (game_event.New < 10 && game_event.New > 0) {
			AchievementTimers.Create("Still Alive", () => {
				if (Achievements.HealthUnchangedForSeconds(5).Invoke(null) && CurrentRoundData.Health < 10)
					Achievements.IncrementAchievementProgress("Still Alive", 1);
			}, 5 * 1000);
		}
		else
			AchievementTimers.Destroy("Still Alive");
	}

	// private static void OnPlayerKillsChanged(PlayerKillsChanged game_event) {
	// 	Console.WriteLine($"PlayerKillsChanged: {game_event.New}");
	// }

	// private static void OnPlayerRoundKillsChanged(PlayerRoundKillsChanged game_event) {
	// 	Console.WriteLine($"PlayerRoundKillsChanged: {game_event.New}");
	// }

	private static async void OnGameover(Gameover game_event) {
		Achievements.OnEvent(Event.GameOver, game_event);
		Logger.Information("Game over!");

		int kills = CurrentRoundData.Kills;
		await Task.Delay(250);
		// Console.WriteLine("delayed check, race conditions yippie");
		if (kills < CurrentRoundData.Kills)
			Achievements.CheckAchievement("ArmsRaceWin");
	}

#if DEBUG
	public static string GetProjectRoot() {
		var dir = AppContext.BaseDirectory;

		for (int i = 0; i < 5; i++) {
			if (Directory.GetFiles(dir, "*.csproj").Length > 0)
				return dir;

			dir = Directory.GetParent(dir)?.FullName;
			if (dir == null)
				break;
		}

		return AppContext.BaseDirectory;
	}

	public static void GenerateImage() {
		var root = GetProjectRoot();
		var path = Path.Combine(root, "ACHIEVEMENTS.png");
		Achievements.GenerateImage(path);
	}
#endif
}

public static class AchievementTimers
{
	static readonly Dictionary<string, (Timer timer, int version)> Timers = new();

	public static void Create(string name, Action action, int delayMs) {
		Destroy(name);

		int version = 0;

		Timer? timer = null;
		timer = new Timer(_ => {
			if (!Timers.TryGetValue(name, out var entry))
				return;

			if (entry.version != version)
				return;

			action();

			Destroy(name);
		}, null, delayMs, Timeout.Infinite);

		Timers[name] = (timer, version);
	}

	public static void Destroy(string name) {
		if (Timers.TryGetValue(name, out var entry)) {
			entry.timer.Dispose();
			Timers.Remove(name);
		}
	}

	public static void DestroyAll() {
		foreach (var timer in Timers.Values)
			timer.timer.Dispose();

		Timers.Clear();
	}
}