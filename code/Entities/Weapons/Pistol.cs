using Sandbox;

namespace MyGame;
public partial class Pistol : Gun
{
	public override string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override AmmoType AmmoType => AmmoType.Pistol;
	public override float ReloadTime => 3.0f;
	public override int MagazinSize => 9;
	public override int Damage => 10;
	public override float Spreed => 0.1f;
	public override float PrimaryRate => 2.5f;
	public override float AimSpeed => 5f;


	public Pistol()
	{
		aimingOffset = new Vector3( -5f, 16.92f, 2.8f );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();
		
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Player.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void PrimaryAttack()
	{
		base.PrimaryAttack();

		ShootEffects();
		Player.PlaySound( "rust_pistol.shoot" );


	}

}
