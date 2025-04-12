using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class Gun : Carriable
{
	[Property] public float AimFOV { get; set; } = 0f;
	[Property] public AngPos AimAnimData { get; set; }
	[Property] public bool ShellReloading { get; set; } = false;
	[Property] public bool BoltBack { get; set; } = false;
	[Property] public float AimSpeed { get; set; } = 1;
	[Property] public float DeployTime { get; set; } = 0.5f;
	[Property] public float HolsterTime { get; set; } = 0.5f;

	int burstCount = 0;
	int barrelHeat = 0;

	protected override void OnStart()
	{
		base.OnStart();

	}

	protected override void OnPickUp( Player ply )
	{
		ply.InventoryController.Give( AmmoType, 100 );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( Owner == null ) return;

		ViewModelRenderer?.Set( EmptyState, IsEmpty );

		if ( !IsReloading && !InBoltBack )
		{
			ViewModelRenderer?.Set( AimState, IsAiming );
		}

		if ( !IsProxy )
		{

			if ( IsDeploying ) return;

			if ( !IsScoping && !IsAiming && Input.Pressed( InputButtonHelper.Inspect ) )
			{
				ViewModelRenderer?.Set( InspectAnim, true );
			}

			if ( !IsScoping && !IsAiming && Input.Pressed( InputButtonHelper.Mode ) )
			{
				ViewModelRenderer?.Set( ModeAnim, true );
			}

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

				Shoot();
			}

		}
	}

	[Rpc.Broadcast]
	public override void Deploy( Player player )
	{
		
		base.Deploy( player );

		ViewModelRenderer?.Set( IsReady ? DeployAnim : ReadyAnim, true );

		AwaitDeployEnd();
	}

	public override bool CanHolster()
	{

		if ( IsShooting() || InBoltBack || IsHolstering || IsDeploying || IsReloading ) return false;
		return true;
	}
	public async void AwaitDeployEnd()
	{
		await GameTask.DelaySeconds( DeployTime );

		if ( !IsReady ) IsReady = true;
		IsDeploying = false;
	}

	public async void AwaitHolsterEnd()
	{
		await GameTask.DelaySeconds( HolsterTime );

		//EndHolster();
	}
}
