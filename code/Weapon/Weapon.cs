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
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	public virtual float PrimaryRate => 5.0f;
	public virtual bool CanDrop => false;




	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;

		if ( ModelPath != null )
		{
			SetModel( ModelPath );
		}

	}



	


	public void OnEquip( FPSPlayer pawn )
	{
		Owner = pawn;
		SetParent( pawn, true );
		EnableDrawing = true;
		CreateViewModel( To.Single( pawn ) );

	}


	public void OnHolster()
	{
		EnableDrawing = false;
		DestroyViewModel( To.Single( Owner ) );
	}

	public virtual void UpdateCamera()
	{
		if ( ViewModelEntity is WeaponViewModel viewModel )
		{
			viewModel.UpdateCamera();
		}
	}



	public override void Simulate( IClient player )
	{
		Animate();

		
		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				PrimaryAttack();
			}
		}
		
	}


	
	public virtual bool CanPrimaryAttack()
	{
		
		if ( !Owner.IsValid() || !Input.Down( "attack1" ) ) return false;
		

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual void PrimaryAttack(){}


	
	protected virtual void Animate()
	{
	}

	

	[ClientRpc]
	public virtual void CreateViewModel()
	{
		
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
