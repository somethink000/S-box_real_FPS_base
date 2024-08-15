

using Sandbox.Citizen;

namespace GeneralGame;

public partial class Inventory
{
	[Property] public GameObject WeaponBone { get; set; }

	[Sync] public WeaponComponent Deployed { get; private set; }

	private bool toolgunActive = false;
	public EquipSlot CurrentWeaponSlot { get; set; } = EquipSlot.FirstWeapon;

	public void DeployCurrent()
	{
		
		var item = _equippedItems[(int)CurrentWeaponSlot];

		if (item == null) return;

		if (item.GameObject.Components.GetInDescendantsOrSelf<WeaponComponent>(true) != null)
		{
			WeaponComponent nextWeapon = item.GameObject.Components.GetInDescendantsOrSelf<WeaponComponent>(true);
			
			
			Deployed = nextWeapon;
			
			nextWeapon.Deploy( );
		}

	}

	[Broadcast]
	public void UpdateWeaponSlot()
	{
		
		if ( Deployed != null )
		{
			Player.AnimationHelper.HoldType = Deployed.HoldType;
		} else
		{

			Player.AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		}

		if ( IsProxy ) return;

		//if ( activeItem is null || !activeItem.CanCarryStop() ) return;
		if ( Input.Pressed( InputButtonHelper.Slot1 ) ) Next();
		else if ( Input.Pressed( InputButtonHelper.Slot2 ) ) Next();
		else if ( Input.MouseWheel.y > 0 ) Next();
		else if ( Input.MouseWheel.y < 0 ) Next();


	}

	public void RemoveEquipUpdate( EquipSlot slot, bool drop = false)
	{
		if ( IsProxy ) return;
		if ( CurrentWeaponSlot == slot ) { 
			Deployed.Holster();
			Deployed = null;
		}
		
	}
	public void AddEquipUpdate( EquipSlot slot )
	{
		if ( IsProxy ) return;
		if ( CurrentWeaponSlot == slot ) DeployCurrent();
	}

	
	public void Next()
	{
		if ( IsProxy ) return;

		Deployed?.Holster();
		Deployed = null;


		if ( CurrentWeaponSlot == EquipSlot.FirstWeapon )
		{
			CurrentWeaponSlot = EquipSlot.SeccondWeapon;
		}
		else
		{
			CurrentWeaponSlot = EquipSlot.FirstWeapon;
		}

		DeployCurrent();

	}

}
