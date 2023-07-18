using Sandbox;

namespace MyGame;

public class CitizenAnimationComponent : AnimationComponent
{
	Entity lastWeapon;
	public override void Simulate( IClient cl )
	{
		var ply = Entity as Player;
		// where should we be rotated to
		var turnSpeed = 0.02f;

		Rotation rotation = ply.ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		ply.Rotation = Rotation.Slerp( ply.Rotation, idealRotation, ply.MovementController.WishVelocity.Length * Time.Delta * turnSpeed );
		ply.Rotation = ply.Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( ply );

		animHelper.WithWishVelocity( ply.MovementController.WishVelocity / Entity.Scale );
		animHelper.WithVelocity( ply.Velocity / Entity.Scale );
		animHelper.WithLookAt( ply.EyePosition + ply.ViewAngles.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, ply.MovementController.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && ply.Client.IsValid()) ? ply.Client.Voice.LastHeard < 0.5f ? ply.Client.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = ply.GroundEntity != null;
		animHelper.IsSitting = ply.MovementController.HasTag( "sitting" );
		animHelper.IsNoclipping = ply.MovementController.HasTag( "noclip" );
		animHelper.IsClimbing = ply.MovementController.HasTag( "climbing" );
		animHelper.IsSwimming = ply.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		if ( ply.MovementController.HasEvent( "jump" ) ) animHelper.TriggerJump();

		if ( ply.Inventory?.ActiveChild != lastWeapon ) animHelper.TriggerDeploy();

		if ( ply.Inventory?.ActiveChild is Carriable carry )
		{
			carry.SimulateAnimator( animHelper );
		}
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;

		}

		lastWeapon = ply.Inventory?.ActiveChild;
	}
}
