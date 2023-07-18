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

		AddChild<Crosshair>();
		AddChild<Health>();
		AddChild<AmmoUI>();
	}
}
