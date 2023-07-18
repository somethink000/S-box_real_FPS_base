using Sandbox;

namespace MyGame;
public class Grenade : Throwable
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override void Throw()
	{
		if ( Game.IsServer )
		{
			var Nade = new ModelEntity();
			Nade.SetModel( WorldModelPath );
			Nade.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			Nade.Position = Owner.AimRay.Position + (Owner.AimRay.Forward * 40);
			Nade.PhysicsBody.Velocity = (Owner.AimRay.Forward * 500) + (Owner.AimRay.Forward.EulerAngles.ToRotation().Up * 200);
		}
	}
}
