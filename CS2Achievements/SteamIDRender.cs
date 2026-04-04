using Microsoft.Win32;

public static class SteamIDReader
{
	public static ulong GetCurrentSteamID64() {
		const string keyPath = @"SOFTWARE\Valve\Steam\ActiveProcess";

		using var key = Registry.CurrentUser.OpenSubKey(keyPath);

		if (key == null)
			throw new InvalidOperationException("Steam is not running (ActiveProcess key missing).");

		object? activeUserObj = key.GetValue("ActiveUser") ?? throw new InvalidOperationException("ActiveUser value is missing.");

		uint accountId;
		try {
			accountId = Convert.ToUInt32(activeUserObj);
		}
		catch {
			throw new InvalidOperationException($"Unexpected ActiveUser type: {activeUserObj.GetType()}");
		}

		if (accountId == 0)
			throw new InvalidOperationException("No Steam user is logged in (ActiveUser = 0).");

		ulong steamID64 = 76561197960265728UL + accountId;
		return steamID64;
	}

	public static string GetCurrentSteamID64String() {
		return GetCurrentSteamID64().ToString();
	}
}