using Sandbox;
using System.Collections.Generic;


namespace FPSGame.Weapons;

public partial class Usp : Gun
{
	//public override string ModelPath => "https://asset.party/facepunch/w_usp";
	//public override string ViewModelPath => "https://asset.party/facepunch/v_usp";
	public AnimatedEntity ViewModelArms { get; set; }
	public override AmmoType AmmoType => AmmoType.Pistol;
	public override float ReloadTime => 1f;
	public override int MagazinSize => 12;
	public override int Damage => 10;
	public override float Spreed => 0.1f;
	public override float PrimaryRate => 5f;
	public override float AimSpeed => 8f;




	


	public Usp()
	{
		aimingOffset = new Vector3( -2f, 4.8f, 1.1f );
	}

	//All of that shit for creating weapons from cloud
	public override void Spawn()
	{
		base.Spawn();

		Model = Cloud.Model( "https://asset.party/facepunch/w_usp" );
		SetBodyGroup( "barrel", 1 );
		SetBodyGroup( "sights", 1 );
		LocalScale = 1.5f;
	}


	[ClientRpc]
	public override void CreateViewModel()
	{
		Log.Info( "1" );
		ViewModelEntity = new WeaponViewModel(this);
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.Model = Cloud.Model( "https://asset.party/facepunch/v_usp" );
		ViewModelEntity.SetBodyGroup( "barrel", 1 );
		ViewModelEntity.SetBodyGroup( "sights", 1 );


		ViewModelArms = new AnimatedEntity( "models/first_person/first_person_arms.vmdl" );
		ViewModelArms.SetParent( ViewModelEntity, true );
		ViewModelArms.EnableViewmodelRendering = true;
	}


	/*public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		ViewModelEntity?.SetAnimParameter( "b_deploy", true );
	}*/
	protected override void Animate()
	{
		Player.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Player.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void Reload()
	{
		base.Reload();
		PlaySound( "glock_clipout" );
		ViewModelEntity?.SetAnimParameter( "b_reload", true );
	}


	public override void OnReloadFinish()
	{

		base.OnReloadFinish();
		PlaySound( "glock_maghit" );
	}

	

	public override void PrimaryAttack()
	{

		base.PrimaryAttack();
		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "b_attack", true );

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		ShootBullet( 0.05f, 1.5f, 9.0f, 3.0f );

	


	}

	private void Discharge()
	{
		var muzzle = GetAttachment( "muzzle" ) ?? default;
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		ShootBullet( pos, rot.Forward, 0.05f, 1.5f, 9.0f, 3.0f );

		ApplyAbsoluteImpulse( rot.Backward * 200.0f );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Speed > 500.0f )
		{
			Discharge();
		}
	}



}
