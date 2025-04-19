using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralGame;

public class HealthController : Component, IHealthComponent
{
	[RequireComponent] public Player ply { get; set; }
	[Property] public float MaxHealth { get; set; } = 100f;

	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public float Health { get; set; } = 100f;

	public ColorAdjustments Adjustments { get; set; }
	public Vignette Vignette { get; set; }

	public bool IsAlive => LifeState == LifeState.Alive;
	public bool IsRagdolled => ply.ModelPhysics.Enabled;

	protected override void OnAwake()
	{
		base.OnAwake();

		Adjustments = ply.Camera.Components.Get<ColorAdjustments>();
		Vignette = ply.Camera.Components.Get<Vignette>();
	}

	protected override void OnStart()
	{

		if ( !IsProxy )
		{
			Respawn();
		}

		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;

		var health = (1f / MaxHealth) * Health;

		Adjustments.Saturation = 1f - (1f - health) * 0.3f;
		Vignette.Intensity = 0.5f * (1f - health);
	}

	public void OnDamage( in DamageInfo damage )
	{
		Log.Info( LifeState );
		if ( IsProxy || !IsAlive )
			return;
	

		Health -= damage.Damage;

		ply.CameraController.ApplyShake( 10, 1 );
		
		if ( Health <= 0 )
			OnDeath( damage );
	}


	public virtual void OnDeath( DamageInfo damage )
	{
		LifeState = LifeState.Dead;

		if ( ply.InventoryController.Deployed != null )
		{
			ply.InventoryController.Deployed.Holster();
			ply.InventoryController.Deployed = null;
		}

		ply.MovementController.CharacterController.Velocity = 0;

		var force = damage.Weapon.WorldRotation.Forward * 10 * damage.Damage;

		Ragdoll( force, damage.Attacker.WorldPosition );

		RespawnWithDelay(10);
	}

	public async void RespawnWithDelay( float delay )
	{
		await GameTask.DelaySeconds( delay );
		Respawn();
	}

	public void Respawn()
	{
		if ( IsProxy )
			return;


		Unragdoll();
		LifeState = LifeState.Alive;
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

		ply.CameraController.EyeAngles = WorldRotation;
	}





	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void Ragdoll( Vector3 force, Vector3 forceOrigin )
	{
		ToggleColliders( false );
		ply.ModelPhysics.Enabled = true;


		foreach ( var body in ply.ModelPhysics.PhysicsGroup.Bodies )
		{

			body.ApplyImpulseAt( forceOrigin, force );
		}
	}

	public void ToggleColliders( bool enable )
	{
		var colliders = ply.ModelPhysics.GameObject.Components.GetAll<Collider>( FindMode.EverythingInSelfAndParent );

		foreach ( var collider in colliders )
		{
			collider.Enabled = enable;
		}
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void Unragdoll()
	{

		ply.ModelPhysics.Renderer.LocalPosition = Vector3.Zero;
		ply.ModelPhysics.Renderer.LocalRotation = Rotation.Identity;
		ply.ModelPhysics.Enabled = false;
		ToggleColliders( true );

	}
}

