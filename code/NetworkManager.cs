using Sandbox.Network;

namespace GeneralGame;


public class NetworkManager : Component, Component.INetworkListener
{
	[Property] public PrefabScene PlayerPrefab { get; set; }

	LobbyConfig LobbyConfig { get; set; } = new LobbyConfig();
	protected override void OnStart()
	{
		if ( !Networking.IsActive )
		{
			Networking.CreateLobby( LobbyConfig );
		}

		base.OnStart();
	}

	// Called on host
	void INetworkListener.OnActive( Connection connection )
	{

		var obj = PlayerPrefab.Clone();
		var player = obj.Components.Get<Player>( FindMode.EverythingInSelfAndDescendants );
		obj.NetworkMode = NetworkMode.Object;
		obj.BreakFromPrefab();
		obj.NetworkSpawn( connection );
		player.SetupConnection( connection );
	}
}
