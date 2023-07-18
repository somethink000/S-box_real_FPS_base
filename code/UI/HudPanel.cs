using Sandbox;
namespace MyGame;
public class HUDEntity : HudEntity<HUDRootPanel>
{
	public static HUDEntity Current;

	public HUDEntity()
	{
		Current = this;

		if ( Game.IsClient )
		{

		}
	}
}
