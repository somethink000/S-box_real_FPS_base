

using Sandbox;
using Sandbox.Citizen;
using static Sandbox.Connection;
using static Sandbox.Diagnostics.PerformanceStats;
using static Sandbox.PhysicsContact;
using static Sandbox.VertexLayout;

namespace GeneralGame;


public partial class Zombie : Component, IHealthComponent
{

	[RequireComponent] public CitizenAnimationHelper animationHelper { get; set; }
	[RequireComponent] public Dresser Dresser { get; set; }
	[RequireComponent] public ModelCollider Collider { get; set; }
	[RequireComponent] NavMeshAgent agent { get; set; }
	[RequireComponent] public ModelPhysics RagdollPhysics { get; set; }

	[Property] public SkinnedModelRenderer Model { get; set; }
	[Property] GroupedCloth Cloths { get; set; }
	[Property] public Model MalModel { get; set; }
	[Property] public Model FemModel { get; set; }
	[Property] public Material ZombTexture { get; set; }
	[Property] public TagSet EnemyTags { get; set; }
	[Property] public float AttackDelay { get; set; } = 1;
	[Property] public float Damage { get; set; } = 10;
	[Property] public int DeleteTime { get; set; } = 10;
	[Property] public float DetectRange { get; set; } = 256f;
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public SoundEvent headshotSounds { get; set; }
	[Property] public SoundEvent hitSounds { get; set; }
	[Property, Group( "AI" )] public bool AlertOthers { get; set; } = true;
	[Property, Group( "AI" )] public float VisionRange { get; set; } = 512f;
	
	//MathF.Max(MathF.Max(GameObject.WorldTransform.Scale.x, GameObject.WorldTransform.Scale.y), GameObject.WorldTransform.Scale.z )
	public float Scale => 1f;
	public float AttackRange { get; set; } = 80f;
	public LifeState LifeState { get; private set; } = LifeState.Alive;
	public float Health { get; private set; } = 100f;
	[Sync( SyncFlags.FromHost )] public GameObject TargetObject { get; private set; } = null;
	[Sync( SyncFlags.FromHost )] public GameObject TargetPrimaryObject { get; set; } = null;
	public bool IsAlive => Health > 0;
	private bool IsRunner { get; set; }
	public bool IsRagdolled => RagdollPhysics.Enabled;

	private TimeUntil timeUntilLastDrawHeands = 0;
	private TimeSince timeSinceDead = 0;

	[Sync( SyncFlags.FromHost )] public TimeUntil NextAttack { get; set; }
	[Sync( SyncFlags.FromHost )] public bool ReachedDestination { get; set; } = true;
	[Sync( SyncFlags.FromHost )] public Vector3 TargetPosition { get; set; }
	[Sync( SyncFlags.FromHost )] public bool FollowingTargetObject { get; set; } = false;
	[Sync( SyncFlags.FromHost )] public int NpcId { get; set; }


	protected override void OnAwake()
	{
		Random rnd = new Random();


		if ( rnd.Next( 0, 2 ) == 1)
		{
			Model.Model = FemModel;
			Collider.Model = FemModel;
			RagdollPhysics.Model = FemModel;
		}
		else
		{
			Model.Model = MalModel;
			Collider.Model = MalModel;
			RagdollPhysics.Model = MalModel;
		}

		if ( rnd.Next( 0, 100 ) > 50 )
		{
			IsRunner = true;
		}


		List<ClothStruct> ClothStructs = new List<ClothStruct>();
		ClothStructs.Add( rnd.FromList( Cloths.Jackets ) );
		ClothStructs.Add( rnd.FromList( Cloths.Shirts ) );
		ClothStructs.Add( rnd.FromList( Cloths.Trousers ) );
		ClothStructs.Add( rnd.FromList( Cloths.Shoes ) );

		foreach ( var c in ClothStructs )
		{
			Dresser.Clothing.Add( c.Cloth );
		};

		Dresser.Apply();

	}
	protected override void OnStart()
	{

		Model.MaterialOverride = ZombTexture;

		NpcId = Scene.GetAllComponents<Zombie>().OrderByDescending( x => x.NpcId ).First().NpcId + 1;

		if ( IsRunner )
		{
			agent.MaxSpeed = 100;
		}
		else { agent.MaxSpeed = 200; }
	}

	//if (Vector3.DistanceBetween(target, GameObject.Transform.Position) < 80f)
	protected override void OnFixedUpdate()
	{
		if ( LifeState == LifeState.Dead )
			return;

		//animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		animationHelper.WithWishVelocity( agent.WishVelocity );
		animationHelper.WithVelocity( agent.Velocity );
		Model.GameObject.WorldRotation = Rotation.Lerp( Model.GameObject.WorldRotation, Rotation.LookAt( agent.Velocity.WithZ( 0f ), Vector3.Up ), Time.Delta * (IsRunner ? 10f : 5f) );
	
		CheckNewTargetPos();
		Model.Set( "wish_x", 360 );
		DetectAround();

		base.OnFixedUpdate();
	}

	protected override void OnUpdate()
	{

		if ( LifeState == LifeState.Dead )
		{
			if ( timeSinceDead > DeleteTime )
			{
				GameObject.Destroy();
			}
			
			return;
		}

	
		animationHelper.HoldType = timeUntilLastDrawHeands > 0 ? CitizenAnimationHelper.HoldTypes.Punch : CitizenAnimationHelper.HoldTypes.None;

	}

	public void OnDamage( in DamageInfo damage )
	{
		
		if ( LifeState == LifeState.Dead )
			return;

		

		Health -= damage.Damage;


		if ( Health <= 0 )
		{
			Ragdoll( damage.Weapon.WorldRotation.Forward * 10 * damage.Damage );
			timeSinceDead = 0;
			LifeState = LifeState.Dead;

			GameObject.Tags.Add( "ragdolled" );
			if ( damage.Attacker.Components.GetInAncestorsOrSelf<Player>() is Player ply )
			{
				//ply.CurrentGame.OnZombieKilled( ply );
			}

		}

	}


	[Rpc.Broadcast]
	public virtual void Ragdoll( Vector3 force )
	{
		Collider.Enabled = false;
		RagdollPhysics.Enabled = true;

		foreach ( var body in RagdollPhysics.PhysicsGroup.Bodies )
		{

			body.ApplyImpulseAt( WorldPosition, force );
		}
	}

	

	public void MoveTo( Vector3 targetPosition )
	{
		TargetPosition = targetPosition;
		ReachedDestination = false;
		agent.MoveTo( targetPosition );
	}


	[Rpc.Broadcast]
	protected virtual void BroadcastOnDetect()
	{

	}

	[Rpc.Broadcast]
	private void BroadcastOnEscape()
	{

	}
	[Rpc.Broadcast]
	protected virtual void BroadcastOnAttack()
	{
		
		animationHelper.Target.Set( "b_attack", true );
		Sound.Play( hitSounds, WorldPosition );

		var damage = new DamageInfo( Damage, GameObject, GameObject );
	
		foreach ( var damageable in TargetObject.Components.GetAll<IHealthComponent>() )
		{

			damageable.OnDamage( damage );


		}
	}
}
