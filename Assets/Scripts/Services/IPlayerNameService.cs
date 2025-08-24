/// <summary>
/// Service for managing player nickname.
/// </summary>
namespace SquareDinoT3.Services
{
	public interface IPlayerNameService
	{
		string GetLocalNick();
		void SaveLocalNick(string nickname);
	}
}
