

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

		HandleReloadEffects();
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


}
