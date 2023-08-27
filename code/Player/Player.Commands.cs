using Sandbox;
using System.Linq;

namespace MyGame;
public partial class Player
{
	[ConCmd.Admin( "noclip" )]
	static void DoPlayerNoclip()
	{
		if ( ConsoleSystem.Caller.Pawn is Player basePlayer )
		{
			if ( basePlayer.MovementController is NoclipController )
			{
				basePlayer.Components.Add( new WalkController() );
			}
			else
			{
				basePlayer.Components.Add( new NoclipController() );
			}
		}
	}

	[ConCmd.Admin( "kill" )]
	static void DoPlayerSuicide()
	{
		if ( ConsoleSystem.Caller.Pawn is Player basePlayer )
		{
			basePlayer.TakeDamage( new DamageInfo { Damage = basePlayer.Health * 99 } );
		}
	}
	[ConCmd.Admin( "respawn" )]
	static void DoPlayerRespawn()
	{
		if ( ConsoleSystem.Caller.Pawn is Player basePlayer )
		{
			( basePlayer.Inventory.ActiveChild as Carriable ).DestroyViewModel();
			basePlayer.Inventory.Items.Clear();
			basePlayer.Respawn();
		}
	}
}
