
using Sandbox.Citizen;
using Sandbox.Services;
using Sandbox;
using System;
using System.Linq;
using static Sandbox.Connection;

namespace GeneralGame;


public partial class PlayerBase : Component, Component.INetworkSpawn, IHealthComponent
{

	

	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public PanelComponent RootDisplay { get; set; }
  //  [Property] public Inventory Inventory { get; set; }
	[Property] public Voice Voice { get; set; }

	private float SaveDelay = 60f;
	private TimeSince SinceSave { get; set; }


	public Guid Id { get; }


	protected override void OnAwake()
	{
		//Components.Create<PlayerJob>();

		OnCameraAwake();
		OnMovementAwake();
	}
	


	public void OnNetworkSpawn( Connection connection )
	{
		
	}

	protected override void OnStart()
	{


		if ( IsProxy )
		{
			if ( Camera is not null )
				Camera.Enabled = false;
		}

		if ( !IsProxy )
		{
			
			SinceSave = 0;


			Respawn();
		}


		base.OnStart();
	}
	
	public void Respawn()
	{
		if ( IsProxy )
			return;



		Unragdoll();
		Health = MaxHealth;

		MoveToSpawnPoint();
		
	}

	
	private void MoveToSpawnPoint()
	{
		if ( IsProxy )
			return;
		
		var spawnpoints = Scene.GetAllComponents<SpawnPoint>();
		var randomSpawnpoint = Game.Random.FromList( spawnpoints.ToList() );
		Network.ClearInterpolation();
		WorldPosition = randomSpawnpoint.WorldPosition;
		WorldRotation = Rotation.FromYaw( randomSpawnpoint.WorldRotation.Yaw() );
		
		EyeAngles = WorldRotation;
	}

	

	[Rpc.Broadcast]
	public virtual void OnDeath( DamageInfo damage )
	{

		var force = damage.Weapon.WorldRotation.Forward * 10 * damage.Damage;

		if ( IsProxy ) return;

		Deaths += 1;
		CharacterController.Velocity = 0;
		Ragdoll( force, damage.Attacker.WorldPosition );

	}
	///GameManager.ActiveScene.LoadFromFile( "scenes/basement.scene" );
	public async void RespawnWithDelay( float delay )
	{
		await GameTask.DelaySeconds( delay );
		Respawn();
	}


	protected override void OnUpdate()
	{
		
		OnCameraUpdate();

		if ( !IsProxy && SinceSave > SaveDelay )
		{
			SinceSave = 0;

		}


		if ( !IsProxy )
		{
			BodyRenderer.SetBodyGroup( "head", 2 );
		}



		if ( IsAlive )
		{
			OnMovementUpdate();
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsAlive ) return;

		OnMovementFixedUpdate();
		if ( !IsProxy )
	
		if (IsProxy)
			return;

		FixedHealthEffectUpdate();
		UpdateInteractions();
	}
}
