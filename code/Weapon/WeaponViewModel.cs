using Sandbox;
using System.Numerics;

namespace FPSGame.Weapons;

public partial class WeaponViewModel : BaseViewModel
{
	private Weapon weapon { get; init; }
	private FPSPlayer player;


	Vector3 normalOffset = Vector3.Zero;
	Vector3 currentLocalPosition = Vector3.Zero;



	public WeaponViewModel( Weapon weapon )
	{
		this.weapon = weapon;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;
		player = weapon.Owner as FPSPlayer;
	}

	//Aiming
	public void UpdateCamera()
	{
		Camera.Main.SetViewModelCamera( 80f, 1, 500 );

		if ( weapon.IsAiming )
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, weapon.aimingOffset, Time.Delta * weapon.AimSpeed );
			
		}
		else
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, normalOffset, weapon.AimSpeed );
		}

		Position = player.EyePosition + currentLocalPosition * player.EyeRotation;
		Rotation =  player.EyeRotation;

	}
	

	// Dont make that place model if you need aiming
	public override void PlaceViewmodel()
	{
		//base.PlaceViewmodel();

		
	}
	
}
