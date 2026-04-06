global using static CS2Achievements.Global;

using CounterStrike2GSI;
using CounterStrike2GSI.EventMessages;
using CounterStrike2GSI.Nodes;

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
	public bool HasBomb;
	public int Health;
	public int Kills;
	public int HeadshotKills;
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
		_gsl.BombExploded += OnBombExploded;
		_gsl.PlayerHealthChanged += OnPlayerHealthChanged;
		_gsl.Gameover += OnGameover;

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

	private static void OnPlayerGotKill(PlayerGotKill game_event) {
		if (game_event.Player.SteamID == SteamID) {

			CurrentRoundData.Kills++;
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
			if (game_event.Weapons.Any(w => w.Name == "weapon_c4"))
				CurrentRoundData.HasBomb = false;
		}
	}

	private static void OnMatchStarted(MatchStarted game_event) => CurrentRoundData = new();

	private static async void OnRoundStarted(RoundStarted game_event) { // Fun fact, this is triggered as soon as a round ends, and NOT when the next freeze time starts, fun
		await Task.Delay(250);
		CurrentRoundData = new();
		IsFirstRound = game_event.IsFirstRound;
		IsLastRound = game_event.IsLastRound;
		IsPistolRound = game_event.IsFirstRound || game_event.Round == 13;
	}

	private static void OnRoundConcluded(RoundConcluded game_event) {
		if (game_event.WinningTeam == CurrentTeam) {
			Achievements.OnEvent(Event.RoundWon, game_event);
			if (game_event.IsLastRound)
				Achievements.OnEvent(Event.MatchWon, game_event);
		}
		Achievements.FlushRoundEndPopups();
	}

	private static void OnBombExploded(BombExploded game_event) {
		// Console.WriteLine($"BombExploded");
	}

	private static void OnPlayerHealthChanged(PlayerHealthChanged game_event) => CurrentRoundData.Health = game_event.New;

	// private static void OnPlayerKillsChanged(PlayerKillsChanged game_event) {
	// 	Console.WriteLine($"PlayerKillsChanged: {game_event.New}");
	// }

	// private static void OnPlayerRoundKillsChanged(PlayerRoundKillsChanged game_event) {
	// 	Console.WriteLine($"PlayerRoundKillsChanged: {game_event.New}");
	// }

	private static void OnGameover(Gameover game_event) => Achievements.OnEvent(Event.GameOver, game_event);
}

