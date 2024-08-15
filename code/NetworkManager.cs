using System.Linq;
using System.Numerics;
using Sandbox;
using Sandbox.Network;

namespace GeneralGame;


public class NetworkManager : Component, Component.INetworkListener
{
	[Property] public PrefabScene PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}
		
		base.OnStart();
	}

	void INetworkListener.OnActive( Connection connection )
	{
		var obj = PlayerPrefab.Clone();
		var player = obj.Components.Get<PlayerObject>( FindMode.EverythingInSelfAndDescendants );
		obj.NetworkMode = NetworkMode.Object;
		obj.BreakFromPrefab();
		obj.NetworkSpawn( connection );
		player.SetupConnection( connection );




	}
}
