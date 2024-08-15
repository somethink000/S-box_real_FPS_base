
using static Sandbox.Clothing;

namespace GeneralGame;

public partial class Inventory : BaseInventory
{
	[Property] public PlayerObject Player { get; set; }
	public override int MAX_SLOTS { get; set; } = 10;

	public int MAX_WEIGHT_IN_GRAMS = 30000;

	public int Weight { get; private set; }
	
	public IReadOnlyList<ItemComponent> EquippedItems => _equippedItems;

	
	private readonly List<ItemComponent> _equippedItems;
	
	
	public Inventory()
	{
		_equippedItems = new List<ItemComponent>( new ItemComponent[Enum.GetNames( typeof( EquipSlot ) ).Length] );
	}
	
	/// <summary>
	/// Returns the index of an item regardless of whether it is equipped or in the backpack.
	/// </summary>
	public override int IndexOf( ItemComponent item )
		=> (item is ItemEquipment equipment && equipment.Equipped ? _equippedItems : _backpackItems).IndexOf( item );

	public ItemComponent GetItemInSlot( EquipSlot slot ) => _equippedItems.ElementAtOrDefault( (int)slot );
	public bool IsSlotOccupied( EquipSlot slot ) => GetItemInSlot( slot ) is not null;


	protected override void OnUpdate()
	{
		UpdateWeaponSlot();
	}

	

	public EquipSlot SearchEquipSlot( List<EquipSlot> slots )
	{
		foreach( EquipSlot slot in slots )
		{
			//Log.Info( IsSlotOccupied( slot ) );
			if(!IsSlotOccupied( slot ) ) {  return slot; }
		}

		return slots.First();
	}

	public EquipSlot CurrentEquipSlot( ItemEquipment equipment )
	{
		
		foreach ( EquipSlot slot in equipment.Slots )
		{
			if ( _equippedItems[(int)slot] == equipment ) return slot;
		}

		return SearchEquipSlot( equipment.Slots );
	}


	/// <summary>
	/// The item is equipped from the backpack and swaps out any previously equipped item.
	/// </summary>
	public bool EquipItemFromBackpack( ItemComponent item )
	{
		var index = _backpackItems.IndexOf( item );
		if ( index == -1 )
			return false;

		if ( item is not ItemEquipment equipment )
			return false;
		RemoveBackpackItem( index );

		
		var slotIndex = SearchEquipSlot( equipment.Slots );
		var previouslyEquippedItem = _equippedItems[(int)slotIndex];
		
		if ( previouslyEquippedItem is not null )
		{
			
			RemoveEquipmentItem( previouslyEquippedItem as ItemEquipment );
			GiveBackpackItem( previouslyEquippedItem, index );
			previouslyEquippedItem.State = ItemState.Backpack;
		}

		GiveEquipmentItem( equipment, slotIndex );

		return true;
	}

	public bool EquipItemFromWorld( ItemComponent item, bool forceReplace = false )
	{
		if ( item is not ItemEquipment equipment )
			return false;

		if ( IsSlotOccupied( SearchEquipSlot( equipment.Slots ) ) && !forceReplace )
			return false;

		if ( IsSlotOccupied( SearchEquipSlot( equipment.Slots ) ) && forceReplace )
		{
			var equippedItem = GetItemInSlot( SearchEquipSlot( equipment.Slots ) );
			var placedInBackpack = UnequipItem( equippedItem );
			if ( !placedInBackpack )
				DropItem( equippedItem );
		}

		SetOwner( item );
		GiveEquipmentItem( equipment, null );
		//equipment.State = ItemState.Equipped;

		return true;
	}

	/// <summary>
	/// The item is unequipped and placed into the backpack if there are free slots. 
	/// </summary>
	public bool UnequipItem( ItemComponent item )
	{
		if ( item is not ItemEquipment equipment || !equipment.Equipped )
			return false;

		var firstFreeSlot = _backpackItems.IndexOf( null );
		if ( firstFreeSlot == -1 )
			return false;

		RemoveEquipmentItem( equipment );
		GiveBackpackItem( equipment, firstFreeSlot );
		equipment.State = ItemState.Backpack;

		return true;
	}
	
	/// <summary>
	/// The item is removed completely from the inventory system.
	/// </summary>
	public bool DropItem( ItemComponent item )
	{
		

		if ( item is ItemEquipment equipment && equipment.Equipped )
			RemoveEquipmentItem( equipment, true );
		else
			RemoveBackpackItem( _backpackItems.IndexOf( item ) );

		item.State = ItemState.None;

		item.GameObject.Parent = null;
		var view = Player.Camera.Transform;
		var trace = Scene.Trace.FromTo( view.Position + view.Rotation.Forward, view.Position + view.Rotation.Forward * 20f )
				.IgnoreGameObject( Player.GameObject )
				.IgnoreGameObject( item.GameObject )
				.Radius( 1.0f )
				.Run();

		item.GameObject.Transform.Rotation = Rotation.Identity;
		item.GameObject.Transform.Position = trace.EndPosition;

		var velocity = Player.CC.Velocity + view.Rotation.Forward * 150f;
		if ( item.GameObject.Components.TryGet<Rigidbody>( out var rigidbody, FindMode.EverythingInSelf ) )
		{
			rigidbody.Velocity = velocity;
			rigidbody.MotionEnabled = true;
		}
		else if ( item.GameObject.Components.TryGet<ModelPhysics>( out var modelPhysics, FindMode.EverythingInSelf ) )
		{
			item.GameObject.Enabled = false;
			item.GameObject.Transform.Position = trace.EndPosition;
			item.GameObject.Enabled = true;
			modelPhysics.PhysicsGroup?.AddVelocity( velocity ); // todo: LOL WHY??
		}

		return true;
	}

