using Sandbox;

namespace MyGame;
public partial class Throwable : Carriable
{
	[ConVar.Replicated] public static bool sv_infinite_grenade { get; set; } = false;
	public TimeSince TimeSinceClicked;
	public override void Simulate( IClient cl )
	{
		if ( Owner is not Player ) return;
		if ( PullPin() )
		{
			TimeSinceClicked = 0;
		}
		if ( WillThrow() )
		{
			using ( LagCompensation() )
			{
				Throw();
			}
			if ( Game.IsServer && !sv_infinite_grenade )
			{
				if ( Owner is Player ply )
				{
					ply.Inventory?.Items.Remove( this );
				}
				Delete();
			}
		}
	}
	public virtual bool PullPin()
	{
		return Input.Pressed( "Attack1" );
	}
	public virtual bool WillThrow()
	{
		return Input.Released( "Attack1" );
	}
	public virtual void Throw()
	{

	}
}
