using Sandbox;
using System.Numerics;

namespace MyGame;

public partial class PlayerViewModel : BaseViewModel
{
	private Carriable item { get; init; }
	private	Player player;


	Vector3 normalOffset = Vector3.Zero;
	Vector3 currentLocalPosition = Vector3.Zero;


	private Vector3 VectorPosition;
	private Vector3 VectorRotation;


	public PlayerViewModel( Carriable item )
	{
		this.item = item;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;
		player = item.Owner as Player;
	}

	//Aiming and anythink you want for move your view model
	public void UpdateCamera()
	{


		Position = player.EyePosition +  currentLocalPosition * player.EyeRotation;   
		Rotation = player.EyeRotation;

		 
		



		Camera.Main.SetViewModelCamera( 80f, 1, 500 );

		var wp = item as Gun;

		if ( wp != null && wp.IsAiming )
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, wp.aimingOffset, Time.Delta * wp.AimSpeed );
			//Screen fov
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 50f );
			//veiw model fov
			Camera.Main.SetViewModelCamera( Screen.CreateVerticalFieldOfView( 30 ), 1, 1000.0f );

		}
		else
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, normalOffset, Time.Delta * 5f );
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
			Camera.Main.SetViewModelCamera( Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView ), 1, 1000.0f );
		}



	}


	// Dont make that base.PlaceViewmodel() if you need aiming
	public override void PlaceViewmodel()
	{
		//base.PlaceViewmodel();



	}

}
