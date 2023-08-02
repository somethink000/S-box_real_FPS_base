using Sandbox.UI;
namespace MyGame;

public class HUDRootPanel : RootPanel
{
	public static HUDRootPanel Current;

	public HUDRootPanel()
	{

		if ( Current != null )
		{
			Current.Delete();
			Current = this;
		}
		Current = this;
		StyleSheet.Load( "/UI/Style/style.scss" );

		AddChild<Chat>();
		AddChild<Crosshair>();
		AddChild<Health>();
		AddChild<AmmoUI>();

		AddChild<VoiceList>();
		AddChild<VoiceSpeaker>();
	}
}
