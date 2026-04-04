using CounterStrike2GSI.EventMessages;

using System.Reflection;

namespace CS2Achievements;

[Flags]
enum Category
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
	KilledPlayer
}

struct Achievement
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
	/// Complete?
	/// </summary>
	public bool Complete { get; set; }
	/// <summary>
	/// Current progress towards completion
	/// </summary>
	public int Progress { get; set; }
	/// <summary>
	/// Amount of progress needed for completion
	/// </summary>
	public int MaxProgress { get; set; }
}

public static class Achievements
{
	static readonly List<Achievement> AchievementList = [
		// new () { Name = "Awardist", Description = "Earn 100 achievements.", Complete = false, Progress = 0, MaxProgress = 100, Category = Category.TeamTactics },
		// new () { Name = "Someone Set Up Us The Bomb", Description = "Win a round by planting a bomb.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Rite of First Defusal", Description = "Win a round by defusing a bomb.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Second to None", Description = "Successfully defuse a bomb with less than one second remaining.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Counter-Counter-Terrorist", Description = "Kill a Counter-Terrorist while he is defusing the bomb.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Short Fuse", Description = "Plant a bomb within 25 seconds [excluding Demolition mode].", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Participation Award", Description = "Kill an enemy within three seconds after they recover a dropped bomb.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Clusterstruck", Description = "Kill five enemies with a bomb you have planted.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Wild Gooseman Chase", Description = "As the last living Terrorist, distract a defuser long enough for the bomb to explode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Blast Will and Testamant", Description = "Win a round by picking up the bomb from a fallen comrade and successfully planting it.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Defusus Interruptus", Description = "Stop defusing the bomb long enough to kill an enemy, then successfully finish defusing it.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Boomala Boomala", Description = "Plant 100 bombs.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "The Hurt Blocker", Description = "Defuse 100 bombs successfully.", Complete = false, Progress = 0, MaxProgress = 100, Category = Category.TeamTactics },
		// new () { Name = "Good Shepherd", Description = "Rescue all hostages in a single round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Dead Shepherd", Description = "Kill an enemy who is carrying a hostage.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Freed With Speed", Description = "Rescue all hostages within 90 seconds.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Cowboy Diplomacy", Description = "Rescue 100 hostages.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "SAR Czar", Description = "Rescue 500 hostages.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Newb World Order", Description = "Win ten rounds.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Pro-moted", Description = "Win 200 rounds.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Leet-er of Men", Description = "Win 5,000 rounds.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Blitzkrieg", Description = "Win a round against five enemies in less than thirty seconds.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Mercy Rule", Description = "Kill the entire opposing team without any members of your team dying.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Clean Sweep", Description = "Kill the entire opposing team without any members of your team taking any damage.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Killanthropist", Description = "Donate 100 weapons to your teammates.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Cold War", Description = "Win a round in which no enemy players die.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "War Bonds", Description = "Earn $50,000 total cash.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Spoils of War", Description = "Earn $2,500,000 total cash.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Blood Money", Description = "Earn $50,000,000 total cash.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "The Cleaner", Description = "In Classic mode, kill five enemies in a single round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "War of Attrition", Description = "Be the last player alive in a round with five players on your team.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Piece Initiative", Description = "Win 5 Pistol Rounds in Competitive Mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Give Piece a Chance", Description = "Win 25 Pistol Rounds in Competitive Mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Piece Treaty", Description = "Win 250 Pistol Rounds in Competitive Mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "Black Bag Operation", Description = "Win a round without making any footstep noise, killing at least one enemy.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		// new () { Name = "The Frugal Beret", Description = "Win ten rounds without dying or spending any cash in Classic mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.TeamTactics },
		new () { Name = "Body Bagger", Description = "Kill 25 enemies.", Complete = false, Progress = 0, MaxProgress = 25, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer },
		new () { Name = "Corpseman", Description = "Kill 500 enemies.", Complete = false, Progress = 0, MaxProgress = 500, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer },
		new () { Name = "God of War", Description = "Kill 10,000 enemies.", Complete = false, Progress = 0, MaxProgress = 10_000, Category = Category.CombatSkills, OnEvent = Event.KilledPlayer },
		// new () { Name = "Shot With Their Pants Down", Description = "Kill an enemy while they are reloading.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Ballistic", Description = "In Classic Mode, Kill four enemy players whithin fifteen seconds.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Variety Hour", Description = "Get kills with five different guns in a single round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Battle Sight Zero", Description = "Kill 250 enemies with headshots.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Shrapnelproof", Description = "Take 80 points of damage from enemy grenades and still survive the round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Blind Ambition", Description = "Kill 25 enemies blinded by flashbangs.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Blind Fury", Description = "Kill an enemy while you are blinded from aflashbang .", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Spray and Pray", Description = "Kill two enemies while you are blinded from a flashbang .", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Friendly Firearms", Description = "Kill 100 enemies with enemy weapons.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Expert Marksman", Description = "Get a kill with every weapon", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Make the Cut", Description = "Win a knife fight.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "The Bleeding Edge", Description = "Win 100 knife fights.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Defuse This!", Description = "Kill the defuser with an HE grenade .", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Eye to Eye", Description = "Kill a zoomed-in enemy sniper with a sniper rifle of your own.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Sknifed", Description = "Kill a zoomed-in enemy sniper with a knife .", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Hip Shot", Description = "Kill an enemy with an un-zoomed sniper rifle.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Snipe Hunter", Description = "Kill 100 zoomed-in enemy snipers.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Dead Man Stalking", Description = "Kill an enemy while at one health", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Street Fighter", Description = "Kill an enemy with a knife during the Pistol Round in Competitive mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Akimbo King", Description = "Use Dual Berettas to kill an enemy player that is also wielding Dual Berettas .", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Three the Hard Way", Description = "Kill three enemies with a single HE grenade .", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Death From Above", Description = "Kill an enemy while you are airborne.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Bunny Hunt", Description = "Kill an airborne enemy.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Aerial Necrobatics", Description = "Kill an airborne enemy while you are also airborne.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Lost and F0wnd", Description = "Kill an enemy with a gun they dropped the current round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Ammo Conservation", Description = "Kill two enemy players with a single bullet.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Points in Your Favor", Description = "Inflict 2,500 total points of damage to enemies.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "You've Made Your Points", Description = "Inflict 50,000 total points of damage to enemies.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "A Million Points of Blight", Description = "Inflict 1,000,000 total points of damage to enemies.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Magic Bullet", Description = "Kill an enemy with the last bullet in your magazine (excluding sniper rifles and Zeus x27)", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Kill One, Get One Spree", Description = "Kill an enemy player who has just killed four of your teammates within 15 seconds.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Primer", Description = "Do at least 95% damage to an enemy who is then killed by another player.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Finishing Schooled", Description = "Kill an enemy who has been reduced to less than 5% health by other players.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Target-Hardened", Description = "Survive damage from five different enemies within a round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "The Unstoppable Force", Description = "Kill four enemies within a single round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "The Immovable Object", Description = "Kill an enemy who has killed four of your teammates within the current round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Head Shred Redemption", Description = "Kill five enemy players with headshots in a single round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "The Road to Hell", Description = "Blind an enemy player who then kills a teammate.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CombatSkills },
		// new () { Name = "Desert Eagle Expert", Description = "Kill 200 enemies with the Desert Eagle. Killing enemies with the R8 Revolver also count towards this achievement.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "P2000/USP Tactical Expert", Description = "Kill 100 enemies with the P2000 or USP.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Glock-18 Expert", Description = "Kill 100 enemies with the Glock-18.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "P250 Expert", Description = "Kill 25 enemies with the P250.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Dual Berettas Expert", Description = "Kill 25 enemies with the Dual Berettas.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Five-SeveN Expert", Description = "Kill 25 enemies with the Five-SeveN.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "AWP Expert", Description = "Kill 500 enemies with the AWP.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "AK-47 Expert", Description = "Kill 1,000 enemies with the AK-47.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "M4 AR Expert", Description = "Kill 1,000 enemies with the M4 Assault Rifle.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "AUG Expert", Description = "Kill 250 enemies with the AUG.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "SG553 Expert", Description = "Kill 100 enemies with the SG553.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "SCAR-20 Expert", Description = "Kill 100 enemies with the SCAR-20.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Galil AR Expert", Description = "Kill 250 enemies with the Galil AR.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "FAMAS Expert", Description = "Kill 100 enemies with the FAMAS.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "SSG 08 Expert", Description = "Kill 100 enemies with the SSG 08.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "G3SG1 Expert", Description = "Kill 100 enemies with the G3SG1.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "P90 Expert", Description = "Kill 500 enemies with the P90.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "MP7 Expert", Description = "Kill 250 enemies with the MP7.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "MP9 Expert", Description = "Kill 100 enemies with the MP9.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "MAC-10 Expert", Description = "Kill 100 enemies with the MAC-10.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "UMP-45 Expert", Description = "Kill 250 enemies with the UMP-45.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Nova Expert", Description = "Kill 100 enemies with the Nova.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "XM1014 Expert", Description = "Kill 200 enemies with the XM1014.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "MAG-7 Expert", Description = "Kill 50 enemies with the MAG-7.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "M249 Expert", Description = "Kill 100 enemies with the M249.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Negev Expert", Description = "Kill 100 enemies with the Negev.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Tec-9 Expert", Description = "Kill 100 enemies with the Tec-9.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Sawed-Off Expert", Description = "Kill 50 enemies with the Sawed-Off.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "PP-Bizon Expert", Description = "Kill 250 enemies with the PP-Bizon.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Knife Expert", Description = "Kill 100 enemies with the Knife.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "HE Grenade Expert", Description = "Kill 100 enemies with theHE grenade.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Flame Expert", Description = "Kill 100 enemies with the Molotov or Incendiary grenade.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Premature Burial", Description = "Kill an enemy with a grenade after dying.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Pistol Master", Description = "Unlock all pistol kill achievements.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Rifle Master", Description = "Unlock all rifle kill achievements.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Sub-Machine Gun Master", Description = "Unlock all sub-machine gun kill achievements.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Shotgun Master", Description = "Unlock all shotgun kill achievements.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Master At Arms", Description = "Unlock every weapon kill achievement.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Zeus x27 Expert", Description = "Kill 10 enemies with the Zeus x27.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.WeaponSpecialist },
		// new () { Name = "Italy Map Veteran", Description = "Win 100 rounds on Italy.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Office Map Veteran", Description = "Win 100 rounds on Office.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Aztec Map Veteran", Description = "Win 100 rounds on Aztec.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Dust Map Veteran", Description = "Win 100 rounds on Dust.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Dust2 Map Veteran", Description = "Win 100 rounds on Dust2.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Inferno Map Veteran", Description = "Win 100 rounds on Inferno.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Nuke Map Veteran", Description = "Win 100 rounds on Nuke.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Train Map Veteran", Description = "Win 100 rounds on Train.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Shoots Vet", Description = "Win five matches in Arms Race mode on Shoots.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Baggage Claimer", Description = "Win five matches in Arms Race mode on Baggage.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Vacation", Description = "Win five matches on Lake.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "My House", Description = "Win five matches on Safehouse.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Run of the Mill", Description = "Win five matches on Sugarcane.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Marcsman", Description = "Win five matches on St. Marc.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Bank On It", Description = "Win five matches on Bank.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Shorttrain Map Veteran", Description = "Win five matches on Shorttrain.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "A World of Pane", Description = "Shoot out 14 windows in a single round on Office.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.GlobalExpertise },
		// new () { Name = "Tourist", Description = "Play a round on every Arms Race and Demolition map.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Denied!", Description = "Kill a player who is on gold knife level in Arms Race mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Marksman", Description = "Win a match on every Arms Race and Demolition map.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Rampage!", Description = "Win an Arms Race match without dying.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "FIRST!", Description = "Be the first player to get a kill in an Arms Race or Demolition match.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "One Shot One Kill", Description = "Kill three consecutive players using the first bullet of your gun in Arms Race mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Conservationist", Description = "Win an Arms Race match without reloading any of your weapons.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Shorter Fuse", Description = "Plant five bombs in Demolition Mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Quick Cut", Description = "Defuse five bombs in Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "First Things First", Description = "Personally kill the entire Terrorist team before the bomb is planted in Demolition Mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Target Secured", Description = "Personally kill the entire Counter-Terrorist team before the bomb is planted in Demolition Mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Born Ready", Description = "Kill an enemy with the first bullet after your respawn protection ends in Arms Race mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Base Scamper", Description = "Kill an enemy just as their respawn protection ends in Arms Race mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Knife on Knife", Description = "Kill an enemy who is on gold knife level with your own knife in Arms Race mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Level Playing Field", Description = "Kill an enemy who is on gold knife level with a sub-machine gun in Arms Race Mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Still Alive", Description = "Survive more than 30 seconds with less than ten health in Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Practice Practice Practice", Description = "Play 100 matches of Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Gun Collector", Description = "Play 500 matches of Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "King of the Kill", Description = "Play 5,000 matches of Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Gungamer", Description = "Win one match in Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Keep on Gunning", Description = "Win 25 matches in Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Kill of the Century", Description = "Win 100 matches in Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "The Professional", Description = "Win 500 matches in Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Cold Pizza Eater", Description = "Win 1,000 matches in Arms Race or Demolition mode.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Repeat Offender", Description = "Dominate an enemy.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Decimator", Description = "Dominate ten enemies.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Insurgent", Description = "Kill an enemy who is dominating you.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Can't Keep a Good Man Down", Description = "Kill 20 enemies who are dominating you.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Overkill", Description = "Kill an enemy whom you are already dominating.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Command and Control", Description = "Kill 100 enemies whom you are already dominating.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Ten Angry Men", Description = "Kill 10 enemies you are already dominating during a single match.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Excessive Brutality", Description = "Kill an enemy whom you are dominating four additional times.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Hat Trick", Description = "Dominate three enemies simultaneously.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Avenging Angel", Description = "Kill an enemy who has killed a player on your friends list in the same round.", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.ArmsRaceDemolition },
		// new () { Name = "Record Breaker", Description = "Beat the active training course record in the Weapons CourseNever implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Combat Ready", Description = "Defuse a bomb with a kit when it would have failed without one.Unreleased", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Mad Props", Description = "Break 15 props in a single roundAfter the beta, Dynamic props replaced with static props", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Seppuku", Description = "Kill yourself while on gold knife level in Arms Race ModeOriginally available in the Beta, but was later removed", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Safety First", Description = "Survive a shot to the head because you had the good sense to wear a helmet (Competitive Mode only)Originally available in the Beta, but was removed", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "The Art of War", Description = "Apply graffiti 100 timesNot Implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Compound Map Veteran", Description = "Win 100 rounds on CompoundLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Havana Map Veteran", Description = "Win 100 rounds on HavanaLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Chateau Map Veteran", Description = "Win 100 rounds on ChateauLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Piranesi Map Veteran", Description = "Win 100 rounds on PiranesiLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Prodigy Map Veteran", Description = "Win 100 rounds on ProdigyLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Tides Map Veteran", Description = "Win 100 rounds on TidesLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Friendly Attire", Description = "Start a round on the same team as 4 of your friends, with all of you wearing the same outfitLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "USP Expert", Description = "Kill 200 enemies with the USP", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "P228 Expert", Description = "Kill 200 enemies with the P228", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "MP5 Expert", Description = "Kill 1,000 enemies with the MP5", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "TMP Expert", Description = "Kill 500 enemies with the TMP", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Dead of Night", Description = "Do 5,000 damage with nightvision activeLeft over achievement from Counter-Strike: Source", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Smorgasbord", Description = "Use every available weapon type in a single round in Scavenger ModeNever implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "I'm Still Standing", Description = "Win the round as the last man standing in Scavenger ModeNever implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Living on the Edge", Description = "Get a knife kill in Scavenger ModeNever implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Urban Warfare", Description = "Win five matches on Embassy Never implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Depot Despot", Description = "Win five matches on Depot Never implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Assault Map Veteran", Description = "Win 100 rounds on the Assault mapNever implemented", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "M4A1 Expert", Description = "Kill 1000 enemies with the M4A1", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Scout Expert", Description = "Kill 1,000 enemies with the Scout", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Galil Expert", Description = "Kill 500 enemies with the Galil", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "SG 550 Expert", Description = "Kill 500 enemies with the SG 550", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "SG 552 Expert", Description = "Kill 500 enemies with the SG 552", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "M3 Shotgun Expert", Description = "Kill 200 enemies with the M3 Shotgun", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent },
		// new () { Name = "Unknown achievement name", Description = "Kill an enemy with an un-zoomed sniper rifle in Arms Race or Demolition.Unreleased", Complete = false, Progress = 0, MaxProgress = 1, Category = Category.CutContent }
	];

