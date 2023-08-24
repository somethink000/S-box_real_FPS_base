using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MyGame
{
	public partial class MP5 : Gun
	{
		public AnimatedEntity ViewModelArms { get; set; }
		public override AmmoType AmmoType => AmmoType.Pistol;
		public override float ReloadTime => 1f;
		public override int MagazinSize => 30;
		public override int Damage => 5;
		public override float Spreed => 0.1f;
		public override float PrimaryRate => 9f;
		public override float AimSpeed => 8f;
		public override bool Automatic => true;


		public MP5()
		{
			aimingOffset = new Vector3( -2f, 3.5f, 0.86f );
		}


		//All of that shit for creating weapons from cloud
		public override void Spawn()
		{
			base.Spawn();

			Model = Cloud.Model( "https://asset.party/facepunch/w_mp5" );
			LocalScale = 1.5f;
		}




		[ClientRpc]
		public override void CreateViewModel()
		{
			ViewModelEntity = new PlayerViewModel( this );
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.Model = Cloud.Model( "https://asset.party/facepunch/v_mp5" );
			ViewModelEntity.SetBodyGroup( "barrel", 1 );
			//Attach sight from body group
			ViewModelEntity.SetBodyGroup( "sights", 2 );


			ViewModelArms = new AnimatedEntity( "models/first_person/first_person_arms.vmdl" );
			ViewModelArms.SetParent( ViewModelEntity, true );
			ViewModelArms.EnableViewmodelRendering = true;
		}




		public override void OnZoomStart()
		{
			base.OnZoomStart();
			ViewModelEntity?.SetAnimParameter( "attack_hold", 1f);
			ViewModelEntity?.SetAnimParameter( "ironsights", 1 );
			ViewModelEntity?.SetAnimParameter( "ironsights_fire_scale", 0.3f );
		}

		public override void OnZoomEnd()
		{
			base.OnZoomEnd();
			ViewModelEntity?.SetAnimParameter( "attack_hold", 0f );
			ViewModelEntity?.SetAnimParameter( "ironsights", 0 );
			ViewModelEntity?.SetAnimParameter( "ironsights_fire_scale", 0.5f );
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
			PlaySound( "glock18-1" );
			//	ShootBullet( 0.05f, 1.5f, 9.0f, 3.0f );




		}

		private void Discharge()
		{
			var muzzle = GetAttachment( "muzzle" ) ?? default;
			var pos = muzzle.Position;
			var rot = muzzle.Rotation;

			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( pos, rot.Forward, 0.05f, 1.5f, 9.0f, 3.0f );

			//ApplyAbsoluteImpulse( rot.Backward * 200.0f );
		}

		public override void SimulateAnimator( CitizenAnimationHelper anim )
		{
			anim.HoldType = CitizenAnimationHelper.HoldTypes.Rifle;
			anim.Handedness = CitizenAnimationHelper.Hand.Both;
			anim.AimBodyWeight = 1.0f;
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( eventData.Speed > 500.0f )
			{
				Discharge();
			}
		}
	}
}
