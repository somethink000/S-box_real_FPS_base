
using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class ThrowableWeapon : Carriable
{
	[Property] public float PrepareTime { get; set; }
	[Property] public GameObject ThrowPrefab { get; set; }

	public bool IsDeploying { get; set; }

	private string DeployAnim { get; set; } = "deploy";
	private string HolsterAnim { get; set; } = "holster";
	private string InspectAnim { get; set; } = "inspect";
	private string ReadyAnim { get; set; } = "ready";
	private string CookingAnim { get; set; } = "cook";

	private bool IsReady = false;
	private bool IsCooking = false;

	public TimeUntil curPrepareTime { get; set; }
	public TimeUntil curReleaseTime { get; set; }
	public bool isPreparing { get; set; } = false;
	public bool waitingThrow { get; set; } = false;


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

		if ( Owner == null || !_deployed || IsProxy ) return;
		
		if ( Input.Pressed( InputButtonHelper.Inspect ) )
		{
			if ( IsDeploying ) return;
			
			ViewModelRenderer?.Set( InspectAnim, true );
		}


		if ( Input.Down( InputButtonHelper.PrimaryAttack ) )
		{
			if ( isPreparing ) return;

			isPreparing = true;
			curPrepareTime = PrepareTime;
			ViewModelRenderer?.Set( CookingAnim, true );

		}

		if ( Input.Released( InputButtonHelper.PrimaryAttack ) )
		{
			if ( !isPreparing || curPrepareTime ) return;
			waitingThrow = true;
		}

		//explode granade in hands
		if ( curPrepareTime && isPreparing )
		{

			createThrow( 0 );
			isPreparing = false;
		}

		//throw 
		if ( waitingThrow && curReleaseTime )
		{
			
			waitingThrow = false;

			ViewModelRenderer?.Set( CookingAnim, false );
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

		if ( IsDeploying || IsCooking ) return false;
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

				case "throw":
					
					createThrow( curPrepareTime );
					isPreparing = false;

					break;

			}

		};
	}


	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void createThrow( float time )
	{




		if ( Networking.IsHost )
		{

			var obj = ThrowPrefab.Clone( this.Transform.World );
			obj.WorldPosition = Owner.Camera.WorldPosition + Owner.Camera.WorldRotation.Forward * 50;
			obj.WorldRotation = Owner.Camera.WorldRotation;
			obj.Components.Get<Rigidbody>().Velocity = Owner.Camera.WorldRotation.Forward * 1000;
			var thrw = obj.Components.Get<EntThrow>();
			thrw.Owner = Owner;
			thrw.ExplodeTime = time;
			obj.NetworkSpawn( null );
		}

	}


}