	static readonly string SaveFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CS2Achievements", "achievements.json");

	public static void SaveAchievements() {
		Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);

		string json = System.Text.Json.JsonSerializer.Serialize(AchievementList);
		File.WriteAllText(SaveFilePath, json);
	}

	public static void LoadAchievements() {
		if (!File.Exists(SaveFilePath))
			return;

		string json = File.ReadAllText(SaveFilePath);
		List<Achievement>? loadedAchievements = System.Text.Json.JsonSerializer.Deserialize<List<Achievement>>(json);

		if (loadedAchievements != null) {
			int updated = 0, added = 0;
			foreach (var loaded in loadedAchievements) {
				int idx = AchievementList.FindIndex(a => string.Equals(a.Name, loaded.Name, StringComparison.OrdinalIgnoreCase));
				if (idx >= 0) {
					AchievementList[idx] = loaded;
					updated++;
				}
				else {
					AchievementList.Add(loaded);
					added++;
				}
			}
			Console.WriteLine($"Loaded {loadedAchievements.Count} achievements from file. Updated: {updated}, Added: {added}.");
		}
		else
			Console.WriteLine("Failed to load achievements from file.");

		// testing
		// if (AchievementList.Count > 0) {
		// 	var achievement = AchievementList[0];
		// 	PopupStack.Show($"{achievement.Name}", $"Progress: {achievement.Progress}/{achievement.MaxProgress}", GetAchievementIcon(achievement.Name));
		// }
	}

	public static void IncrementAchievementProgress(string achievementName, int amount = 1) {
		int idx = AchievementList.FindIndex(a => string.Equals(a.Name, achievementName, StringComparison.OrdinalIgnoreCase));

		if (idx == -1) {
			Console.WriteLine($"Achievement '{achievementName}' not found.");
			return;
		}

		var achievement = AchievementList[idx];
		if (achievement.Complete) {
			Console.WriteLine($"Achievement '{achievement.Name}' is already complete.");
			return;
		}

		achievement.Progress += amount;
		Console.WriteLine($"Progress for achievement '{achievement.Name}' increased by {amount}. Current progress: {achievement.Progress}/{achievement.MaxProgress}");
		if (achievement.Progress >= achievement.MaxProgress) {
			achievement.Progress = achievement.MaxProgress;
			achievement.Complete = true;
			Console.WriteLine($"Achievement Unlocked: {achievement.Name} - {achievement.Description}");
			PopupStack.Show($"Achievement Unlocked!", $"{achievement.Name}: {achievement.Description}");
		}
		else
			PopupStack.Show($"{achievement.Name}", $"Progress: {achievement.Progress}/{achievement.MaxProgress}");

		AchievementList[idx] = achievement;
		SaveAchievements();
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

	private static string? NormalizeNameForIcon(string name) => new([.. name.ToLower().Select(c => char.IsWhiteSpace(c) ? '_' : c)]);

	public static void OnEvent(Event eventName) {
		foreach (Achievement achievement in AchievementList.ToList()) {
			if (achievement.OnEvent == eventName)
				IncrementAchievementProgress(achievement.Name);
		}
	}
}