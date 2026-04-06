using System.Drawing;

using CounterStrike2GSI.EventMessages;
using CounterStrike2GSI.Nodes;

using System.Reflection;
using System.Text.Json.Serialization;

namespace CS2Achievements;

[Flags]
public enum Category
{
	TeamTactics,
	CombatSkills,
	WeaponSpecialist,
	GlobalExpertise,
	ArmsRaceDemolition,
	CutContent,
	NumCategories
}

[Flags]
public enum Event
{
	KilledPlayer,
	RoundWon,
	MatchWon
}

public struct Achievement
{
	/// <summary>
	/// Name of achievement
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// Description of achievement
	/// </summary>
	public string Description { get; set; }
	/// <summary>
	/// Category of achievement
	/// </summary>
	public Category Category { get; set; }
	/// <summary>
	/// Will increase progress if this event is fired
	/// </summary>
	public Event? OnEvent { get; set; }
	/// <summary>
	/// Complete? Derived on load, not persisted.
	/// </summary>
	[JsonIgnore]
	public bool Complete { get; set; }
	/// <summary>
	/// Current progress towards completion
	/// </summary>
	public int Progress { get; set; }
	/// <summary>
	/// Amount of progress needed for completion. Derived on load, not persisted.
	/// </summary>
	[JsonIgnore]
	public int MaxProgress { get; set; }
	/// <summary>
	/// Name of the achievement that must be completed before this one shows progress popups
	/// </summary>
	public string? Prerequisite { get; set; }
	/// <summary>
	/// Optional filters: event data must pass ALL predicates for progress to increment.
	/// Assign a single filter or a collection: Filters = [WithWeapon(x), OnMap(y)]
	/// </summary>
	[JsonIgnore]
	public Func<object?, bool>[]? Filters { get; set; }
	/// <summary>
	/// Required items to collect for set-based achievements (e.g. weapon names).
	/// MaxProgress and Progress are derived automatically. Defined in code only.
	/// </summary>
	[JsonIgnore]
	public HashSet<string>? Items { get; set; }
	/// <summary>
	/// Persisted set of items already collected. Populated automatically by OnEvent.
	/// </summary>
	public HashSet<string>? CollectedItems { get; set; }
	/// <summary>
	/// All named achievements must be complete for this one to unlock. No progress popups.
	/// </summary>
	[JsonIgnore]
	public HashSet<string>? RequiredAchievements { get; set; }
}

public static class Achievements
{
	static readonly HashSet<string> AllWeapons = [
		// Pistol
		"weapon_glock", // Glock-18
		"weapon_hkp2000", // P2000
		"weapon_usp_silencer", // USP-S
		"weapon_elite", // Dual Berettas
		"weapon_p250", // P250
		"weapon_tec9", // Tec-9
		"weapon_cz75a", // CZ75-Auto
		"weapon_fiveseven", // Five-SeveN
		"weapon_deagle", // Desert Eagle
		"weapon_revolver", // R8 Revolver
		// SMG
		"weapon_mac10", // MAC-10
		"weapon_mp9", // MP9
		"weapon_mp7", // MP7
		"weapon_mp5sd", // MP5-SD
		"weapon_ump45", // UMP-45
		"weapon_p90", // P90
		"weapon_bizon", // PP-Bizon
		// Heavy
		"weapon_nova", // Nova
		"weapon_xm1014", // XM1014
		"weapon_sawedoff", // Sawed-Off
		"weapon_mag7", // MAG-7
		"weapon_m249", // M249
		"weapon_negev", // Negev
		// Rifle
		"weapon_galilar", // Galil AR
		"weapon_famas", // FAMAS
		"weapon_ak47", // AK-47
		"weapon_m4a1", // M4A4
		"weapon_m4a1_silencer", // M4A1-S
		"weapon_ssg08", // SSG 08
		"weapon_sg556", // SG 553
		"weapon_aug", // AUG
		"weapon_awp", // AWP
		"weapon_g3sg1", // G3SG1
		"weapon_scar20", // SCAR-20
		// Equipment
		// "weapon_knife",
		"weapon_taser" // Zeus x27
	];

