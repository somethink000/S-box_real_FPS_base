using Sandbox.Citizen;
using System;
using static Sandbox.Clothing;
using static Sandbox.SerializedProperty;
using static Sandbox.Services.Inventory;

namespace GeneralGame;
public enum AmmoType
{
	Pistol,
	Rifle
}

public partial class InventoryController : Component
{
	
	[RequireComponent] public Player ply { get; set; }
	//TODO make all of this sync hosted
	[Property]public List<Carriable> Weapons { get; set; } = new List<Carriable>( new Carriable[5] );
	[Sync]public Carriable Deployed { get; set; }
	public int Slot { get; set; } = 0;
	

	protected override void OnStart()
	{
		base.OnStart();
		ply.InventoryController.Give( AmmoType.Pistol, 500 );
		ply.InventoryController.Give( AmmoType.Rifle, 1000 );

		foreach ( var weapon in Weapons )
		{
			if ( weapon == null ) continue; 
			weapon.Components.Get<ModelCollider>( FindMode.InSelf ).Enabled = false;
			weapon.Components.Get<Rigidbody>( FindMode.InSelf ).Enabled = false;
			SetOwner( weapon );
			weapon.GameObject.Enabled = false;
		}
		

	}

	public bool HaveFreeSpace()
		=> Weapons.IndexOf( null ) != -1;

	public void Clear()
	{
		foreach ( var weapon in Weapons )
		{
			weapon.GameObject.Destroy();
		}
	}

	private void ChangeDeployed( Carriable carriable, int slot )
	{

		if ( Deployed != null )
		{

			ClearDeployed();
		}

		Deployed = carriable;
		Slot = slot;
		SetOwner(carriable);
		Deployed.Deploy( );
	}
	
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	private void SetOwner( Carriable item )
	{
		if ( Networking.IsHost )
		{
			item.Owner = ply;
		}
	}

	private void ClearDeployed()
	{
		Deployed.Holster();
		Deployed = null;
	}

	

	private void DeployWeapon( int index )
	{

		if (index < 0)
		{
			index = Weapons.Count - 1;
		}

		if ( index > (Weapons.Count - 1) )
		{
			index = 0;
		}

		var item = Weapons.ElementAtOrDefault(index);

		if ( (Deployed != null && !Deployed.CanHolster()) || !ply.HealthController.IsAlive ) return;

		if ( item == null )
		{
			if ( Deployed != null )
			{
				ClearDeployed();
			}
			Slot = index;
			
			return;
		}

		if ( item.GameObject.Components.GetInDescendantsOrSelf<Carriable>( true ) != null )
		{
			Carriable nextWeapon = item.GameObject.Components.GetInDescendantsOrSelf<Carriable>( true );
			
			ChangeDeployed( nextWeapon, index );
		}

	}


	protected override void OnUpdate()
	{
		

		//TODO Make it in SlotChanged() 
		if ( Deployed != null )
		{

			ply.Animator.HoldType = Deployed.HoldType;
			ply.Animator.Handedness = Deployed.Hand;
		}
		else
		{
			
			ply.Animator.HoldType = CitizenAnimationHelper.HoldTypes.None;

		}

		if (IsProxy) return;

		if ( Input.Pressed( InputButtonHelper.Slot1 ) ) DeployWeapon( 0 );
		else if ( Input.Pressed( InputButtonHelper.Slot2 ) ) DeployWeapon( 1 );
		else if ( Input.Pressed( InputButtonHelper.Slot3 ) ) DeployWeapon( 2 );
		else if ( Input.Pressed( InputButtonHelper.Slot4 ) ) DeployWeapon( 3 );
		else if ( Input.Pressed( InputButtonHelper.Slot5 ) ) DeployWeapon( 4 );
		else if ( Input.MouseWheel.y > 0 ) DeployWeapon( Slot - 1 );
		else if ( Input.MouseWheel.y < 0 ) DeployWeapon( Slot + 1 );
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void GiveItem( Carriable item )
	{

		int freeSlot = Weapons.IndexOf( null );

		//check if there is a free slot and new slot les than inventory size
		if ( freeSlot >= 0 && freeSlot < Weapons.Count ) {

			if ( Networking.IsHost )
			{
				//item.GameObject.BreakFromPrefab();
				item.GameObject.Enabled = false;
				item.GameObject.Parent = this.GameObject;
				item.GameObject.WorldPosition = this.GameObject.WorldPosition;
				item.GameObject.WorldRotation = this.GameObject.WorldRotation;

				item.Components.Get<ModelCollider>( FindMode.InSelf ).Enabled = false;
				item.Components.Get<Rigidbody>( FindMode.InSelf ).Enabled = false;

				item.GameObject.Network.AssignOwnership( this.GameObject.Network.Owner );
				//item.GameObject.Network.ClearInterpolation();
				//item.GameObject.Network.Refresh();


				DeployItem( item, freeSlot );
			}

		}
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void DeployItem( Carriable item, int freeSlot )
	{

		//	Components.Get<ModelCollider>( FindMode.InSelf ).Enabled = true;
		//	Components.Get<Rigidbody>( FindMode.InSelf ).Enabled = true;


		if ( IsProxy ) return;

		Weapons[freeSlot] = item;

		if ( freeSlot == Slot )
		{

			Deployed = item;
			Slot = freeSlot;
			Deployed.Deploy( );
		}
		
	}

	

}
