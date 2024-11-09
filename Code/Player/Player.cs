using System;
using System.Collections.Generic;
using System.Linq;

namespace General;

public partial class Player : Component, Component.INetworkSpawn, IPlayer
{
	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public PanelComponent RootDisplay { get; set; }
	[Property] public Voice Voice { get; set; }

	[Sync] public bool IsBot { get; set; }

	public string DisplayName => !IsBot ? (Network.OwnerConnection?.DisplayName ?? "Disconnected") : GameObject.Name;
	public ulong SteamId => !IsBot ? Network.OwnerConnection.SteamId : 0;
	public bool IsHost => !IsBot && Network.OwnerConnection.IsHost;
	public bool IsSpeaking => Voice.Amplitude > 0;


	Guid IPlayer.Id { get => GameObject.Id; }

	protected override void OnAwake()
	{
		Voice = Components.GetInChildren<Voice>();
		OnCameraAwake();

		if ( IsBot ) return;

		// Hide client until fully loaded in OnStart
		if ( !IsProxy )
		{
			Transform.Position = new( 0, 0, -999999 );
			Network.ClearInterpolation();
		}

		OnMovementAwake();
	}

	public void OnNetworkSpawn( Connection connection )
	{
		ApplyClothes( connection );
	}

	protected override void OnStart()
	{


		if ( IsProxy || IsBot )
		{
			if ( Camera is not null )
				Camera.Enabled = false;
		}

		if ( IsBot )
		{
			var screenPanel = Components.GetInChildrenOrSelf<ScreenPanel>();

			if ( screenPanel is not null )
				screenPanel.Enabled = false;
		}

		if ( !IsProxy )
			Respawn();
	}


	public async void RespawnWithDelay( float delay )
	{
		await GameTask.DelaySeconds( delay );
		Respawn();
	}

	[Broadcast]
	public void RespawnWithDelayBroadCast( float delay )
	{
		RespawnWithDelay( delay );
	}

	[Broadcast]
	public void RespawnBroadCast()
	{
		Respawn();
	}

	public virtual void Respawn()
	{
		//Unragdoll();
		
		Health = MaxHealth;

		var spawnLocation = GetSpawnLocation();
		Transform.Position = spawnLocation.Position;
		EyeAngles = spawnLocation.Rotation.Angles();
		Network.ClearInterpolation();

		if ( IsBot )
		{
			Body.Transform.Rotation = new Angles( 0, EyeAngles.ToRotation().Yaw(), 0 ).ToRotation();
			AnimationHelper.WithLook( EyeAngles.ToRotation().Forward, 1f, 0.75f, 0.5f );
		}
	}

	public virtual Transform GetSpawnLocation()
	{
		var spawnPoints = Scene.Components.GetAll<SpawnPoint>();

		if ( !spawnPoints.Any() )
			return new Transform();

		var rand = new Random();
		var randomSpawnPoint = spawnPoints.ElementAt( rand.Next( 0, spawnPoints.Count() - 1 ) );

		return randomSpawnPoint.Transform.World;
	}

	protected override void OnUpdate()
	{
		if ( IsBot ) return;

		OnCameraUpdate();


		if ( IsAlive )
			OnMovementUpdate();

		
		UpdateClothes();
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsAlive || IsBot ) return;
		OnMovementFixedUpdate();
	}

	public static Player GetLocal()
	{
		var players = GetAll();
		return players.First( ( player ) => !player.IsProxy && !player.IsBot );
	}

	public static IEnumerable<Player> GetAll()
	{
		return Game.ActiveScene.GetAllComponents<Player>();
	}
}
