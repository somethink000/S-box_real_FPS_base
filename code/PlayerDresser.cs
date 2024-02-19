using Sandbox;

namespace Facepunch.Arena;

[Group( "Arena" )]
[Title( "Player Dresser" )]
public sealed class PlayerDresser : Component, Component.INetworkSpawn
{
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( owner.GetUserData( "avatar" ) );
		clothing.ApplyWithComponent( BodyRenderer );
	}
}
