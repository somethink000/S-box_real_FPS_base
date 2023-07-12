using Sandbox;

namespace MyGame.Weapons;

public partial class Light : Weapon
{
	public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override AmmoType AmmoType => AmmoType.None;
	public override float ReloadTime => 3.0f;
	public override int MagazinSize => 9;
	public override int Damage => 10;
	public override float Spreed => 0.1f;
	public override float PrimaryRate => 10.0f;

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Pawn.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void PrimaryAttack()
	{
		ShootEffects();
		Pawn.PlaySound( "rust_pistol.shoot" );
		ShootBullet( Spreed, 100, Damage, 2 );
		if ( !TakeAmmo( 1 ) )
		{
			//PlaySound( "pistol.dryfire" );
			return;
		}

	}



	protected override void Animate()
	{
		Pawn.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}
}
