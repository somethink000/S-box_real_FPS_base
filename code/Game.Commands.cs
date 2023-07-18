using Sandbox;
using System.Linq;

namespace MyGame;
public partial class MyGame
{


	[ConCmd.Server( "ent_create" )]
	public static void SpawnEntity( string entName )
	{
		Log.Info( "creating " + entName );
		var owner = ConsoleSystem.Caller.Pawn as Player;

		if ( owner == null )
		{
			Log.Info( "Failed to create " + entName );
			return;
		}

		var entityType = TypeLibrary.GetType<Entity>( entName )?.TargetType;
		if ( entityType == null )
		{
			Log.Info( "Failed to create " + entName );
			return;
		}

		var tr = Trace.Ray( owner.AimRay, 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		var ent = TypeLibrary.Create<Entity>( entityType );

		ent.Position = tr.EndPosition;
		ent.Rotation = Rotation.From( new Angles( 0, owner.AimRay.Forward.EulerAngles.yaw, 0 ) );

		//Log.Info( $"ent: {ent}" );
	}

	[ConCmd.Server( "give" )]
	public static void GiveWeapon( string entName )
	{
		if ( ConsoleSystem.Caller.Pawn is Player player )
		{
			if ( player.Inventory.AddItem( TypeLibrary.Create<Entity>( entName ) ) )
			{
				Log.Info( $"Giving {entName} to {player.Client.Name}" );
				return;
			}
			Log.Info( $"Failed to give {entName}. Not a valid entity." );
		}
	}

	[ConCmd.Server( "reset_game" )]
	public static void ResetGame()
	{
		/*
		// If we don't have permission, exit.
		if ( !ConsoleSystem.Caller.HasPermission( "admin" ) )
		{
			Log.Info( "No permission: reset_game" );
			return;
		}
		*/

		// Tell our game that all clients have just left.
		foreach ( IClient cl in Game.Clients )
		{
			MyGame.Current.ClientDisconnect( cl, NetworkDisconnectionReason.DISCONNECT_BY_USER );
		}

		// Cleanup on clients
		CleanupClientEntities( To.Everyone );

		// Delete everything except the clients and the world 
		foreach ( Entity ent in Entity.All )
		{
			if ( ent is not IClient &&
				ent is not WorldEntity )
				ent.Delete();
		}

		// Reset the map, this will respawn all map entities
		Game.ResetMap( Entity.All.Where( x => x is Player ).ToArray() );

		// Create a brand new game
		MyGame.Current = new MyGame();

		// Fake a post level load after respawning entities, just incase something uses it
		MyGame.Current.PostLevelLoaded();

		// Tell our new game that all clients have just joined to set them all back up.
		foreach ( IClient cl in Game.Clients )
		{
			MyGame.Current.ClientJoined( cl );
		}
	}
	[ClientRpc]
	public static void CleanupClientEntities()
	{
		foreach ( Entity ent in Entity.All )
		{
			if ( ent.IsClientOnly )
				ent.Delete();
		}
	}
}
