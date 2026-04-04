global using static CS2Achievements.Global;

using CounterStrike2GSI;
using CounterStrike2GSI.EventMessages;
using CounterStrike2GSI.Nodes;

using Serilog;

namespace CS2Achievements;

public static class Global {
	public static CounterStrike2GSI.GameState? CurrentGameState;
	public static GameMode CurrentGameMode = GameMode.Undefined;
	public static string CurrentMap = string.Empty;
	public static PlayerTeam? CurrentTeam = null;
	public static string? SteamID = null;
	public static Player? Self = null;

	public static ILogger Logger { get; } = new LoggerConfiguration()
		.MinimumLevel.Verbose()
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.CreateLogger();

}

public class Program
{
	static GameStateListener? _gsl;

	static void Main(string[] args) {
		_gsl = new GameStateListener(4000);

		if (!_gsl.GenerateGSIConfigFile("Achievements"))
			Logger.Error("Could not generate GSI configuration file.");

		Logger.Debug("Loading achievements...");

		Achievements.LoadAchievements();
		Achievements.SaveAchievements();

		Logger.Information("Loaded {Count} achievements.", Achievements.AchievementList.Count.ToString());

		try {
			SteamID = SteamIDReader.GetCurrentSteamID64String();
			Logger.Information("Current SteamID64: {SteamID}", SteamID);
		}
		catch (Exception ex) {
			Logger.Error("Failed to get SteamID: {Message}", ex.Message);
		}

		_gsl.NewGameState += OnNewGameState;
		_gsl.BombStateUpdated += OnBombStateUpdated;
		_gsl.PlayerGotKill += OnPlayerGotKill;
		// _gsl.KillFeed += OnKillFeed;
		_gsl.PlayerWeaponsPickedUp += OnPlayerWeaponsPickedUp;
		_gsl.PlayerWeaponsDropped += OnPlayerWeaponsDropped;
		_gsl.RoundStarted += OnRoundStarted;
		_gsl.RoundConcluded += OnRoundConcluded;
		_gsl.GamemodeChanged += OnGamemodeChanged;
		_gsl.MapUpdated += OnMapUpdated;

		if (!_gsl.Start()) {
			Logger.Fatal("GameStateListener could not start. Try running this program as Administrator. Exiting.");
			Console.ReadLine();
			Environment.Exit(0);
		}
		Logger.Information("Listening for game integration calls...");

		new Thread(() => {
			while (true) {
				try {
					string currentSteamID = SteamIDReader.GetCurrentSteamID64String();
					if (currentSteamID != SteamID) {
						SteamID = currentSteamID;
						Logger.Information("SteamID changed: {SteamID}", SteamID);
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

		Logger.Information("Press ESC to quit.");
		do {
			while (!Console.KeyAvailable)
				Thread.Sleep(1000);
		} while (Console.ReadKey(true).Key != ConsoleKey.Escape);
	}

	private static void OnNewGameState(CounterStrike2GSI.GameState gamestate) {
		if (SteamID == null) return;
		CurrentGameState = gamestate;

		if (gamestate.Player == null || gamestate.Player.SteamID != SteamID) return;
		Self = gamestate.Player;
		CurrentTeam = gamestate.Player.Team;
	}

	private static void OnBombStateUpdated(BombStateUpdated game_event) {
		Console.WriteLine($"The bomb is now {game_event.New}.");
	}

	private static void OnGamemodeChanged(GamemodeChanged game_event) {
		if (game_event.New == CurrentGameMode) return;

		CurrentGameMode = game_event.New;
		Logger.Debug($"Gamemode changed to {game_event.New}.");
	}

	private static void OnMapUpdated(MapUpdated game_event) {
		if (game_event.New.Name == CurrentMap) return;

		CurrentMap = game_event.New.Name;
		Logger.Debug($"Map updated to {game_event.New.Name}.");
	}

	private static void OnPlayerGotKill(PlayerGotKill game_event) {
		// Logger.Debug($"PlayerGotKill event: Player={game_event.Player.Name}, Weapon={game_event.Weapon.Name}, IsHeadshot={game_event.IsHeadshot}, IsAce={game_event.IsAce}");
		if (game_event.Player.SteamID == SteamID) {
			Achievements.OnEvent(Event.KilledPlayer, game_event);
			Achievements.AddUniqueItem("Expert Marksman", game_event.Weapon.Name);
		}
	}

	private static void OnKillFeed(KillFeed game_event) {
		// Console.WriteLine($"{game_event.Killer.Name} killed {game_event.Victim.Name} with {game_event.Weapon.Name}{(game_event.IsHeadshot ? " as a headshot." : ".")}");
	}

	private static void OnPlayerWeaponsPickedUp(PlayerWeaponsPickedUp game_event) {
		// Console.WriteLine($"The player {game_event.Player.Name} picked up the following weapons:");
		// foreach (var weapon in game_event.Weapons)
		// 	Console.WriteLine($"\t{weapon.Name}");
	}

	private static void OnPlayerWeaponsDropped(PlayerWeaponsDropped game_event) {
		// Console.WriteLine($"The player {game_event.Player.Name} dropped the following weapons:");
		// foreach (var weapon in game_event.Weapons)
		// 	Console.WriteLine($"\t{weapon.Name}");
	}

	private static void OnRoundStarted(RoundStarted game_event) {
		// if (game_event.IsFirstRound)
		// 	Console.WriteLine($"First round {game_event.Round} started.");
		// else if (game_event.IsLastRound)
		// 	Console.WriteLine($"Last round {game_event.Round} started.");
		// else
		// 	Console.WriteLine($"A new round {game_event.Round} started.");
	}

	private static void OnRoundConcluded(RoundConcluded game_event) {
		// Console.WriteLine($"Round {game_event.Round} concluded by {game_event.WinningTeam} for reason: {game_event.RoundConclusionReason}");

		if (game_event.WinningTeam == CurrentTeam)
			Achievements.OnEvent(Event.RoundWon, game_event);
	}
}

