using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class Gun
{

	public virtual SkinnedModelRenderer GetEffectRenderer()
	{
		SkinnedModelRenderer effectModel = WorldModelRenderer;

		if ( CanSeeViewModel )
			effectModel = ViewModelRenderer;

		return effectModel;
	}

	public virtual Transform? GetArrachmentTransform( string bone)
	{
		var effectRenderer = GetEffectRenderer();
		var effectAttachment = bone;


		return effectRenderer?.GetAttachment( effectAttachment );
	}

	public bool HasAmmo()
	{

		if ( ClipSize == -1 )
		{
			return Owner.InventoryController.CanTake( AmmoType, 1, out var ammo );

		}

		if ( Clip == 0 )
			return false;

		return true;
	}

	public bool IsShooting()
	{

		return GetRealRPM( RPM ) > TimeSinceShoot;

	}


	public static float GetRealRPM( int rpm )
	{
		return 60f / rpm;
	}

	public virtual float GetRealSpread( float baseSpread = -1 )
	{
		if ( !Owner.IsValid() ) return 0;

		float spread = baseSpread != -1 ? baseSpread : Spread;
		float floatMod = 1f;

		// Ducking
		if ( IsCrouching && !IsAiming )
			floatMod -= 0.25f;

		// Aiming
		if ( IsAiming && Bullets == 1 )
			floatMod /= 4;

		if ( !Owner.MovementController.IsOnGround )
		{
			// Jumping
			floatMod += 0.75f;
		}
		else if ( Owner.MovementController.Velocity.Length > 100 )
		{
			// Moving 
			floatMod += 0.25f;
		}

		return spread * floatMod;
	}

	public virtual Angles GetRecoilAngles()
	{
		var recoilX = IsAiming ? -Recoil * 0.4f : -Recoil;
		var recoilY = Game.Random.NextFloat( -0.2f, 0.2f ) * recoilX;
		var recoilAngles = new Angles( recoilX, recoilY, 0 );
		return recoilAngles;
	}

}
