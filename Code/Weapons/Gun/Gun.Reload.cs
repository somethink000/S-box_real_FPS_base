

using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class Gun
{
	public virtual void Reload()
	{

		if ( IsReloading || InBoltBack || IsShooting() || IsHolstering )
			return;

		var maxClipSize = BulletCocking ? ClipSize + 1 : ClipSize;

		if ( Clip >= maxClipSize || ClipSize == -1 )
			return;

		var isEmptyReload = Clip == 0;


		//if ( !Owner.Inventory.HasItems( AmmoType ) )
		//	return;

		IsReloading = true;

		ViewModelRenderer?.Set( ReloadAnim, true );

		// Player anim
		HandleReloadEffects();

	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
		var maxClipSize = BulletCocking && Clip > 0 ? ClipSize + 1 : ClipSize;

		//if ( Owner.CurrentGame.InfiniteAmmo )
		//{
		//	Ammo = maxClipSize;
		//	IsEmpty = false;
		//	return;
		//}



		var ammo = 10;//Owner.Inventory.TryTake( AmmoType, maxClipSize - Ammo ); //Owner.TakeAmmo( Primary.AmmoType, maxClipSize - Primary.Ammo );
	
		if ( ammo == 0 )
			return;

		IsEmpty = false;
		Clip += ammo;
	}

	public virtual void CancelShellReload()
	{
		ViewModelRenderer.Set( ReloadAnim, false );
	}

	public virtual void StartShellReload()
	{
		if ( IsReloading || InBoltBack || IsShooting() || IsHolstering )
			return;

		var maxClipSize = BulletCocking ? ClipSize + 1 : ClipSize;

		if ( Clip >= maxClipSize || ClipSize == -1 )
			return;

		var isEmptyReload = Clip == 0;


		//if ( !Owner.Inventory.HasItems( AmmoType ) )
		//	return;


		IsReloading = true;
		ViewModelRenderer.Set( ReloadAnim, true );
	}

	public virtual void ShellReload()
	{
		IsReloading = false;


		var ammo = 1;//Owner.Inventory.TryTake( AmmoType, 1 );

		Clip += 1;

		if ( ammo != 0 && Clip < ClipSize )
		{
			Reload();
		}
		else
		{
			CancelShellReload();
		}
	}

	async void AsyncBoltBack( float boltBackDelay )
	{
		InBoltBack = true;
		// Start boltback
		await GameTask.DelaySeconds( boltBackDelay );
		if ( !IsValid ) return;
		if ( !IsProxy )
			ViewModelRenderer?.Set( BoltBackAnim, true );

	}


	[Rpc.Broadcast]
	public virtual void HandleReloadEffects()
	{
		// Player
		Owner.BodyRenderer.Set( "b_reload", true );
	}
}
