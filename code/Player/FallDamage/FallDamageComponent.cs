using Sandbox;

namespace MyGame;
public partial class FallDamageComponent : SimulatedComponent, ISingletonComponent
{
	float PreviousZVelocity = 0;
	const float LethalFallSpeed = 1024;
	const float SafeFallSpeed = 580;
	const float DamageForSpeed = (float)100 / (LethalFallSpeed - SafeFallSpeed); // damage per unit per second.
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		var FallSpeed = -PreviousZVelocity;
		if ( FallSpeed > (SafeFallSpeed * Entity.Scale) && Entity.GroundEntity != null )
		{
			var FallDamage = (FallSpeed - (SafeFallSpeed * Entity.Scale)) * (DamageForSpeed * Entity.Scale);
			var info = DamageInfo.Generic( FallDamage ).WithTag( "fall" );
			Entity.TakeDamage( info );
			Entity.PlaySound( "falldamage" );
		}
		PreviousZVelocity = Entity.Velocity.z;
	}
}
