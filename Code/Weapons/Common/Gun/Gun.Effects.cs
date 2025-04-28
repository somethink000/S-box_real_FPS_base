using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.PhysicsContact;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class Gun
{

	[Property] public BasicParticleEmiter MuzzleFlashEmiter { get; set; }
	[Property] public BasicParticleEmiter MuzzleSmokeEmiter { get; set; }
	[Property] public BasicParticleEmiter EjectShellEmiter { get; set; }
	string muzzleBone { get; set; } = "muzzle";
	string ejectBone { get; set; } = "eject";

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

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void HandleReloadEffects()
	{
		Owner.BodyRenderer.Set( "b_reload", true );
	}

	protected override void SetupAnimEvents()
	{

		ViewModelRenderer.OnGenericEvent = ( a ) =>
		{
			string t = a.Type;

			switch ( t )
			{
				case "reload_end":

					if ( !ShellReloading )
					{
						OnReloadFinish();
					}
					else
					{
						EndShellReload();
					}

					break;

				case "pump_end":

					InBoltBack = false;

					break;

				case "eject_shell":

					var transform = GetArrachmentTransform( ejectBone );

					EjectShellEmiter.Emit( transform.Value, Owner.MovementController.Velocity );
					

					break;

				case "shell_insert":

					
					InsertShell();

					break;

				case "deployed":

					if ( !IsReady ) IsReady = true;
					IsDeploying = false;

					break;

				case "holstered":

					//EndHolster();

					break;

			}

		};
	}

	void ShootEffect()
	{
		var transform = GetArrachmentTransform(muzzleBone);
		
		IEnumerable<BasicParticleEmiter> emiters = MuzzleFlashEmiter.GameObject.Components.GetAll<BasicParticleEmiter>(FindMode.EnabledInSelfAndChildren);

		foreach ( var emiter in emiters ) 
		{
			
			emiter.Emit( transform.Value, Owner.MovementController.Velocity );
			
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
