using Sandbox;
using System.Collections.Generic;
namespace MyGame;
public partial class InventoryComponent : SimulatedComponent, ISingletonComponent
{
	[Net, Predicted] public Entity ActiveChild { get; set; }
	[ClientInput] public Entity ActiveChildInput { get; set; }
	[Net] public List<Entity> Items { get; set; } = new();
	public static int MaxItems { get; set; } = 32;

	[Predicted] Entity PreviousActiveChild { get; set; }

	public bool AddItem( Entity item )
	{
		if ( Items.Count < MaxItems )
		{
			Items.Add( item );
			if ( item is Carriable cr1 ) cr1.OnPickup( Entity );
			ActiveChildInput = item;
			return true;
		}
		return false;
	}
	public bool DropItem( Entity item )
	{
		if ( item is Carriable cr1 )
		{
			cr1.OnDrop( Entity );
		}
		if ( ActiveChild == item )
		{
			ActiveChild = null;
		}
		if ( ActiveChildInput == item )
		{
			ActiveChildInput = null;
		}
		Items.Remove( item );
		return true;
	}
	/// <summary>
	/// Get the item in this slot
	/// </summary>
	public virtual Entity GetSlot( int i )
	{
		if ( Items.Count <= i ) return null;
		if ( i < 0 ) return null;

		return Items[i];
	}
	/// <summary>
	/// Set our active entity to the entity on this slot
	/// </summary>
	public virtual bool SetActiveSlot( int i, bool evenIfEmpty = false )
	{
		var ent = GetSlot( i );
		if ( ActiveChild == ent )
			return false;

		if ( !evenIfEmpty && ent == null )
			return false;

		ActiveChild = ent;
		ActiveChildInput = ent;
		return ent.IsValid();
	}
	/// <summary>
	/// Returns the index of the currently active child
	/// </summary>
	public virtual int GetActiveSlot()
	{
		var ae = ActiveChild;
		var count = Items.Count;

		for ( int i = 0; i < count; i++ )
		{
			if ( Items[i] == ae )
				return i;
		}

		return -1;
	}
	/// <summary>
	/// Switch to the slot next to the slot we have active.
	/// </summary>
	public virtual bool SwitchActiveSlot( int idelta, bool loop )
	{
		var count = Items.Count;
		if ( count == 0 ) return false;

		var slot = GetActiveSlot();
		var nextSlot = slot + idelta;

		if ( loop )
		{
			while ( nextSlot < 0 ) nextSlot += count;
			while ( nextSlot >= count ) nextSlot -= count;
		}
		else
		{
			if ( nextSlot < 0 ) return false;
			if ( nextSlot >= count ) return false;
		}

		return SetActiveSlot( nextSlot, false );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// drop weapons

		if ( Input.Pressed( "Drop" ) && ActiveChild != null )
		{
			var item = ActiveChild;
			DropItem( item );
			item.Velocity = Entity.AimRay.Forward * 200;
			item.Position = Entity.AimRay.Position + Entity.AimRay.Forward * 48;
			item.EnableDrawing = true;
		}

		if ( ActiveChildInput.IsValid() && ActiveChildInput.Owner == Entity )
		{
			ActiveChild = ActiveChildInput;
		}
		else
		{
			ActiveChildInput = null;
			if ( Game.IsServer ) ActiveChild = null;
		}

		// Check to see if we've changed weapons
		if ( ActiveChild != PreviousActiveChild )
		{
			if ( PreviousActiveChild is Carriable cr1 ) cr1.OnActiveEnd();
			PreviousActiveChild = ActiveChild;
			if ( ActiveChild is Carriable cr2 ) cr2.OnActiveStart();
		}

		ActiveChild?.Simulate( cl );
	}
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		ActiveChild?.FrameSimulate( cl );
	}
	public override void BuildInput()
	{
		base.BuildInput();
		ActiveChild?.BuildInput();
	}
	public virtual void SetActiveChild( int index )
	{

	}
	public virtual void SetActiveChild( Entity entity )
	{
	}

	[ConCmd.Client]
	public static void ConCmdSetActiveChild( int i )
	{
		if ( ConsoleSystem.Caller.Pawn is Player ply )
		{
		}
	}
}
