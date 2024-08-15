using static Sandbox.Clothing;

namespace GeneralGame;

public enum ItemState
{
	None,
	Backpack,
	Equipped
}

public class ItemComponent : Component
{
	/// <summary>
	/// The name of the item.
	/// </summary>
	[Sync]
	[Property]
	public string Name { get; set; }

	/// <summary>
	/// The icon to display.
	/// </summary>
	[Property] public IconSettings Icon { get; set; }

	/// <summary>
	/// The description of the item.
	/// </summary>
	[Property] public string Description { get; set; }

	/// <summary>
	/// The weight (in grams) of the item.
	/// </summary>
	[Property] public int WeightInGrams { get; set; }

	/// <summary>
	/// Maximum amount of items in this stack, default is 0 for not stackable.
	/// </summary>
	[Property]
	public int MaxStack
	{
		get => _maxStack;
		set
		{
			_maxStack = value;
			/*Count = value;*/
		}
	}

	private int _maxStack;

	[Property, Sync, HideIf( "MaxStack", 0 )] public int Count { get; set; }
	[Sync] public string Prefab { get; private set; }

	public Texture IconTextureEquip => Texture.Load( FileSystem.Mounted, Icon.EquipPath );

	public Texture IconTexture => Texture.Load( FileSystem.Mounted, Icon.Path );
	public static implicit operator ItemComponent( GameObject obj )
		=> obj.Components.Get<ItemComponent>();

	/// <summary>
	/// If the item is in the player's inventory (this includes backpack and equipped items).
	/// </summary>
	public bool InInventory
	{
		get => State != ItemState.None;
	}

	/// <summary>
	/// Whether the item can be sold.
	/// </summary>
	public bool IsStackable => MaxStack >= 1;

	public int totalWeight => IsStackable ? WeightInGrams * Count : WeightInGrams;
	/// <summary>
	/// The last player that had this item parented to them.
	/// </summary>
	public PlayerObject LastOwner { get; set; }

	private readonly SoundEvent _pickupSound = ResourceLibrary.Get<SoundEvent>( "sounds/misc/pickup.sound" );

	private ItemState _state;

	/// <summary>
	/// If the item is in the player's backpack (note not equipped!).
	/// </summary>
	[Sync]
	public ItemState State
	{
		get => _state;
		set
		{
			_state = value;
			UpdateState();
		}
	}

	private void UpdateState()
	{
		GameObject.Enabled = State == ItemState.None;
		
		
		if ( this is ItemEquipment equipment )
			equipment.UpdateEquipped();
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Prefab = GameObject.PrefabInstanceSource;
	}

	protected override void OnStart()
	{
		GameObject.SetupNetworking();
		
		// Pickup
		var interactions = Components.GetOrCreate<Interactions>();
		interactions.AddInteraction( new Interaction()
		{
			Action = ( PlayerObject interactor, GameObject obj ) => interactor.Inventory.GiveItem( this ),
			Keybind = "use",
			Description = "Pickup",
			Disabled = () => !PlayerObject.GetLocal().Inventory.HasSpaceInBackpack(),
			ShowWhenDisabled = () => true,
			Accessibility = AccessibleFrom.World,
			Sound = () => _pickupSound,
		} );
	}

	protected override void OnDestroy()
	{
		if ( IsProxy || !Game.IsPlaying )
			return;

        PlayerObject.GetLocal()?.Inventory?.ClearItem( this );
	}
}
