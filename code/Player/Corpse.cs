using Sandbox;

namespace MyGame;

public partial class Corpse : ModelEntity
{
	public DamageInfo KillDamage { get; set; }
	[Net] public Entity Attacker { get; set; }
	[Net] public Entity Weapon { get; set; }
	[Net] public IClient OwnerClient { get; set; }
}
