using Sandbox;
using Sandbox.Citizen;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class MeleWeapon : Carriable
{
	[Property] public float AttackDelay { get; set; }
	[Property] public float Damage { get; set; } = 5;
	[Property] public SoundEvent HitSound { get; set; }

	[Sync] public bool IsDeploying { get; set; }

	private string DeployAnim { get; set; } = "deploy";
	private string HolsterAnim { get; set; } = "holster";
	private string InspectAnim { get; set; } = "inspect";
	private string ReadyAnim { get; set; } = "ready";

	private bool IsReady = false;
	private bool IsAttacking = false;
	private bool AttackTraced = false;

	private TimeUntil AttackHitDelay { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

	}

	protected override void OnPickUp( Player ply )
	{
		
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();


		if ( Input.Pressed( InputButtonHelper.Inspect ) )
		{
			if ( IsDeploying ) return;

			ViewModelRenderer?.Set( InspectAnim, true );
		}

		if (!IsAttacking ) 
		{

			if ( Input.Down( InputButtonHelper.PrimaryAttack ) )
			{
				IsAttacking = true;
				AttackTraced = false;
				AttackHitDelay = AttackDelay;
				ViewModelRenderer?.Set( "attack", true );
			}
			if ( Input.Down( InputButtonHelper.SecondaryAttack ) )
			{
				IsAttacking = true;
				AttackTraced = false;
				AttackHitDelay = AttackDelay;
				ViewModelRenderer?.Set( "attack2", true );
			}

		}

		if (AttackHitDelay <= 0 && !AttackTraced)
		{
			Attack();
		}

	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void Attack( )
	{
		AttackTraced = true;
		Owner.BodyRenderer.Set( "b_attack", true );


		var player = Owner;
		var forward = player.CameraController.EyeAngles.Forward;
		forward = forward.Normal;
		var endPos = player.CameraController.EyePos + forward * 80;
		var bulletTr = MakeTrace( player.CameraController.EyePos, endPos );
		var hitObj = bulletTr.GameObject;

		if ( SurfaceUtil.IsSkybox( bulletTr.Surface ) || bulletTr.HitPosition == Vector3.Zero ) return;

		//// Impact
		CreateBulletImpact( bulletTr );
		GameObject.PlaySound(HitSound);

		// Damage
		if ( hitObj is not null )
		{

			var damage = new DamageInfo( Damage, Owner.GameObject, GameObject, bulletTr.Hitbox );
			damage.Position = bulletTr.HitPosition;
			damage.Shape = bulletTr.Shape;


			if ( bulletTr.GameObject.Components.GetInAncestorsOrSelf<IHealthComponent>() is IHealthComponent damagable )
			{
				damagable.OnDamage( damage );
			}

		}
	}

	public override void Deploy()
	{
		base.Deploy();

		if ( !IsProxy )
			ViewModelRenderer?.Set( IsReady ? DeployAnim : ReadyAnim, true );
	}
	

	public override bool CanHolster()
	{

		if ( IsDeploying || IsAttacking ) return false;
		return true;
	}

	protected override void SetupAnimEvents()
	{

		ViewModelRenderer.OnGenericEvent = ( a ) =>
		{
			string t = a.Type;

			switch ( t )
			{

				case "deployed":

					if ( !IsReady ) IsReady = true;
					IsDeploying = false;

					break;

				case "holstered":

					//EndHolster();

					break;

				case "end_attack":

					IsAttacking = false;

					break;

			}

		};
	}

	public virtual void CreateBulletImpact( SceneTraceResult tr )
	{

		// Sound
		tr.Surface.PlayCollisionSound( tr.HitPosition );

		// Particles
		if ( tr.Surface.ImpactEffects.BulletDecal is not null )
		{
			var effectPath = Game.Random.FromList( tr.Surface.ImpactEffects.BulletDecal, "particles/impact.generic.smokepuff.vpcf" );

		}

		// Decal
		if ( tr.Surface.ImpactEffects.BulletDecal is not null )
		{
			var decalPath = Game.Random.FromList( tr.Surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );

			if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalDef ) )
			{
				var decalEntry = Game.Random.FromList( decalDef.Decals );

				var gameObject = Scene.CreateObject();
				//gameObject.SetParent( tr.GameObject, false );
				gameObject.WorldPosition = tr.HitPosition;
				gameObject.WorldRotation = Rotation.LookAt( -tr.Normal );

				var decalRenderer = gameObject.Components.Create<DecalRenderer>();
				decalRenderer.Material = decalEntry.Material;
				decalRenderer.Size = new( decalEntry.Height.GetValue(), decalEntry.Height.GetValue(), decalEntry.Depth.GetValue() );
				gameObject.DestroyAsync( 30f );
			}
		}
	}

}
