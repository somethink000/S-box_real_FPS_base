using Sandbox;
using System.Numerics;

namespace MyGame;

public partial class PlayerViewModel : BaseViewModel
{
	private Carriable item { get; init; }
	private	Player player;


	Vector3 normalOffset = Vector3.Zero;
	Vector3 currentLocalPosition = Vector3.Zero;
	private Vector3 finalVectorPos;


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

		Position = player.EyePosition + currentLocalPosition * player.EyeRotation;//Vector3.Lerp( Position, finalVectorPos, 2000 * RealTime.Delta );
		Rotation = player.EyeRotation;


		//	finalVectorPos = 

		Camera.Main.SetViewModelCamera( 80f, 1, 500 );

		var wp = item as Gun;

		if ( wp != null && wp.IsAiming )
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, wp.aimingOffset, Time.Delta * wp.AimSpeed );

		}
		else
		{
			currentLocalPosition = Vector3.Lerp( currentLocalPosition, normalOffset, Time.Delta * 5f );
		}



	}


	// Dont make that base.PlaceViewmodel() if you need aiming
	public override void PlaceViewmodel()
	{
		//base.PlaceViewmodel();



	}

}
