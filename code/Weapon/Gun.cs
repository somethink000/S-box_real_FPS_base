using Sandbox;
using System;

namespace FPSGame.Weapons
{
	public partial class Gun : Weapon
	{
		public virtual AmmoType AmmoType => AmmoType.Pistol;

		public virtual int MagazinSize => 15;
		public virtual float ReloadTime => 5.0f;
		[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
		[Net, Predicted] public int InMagazin { get; set; }
		[Net, Predicted] public bool IsReloading { get; set; }

		public bool IsAiming { get; set; }
		public bool wasAiming { get; set; }


		//Stats
		public virtual float AimSpeed => 3f;
		public virtual float Spreed => 0.5f;
		public virtual int Damage => 10;


		public Vector3 aimingOffset { get; set; }

		

		public int AvailableAmmo()
		{
			if ( Owner is not FPSPlayer owner ) return 0;
			return owner.AmmoCount( AmmoType );
		}



		public override void Spawn()
		{
			base.Spawn();

			InMagazin = MagazinSize;

		}


		public virtual void StartReloadEffects()
		{
			ViewModelEntity?.SetAnimParameter( "reload", true );
		}





		public override void Simulate( IClient player )
		{
			
			base.Simulate( player );


			if ( !IsReloading )
			{
				base.Simulate( player );
			}
			
			if ( CanReload() )
			{
				Reload();
			}

			if ( IsReloading && TimeSinceReload > ReloadTime )
			{
				OnReloadFinish();
			}

			IsAiming = Input.Down( "attack2" ) && !IsReloading;

			if ( !wasAiming && IsAiming )
			{
				OnZoomStart();
			}

			if ( wasAiming && !IsAiming )
			{
				OnZoomEnd();
			}

			wasAiming = IsAiming;
		}

		public virtual void OnZoomStart() { }
		public virtual void OnZoomEnd() { }




	

		public virtual bool CanReload()
		{
			if ( !Owner.IsValid() || !Input.Down( "reload" ) ) return false;
			if ( IsReloading || AvailableAmmo() <= 0 || InMagazin >= MagazinSize ) return false;


			return true;
		}

		public virtual void Reload()
		{

			TimeSinceReload = 0;
			IsReloading = true;

			(Owner as AnimatedEntity)?.SetAnimParameter( "b_reload", true );

			StartReloadEffects();
		}



		public virtual void OnReloadFinish()
		{

			IsReloading = false;

			if ( Owner is FPSPlayer player )
			{
				var ammo = player.TakeAmmo( AmmoType, MagazinSize - InMagazin );

				if ( ammo == 0 )
					return;

				InMagazin += ammo;
			}
		}



		public override bool CanPrimaryAttack()
		{
			if ( !Owner.IsValid() || !Input.Down( "attack1" ) || IsReloading ) return false;

			var rate = PrimaryRate;
			if ( rate <= 0 ) return true;

			return TimeSincePrimaryAttack > (1 / rate);

		}

		

		public bool TakeAmmo( int amount )
		{
			if ( InMagazin <= 0 )
				return false;

			InMagazin -= amount;
			return true;
		}


		[ClientRpc]
		public override void CreateViewModel()
		{

			if ( ViewModelPath == null ) return;

			var vm = new WeaponViewModel( this );
			vm.Model = Model.Load( ViewModelPath );
			ViewModelEntity = vm;
		}

	}
}
