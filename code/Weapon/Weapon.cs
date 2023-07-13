using Sandbox;
using System.Collections.Generic;
using System.Numerics;
using FPSGame;

namespace FPSGame.Weapons;

public partial class Weapon : AnimatedEntity
{
	//Objects
	public WeaponViewModel ViewModelEntity { get; protected set; }
	public FPSPlayer Player => Owner as FPSPlayer;
	public AnimatedEntity EffectEntity => Camera.FirstPersonViewer == Owner ? ViewModelEntity : this;
	public virtual string ViewModelPath => null;
	public virtual string ModelPath => null;



	//Ammo
	public virtual AmmoType AmmoType => AmmoType.Pistol;
	
	public virtual int MagazinSize => 15;
	public virtual float ReloadTime => 5.0f;
	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
	[Net, Predicted] public int InMagazin { get; set; }
	[Net, Predicted] public bool IsReloading { get; set; }


	//Stats
	public virtual float Spreed => 0.5f;
	public virtual int Damage => 10;
	public virtual float PrimaryRate => 5.0f;




	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }


	public int AvailableAmmo()
	{
		if ( Owner is not FPSPlayer owner ) return 0;
		return owner.AmmoCount( AmmoType );
	}


	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;

		if ( ModelPath != null )
		{
			SetModel( ModelPath );
		}

		InMagazin = MagazinSize;
	
	}



	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}



	public void OnEquip( FPSPlayer pawn )
	{
		Owner = pawn;
		SetParent( pawn, true );
		EnableDrawing = true;
		CreateViewModel( To.Single( pawn ) );

	}

	/// <summary>
	/// Called when the weapon is either removed from the player, or holstered.
	/// </summary>
	public void OnHolster()
	{
		EnableDrawing = false;
		DestroyViewModel( To.Single( Owner ) );
	}








	/// <summary>
	/// Called from <see cref="FPSPlayer.Simulate(IClient)"/>.
	/// </summary>
	/// <param name="player"></param>
	public override void Simulate( IClient player )
	{
		Animate();


		if ( !IsReloading )
		{
			base.Simulate( player );
		}



		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				PrimaryAttack();
			}
		}

		if ( CanSecondaryAttack() )
		{
			SecondaryAttack();
		
		}
		
		

		
		


		if ( CanReload() )
		{
			Reload();
		}




		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}






//INPUTS

/// <summary>
/// Called every <see cref="Simulate(IClient)"/> to see if we can shoot our gun.
/// </summary>
/// <returns></returns>
public virtual bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack1" ) || InMagazin <= 0 || IsReloading) return false;

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual bool CanSecondaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack2" ) ) return false;

		return true;
	}
	public virtual bool CanReload()
	{
		if ( !Owner.IsValid() || !Input.Down( "reload" ) ) return false;
		if ( IsReloading || AvailableAmmo() <= 0 || InMagazin >= MagazinSize ) return false;
		

		return true;
	}



	//ACTIONS




	/// <summary>
	/// Called when your gun shoots.
	/// </summary>
	public virtual void PrimaryAttack(){}

	public virtual void SecondaryAttack(){}

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





	public bool TakeAmmo( int amount )
	{
		if ( InMagazin <= 0 )
			return false;

		InMagazin -= amount;
		return true;
	}




	//VIEW MODEL


	/// <summary>
	/// Useful for setting anim parameters based off the current weapon.
	/// </summary>
	protected virtual void Animate()
	{
	}

	

	[ClientRpc]
	public void CreateViewModel()
	{
		if ( ViewModelPath == null ) return;

		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( ViewModelPath );
		ViewModelEntity = vm;
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		if ( ViewModelEntity.IsValid() )
		{
			ViewModelEntity.Delete();
		}
	}
	
}
