

namespace GeneralGame;

public partial class BaseInventory : Component
{
	public virtual int MAX_SLOTS { get; set; } = 9;
	public IReadOnlyList<ItemComponent> BackpackItems => _backpackItems;
	protected readonly List<ItemComponent> _backpackItems;

	/// <summary>
	/// Returns the index of an item regardless of whether it is equipped or in the backpack.
	/// </summary>
	public virtual int IndexOf( ItemComponent item )
		=> _backpackItems.IndexOf( item );

	/// <summary>
	/// Returns true if the backpack has any free slots.
	/// </summary>
	/// <returns></returns>
	public bool HasSpaceInBackpack()
		=> _backpackItems.IndexOf( null ) != -1;


	protected static bool CanStack( ItemComponent first, ItemComponent second )
		=> first.Prefab == second.Prefab
		&& first.IsStackable && second.IsStackable
		&& second.Count < second.MaxStack;



	public BaseInventory()
	{
		_backpackItems = new List<ItemComponent>( new ItemComponent[MAX_SLOTS] );
	}


	/// <summary>
	/// Item is given to the inventory system if they have free slots.
	/// </summary>
	/// 
	public virtual bool GiveItem( ItemComponent item )
	{

		int firstFreeSlot = _backpackItems.IndexOf( null );


		if ( item.IsStackable )
		{
			foreach ( ItemComponent itemTo in _backpackItems )
			{
				if ( itemTo != null && CanStack( item, itemTo ) )
				{

					var amount = Math.Min( Math.Abs( itemTo.Count - itemTo.MaxStack ), item.Count );

					// can make there validation
					item.Count -= amount;

					itemTo.Count += amount;

					if ( item.Count <= 0 )
					{
						item.GameObject.Destroy();
						return true;
					}
				}
			}
		}

		if ( firstFreeSlot == -1 )
			return false;


		SetOwner( item );
		GiveBackpackItem( item, firstFreeSlot );
		item.State = ItemState.Backpack;

		return true;
	}

	/// <summary>
	/// The item is created from the prefab file and given to the inventory system if they have free slots.
	/// </summary>
	public bool GiveItem( PrefabFile prefabFile )
	{

		var obj = SceneUtility.GetPrefabScene( prefabFile ).Clone();
		obj.NetworkMode = NetworkMode.Object;
		obj.NetworkSpawn();

		var res = GiveItem( obj.Components.Get<ItemComponent>() );
		if ( !res )
			obj.Destroy();

		return res;
	}

	/// <summary>
	/// A swap is performed from a backpack slot to another backpack slot.
	/// </summary>
	public bool SwapItems( int firstIndex, int secondIndex )
	{

		var firstItem = _backpackItems.ElementAtOrDefault( firstIndex );
		if ( firstItem is null )
			return false;

		RemoveBackpackItem( firstIndex );

		var secondItem = _backpackItems.ElementAtOrDefault( secondIndex );
		var invert = false;

		if ( secondItem is not null )
		{
			RemoveBackpackItem( secondIndex );


			// Stacking
			if ( CanStack( firstItem, secondItem ) )
			{

				var from = firstItem;
				var to = secondItem;
				invert = true;

				var amount = Math.Min( Math.Abs( to.Count - to.MaxStack ), from.Count );
				RemoveAmount( from, amount );
				to.Count += amount;

				if ( from == null || from.Count <= 0 )
				{
					GiveBackpackItem( to, secondIndex );
					return true;
				}
			}
			else if ( CanStack( secondItem, firstItem ) )
			{
				var from = secondItem;
				var to = firstItem;
				invert = true;

				var amount = Math.Min( Math.Abs( to.Count - to.MaxStack ), from.Count );
				RemoveAmount( from, amount );
				to.Count += amount;

				if ( from == null || from.Count <= 0 )
				{
					GiveBackpackItem( to, secondIndex );
					return true;
				}
			}

			GiveBackpackItem( secondItem, invert ? secondIndex : firstIndex );
		}

		GiveBackpackItem( firstItem, invert ? firstIndex : secondIndex );

		return true;
	}


	/// <summary>
	/// Bypasses any restrictions and sets the item at the given index. Do not use this for regular inventory usage.
	/// </summary>
	public virtual void SetItem( ItemComponent item, int index )
	{
		SetOwner( item );
		GiveBackpackItem( item, index );
		item.State = ItemState.Backpack;
	}

	public bool RemoveAmountEasy( string name, int count = 1, bool destroy = true )
	{
		foreach ( var item in _backpackItems )
			if ( item.Name.ToLower().Replace( " ", "" ) == name.ToLower().Replace( " ", "" ) )
				return RemoveAmount( item, count, destroy );

		return false;
	}

