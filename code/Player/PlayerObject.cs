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
	[Property] public AmmoContainer Ammo { get; set; }
	[Property] public WeaponContainer Weapons { get; set; }
	[Property] public SoundEvent HurtSound { get; set; }
	[Property] public float HealthRegenPerSecond { get; set; } = 10f;
		
	[Sync, Property] public float MaxHealth { get; private set; } = 100f;
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public float Health { get; private set; } = 100f;

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

		Weapons.Clear();
		Weapons.GiveDefault();
		
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

	

	protected virtual void OnKilled( GameObject attacker )
	{
		
		if ( IsProxy )
			return;

		if ( Weapons.Deployed.IsValid() ) 
		{
			Weapons.Deployed.Holster();
		}
		
		
		RespawnAsync( 3f );
		
	}

	

	protected override void OnAwake()
	{
		base.OnAwake();
		OnControllerAwake();
		OnCameraAwake();
	}

	protected override void OnStart()
	{
		OnControllerStart();

		if ( !IsProxy )
		{
			Respawn();
		}
			
		base.OnStart();
	}
	
	protected override void OnUpdate()
	{
		base.OnUpdate();

		ControllerUpdate();
		CameraUpdate();
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

		if ( Input.MouseWheel.y > 0 )
			Weapons.Next();
		else if ( Input.MouseWheel.y < 0 )
			Weapons.Previous();

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

		var weapon = Weapons.Deployed;
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
	protected override void OnPreRender()
	{
		base.OnPreRender();
		OnCameraPreRender();
		BodyPreRender();

	}
}
