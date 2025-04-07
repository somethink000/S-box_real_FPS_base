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

	public bool IsAlive => Health > 0;


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
		if ( !IsAlive )
			return;

		Health -= damage.Damage;


		if ( Health <= 0 )
			OnDeath( damage );
	}

	[Rpc.Broadcast]
	public virtual void OnDeath( DamageInfo damage )
	{

		var force = damage.Weapon.WorldRotation.Forward * 10 * damage.Damage;

		if ( IsProxy ) return;

		ply.MovementController.CharacterController.Velocity = 0;
		ply.RagdollManager.Ragdoll( force, damage.Attacker.WorldPosition );

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


		ply.RagdollManager.Unragdoll();
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
	
}