	/// <summary>
	/// Removes a specific amount from an item if the item is stackable and has more than the amount.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="count"></param>
	/// <param name="destroy"></param>
	/// <param name="predicate"></param>
	/// <returns></returns>
	public virtual bool RemoveAmount( ItemComponent item, int count = 1, bool destroy = true, Func<ItemComponent, bool> predicate = null )
	{
		// Non stackables.
		if ( !item.IsStackable )
		{
			var items = BackpackItems
				.Where( x => x != item && x?.Prefab == item?.Prefab && (predicate?.Invoke( x ) ?? true) )
				.ToList();

			if ( count > items.Count + 1 )
				return false;

			ClearItem( item );
			if ( destroy )
			{
				item.State = ItemState.None;
				item?.GameObject?.Destroy();
			}

			for ( int i = 0; i < count - 1; i++ )
			{
				var target = items[i];
				ClearItem( target );
				if ( destroy )
				{
					target.State = ItemState.None;
					target?.GameObject?.Destroy();
				}
			}

			return true;
		}

		// Stackables.
		if ( count > item.Count )
			return false;

		item.Count -= count;

		if ( item.Count <= 0 )
		{
			ClearItem( item );
			if ( destroy )
			{
				item.State = ItemState.None;
				item?.GameObject?.Destroy();
			}
		}

		//Weight = GetTotalWeightInGrams();

		return true;
	}

	public int TryTake( PrefabFile prefab, int amount )
	{

		var all = allItems( prefab );
		if ( all == null ) return 0;

		var taken = 0;
		foreach ( int id in all )
		{
			ItemComponent item = _backpackItems[id];
			var count = item.Count;
			if ( count <= 0 )
			{
				taken = 0;
				ClearItem( item );
				item.State = ItemState.None;
				item?.GameObject?.Destroy();
			}

			if ( count >= amount )
			{
				taken = amount;
				item.Count -= taken;
				if ( item.Count <= 0 )
				{
					ClearItem( item );
					item.State = ItemState.None;
					item?.GameObject?.Destroy();
				}
			}
			else
			{
				taken += count;
				ClearItem( item );
				item.State = ItemState.None;
				item?.GameObject?.Destroy();
			}

			if ( taken >= amount ) return taken;
		}

		return taken;
	}

	public int allItemsCount( PrefabFile prefab )
	{
		int count = 0;
		var allItems = BackpackItems.Where( x => x?.Prefab == prefab?.ResourcePath );
		if ( allItems == null ) return count;

		foreach ( ItemComponent item in allItems )
		{
			count += item.Count;
		}

		return count;
	}

	public bool HasItems( PrefabFile prefab )
	{

		var Item = BackpackItems.FirstOrDefault( x => x?.Prefab == prefab?.ResourcePath );
		if ( Item == null || Item.Count <= 0 ) return false;

		return true;
	}

	public List<int> allItems( PrefabFile prefab )
	{
		var allItems = BackpackItems.Where( x => x?.Prefab == prefab?.ResourcePath );
		new List<ItemComponent>( allItems );
		if ( allItems == null ) return null;

		List<int> items = new List<int>();
		foreach ( var item in allItems )
		{
			items.Add( IndexOf( item ) );
		}

		return items;
	}



	/// <summary>
	/// Bypasses any restrictions and clears the item. Do not use this for regular inventory usage.
	/// </summary>
	public virtual void ClearItem( ItemComponent item )
	{
		if ( _backpackItems.Contains( item ) )
		{
			_backpackItems[_backpackItems.IndexOf( item )] = null;
			item.State = ItemState.None;
			return;
		}
		
	}

	protected virtual void SetOwner( ItemComponent item )
	{
		item.GameObject.SetupNetworking();
		item.GameObject.Network.TakeOwnership();
		item.GameObject.Parent = this.GameObject;
		item.GameObject.Transform.Position = this.GameObject.Transform.Position;
		item.GameObject.Transform.Rotation = this.GameObject.Transform.Rotation;
		item.LastOwner = null;
	}


	/// <summary>
	/// The item is given to the backpack.
	/// </summary>
	protected virtual void GiveBackpackItem( ItemComponent item, int index )
	{

		if ( index >= 0 && index < _backpackItems.Count )
			_backpackItems[index] = item;

		/*Weight = GetTotalWeightInGrams();*/
	}

	/// <summary>
	/// The item is removed from the backpack.
	/// </summary>
	protected virtual void RemoveBackpackItem( int index )
	{

		if ( index >= 0 && index < _backpackItems.Count )
			_backpackItems[index] = null;

		/*Weight = GetTotalWeightInGrams();*/
	}

	public int GetTotalItemCount( string name )
	{
		if ( BackpackItems == null ) return 0;
		return BackpackItems.Where( x => x.IsValid() && x.Name.ToLower() == name.ToLower() )?.Count() ?? 0;
	}

	public int GetTotalItemCountWithTag( string tag )
	{
		if ( BackpackItems == null ) return 0;
		return BackpackItems.Where( x => x.IsValid() && x.Tags.Has( tag ) )?.Count() ?? 0;
	}

	public bool HasItem( string name )
	{
		return BackpackItems.Any( x => x.Name == name );
	}

}