	/// <summary>
	/// A swap is performed from the backpack to the equipment slots.
	/// </summary>
	public bool SwapItems( int index, EquipSlot slot )
	{
		
		var item = _backpackItems.ElementAtOrDefault( index );
		if ( item is null )
			return false;

		if ( item is not ItemEquipment equipment || !equipment.Slots.Contains( slot ) )
			return false;

		RemoveBackpackItem( index );

		var previouslyEquippedItem = _equippedItems[(int)slot];
		if ( previouslyEquippedItem is not null )
		{
			RemoveEquipmentItem( previouslyEquippedItem as ItemEquipment );
			GiveBackpackItem( previouslyEquippedItem, index );
			previouslyEquippedItem.State = ItemState.Backpack;
		}

		GiveEquipmentItem( equipment, slot );
		
		
		return true;
	}

	/// <summary>
	/// A swap is performed from a backpack slot to another backpack slot.
	/// </summary>
	public bool SwapItems( EquipSlot slot, EquipSlot slotTo )
	{
		var item = _equippedItems[(int)slot];
		if ( item is null )
			return false;

		if ( item is not ItemEquipment equipment || !equipment.Slots.Contains( slot ) )
			return false;

		var previouslyEquippedItem = _equippedItems[(int)slotTo];

		RemoveEquipmentItem( equipment );
		equipment.State = ItemState.None;

		if ( previouslyEquippedItem is not null )
		{
			
			RemoveEquipmentItem( previouslyEquippedItem as ItemEquipment );
			GiveEquipmentItem( previouslyEquippedItem as ItemEquipment, slot );
			//previouslyEquippedItem.State = ItemState.Equipped;
		}

		GiveEquipmentItem( equipment, slotTo );
		//equipment.State = ItemState.Equipped;

		
		return true;
	}


	/// <summary>
	/// Bypasses any restrictions and clears the item. Do not use this for regular inventory usage.
	/// </summary>
	public override void ClearItem( ItemComponent item )
	{
		base.ClearItem(item);
		if ( _equippedItems.Contains( item ) )
		{
			_equippedItems[_equippedItems.IndexOf( item )] = null;
			item.State = ItemState.None;
			return;
		}
	}


	/// <summary>
	/// The item is given to the backpack.
	/// </summary>
	protected override void GiveBackpackItem( ItemComponent item, int index )
	{
		base.GiveBackpackItem( item , index );
		Weight = GetTotalWeightInGrams();
		
	}

	/// <summary>
	/// The item is removed from the backpack.
	/// </summary>
	protected override void RemoveBackpackItem( int index )
	{
		base.RemoveBackpackItem( index );
		Weight = GetTotalWeightInGrams();
	}




	/// <summary>
	/// A swap is performed from the equipment slot to the backpack.
	/// </summary>
	public bool SwapItems( EquipSlot slot, int index )
	{
		
		var item = _equippedItems[(int)slot];
		
		if ( item is null )
			return false;
		
		var previousBackpackItem = _backpackItems[index];
		if ( previousBackpackItem is not null && (previousBackpackItem is not ItemEquipment itemToEquip || itemToEquip.Slots.Contains( slot )) )
			return false;

		RemoveEquipmentItem( item as ItemEquipment );
		
		if ( previousBackpackItem is not null )
		{
			RemoveBackpackItem( index );
			GiveEquipmentItem( previousBackpackItem as ItemEquipment, null );
			//previousBackpackItem.State = ItemState.Equipped;
		}

		GiveBackpackItem( item, index );
		item.State = ItemState.Backpack;

		return true;
	}

	
	public int GetTotalWeightInGrams()
	{

		return _backpackItems.Sum( i => i?.totalWeight ?? 0 ) + _equippedItems.Sum( i => i?.totalWeight ?? 0 );
	}

	/// <summary>
	/// The item is equipped.
	/// </summary>
	private void GiveEquipmentItem( ItemEquipment equipment, EquipSlot? slot )
	{
		
		slot ??= SearchEquipSlot( equipment.Slots );
		
		_equippedItems[(int)slot] = equipment;
		

		Weight = GetTotalWeightInGrams();


		// Can be fucked up
		equipment.State = ItemState.Equipped;

		AddEquipUpdate( slot.Value );

	}
	
	/// <summary>
	/// The item is unequipped.
	/// </summary>
	private void RemoveEquipmentItem( ItemEquipment equipment, bool onDrop = false )
	{
		EquipSlot curslot = CurrentEquipSlot( equipment );

		RemoveEquipUpdate( curslot, onDrop );

		_equippedItems[(int)curslot] = null;

		Weight = GetTotalWeightInGrams();
		
	}


	protected override void SetOwner( ItemComponent item )
	{
		item.GameObject.SetupNetworking();
		item.GameObject.Network.TakeOwnership();
		item.GameObject.Parent = Player.GameObject;
		item.GameObject.Transform.Position = Player.GameObject.Transform.Position;
		item.GameObject.Transform.Rotation = Player.GameObject.Transform.Rotation;
		item.LastOwner = Player;

		Player.AnimationHelper.Target.Set( "b_pickup", true );
	}
}