	public static readonly List<Achievement> AchievementList = [
		new () { Name = "Awardist", Description = "Earn 100 achievements.", MaxProgress = 100, Category = Category.TeamTactics },
		// new () { Name = "Someone Set Up Us The Bomb", Description = "Win a round by planting a bomb.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Rite of First Defusal", Description = "Win a round by defusing a bomb.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Second to None", Description = "Successfully defuse a bomb with less than one second remaining.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Counter-Counter-Terrorist", Description = "Kill a Counter-Terrorist while he is defusing the bomb.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Short Fuse", Description = "Plant a bomb within 25 seconds [excluding Demolition mode].", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Participation Award", Description = "Kill an enemy within three seconds after they recover a dropped bomb.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Clusterstruck", Description = "Kill five enemies with a bomb you have planted.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Wild Gooseman Chase", Description = "As the last living Terrorist, distract a defuser long enough for the bomb to explode.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Blast Will and Testamant", Description = "Win a round by picking up the bomb from a fallen comrade and successfully planting it.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Defusus Interruptus", Description = "Stop defusing the bomb long enough to kill an enemy, then successfully finish defusing it.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Boomala Boomala", Description = "Plant 100 bombs.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "The Hurt Blocker", Description = "Defuse 100 bombs successfully.", MaxProgress = 100, Category = Category.TeamTactics },
		// new () { Name = "Good Shepherd", Description = "Rescue all hostages in a single round.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Dead Shepherd", Description = "Kill an enemy who is carrying a hostage.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Freed With Speed", Description = "Rescue all hostages within 90 seconds.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Cowboy Diplomacy", Description = "Rescue 100 hostages.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "SAR Czar", Description = "Rescue 500 hostages.", MaxProgress = 1, Category = Category.TeamTactics },
		new () { Name = "Newb World Order", Description = "Win ten rounds.", MaxProgress = 10, Category = Category.TeamTactics, OnEvent = Event.RoundWon, Filters = [WithGameMode(GameMode.Competitive)] },
		new () { Name = "Pro-moted", Description = "Win 200 rounds.", MaxProgress = 200, Category = Category.TeamTactics, OnEvent = Event.RoundWon, Filters = [WithGameMode(GameMode.Competitive)], Prerequisite = "Newb World Order" },
		new () { Name = "Leet-er of Men", Description = "Win 5,000 rounds.", MaxProgress = 5_000, Category = Category.TeamTactics, OnEvent = Event.RoundWon, Filters = [WithGameMode(GameMode.Competitive)], Prerequisite = "Pro-moted" },
		// new () { Name = "Blitzkrieg", Description = "Win a round against five enemies in less than thirty seconds.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Mercy Rule", Description = "Kill the entire opposing team without any members of your team dying.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Clean Sweep", Description = "Kill the entire opposing team without any members of your team taking any damage.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Killanthropist", Description = "Donate 100 weapons to your teammates.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Cold War", Description = "Win a round in which no enemy players die.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "War Bonds", Description = "Earn $50,000 total cash.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Spoils of War", Description = "Earn $2,500,000 total cash.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Blood Money", Description = "Earn $50,000,000 total cash.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "The Cleaner", Description = "In Classic mode, kill five enemies in a single round.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "War of Attrition", Description = "Be the last player alive in a round with five players on your team.", MaxProgress = 1, Category = Category.TeamTactics },
		new () { Name = "Piece Initiative", Description = "Win 5 Pistol Rounds in Competitive Mode.", MaxProgress = 5, Category = Category.TeamTactics, OnEvent = Event.RoundWon, Filters = [WithGameMode(GameMode.Competitive), OnPistolRound()] },
		new () { Name = "Give Piece a Chance", Description = "Win 25 Pistol Rounds in Competitive Mode.", MaxProgress = 25, Category = Category.TeamTactics, OnEvent = Event.RoundWon, Filters = [WithGameMode(GameMode.Competitive), OnPistolRound()], Prerequisite = "Piece Initiative" },
		new () { Name = "Piece Treaty", Description = "Win 250 Pistol Rounds in Competitive Mode.", MaxProgress = 250, Category = Category.TeamTactics, OnEvent = Event.RoundWon, Filters = [WithGameMode(GameMode.Competitive), OnPistolRound()], Prerequisite = "Give Piece a Chance" },
		// new () { Name = "Black Bag Operation", Description = "Win a round without making any footstep noise, killing at least one enemy.", MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "The Frugal Beret", Description = "Win ten rounds without dying or spending any cash in Classic mode.", MaxProgress = 1, Category = Category.TeamTactics },
		new () { Name = "Body Bagger", Description = "Kill 25 enemies.", MaxProgress = 25, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer },
		new () { Name = "Corpseman", Description = "Kill 500 enemies.", MaxProgress = 500, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Prerequisite = "Body Bagger" },
		new () { Name = "God of War", Description = "Kill 10,000 enemies.", MaxProgress = 10_000, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Prerequisite = "Corpseman" },
		// new () { Name = "Shot With Their Pants Down", Description = "Kill an enemy while they are reloading.", MaxProgress = 1, Category = Category.CombatSkills },
		new () { Name = "Ballistic", Description = "In Classic Mode, Kill four enemy players whithin fifteen seconds.", MaxProgress = 1, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithNumKillsInSeconds(4, 15), WithGameMode(GameMode.Competitive)] },
		new () { Name = "Variety Hour", Description = "Get kills with five different guns in a single round.", MaxProgress = 1, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithUniqueWeaponsUsed(5)] },
		new () { Name = "Battle Sight Zero", Description = "Kill 250 enemies with headshots.", MaxProgress = 250, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithHeadshot()] },
		// new () { Name = "Shrapnelproof", Description = "Take 80 points of damage from enemy grenades and still survive the round.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Blind Ambition", Description = "Kill 25 enemies blinded by flashbangs.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Blind Fury", Description = "Kill an enemy while you are blinded from a flashbang .", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Spray and Pray", Description = "Kill two enemies while you are blinded from a flashbang .", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Friendly Firearms", Description = "Kill 100 enemies with enemy weapons.", MaxProgress = 1, Category = Category.CombatSkills },
		new () { Name = "Expert Marksman", Description = "Get a kill with every weapon", Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Items = AllWeapons},
		// new () { Name = "Make the Cut", Description = "Win a knife fight.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "The Bleeding Edge", Description = "Win 100 knife fights.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Defuse This!", Description = "Kill the defuser with an HE grenade .", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Eye to Eye", Description = "Kill a zoomed-in enemy sniper with a sniper rifle of your own.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Sknifed", Description = "Kill a zoomed-in enemy sniper with a knife .", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Hip Shot", Description = "Kill an enemy with an un-zoomed sniper rifle.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Snipe Hunter", Description = "Kill 100 zoomed-in enemy snipers.", MaxProgress = 1, Category = Category.CombatSkills },
		new () { Name = "Dead Man Stalking", Description = "Kill an enemy while at one health", MaxProgress = 1, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithPlayerHealth(1)] },
		new () { Name = "Street Fighter", Description = "Kill an enemy with a knife during the Pistol Round in Competitive mode.", MaxProgress = 1, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithAnyOfWeapons("weapon_knife", "weapon_knife_t"), WithGameMode(GameMode.Competitive), OnPistolRound()] },
		// new () { Name = "Akimbo King", Description = "Use Dual Berettas to kill an enemy player that is also wielding Dual Berettas .", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Three the Hard Way", Description = "Kill three enemies with a single HE grenade .", MaxProgress = 1, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_hegrenade"), WithNumKillsInSeconds(3, 0)] },
		// new () { Name = "Death From Above", Description = "Kill an enemy while you are airborne.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Bunny Hunt", Description = "Kill an airborne enemy.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Aerial Necrobatics", Description = "Kill an airborne enemy while you are also airborne.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Lost and F0wnd", Description = "Kill an enemy with a gun they dropped the current round.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Ammo Conservation", Description = "Kill two enemy players with a single bullet.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Points in Your Favor", Description = "Inflict 2,500 total points of damage to enemies.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "You've Made Your Points", Description = "Inflict 50,000 total points of damage to enemies.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "A Million Points of Blight", Description = "Inflict 1,000,000 total points of damage to enemies.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Magic Bullet", Description = "Kill an enemy with the last bullet in your magazine (excluding sniper rifles and Zeus x27)", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Kill One, Get One Spree", Description = "Kill an enemy player who has just killed four of your teammates within 15 seconds.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Primer", Description = "Do at least 95% damage to an enemy who is then killed by another player.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Finishing Schooled", Description = "Kill an enemy who has been reduced to less than 5% health by other players.", MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Target-Hardened", Description = "Survive damage from five different enemies within a round.", MaxProgress = 1, Category = Category.CombatSkills },
		new () { Name = "The Unstoppable Force", Description = "Kill four enemies within a single round.", MaxProgress = 1, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithRoundKills(4)] },
		// new () { Name = "The Immovable Object", Description = "Kill an enemy who has killed four of your teammates within the current round.", MaxProgress = 1, Category = Category.CombatSkills },
		new () { Name = "Head Shred Redemption", Description = "Kill five enemy players with headshots in a single round.", MaxProgress = 1, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer, Filters = [WithRoundHeadshots(5)] },
		// new () { Name = "The Road to Hell", Description = "Blind an enemy player who then kills a teammate.", MaxProgress = 1, Category = Category.CombatSkills },
		new () { Name = "Desert Eagle Expert", Description = "Kill 200 enemies with the Desert Eagle or R8 Revolver.", MaxProgress = 200, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithAnyOfWeapons("weapon_deagle", "weapon_revolver")] },
		new () { Name = "P2000/USP Tactical Expert", Description = "Kill 100 enemies with the P2000 or USP.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithAnyOfWeapons("weapon_hkp2000", "weapon_usp_silencer")] },
		new () { Name = "Glock-18 Expert", Description = "Kill 100 enemies with the Glock-18.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_glock")] },
		new () { Name = "P250 Expert", Description = "Kill 25 enemies with the P250.", MaxProgress = 25, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_p250")] },
		new () { Name = "Dual Berettas Expert", Description = "Kill 25 enemies with the Dual Berettas.", MaxProgress = 25, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_elite")] },
		new () { Name = "Five-SeveN Expert", Description = "Kill 25 enemies with the Five-SeveN.", MaxProgress = 25, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_fiveseven")] },
		new () { Name = "AWP Expert", Description = "Kill 500 enemies with the AWP.", MaxProgress = 500, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_awp")] },
		new () { Name = "AK-47 Expert", Description = "Kill 1,000 enemies with the AK-47.", MaxProgress = 1_000, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_ak47")] },
		new () { Name = "M4 AR Expert", Description = "Kill 1,000 enemies with the M4 Assault Rifle.", MaxProgress = 1_000, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithAnyOfWeapons("weapon_m4a1", "weapon_m4a1_silencer")] },
		new () { Name = "AUG Expert", Description = "Kill 250 enemies with the AUG.", MaxProgress = 250, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_aug")] },
		new () { Name = "SG553 Expert", Description = "Kill 100 enemies with the SG553.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_sg556")] },
		new () { Name = "SCAR-20 Expert", Description = "Kill 100 enemies with the SCAR-20.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_scar20")] },
		new () { Name = "Galil AR Expert", Description = "Kill 250 enemies with the Galil AR.", MaxProgress = 250, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_galilar")] },
		new () { Name = "FAMAS Expert", Description = "Kill 100 enemies with the FAMAS.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_famas")] },
		new () { Name = "SSG 08 Expert", Description = "Kill 100 enemies with the SSG 08.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_ssg08")] },
		new () { Name = "G3SG1 Expert", Description = "Kill 100 enemies with the G3SG1.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_g3sg1")] },
		new () { Name = "P90 Expert", Description = "Kill 500 enemies with the P90.", MaxProgress = 500, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_p90")] },
		new () { Name = "MP7 Expert", Description = "Kill 250 enemies with the MP7.", MaxProgress = 250, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_mp7")] },
		new () { Name = "MP9 Expert", Description = "Kill 100 enemies with the MP9.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_mp9")] },
		new () { Name = "MAC-10 Expert", Description = "Kill 100 enemies with the MAC-10.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_mac10")] },
		new () { Name = "UMP-45 Expert", Description = "Kill 250 enemies with the UMP-45.", MaxProgress = 250, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_ump45")] },
		new () { Name = "Nova Expert", Description = "Kill 100 enemies with the Nova.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_nova")] },
		new () { Name = "XM1014 Expert", Description = "Kill 200 enemies with the XM1014.", MaxProgress = 200, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_xm1014")] },
		new () { Name = "MAG-7 Expert", Description = "Kill 50 enemies with the MAG-7.", MaxProgress = 50, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_mag7")] },
		new () { Name = "M249 Expert", Description = "Kill 100 enemies with the M249.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_m249")] },
		new () { Name = "Negev Expert", Description = "Kill 100 enemies with the Negev.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_negev")] },
		new () { Name = "Tec-9 Expert", Description = "Kill 100 enemies with the Tec-9.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_tec9")] },
		new () { Name = "Sawed-Off Expert", Description = "Kill 50 enemies with the Sawed-Off.", MaxProgress = 50, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_sawedoff")] },
		new () { Name = "PP-Bizon Expert", Description = "Kill 250 enemies with the PP-Bizon.", MaxProgress = 250, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_bizon")] },
		new () { Name = "Knife Expert", Description = "Kill 100 enemies with the Knife.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithAnyOfWeapons("weapon_knife", "weapon_knife_t")] },
		new () { Name = "HE Grenade Expert", Description = "Kill 100 enemies with the HE grenade.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_hegrenade")] },
		new () { Name = "Flame Expert", Description = "Kill 100 enemies with the Molotov or Incendiary grenade.", MaxProgress = 100, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithAnyOfWeapons("weapon_molotov", "weapon_incgrenade")] },
		// new () { Name = "Premature Burial", Description = "Kill an enemy with a grenade after dying.", MaxProgress = 1, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_hegrenade"), WithPlayerHealth(0)] },
		new () { Name = "Pistol Master", Description = "Unlock all pistol kill achievements.", Category = Category.WeaponSpecialist, RequiredAchievements = ["Desert Eagle Expert", "P2000/USP Tactical Expert", "Glock-18 Expert", "P250 Expert", "Dual Berettas Expert", "Five-SeveN Expert", "Tec-9 Expert"] },
		new () { Name = "Rifle Master", Description = "Unlock all rifle kill achievements.", Category = Category.WeaponSpecialist, RequiredAchievements = ["AK-47 Expert", "M4 AR Expert", "AUG Expert", "SG553 Expert", "SCAR-20 Expert", "Galil AR Expert", "FAMAS Expert", "SSG 08 Expert", "G3SG1 Expert", "AWP Expert"] },
		new () { Name = "Sub-Machine Gun Master", Description = "Unlock all sub-machine gun kill achievements.", Category = Category.WeaponSpecialist, RequiredAchievements = ["P90 Expert", "MP7 Expert", "MP9 Expert", "MAC-10 Expert", "UMP-45 Expert", "PP-Bizon Expert"] },
		new () { Name = "Shotgun Master", Description = "Unlock all shotgun kill achievements.", Category = Category.WeaponSpecialist, RequiredAchievements = ["Nova Expert", "XM1014 Expert", "MAG-7 Expert", "Sawed-Off Expert"] },
		new () { Name = "Master At Arms", Description = "Unlock every weapon kill achievement.", Category = Category.WeaponSpecialist, RequiredAchievements = ["Pistol Master", "Rifle Master", "Sub-Machine Gun Master", "Shotgun Master", "Knife Expert", "HE Grenade Expert", "Flame Expert", "M249 Expert", "Negev Expert", "Zeus x27 Expert"] },
		new () { Name = "Zeus x27 Expert", Description = "Kill 10 enemies with the Zeus x27.", MaxProgress = 10, Category = Category.WeaponSpecialist, OnEvent = Event.KilledPlayer, Filters = [WithWeapon("weapon_taser")] },
		new () { Name = "Italy Map Veteran", Description = "Win 100 rounds on Italy.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("cs_italy"), WithGameMode(GameMode.Competitive)] },
		new () { Name = "Office Map Veteran", Description = "Win 100 rounds on Office.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("cs_office"), WithGameMode(GameMode.Competitive)] },
		// new () { Name = "Aztec Map Veteran", Description = "Win 100 rounds on Aztec.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("de_aztec"), WithGameMode(GameMode.Competitive)] },
		// new () { Name = "Dust Map Veteran", Description = "Win 100 rounds on Dust.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("de_dust"), WithGameMode(GameMode.Competitive)] },
		new () { Name = "Dust2 Map Veteran", Description = "Win 100 rounds on Dust2.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("de_dust2"), WithGameMode(GameMode.Competitive)] },
		new () { Name = "Inferno Map Veteran", Description = "Win 100 rounds on Inferno.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("de_inferno"), WithGameMode(GameMode.Competitive)] },
		new () { Name = "Nuke Map Veteran", Description = "Win 100 rounds on Nuke.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("de_nuke"), WithGameMode(GameMode.Competitive)] },
		new () { Name = "Train Map Veteran", Description = "Win 100 rounds on Train.", MaxProgress = 100, Category = Category.GlobalExpertise, OnEvent = Event.RoundWon, Filters = [WithMap("de_train"), WithGameMode(GameMode.Competitive)] },
		// new () { Name = "Shoots Vet", Description = "Win five matches in Arms Race mode on Shoots.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "Baggage Claimer", Description = "Win five matches in Arms Race mode on Baggage.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "Vacation", Description = "Win five matches on Lake.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "My House", Description = "Win five matches on Safehouse.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "Run of the Mill", Description = "Win five matches on Sugarcane.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "Marcsman", Description = "Win five matches on St. Marc.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "Bank On It", Description = "Win five matches on Bank.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "Shorttrain Map Veteran", Description = "Win five matches on Shorttrain.", MaxProgress = 5, Category = Category.GlobalExpertise },
		// new () { Name = "A World of Pane", Description = "Shoot out 14 windows in a single round on Office.", MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Tourist", Description = "Play a round on every Arms Race and Demolition map.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Denied!", Description = "Kill a player who is on gold knife level in Arms Race mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Marksman", Description = "Win a match on every Arms Race and Demolition map.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Rampage!", Description = "Win an Arms Race match without dying.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "FIRST!", Description = "Be the first player to get a kill in an Arms Race or Demolition match.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "One Shot One Kill", Description = "Kill three consecutive players using the first bullet of your gun in Arms Race mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Conservationist", Description = "Win an Arms Race match without reloading any of your weapons.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Shorter Fuse", Description = "Plant five bombs in Demolition Mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Quick Cut", Description = "Defuse five bombs in Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "First Things First", Description = "Personally kill the entire Terrorist team before the bomb is planted in Demolition Mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Target Secured", Description = "Personally kill the entire Counter-Terrorist team before the bomb is planted in Demolition Mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Born Ready", Description = "Kill an enemy with the first bullet after your respawn protection ends in Arms Race mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Base Scamper", Description = "Kill an enemy just as their respawn protection ends in Arms Race mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Knife on Knife", Description = "Kill an enemy who is on gold knife level with your own knife in Arms Race mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Level Playing Field", Description = "Kill an enemy who is on gold knife level with a sub-machine gun in Arms Race Mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Still Alive", Description = "Survive more than 30 seconds with less than ten health in Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Practice Practice Practice", Description = "Play 100 matches of Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Gun Collector", Description = "Play 500 matches of Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "King of the Kill", Description = "Play 5,000 matches of Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Gungamer", Description = "Win one match in Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Keep on Gunning", Description = "Win 25 matches in Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Kill of the Century", Description = "Win 100 matches in Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "The Professional", Description = "Win 500 matches in Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Cold Pizza Eater", Description = "Win 1,000 matches in Arms Race or Demolition mode.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Repeat Offender", Description = "Dominate an enemy.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Decimator", Description = "Dominate ten enemies.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Insurgent", Description = "Kill an enemy who is dominating you.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Can't Keep a Good Man Down", Description = "Kill 20 enemies who are dominating you.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Overkill", Description = "Kill an enemy whom you are already dominating.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Command and Control", Description = "Kill 100 enemies whom you are already dominating.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Ten Angry Men", Description = "Kill 10 enemies you are already dominating during a single match.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Excessive Brutality", Description = "Kill an enemy whom you are dominating four additional times.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Hat Trick", Description = "Dominate three enemies simultaneously.", MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Avenging Angel", Description = "Kill an enemy who has killed a player on your friends list in the same round.", MaxProgress = 1, Category = Category.ArmsRaceDemolition }
	];

	static readonly string SaveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CS2Achievements");

	static string? _loadedSteamId;
	static readonly object _lock = new();

	static string GetSaveFilePath(string steamId) => Path.Combine(SaveDir, $"achievements_{steamId}.json");

	public static string? LoadedSteamId => _loadedSteamId;

	public static void SaveAchievements() {
		lock (_lock) {
			if (_loadedSteamId == null) return;
			Directory.CreateDirectory(SaveDir);
			string json = System.Text.Json.JsonSerializer.Serialize(AchievementList);
			File.WriteAllText(GetSaveFilePath(_loadedSteamId), json);
		}
	}

	public static void LoadForSteamId(string steamId) {
		lock (_lock) {
			if (_loadedSteamId == steamId) return;

			if (_loadedSteamId != null)
				SaveAchievements();

			ResetInMemory();

			_loadedSteamId = steamId;
			string path = GetSaveFilePath(steamId);
			if (!File.Exists(path)) {
				Logger.Information("No save data found for SteamID {SteamID}, starting fresh.", steamId);
				CheckMetaAchievements(silent: true);
				return;
			}

			string json = File.ReadAllText(path);
			List<Achievement>? loadedAchievements = System.Text.Json.JsonSerializer.Deserialize<List<Achievement>>(json);

			if (loadedAchievements != null) {
				foreach (var loaded in loadedAchievements) {
					int idx = AchievementList.FindIndex(a => string.Equals(a.Name, loaded.Name, StringComparison.OrdinalIgnoreCase));
					if (idx >= 0) {
						var a = AchievementList[idx];
						a.CollectedItems = loaded.CollectedItems;
						if (a.Items != null) {
							a.MaxProgress = a.Items.Count;
							a.Progress = a.CollectedItems?.Count ?? 0;
						}
						else {
							a.Progress = loaded.Progress;
						}
						a.Complete = a.MaxProgress > 0 && a.Progress >= a.MaxProgress;
						AchievementList[idx] = a;
					}
				}
			}
			else
				Logger.Warning("Failed to deserialize achievements for SteamID {SteamID}.", steamId);

			CheckMetaAchievements(silent: true);
			Logger.Information("Loaded achievements for SteamID {SteamID}.", steamId);
		}
	}

	static void ResetInMemory() {
		for (int i = 0; i < AchievementList.Count; i++) {
			var a = AchievementList[i];
			a.Progress = 0;
			a.Complete = false;
			a.CollectedItems = null;
			if (a.Items != null)
				a.MaxProgress = a.Items.Count;
			AchievementList[i] = a;
		}
	}

	public static void ResetAllProgress() {
		lock (_lock) {
			ResetInMemory();
			SaveAchievements();
			Logger.Information("All achievement progress has been reset for SteamID {SteamID}.", _loadedSteamId ?? "unknown");
		}
	}

	public static void IncrementAchievementProgress(string achievementName, int amount = 1) {
		lock (_lock) {
			if (_loadedSteamId == null) return;

			int idx = AchievementList.FindIndex(a => string.Equals(a.Name, achievementName, StringComparison.OrdinalIgnoreCase));

			if (idx == -1) {
				Logger.Warning($"Achievement '{achievementName}' not found.");
				return;
			}

			var achievement = AchievementList[idx];
			if (achievement.Complete) return;
			if (achievement.Items != null) return;

			int oldProgress = achievement.Progress;
			achievement.Progress += amount;
			Logger.Debug($"Progress for achievement '{achievement.Name}' increased by {amount}. Current progress: {achievement.Progress}/{achievement.MaxProgress}");

			bool prerequisiteMet = achievement.Prerequisite == null ||
				AchievementList.Find(a => string.Equals(a.Name, achievement.Prerequisite, StringComparison.OrdinalIgnoreCase)) is { Complete: true };

			if (achievement.Progress >= achievement.MaxProgress) {
				achievement.Progress = achievement.MaxProgress;
				achievement.Complete = true;
				Logger.Information($"Achievement Unlocked: {achievement.Name} - {achievement.Description}");
				if (AppConfig.Instance.ShowUnlockPopups)
					PopupStack.Show(achievement.Name, "Achievement Unlocked!", achievement.Description, GetAchievementIcon(achievement.Name), achievement.MaxProgress, achievement.MaxProgress);
			}
			else if (prerequisiteMet && ShouldShowProgressPopup(oldProgress, achievement.Progress, achievement.MaxProgress))
				PopupStack.Show(achievement.Name, achievement.Name, achievement.Description, GetAchievementIcon(achievement.Name), achievement.Progress, achievement.MaxProgress);

			AchievementList[idx] = achievement;
			SaveAchievements();
			CheckMetaAchievements();
		}
	}

	static bool ShouldShowProgressPopup(int oldProgress, int newProgress, int maxProgress) {
		if (maxProgress <= 0) return false;
		return AppConfig.Instance.ProgressPopups switch {
			PopupMode.Always => true,
			PopupMode.OnRoundEnd => false, // handled externally at round end
			PopupMode.AtMilestones => HitMilestone(oldProgress, newProgress, maxProgress),
			_ => true
		};
	}

	static bool HitMilestone(int oldProgress, int newProgress, int maxProgress) {
		if (maxProgress <= 0) return false;
		int step = Math.Max(1, maxProgress / 10);
		return oldProgress / step != newProgress / step;
	}

	/// <summary>
	/// Called at round end to flush any pending progress popups in OnRoundEnd mode.
	/// </summary>
	public static void FlushRoundEndPopups() {
		if (AppConfig.Instance.ProgressPopups != PopupMode.OnRoundEnd) return;
		lock (_lock) {
			foreach (var a in AchievementList) {
				if (a.Complete || a.Progress <= 0 || a.MaxProgress <= 0) continue;
				bool prerequisiteMet = a.Prerequisite == null ||
					AchievementList.Find(x => string.Equals(x.Name, a.Prerequisite, StringComparison.OrdinalIgnoreCase)) is { Complete: true };
				if (prerequisiteMet)
					PopupStack.Show(a.Name, a.Name, a.Description, GetAchievementIcon(a.Name), a.Progress, a.MaxProgress);
			}
		}
	}

	static Dictionary<string, Image> CachedIcons = [];
	private static Image GetAchievementIcon(string name) {
		if (CachedIcons.TryGetValue(name, out Image? cachedIcon))
			return cachedIcon;

		string? normalizedName = NormalizeNameForIcon(name);
		if (normalizedName == null)
			return SystemIcons.Question.ToBitmap();


		string resourcePath = $"CS2Achievements.Images.{normalizedName}.png";
		using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
		if (stream != null) {
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			var skBitmap = SkiaSharp.SKBitmap.Decode(ms.ToArray());
			if (skBitmap != null) {
				using var skImage = SkiaSharp.SKImage.FromBitmap(skBitmap);
				using var encoded = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
				using var pngStream = new MemoryStream(encoded.ToArray());
				Image icon = Image.FromStream(pngStream);
				CachedIcons[name] = icon;
				return icon;
			}
		}

		return SystemIcons.Question.ToBitmap();
	}

	internal static string? NormalizeNameForIcon(string name) => new([.. name.ToLower().Where(c => c != '-' && c != '/').Select(c => char.IsWhiteSpace(c) ? '_' : c)]);

	public static void AddUniqueItem(string achievementName, string item) {
		lock (_lock) {
			if (_loadedSteamId == null) return;

			int idx = AchievementList.FindIndex(a => string.Equals(a.Name, achievementName, StringComparison.OrdinalIgnoreCase));
			if (idx == -1) {
				Logger.Warning($"Achievement '{achievementName}' not found.");
				return;
			}

			var achievement = AchievementList[idx];
			if (achievement.Complete) return;

			if (achievement.Items != null && !achievement.Items.Contains(item)) {
				Logger.Warning($"Item '{item}' is not in the required set for achievement '{achievement.Name}'.");
				return;
			}

			achievement.CollectedItems ??= [];
			if (!achievement.CollectedItems.Add(item)) return;

			if (achievement.Items != null)
				achievement.MaxProgress = achievement.Items.Count;
			int oldProgress = achievement.Progress;
			achievement.Progress = achievement.CollectedItems.Count;
			Logger.Debug($"Item '{item}' added to '{achievement.Name}'. Progress: {achievement.Progress}/{achievement.MaxProgress}");

			bool prerequisiteMet = achievement.Prerequisite == null ||
				AchievementList.Find(a => string.Equals(a.Name, achievement.Prerequisite, StringComparison.OrdinalIgnoreCase)) is { Complete: true };

			if (achievement.Progress >= achievement.MaxProgress) {
				achievement.Progress = achievement.MaxProgress;
				achievement.Complete = true;
				Logger.Information($"Achievement Unlocked: {achievement.Name} - {achievement.Description}");
				if (AppConfig.Instance.ShowUnlockPopups)
					PopupStack.Show(achievement.Name, "Achievement Unlocked!", achievement.Description, GetAchievementIcon(achievement.Name), achievement.MaxProgress, achievement.MaxProgress);
			}
			else if (prerequisiteMet && ShouldShowProgressPopup(oldProgress, achievement.Progress, achievement.MaxProgress))
				PopupStack.Show(achievement.Name, achievement.Name, achievement.Description, GetAchievementIcon(achievement.Name), achievement.Progress, achievement.MaxProgress);

			AchievementList[idx] = achievement;
			SaveAchievements();
			CheckMetaAchievements();
		}
	}

	static void CheckMetaAchievements(bool silent = false) {
		foreach (var name in AchievementList.Where(a => a.RequiredAchievements != null || a.Name == "Awardist").Select(a => a.Name).ToList()) {
			int idx = AchievementList.FindIndex(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
			var achievement = AchievementList[idx];
			if (achievement.Complete) continue;

			if (achievement.Name == "Awardist") {
				achievement.MaxProgress = AchievementList.Count(a => a.MaxProgress > 0);
				achievement.Progress = AchievementList.Count(a => a.Complete && a.MaxProgress > 0);
				if (achievement.Progress >= achievement.MaxProgress && achievement.MaxProgress > 0) {
					achievement.Complete = true;
					if (!silent) {
						Logger.Information($"Achievement Unlocked: {achievement.Name} - {achievement.Description}");
						PopupStack.Show(achievement.Name, "Achievement Unlocked!", achievement.Description, GetAchievementIcon(achievement.Name), achievement.MaxProgress, achievement.MaxProgress);
						SaveAchievements();
					}
				}
				AchievementList[idx] = achievement;
				continue;
			}

			achievement.MaxProgress = achievement.RequiredAchievements!.Count;
			achievement.Progress = achievement.RequiredAchievements.Count(req =>
				AchievementList.Find(a => string.Equals(a.Name, req, StringComparison.OrdinalIgnoreCase)) is { Complete: true });

			if (achievement.Progress >= achievement.MaxProgress && achievement.MaxProgress > 0) {
				achievement.Complete = true;
				if (!silent) {
					Logger.Information($"Achievement Unlocked: {achievement.Name} - {achievement.Description}");
					PopupStack.Show(achievement.Name, "Achievement Unlocked!", achievement.Description, GetAchievementIcon(achievement.Name), achievement.MaxProgress, achievement.MaxProgress);
					SaveAchievements();
				}
			}

			AchievementList[idx] = achievement;
		}
	}

	public static void OnEvent(Event eventName, object? data = null) {
		if (_loadedSteamId == null) return;
		foreach (Achievement achievement in AchievementList.ToList()) {
			if (achievement.OnEvent == eventName && (achievement.Filters == null || achievement.Filters.All(f => f(data))))
				IncrementAchievementProgress(achievement.Name);
		}
	}

	// Filters
	public static Func<object?, bool> WithWeapon(string weapon) => data => data is PlayerGotKill evnt && evnt.Weapon.Name.Equals(weapon, StringComparison.OrdinalIgnoreCase);
	public static Func<object?, bool> WithAnyOfWeapons(params string[] weapons) => data => data is PlayerGotKill evnt && weapons.Any(w => evnt.Weapon.Name.Equals(w, StringComparison.OrdinalIgnoreCase));
	public static Func<object?, bool> WithMap(string map) => data => CurrentMap.Equals(map, StringComparison.OrdinalIgnoreCase);
	public static Func<object?, bool> WithAnyOfMaps(params string[] maps) => data => maps.Any(m => CurrentMap.Equals(m, StringComparison.OrdinalIgnoreCase));
	public static Func<object?, bool> WithGameMode(GameMode mode) => data => CurrentGameMode == mode;
	public static Func<object?, bool> WithAnyOfGameModes(params GameMode[] modes) => data => modes.Contains(CurrentGameMode);
	public static Func<object?, bool> WithHeadshot() => data => data is PlayerGotKill evnt && evnt.IsHeadshot;
	public static Func<object?, bool> WithAce() => data => data is PlayerGotKill evnt && evnt.IsAce;
	public static Func<object?, bool> OnFirstRound() => data => IsFirstRound;
	public static Func<object?, bool> OnPistolRound() => data => IsPistolRound;
	public static Func<object?, bool> WithNumKillsInSeconds(int numKills, int seconds) => data => data is PlayerGotKill evnt && RecentKills.Count(k => (DateTime.Now - k.Timestamp).TotalSeconds <= seconds) >= numKills;
	public static Func<object?, bool> WithPlayerHealth(int health) => data => CurrentRoundData.Health == health;
	public static Func<object?, bool> WithRoundKills(int kills) => data => CurrentRoundData.Kills == kills;
	public static Func<object?, bool> WithRoundHeadshots(int headshots) => data => CurrentRoundData.HeadshotKills == headshots;
	public static Func<object?, bool> WithUniqueWeaponsUsed(int count) => data => CurrentRoundData.UniqueWeaponsUsed.Count >= count;
}