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

	//Aiming and anythink you want for move your view model
	public void UpdateCamera()
	{
		var wp = weapon as Gun;
		
		Camera.Main.SetViewModelCamera( 80f, 1, 500 );

		if ( wp != null && wp.IsAiming )
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, wp.aimingOffset, Time.Delta * wp.AimSpeed );
			
		}
		else
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, normalOffset, Time.Delta * wp.AimSpeed );
		}

		Position = player.EyePosition + currentLocalPosition * player.EyeRotation;
		Rotation =  player.EyeRotation;

	}


	// Dont make that base.PlaceViewmodel() if you need aiming
	public override void PlaceViewmodel()
	{
		//base.PlaceViewmodel();
		UpdateCamera();


	}

}
