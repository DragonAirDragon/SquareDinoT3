using System;
namespace SquareDinoT3.Network
{
	/// <summary>
	/// Events for network client
	/// </summary>
	public interface INetworkClientEvents
	{
		event Action ClientConnected;
		event Action ClientDisconnected;
	}

}
