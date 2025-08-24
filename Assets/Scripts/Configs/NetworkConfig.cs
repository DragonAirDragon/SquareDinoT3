using UnityEngine;
namespace SquareDinoT3.Configs
{
	/// <summary>
	/// Config for network
	/// </summary>
	[CreateAssetMenu(fileName = "NetworkConfig", menuName = "Configs/Network Config")]
	public class NetworkConfig : ScriptableObject
	{
		[Header("Rates")]
		[Min(1f)] public float SendRateHz = 60f;
	}
}


