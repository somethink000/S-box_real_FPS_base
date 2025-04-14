
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
	[Property, Group( "Reloading" )] public float InsertShellTime { get; set; } = 1;
	private bool CancelReload { get; set; } = false; 

	private void StartShellReload()
	{
		if ( IsReloading || InBoltBack || IsShooting() || IsHolstering )
			return;

		var maxClipSize = BulletCocking ? ClipSize + 1 : ClipSize;

		if ( Clip >= maxClipSize || ClipSize == -1 )
			return;

		int toTake;

		if ( IsEmpty )
		{
			toTake = 2;
		}
		else
		{
			toTake = 1;
		}

		if ( !Owner.InventoryController.CanTake( AmmoType, toTake, out var ammo ) )
				return;


		IsReloading = true;
		ViewModelRenderer.Set( ReloadAnim, true );

		AwaitShellStart();
	}

	async void AwaitShellStart()
	{
		float time = IsEmpty ? EmptyReloadTime : ReloadTime;

		await GameTask.DelaySeconds( time );
		InsertShell();
	}
	void InsertShell()
	{
		
		int toTake;

		if ( IsEmpty )
		{
			toTake = 2;
		}
		else
		{
			toTake = 1;
		}

		Owner.InventoryController.TryTake( AmmoType, toTake, out var ammo );

		Clip += toTake;
		IsEmpty = false;

		if ( ammo != 0 && !CancelReload && CanInsertShell() )
		{
			AwaitShelInsert();
		}
		else
		{
			EndShellReload();
		}

	}
	async void AwaitShelInsert()
	{
		await GameTask.DelaySeconds( InsertShellTime );
		InsertShell();
	}

	bool CanInsertShell()
	{
		var maxClipSize = BulletCocking ? ClipSize + 1 : ClipSize;

		if ( Clip >= maxClipSize || ClipSize == -1 )
			return false;

		if ( !Owner.InventoryController.CanTake( AmmoType, 1, out var ammo ) )
			return false;

		return true;
	}
	

	void EndShellReload()
	{
		CancelReload = false;
		ViewModelRenderer.Set( ReloadAnim, false );

		IsReloading = false;
	}

	
	async void AsyncBoltBack( float boltBackDelay )
	{
		InBoltBack = true;
		// Start boltback
		await GameTask.DelaySeconds( boltBackDelay );
		if ( !IsValid ) return;
		if ( !IsProxy )
			ViewModelRenderer?.Set( BoltBackAnim, true );

		await GameTask.DelaySeconds( BoltBackTime );
		InBoltBack = false;

	}
}
