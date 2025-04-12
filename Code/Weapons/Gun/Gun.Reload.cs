

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
	[Property, Group( "Reloading" )] public float ReloadTime { get; set; } = 2;
	[Property, Group( "Reloading" )] public float EmptyReloadTime { get; set; } = 2;
	[Property, Group( "Reloading" )] public AmmoType AmmoType { get; set; } = AmmoType.Pistol;

	public virtual void Reload()
	{
		
		if ( IsReloading || InBoltBack || IsShooting() || IsHolstering )
			return;

		var maxClipSize = BulletCocking ? ClipSize + 1 : ClipSize;

		if ( Clip >= maxClipSize || ClipSize == -1 )
			return;

		var isEmptyReload = Clip == 0;

		if ( !Owner.InventoryController.CanTake( AmmoType, maxClipSize - Clip, out var ammo ) )
			return;

		IsReloading = true;
		AwaitReloadEnd();
		HandleReloadEffects();
	}
	public async void AwaitReloadEnd( )
	{
		float delay = IsEmpty ? EmptyReloadTime : ReloadTime;
		
		await GameTask.DelaySeconds( delay );
		
		OnReloadFinish();
	}

	public void OnReloadFinish()
	{
		IsReloading = false;
		
		var maxClipSize = BulletCocking && Clip > 0 ? ClipSize + 1 : ClipSize;

		Owner.InventoryController.TryTake( AmmoType, maxClipSize - Clip, out var ammo );
	
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


		if ( !Owner.InventoryController.CanTake( AmmoType, 1, out var ammo ) )
			return;


		IsReloading = true;
		ViewModelRenderer.Set( ReloadAnim, true );
	}

	public virtual void ShellReload()
	{
		IsReloading = false;


		Owner.InventoryController.TryTake( AmmoType, 1, out var ammo );

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
}
