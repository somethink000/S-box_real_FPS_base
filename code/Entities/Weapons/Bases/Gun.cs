using Sandbox;
using System.Collections.Generic;

namespace MyGame;
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
	bool punched = false;

	//Stats

	public virtual bool Automatic => false;
	public virtual float AimSpeed => 3f;
	public virtual float Spreed => 0.5f;
	public virtual float punchEffectPower => 1f;
	public virtual int Damage => 10;
	internal float viewpunchmod = 0;


	
	public Vector3 aimingOffset { get; set; }



	public int AvailableAmmo()
	{
		if ( Owner is not Player owner ) return 0;
		return owner.Ammo.AmmoCount( AmmoType );
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


	public virtual void DoViewPunch( float punch )
	{
		if ( Game.IsClient )
		{
			(Owner as Player).ViewAngles += new Angles( -punch, 0, 0 );
			viewpunchmod = 0.5f;
			punched = false;
		}
	}


	public override void Simulate( IClient player )
	{

		base.Simulate( player );

		if ( (Owner as Player).GroundEntity != null)
		{
			ViewModelEntity?.SetAnimParameter( "b_grounded", true );
		}
		

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
		ViewPunchEffectFrame();
		wasAiming = IsAiming;
	}


	public virtual void ViewPunchEffectFrame()
	{
		if ( Owner == null || Owner.LifeState != LifeState.Alive ) return;
		if ( viewpunchmod <= -0.1f )
		{
			punched = true;

		}
		if ( punched )
		{
			viewpunchmod = viewpunchmod.LerpTo( 0, Time.Delta * 28 );
		}
		else
		{
			viewpunchmod = viewpunchmod.LerpTo( -0.12f, Time.Delta * 48 );
		}
		(Owner as Player).ViewAngles += new Angles( -viewpunchmod, 0, 0 );
	}
	public virtual void OnZoomStart() { }
	public virtual void OnZoomEnd() { }



	public override void PrimaryAttack()
	{

		DoViewPunch( punchEffectPower );

		if ( !TakeAmmo( 1 ) )
		{
			//PlaySound( "pistol.dryfire" );
			return;
		}
		ShootBullet( 0.05f, 1.5f, Damage, 3.0f );


	}


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

		(Owner as Player)?.SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}



	public virtual void OnReloadFinish()
	{

		IsReloading = false;

		if ( Owner is Player player )
		{
			var ammo = player.Ammo.TakeAmmo( AmmoType, MagazinSize - InMagazin );

			if ( ammo == 0 )
				return;

			InMagazin += ammo;
		}
	}



	public override bool CanPrimaryAttack()
	{
		if (Automatic)
		{
			if ( !Owner.IsValid() || !Input.Down( "attack1" ) || IsReloading || InMagazin <= 0 ) return false;
		}
		else
		{
			if ( !Owner.IsValid() || !Input.Pressed( "attack1" ) || IsReloading || InMagazin <= 0 ) return false;
		}
		
		
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




	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		Game.SetRandomSeed( Time.Tick );

		var ray = Owner.AimRay;
		ShootBullet( ray.Position, ray.Forward, spread, force, damage, bulletSize );
	}
}
