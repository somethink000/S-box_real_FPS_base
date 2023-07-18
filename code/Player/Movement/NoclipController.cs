
using Sandbox;

namespace MyGame;
[Library]
public partial class NoclipController : MovementComponent
{

	[Net] public float EyeHeight { get; set; } = 64.0f;
	public override void BuildInput()
	{
		var pl = Entity as Player;
		pl.InputDirection = Input.AnalogMove;
	}
	public override void Simulate( IClient cl )
	{

		var pl = Entity as Player;

		Events?.Clear();
		Tags?.Clear();

		pl.EyeLocalPosition = Vector3.Up * EyeHeight;
		pl.EyeRotation = pl.ViewAngles.ToRotation();

		var fwd = pl.InputDirection.x.Clamp( -1f, 1f );
		var left = pl.InputDirection.y.Clamp( -1f, 1f );
		var rotation = pl.ViewAngles.ToRotation();

		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( "Jump" ) )
		{
			vel += Vector3.Up * 1;
		}

		vel = vel.Normal * (2000 * pl.Scale);

		if ( Input.Down( "Run" ) )
			vel *= 5.0f;

		if ( Input.Down( "Duck" ) )
			vel *= 0.2f;

		pl.Velocity += vel * Time.Delta;

		if ( pl.Velocity.LengthSquared > 0.01f )
		{
			pl.Position += pl.Velocity * Time.Delta;
		}

		pl.Velocity = pl.Velocity.Approach( 0, pl.Velocity.Length * Time.Delta * 5.0f );

		WishVelocity = pl.Velocity;
		pl.GroundEntity = null;
		pl.BaseVelocity = Vector3.Zero;

		SetTag( "noclip" );
	}
}
