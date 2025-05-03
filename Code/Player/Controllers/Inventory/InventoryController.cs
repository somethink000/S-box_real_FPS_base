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

	[Sync( SyncFlags.FromHost )] public List<Carriable> Weapons { get; set; } = new List<Carriable>( new Carriable[7] );
	[Sync( SyncFlags.FromHost )] public Carriable Deployed { get; set; }

	public int Slot { get; set; } = 0;
	

	public bool HaveFreeSpace()
		=> Weapons.IndexOf( null ) != -1;

	public void ClearWeapons()
	{
		
		ClearDeployed();
		
		for ( int i = 0; i < Weapons.Count; i++ )
		{
			var weapon = Weapons[i];

			if ( weapon == null ) continue;

			weapon.GameObject.Destroy();
			Weapons[i] = null;
		}
	}

	

	
	//should called on bradcast
	private void ClearDeployed()
	{
		
		if ( Deployed != null )
		{
			Deployed.Holster();
	
			if ( Networking.IsHost )
			{
				Deployed = null;
			}
		}

	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
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
			
			ClearDeployed();
			
			Slot = index;
			
			return;
		}

		if ( item.GameObject.Components.GetInDescendantsOrSelf<Carriable>( true ) != null )
		{
			Carriable nextWeapon = item.GameObject.Components.GetInDescendantsOrSelf<Carriable>( true );

			ClearDeployed();

			if ( Networking.IsHost )
			{
				Deployed = nextWeapon;
			}

			Slot = index;
			nextWeapon.Deploy();
		}

	}


	protected override void OnUpdate()
	{
		
		if (IsProxy) return;


		if ( Input.Pressed( InputButtonHelper.Drop ) ) ClearWeapons( );

		if ( Input.Pressed( InputButtonHelper.Slot1 ) ) DeployWeapon( 0 );
		else if ( Input.Pressed( InputButtonHelper.Slot2 ) ) DeployWeapon( 1 );
		else if ( Input.Pressed( InputButtonHelper.Slot3 ) ) DeployWeapon( 2 );
		else if ( Input.Pressed( InputButtonHelper.Slot4 ) ) DeployWeapon( 3 );
		else if ( Input.Pressed( InputButtonHelper.Slot5 ) ) DeployWeapon( 4 );
		else if ( Input.MouseWheel.y > 0 ) DeployWeapon( Slot - 1 );
		else if ( Input.MouseWheel.y < 0 ) DeployWeapon( Slot + 1 );
	}


	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void GiveItem( Carriable item )
	{

		int freeSlot = Weapons.IndexOf( null );

		//check if there is a free slot and new slot les than inventory size
		if ( freeSlot >= 0 && freeSlot < Weapons.Count ) {


			item.GameObject.Parent = this.GameObject;
			item.GameObject.WorldPosition = this.GameObject.WorldPosition;
			item.GameObject.WorldRotation = this.GameObject.WorldRotation;

			//NEVER FUCKING DISABLE ANY COMPONENT ON WEAPON OR VIEW MODEL FUCKED UP BECAUE OF S&BOX BUG
			///{ 
				ModelCollider collider = item.Components.Get<ModelCollider>( FindMode.InSelf );
				collider.Static = true;
				collider.IsTrigger = true;

				Rigidbody rigid = item.Components.Get<Rigidbody>( FindMode.InSelf );
				rigid.Gravity = false;
				rigid.MotionEnabled = false;
			//}
			item.Owner = ply;

			item.GameObject.Network.AssignOwnership( this.GameObject.Network.Owner );
			

			Weapons[freeSlot] = item;

			if ( freeSlot == Slot )
			{
				Deployed = item;
			}

			DeployItem( item, freeSlot );
		}

		
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void DeployItem( Carriable item, int freeSlot )
	{

		item.WorldModelRenderer.Tint = item.WorldModelRenderer.Tint.WithAlpha( 0 );

		if ( IsProxy ) return;

		if ( freeSlot == Slot )
		{
			Slot = freeSlot;
			item.Deploy();
		}

	}

	

}
