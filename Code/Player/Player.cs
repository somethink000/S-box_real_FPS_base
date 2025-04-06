
using Sandbox.Citizen;
using Sandbox.Services;
using Sandbox;
using System;
using System.Linq;
using static Sandbox.Connection;

namespace GeneralGame;


public partial class Player : Component, Component.INetworkSpawn, IHealthComponent
{

	[Property, Category( "Relatives" )] public GameObject Head { get; set; }
	[Property, Category( "Relatives" )] public GameObject Body { get; set; }
	[Property, Category( "Relatives" )] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property, Category( "Relatives" )] public CameraComponent Camera { get; set; }
	[Property, Category( "Relatives" )] public PanelComponent RootDisplay { get; set; }
	[Property, Category( "Relatives" )] public Voice Voice { get; set; }
	[Property, Category( "Relatives" )] public ModelPhysics ModelPhysics { get; set; }

	[Property, Category( "Movement" )] public float BaseGroundControl { get; set; } = 4.0f;
	[Property, Category( "Movement" )] public float AirControl { get; set; } = 0.1f;
	[Property, Category( "Movement" )] public float MaxForce { get; set; } = 50f;
	[Property, Category( "Movement" )] public float RunSpeed { get; set; } = 290f;
	[Property, Category( "Movement" )] public float WalkSpeed { get; set; } = 160f;
	[Property, Category( "Movement" )] public float CrouchSpeed { get; set; } = 90f;
	[Property, Category( "Movement" )] public float JumpForce { get; set; } = 350f;


	[Property, Category( "Stats" )] public float MaxHealth { get; set; } = 100f;

	public RagdollController RagdollController { get; }

	private float SaveDelay = 60f;
	private TimeSince SinceSave { get; set; }


	public Guid Id { get; }


	public Player() 
	{
		RagdollController = new RagdollController( ModelPhysics );
	}

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



		RagdollController.Unragdoll();
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

		CharacterController.Velocity = 0;
		RagdollController.Ragdoll( force, damage.Attacker.WorldPosition );

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
