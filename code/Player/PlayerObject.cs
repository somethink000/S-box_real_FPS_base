using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace GeneralGame;

//Player game logic,
//To use Camera, Speed, CharachterController variables take them from PlayerBody, PlayerCamera, PlayerController. like (player.CameraController.Camera)
public partial class PlayerObject : Component, IHealthComponent
{

	[Property] public RagdollController Ragdoll { get; private set; }
	[Property] public Inventory Inventory { get; set; }
	[Property] public SoundEvent HurtSound { get; set; }
	[Property] public float HealthRegenPerSecond { get; set; } = 10f;
		
	[Sync, Property] public float MaxHealth { get; private set; } = 100f;
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public float Health { get; private set; } = 100f;

	//Needs {
		public float MaxNeeds { get; private set; } = 100f;
		[Sync] public float Hunger { get; private set; } = 100f;
		[Property] public float HungerPerSecond { get; set; } = 0.1f;
		[Sync] public float Stamina { get; private set; } = 100f;
		[Property] public float StaminaPerSecond { get; set; } = -5f;
	//}

	public int MaxCarryWeight { get; set; }
	public bool IsEncumbered => Inventory.Weight > MaxCarryWeight;
	private RealTimeSince TimeSinceDamaged { get; set; }

	public async void RespawnAsync( float seconds )
	{
		if ( IsProxy ) return;

		await Task.DelaySeconds( seconds );
		Respawn();
	}

	public void Respawn()
	{
		if ( IsProxy )
			return;

		MaxCarryWeight = Inventory.MAX_WEIGHT_IN_GRAMS;
		Ragdoll.Unragdoll();
		MoveToSpawnPoint();
		LifeState = LifeState.Alive;
		Health = MaxHealth;
	}
	
	[Broadcast]
	public void TakeDamage( DamageType type, float damage, Vector3 position, Vector3 force, Guid attackerId )
	{
		if ( LifeState == LifeState.Dead )
			return;
		
		if ( type == DamageType.Bullet )
		{
			var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
			p.SetControlPoint( 0, position );
			p.SetControlPoint( 0, Rotation.LookAt( force.Normal * -1f ) );
			p.PlayUntilFinished( Task );

			if ( HurtSound is not null )
			{
				Sound.Play( HurtSound, Transform.Position );
			}
		}
		
		if ( IsProxy )
			return;

		TimeSinceDamaged = 0f;
		Health = MathF.Max( Health - damage, 0f );
		
		if ( Health <= 0f )
		{
			LifeState = LifeState.Dead;
			Ragdoll.Ragdoll( position, force );
			SendKilledMessage( attackerId );
			
		}
	}

	protected override void OnUpdate()
	{
		CameraUpdate();
		ControllerUpdate();
	}

	protected virtual void OnKilled( GameObject attacker )
	{
		
		if ( IsProxy )
			return;

		if ( Inventory.Deployed.IsValid() ) 
		{
			Inventory.Deployed.Holster();
		}
		
		
		RespawnAsync( 3f );
		
	}

	

	protected override void OnAwake()
	{
		base.OnAwake();

		
		CameraAwake();
		ControllerAwake();
	}

	protected override void OnStart()
	{
		ControllerStart();

		if ( !IsProxy )
		{
			Respawn();
		}
			
		base.OnStart();
	}
	protected override void OnPreRender()
	{
		BodyPreRender();
		CameraPreRender();
	}
	

	protected override void OnFixedUpdate()
	{
		
		ControllerFixedUpdate();

		if ( IsProxy )
			return;

		if ( Ragdoll.IsRagdolled || LifeState == LifeState.Dead )
			return;

		if ( TimeSinceDamaged > 3f )
		{
			Health += HealthRegenPerSecond * Time.Delta;
			Health = MathF.Min( Health, MaxHealth );
		}

		

		Hunger = Math.Clamp( Hunger - HungerPerSecond * Time.Delta, 0, 100 );
		Stamina = Math.Clamp( Stamina + StaminaPerSecond * Time.Delta, 0, 100 );

		if ( Hunger <= 0 ) MaxCarryWeight = Inventory.MAX_WEIGHT_IN_GRAMS / 2;

		UpdateInteractions();


		if ( Input.MouseWheel.y > 0 )
			Inventory.Next();
		else if ( Input.MouseWheel.y < 0 )
			Inventory.Next();

		if ( Input.Pressed( "use" ) )
		{
			var startPos = Camera.Transform.Position;
			var direction = Camera.Transform.Rotation.Forward;

			var endPos = startPos + direction * 10000f;
			var trace = Scene.Trace.Ray( startPos, endPos )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.UsePhysicsWorld()
				.UseHitboxes()
				.Run();

			IUse usable = null;

			if ( trace.Component.IsValid() )
				usable = trace.Component.Components.GetInAncestorsOrSelf<IUse>();

			if ( usable is not null )
			{
				usable.OnUse( GameObject.Id );
			}
		}

		var weapon = Inventory.Deployed;
		if ( !weapon.IsValid() ) return;


		if ( Input.Pressed( "Reload" ) )
		{
			weapon.reloadAction();
		}
	
		if ( Input.Pressed( "Attack1" ) )
		{
			weapon.primaryAction();
		}

		if ( Input.Released( "Attack1" ) )
		{
			weapon.primaryActionRelease();
		}

		if ( Input.Pressed( "Attack2" ) )
		{
			weapon.seccondaryAction();
		}

		if ( Input.Released( "Attack2" ) )
		{
			weapon.seccondaryActionRelease();
		}
	} 
	
	private void MoveToSpawnPoint()
	{
		if ( IsProxy )
			return;
		
		var spawnpoints = Scene.GetAllComponents<SpawnPoint>();
		var randomSpawnpoint = Game.Random.FromList( spawnpoints.ToList() );

		Transform.Position = randomSpawnpoint.Transform.Position;
		Transform.Rotation = Rotation.FromYaw( randomSpawnpoint.Transform.Rotation.Yaw() );
		EyeAngles = Transform.Rotation;
	}



	[Broadcast]
	private void SendKilledMessage( Guid attackerId )
	{
		var attacker = Scene.Directory.FindByGuid( attackerId );
		OnKilled( attacker );
	}
	
}
