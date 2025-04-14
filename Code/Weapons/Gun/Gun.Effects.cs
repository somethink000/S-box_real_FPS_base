using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class Gun
{
	[Property, Group( "Shooting" )] public ParticleSystem BulletEjectParticle { get; set; }
	[Property, Group( "Shooting" )] public ParticleSystem MuzzleFlashParticle { get; set; }
	[Property, Group( "Shooting" )] public ParticleSystem BarrelSmokeParticle { get; set; }

	[Sync] public bool IsReloading { get; set; }
	[Sync] public bool IsAiming { get; set; }
	[Sync] public bool IsScoping { get; set; }
	[Sync] public bool InBoltBack { get; set; }
	[Sync] public bool IsDeploying { get; set; }
	[Sync] public bool IsHolstering { get; set; }

	public bool IsEmpty = false;
	public bool IsReady = false;
	public bool ShellReloadReady = false;

	//Basic Anims
	private string ShootAnim { get; set; } = "shoot";
	public string ReloadAnim { get; set; } = "reload";
	public string BoltBackAnim { get; set; } = "boltback";
	//Features  Anims
	public string DeployAnim { get; set; } = "deploy";
	public string HolsterAnim { get; set; } = "holster";
	public string InspectAnim { get; set; } = "inspect";
	public string ReadyAnim { get; set; } = "ready";
	public string ModeAnim { get; set; } = "mode";
	public string FixAnim { get; set; } = "fix";
	//Anim states
	public string EmptyState { get; set; } = "empty";
	public string AimState { get; set; } = "aiming";


	protected override void SetupViewModel( GameObject viewModelGO )
	{
		GunViewModel gvm = viewModelGO.Components.Create<GunViewModel>();
		ViewModelHandler = gvm;

		//TODO What the fuck is that
		gvm.Carriable = this;
		gvm.Weapon = this;

		ViewModelHandler.ViewModelRenderer = ViewModelRenderer;
		ViewModelHandler.Camera = Owner.Camera;
	}

	[Rpc.Broadcast]
	public virtual void HandleReloadEffects()
	{
		ViewModelRenderer?.Set( ReloadAnim, true );
		Owner.BodyRenderer.Set( "b_reload", true );
	}

	protected override void SetupAnimEvents()
	{
		//Sometimes animations fucked because of time differance and they dont wark when you dont have view model.
		//So for functional we using TimeSince but for youe view model GenericEvents

		ViewModelRenderer.OnGenericEvent = ( a ) =>
		{
			string t = a.Type;

			switch ( t )
			{
				//case "reload_end":

				//	if ( !ShellReloading )
				//	{
				//		OnReloadFinish();
				//	}
				//	else
				//	{
				//		IsReloading = false;
				//	}

				//	break;

				//case "pump_end":

				//	InBoltBack = false;

				//	break;

				case "eject_shell":

					CreateParticle( BulletEjectParticle, "ejection_point" );

					break;

				//case "shell_insert":

				//	ShellReload();

				//	break;

				//case "deployed":

				//	if ( !IsReady ) IsReady = true;
				//	IsDeploying = false;

				//	break;

				//case "holstered":

				//	//EndHolster();

				//	break;

			}

		};
	}


	void ParticleToMuzzlePos( SceneParticles particles )
	{
		var transform = GetMuzzleTransform();

		if ( transform.HasValue )
		{
			// Apply velocity to prevent muzzle shift when moving fast
			particles?.SetControlPoint( 0, transform.Value.Position + Owner.MovementController.Velocity * 0.03f );
			particles?.SetControlPoint( 0, transform.Value.Rotation );
		}
		else
		{
			particles?.Delete();
		}
	}

	/// <summary>Create a bullet impact effect</summary>
	public virtual void CreateBulletImpact( SceneTraceResult tr )
	{
		// Sound
		tr.Surface.PlayCollisionSound( tr.HitPosition );

		// Particles
		if ( tr.Surface.ImpactEffects.BulletDecal is not null )
		{
			var effectPath = Game.Random.FromList( tr.Surface.ImpactEffects.BulletDecal, "particles/impact.generic.smokepuff.vpcf" );

			if ( effectPath is not null )
			{
				// Surface def for flesh has wrong blood particle linked
				if ( effectPath.Contains( "impact.flesh" ) )
				{
					effectPath = "particles/impact.flesh.bloodpuff.vpcf";
				}
				else if ( effectPath.Contains( "impact.wood" ) )
				{
					effectPath = "particles/impact.generic.smokepuff.vpcf";
				}

				var p = new SceneParticles( Scene.SceneWorld, effectPath );
				p.SetControlPoint( 0, tr.HitPosition );
				p.SetControlPoint( 0, Rotation.LookAt( tr.Normal ) );
				p.PlayUntilFinished( TaskSource.Create() );
			}
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
				gameObject.Transform.Position = tr.HitPosition;
				gameObject.Transform.Rotation = Rotation.LookAt( -tr.Normal );

				var decalRenderer = gameObject.Components.Create<DecalRenderer>();
				decalRenderer.Material = decalEntry.Material;
				decalRenderer.Size = new( decalEntry.Height.GetValue(), decalEntry.Height.GetValue(), decalEntry.Depth.GetValue() );
				gameObject.DestroyAsync( 30f );
			}
		}
	}

	/// <summary>Create a weapon particle</summary>
	public virtual void CreateParticle( ParticleSystem particle, string attachment, Action<SceneParticles> OnFrame = null )
	{
		var effectRenderer = GetEffectRenderer();

		if ( effectRenderer is null || effectRenderer.SceneModel is null ) return;

		var transform = effectRenderer.SceneModel.GetAttachment( attachment );

		if ( !transform.HasValue ) return;

		CreateParticle( particle, transform.Value, OnFrame );
	}

	public virtual void CreateParticle( ParticleSystem particle, Transform transform, Action<SceneParticles> OnFrame = null )
	{
		SceneParticles particles = new( Scene.SceneWorld, particle );
		particles?.SetControlPoint( 0, transform.Position );
		particles?.SetControlPoint( 0, transform.Rotation );
		particles?.SetNamedValue( "scale", 1 );

		if ( CanSeeViewModel )
			particles.Tags.Add( TagsHelper.ViewModel );

		particles?.PlayUntilFinished( Task, OnFrame );
	}
}
