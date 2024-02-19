using System;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace Facepunch.Arena;

public sealed class Zombie : Component, IHealthComponent
{
	[Property] public GameObject body { get; set; }
	[Property] public GameObject eye { get; set; }
	[Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Property] public SoundEvent hitSound { get; set; }
	[Sync, Property] public float MaxHealth { get; private set; } = 100f;
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public float Health { get; private set; } = 100f;
	[Property] public GameObject ZombieRagedol { get; set; }
	private NavMeshAgent agent;
	private PlayerController playerController;
	public TimeSince timeSinceHit = 0;

	protected override void OnAwake()
	{
		agent = Components.Get<NavMeshAgent>();
		playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault();
	}
	protected override void OnUpdate()
	{

		var target = playerController.Transform.Position;
		playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault();
		animationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
		animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		UpdateAnimtions();
		if (Vector3.DistanceBetween(target, GameObject.Transform.Position ) < 150f)
		{
			agent.Stop();

		}
		else
		{
			agent.MoveTo(playerController.Transform.Position);
		}

		if (playerController.Transform.Position.z > GameObject.Transform.Position.z + 50)
		{
			UpwardTrace();
		}
		else
		{
			NormalTrace();
		}


	}
	
	protected override void OnFixedUpdate()
	{

		
	}

	void UpdateAnimtions()
	{
		var bodyRot = body.Transform.Rotation.Angles();
		animationHelper.WithWishVelocity(agent.WishVelocity);
		animationHelper.WithVelocity(agent.Velocity);
		var targetRot = Rotation.LookAt(playerController.GameObject.Transform.Position.WithZ(Transform.Position.z) - body.Transform.Position);
		body.Transform.Rotation = Rotation.Slerp(body.Transform.Rotation, targetRot, Time.Delta * 5.0f);
	}
	void NormalTrace()
	{
		var tr = Scene.Trace.Ray(body.Transform.Position, body.Transform.Position + body.Transform.Rotation.Forward * 75).Run();

		if (tr.Hit && tr.GameObject.Tags.Has("player") && timeSinceHit > 1.0f && GameObject is not null)
		{
			IHealthComponent damageable = null;
			damageable = tr.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

			damageable.TakeDamage( DamageType.Bullet, 15, tr.EndPosition, tr.Direction * 5, GameObject.Id );
			//playerController.TakeDamage(25);
			animationHelper.Target.Set("b_attack", true);
			timeSinceHit = 0;
			Sound.Play(hitSound);
		}

	}

	void UpwardTrace()
	{
		var tr = Scene.Trace.Ray(eye.Transform.Position, eye.Transform.Position + eye.Transform.Rotation.Up * 50).Run();

		if (tr.Hit && tr.GameObject.Tags.Has("player") && timeSinceHit > 1.0f && GameObject is not null)
		{

			IHealthComponent damageable = null;
			damageable = tr.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

			damageable.TakeDamage( DamageType.Bullet, 15, tr.EndPosition, tr.Direction * 5, GameObject.Id );
			//playerController.TakeDamage(25);
			animationHelper.Target.Set("b_attack", true);
			timeSinceHit = 0;
			Sound.Play(hitSound);
		}

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
		}

		if ( IsProxy )
			return;


		Health = MathF.Max( Health - damage, 0f );

		if ( Health <= 0f )
		{
			LifeState = LifeState.Dead;
			var zombie = ZombieRagedol.Clone( this.GameObject.Transform.Position, this.GameObject.Transform.Rotation );
			this.GameObject.Destroy();
		}

	}

}
