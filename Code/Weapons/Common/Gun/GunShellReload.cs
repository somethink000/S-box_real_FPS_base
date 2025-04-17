
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
	private bool CancelReload { get; set; } = false; 

	private void StartShellReload()
	{
		if ( IsReloading || InBoltBack || IsShooting() || IsHolstering )
			return;

		var maxClipSize = BulletCocking ? ClipSize + 1 : ClipSize;

		if ( Clip >= maxClipSize || ClipSize == -1 )
			return;

		

		if ( !Owner.InventoryController.CanTake( AmmoType, GetToTake(), out var ammo ) )
				return;


		IsReloading = true;
		ViewModelRenderer.Set( ReloadAnim, true );

	}
	
	void InsertShell()
	{

		int toTake = GetToTake();

		Owner.InventoryController.TryTake( AmmoType, toTake, out var ammo );

		Clip += toTake;
		
		IsEmpty = false;
		

		//TODO
		if ( ammo != 0 && !CancelReload && CanInsertShell() )
		{
			//AwaitShelInsert();
		}
		else
		{
			EndShellReload();
		}
	}

	//first animation inserting 2 bullets
	int GetToTake()
	{
		if ( IsEmpty )
		{
			return 2;
		}
		else
		{
			return 1;
		}
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

	
	
}
