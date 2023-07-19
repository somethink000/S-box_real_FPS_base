using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace MyGame;
public class MyWorldPanel : WorldPanel
{
	public Item Item;
	public string Title { get; set; }
	public Panel BackGround;
	public Label Name;

	public MyWorldPanel()
	{
		//Log.Info("ddw");
		StyleSheet.Load( "/Entities/items/BaseWorldPanel.scss" );
		BackGround = Add.Panel( "back" );
		Name = BackGround.Add.Label( "hello world" );

		//IF YOU NEED SKALE THE REAL SIZE OF YOUR WORLD PANEL
		//PanelBounds = new Rect( -500, -100, 1000, 200 );
	}

	public override void Tick()
	{
		base.Tick();


		var player = Game.LocalPawn as Player;
		if ( player == null ) return;


		if ( Item.IsValid() )
		{
			//Log.Info( player.EyeRotation );
			Position = Item.Position + new Vector3( 0, 0, 20 );
			Rotation = Rotation.LookAt( player.EyePosition - Position ); ;
			Name.Text = $"{Title}";

		}
		else
		{
			Delete();
		}
		
		

	//	
	}

}
