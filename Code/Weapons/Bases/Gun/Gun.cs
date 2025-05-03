using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class Gun : Carriable, IUseAmmo
{
	[Property] public float AimFOV { get; set; } = 0f;
	[Property] public AngPos AimAnimData { get; set; }
	[Property] public bool ShellReloading { get; set; } = false;
	[Property] public bool BoltBack { get; set; } = false;
	[Property] public float AimSpeed { get; set; } = 1;

	int burstCount = 0;
	int barrelHeat = 0;

	protected override void OnStart()
	{
		base.OnStart();

		if ( Clip <= 0 ) IsEmpty = true;
	}

	protected override void OnPickUp( Player ply )
	{
		ply.InventoryController.Give( AmmoType, 100 );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();


		if ( Owner == null || !_deployed ) return;

		if ( !IsProxy )
		{
			ViewModelRenderer?.Set( EmptyState, IsEmpty );

			if ( !IsReloading && !InBoltBack )
			{
				ViewModelRenderer?.Set( AimState, IsAiming );
			}

			if ( IsDeploying ) return;

			if ( Input.Pressed( InputButtonHelper.Inspect ) )
			{
				if ( IsShooting() || InBoltBack || IsHolstering || IsDeploying || IsReloading ) return;

				ViewModelRenderer?.Set( InspectAnim, true );
			}

			//TODO make fire types
			//if (Input.Pressed( InputButtonHelper.Mode ) )
			//{
			//	ViewModelRenderer?.Set( ModeAnim, true );
			//}

			IsAiming = !IsRunning && AimAnimData != AngPos.Zero && Input.Down( InputButtonHelper.SecondaryAttack );

			if ( IsAiming )
			{
				Owner.CameraController.ApplyFov( AimFOV );
			}

			ResetBurstFireCount( InputButtonHelper.PrimaryAttack );


			var shouldTuck = ShouldTuck();

			if ( Input.Down( InputButtonHelper.Reload ) )
			{
				if ( ShellReloading )
					StartShellReload();
				else
					Reload();
			}


			if ( Input.Down( InputButtonHelper.PrimaryAttack ) )
			{

				if ( IsReloading && ShellReloading && !IsEmpty )
				{
					CancelReload = true;
				}

				Shoot();
			
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

		if ( IsShooting() || InBoltBack || IsHolstering || IsDeploying || IsReloading ) return false;
		return true;
	}
	
}
