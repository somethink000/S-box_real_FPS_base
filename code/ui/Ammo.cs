using FPSGame;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Ammo : Panel
{

	public Label Weapon;
	public Label PlayerAmmo;

	public Ammo()
	{
		
		Weapon = Add.Label( "100", "weapon" );
		PlayerAmmo = Add.Label( "100", "plyaerammo" );


	}

	public override void Tick()
	{

		var player = Game.LocalPawn as FPSPlayer;
		if ( player == null ) return;

		var weapon = player.ActiveChild as Weapon;
		var isValid = (weapon != null);

		SetClass( "active", isValid );


		if ( !isValid ) return;



		Weapon.Text = $"{weapon.InMagazin}";


		var inv = weapon.AvailableAmmo();
		PlayerAmmo.Text = $" / {inv}";
		PlayerAmmo.SetClass( "active", inv >= 0 );


	}
}
