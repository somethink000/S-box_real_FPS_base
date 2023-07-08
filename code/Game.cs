using Sandbox;
using System.Linq;
using System.Threading.Tasks;

partial class FPSGame : GameManager
{
	public FPSGame()
	{
		if ( Game.IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );
		var player = new FPSPlayer( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	
	
	[ClientRpc]
	public override void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
		KillFeed.Current?.AddEntry( leftid, left, rightid, right, method );
	}

	
}
