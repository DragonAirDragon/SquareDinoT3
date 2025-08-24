using UnityEngine;

namespace SquareDinoT3.Services
{
	/// <summary>
	/// Service for managing player nickname.
	/// </summary>
	public sealed class PlayerPrefsNameService : IPlayerNameService
	{
		private const string Key = "nickname";

		public string GetLocalNick()
		{
			var s = PlayerPrefs.GetString(Key, string.Empty);
			if (string.IsNullOrWhiteSpace(s)) s = $"Player_{Random.Range(1000, 9999)}";
			if (s.Length > 24) s = s.Substring(0, 24);
			return s.Trim();
		}

		public void SaveLocalNick(string nickname)
		{
			var s = string.IsNullOrWhiteSpace(nickname) ? $"Player_{Random.Range(1000, 9999)}" : nickname.Trim();
			if (s.Length > 24) s = s.Substring(0, 24);
			PlayerPrefs.SetString(Key, s);
			PlayerPrefs.Save();
		}
	}
}

