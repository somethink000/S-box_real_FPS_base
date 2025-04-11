using Sandbox.Citizen;
using System;
using static Sandbox.Clothing;
using static Sandbox.SerializedProperty;
using static Sandbox.Services.Inventory;

namespace GeneralGame;

public partial class InventoryController : Component
{
	[RequireComponent] public Player ply { get; set; }
	[Property] public List<Carriable> Weapons { get; set; } = new List<Carriable>( new Carriable[9] );

	public Carriable Deployed { get; set; }
	public int Slot { get; set; }

	//public bool Has( GameObject prefab )
	//{
	//	//TODO make this beter
	//	return Weapons.Any( w => w.GameObject.Components.GetInDescendantsOrSelf<Carriable>( true ).DisplayName == prefab.Components.GetInDescendantsOrSelf<Carriable>( true ).DisplayName );
	//}

	public void Clear()
	{
		if ( IsProxy ) return;

		foreach ( var weapon in Weapons )
		{
			weapon.GameObject.Destroy();
		}
	}

	private void ChangeDeployed( Carriable carriable, int slot )
	{
		if ( Deployed != null )
		{

			if ( !Deployed.CanHolster() ) return;

			ClearDeployed();
		}
		
		Deployed = carriable;
		Slot = slot;
		Deployed.Deploy( ply );
	}

	private void ClearDeployed()
	{
		Deployed.Holster();
		Deployed = null;
	}

	

	private void DeployWeapon( int index )
	{
		
		var item = Weapons[(int)index];

		if ( item == null ) return;

		if ( item.GameObject.Components.GetInDescendantsOrSelf<Carriable>( true ) != null )
		{
			Carriable nextWeapon = item.GameObject.Components.GetInDescendantsOrSelf<Carriable>( true );
			
			ChangeDeployed( nextWeapon, index );
		}

	}


	protected override void OnUpdate()
	{
		if ( IsProxy ) return;


		if ( Deployed != null )
		{
			foreach ( var animator in ply.Animators )
			{
				animator.HoldType = Deployed.HoldType;
				animator.Handedness = Deployed.Hand;
			}

			if ( ply.IsFirstPerson && Deployed.ViewModel is not null ) { 
				ply.BodyRenderer.SetBodyGroup( "chest", 1 );
				ply.BodyRenderer.SetBodyGroup( "hands", 1 );
			}
		}
		else
		{
			foreach ( var animator in ply.Animators )
			{
				animator.HoldType = CitizenAnimationHelper.HoldTypes.None;
			}
			if ( ply.IsFirstPerson && Deployed.ViewModel is not null )
			{
				ply.BodyRenderer.SetBodyGroup( "chest", 0 );
				ply.BodyRenderer.SetBodyGroup( "hands", 0 );
			}
		}


		if ( Input.Pressed( InputButtonHelper.Slot0 ) ) DeployWeapon( 0 );
		else if ( Input.Pressed( InputButtonHelper.Slot1 ) ) DeployWeapon( 1 );
		else if ( Input.Pressed( InputButtonHelper.Slot2 ) ) DeployWeapon( 2 );
		else if ( Input.Pressed( InputButtonHelper.Slot3 ) ) DeployWeapon( 3 );
		else if ( Input.Pressed( InputButtonHelper.Slot4 ) ) DeployWeapon( 4 );
		else if ( Input.Pressed( InputButtonHelper.Slot5 ) ) DeployWeapon( 5 );
		else if ( Input.Pressed( InputButtonHelper.Slot6 ) ) DeployWeapon( 6 );
		else if ( Input.Pressed( InputButtonHelper.Slot7 ) ) DeployWeapon( 7 );
		else if ( Input.Pressed( InputButtonHelper.Slot8 ) ) DeployWeapon( 8 );
		else if ( Input.Pressed( InputButtonHelper.Slot9 ) ) DeployWeapon( 9 );
		else if ( Input.MouseWheel.y > 0 ) DeployWeapon( Slot + 1 );
		else if ( Input.MouseWheel.y < 0 ) DeployWeapon( Slot - 1 );

	}

	public void GiveItem( Carriable item )
	{

		//item.GameObject.SetupNetworking();
		item.GameObject.Network.TakeOwnership();
		item.GameObject.Parent = this.GameObject;
		item.GameObject.WorldPosition = this.GameObject.WorldPosition;
		item.GameObject.WorldRotation = this.GameObject.WorldRotation;
	
	
		item.Components.Get<ModelCollider>( FindMode.InSelf ).Enabled = false;
		item.Components.Get<Rigidbody>( FindMode.InSelf ).Enabled = false;
		
		//	Components.Get<ModelCollider>( FindMode.InSelf ).Enabled = true;
		//	Components.Get<Rigidbody>( FindMode.InSelf ).Enabled = true;
		
		
		item.GameObject.Enabled = false;
		Weapons.Add( item );

	}
}
