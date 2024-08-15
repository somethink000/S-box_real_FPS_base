using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace GeneralGame;

//Movement
partial class PlayerObject
{

	[Property] public float StandHeight { get; set; } = 64f;
	[Property] public float DuckHeight { get; set; } = 28f;
	[Property] public Vector3 Gravity { get; set; } = new( 0f, 0f, 800f );

	[Property] public CharacterController CC { get; set; }
	[Property] public Action OnJump { get; set; }
	
	[Property] public List<CitizenAnimationHelper> Animators { get; private set; } = new();
	[Property] public CitizenAnimationHelper ShadowAnimator { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	
	[Sync] public bool IsRunning { get; set; }
	[Sync] public bool IsCrouching { get; set; }
	[Sync] public float MoveSpeed { get; set; }

	public Vector3 WishVelocity { get; private set; }
	private RealTimeSince LastGroundedTime { get; set; }
	private RealTimeSince LastUngroundedTime { get; set; }
	private PlayerObject ply { get; set; }


	//Speed {
		[Property] public float baseWalkSpeed { get; set; } = 110f;
		[Property] public float baseRunSpeed { get; set; } = 260f;
		[Property] public float baseCrouchSpeed { get; set; } = 64f;
		private float walkSpeed { get; set; }
		private float runSpeed { get; set; }
		private float crouchSpeed { get; set; }
		private bool WantsToCrouch { get; set; }
	// }

	public void setWalkSpeed( float speed )
	{
		walkSpeed = speed;
	}
	public void setRunSpeed( float speed )
	{
		runSpeed = speed;
	}
	public void setCrouchSpeed( float speed )
	{
		crouchSpeed = speed;
	}
	
	protected virtual bool CanUncrouch()
	{
		if ( !IsCrouching ) return true;
		if ( LastUngroundedTime < 0.2f ) return false;

		var tr = CC.TraceDirection( Vector3.Up * DuckHeight );
		return !tr.Hit;
	}

	public void ControllerAwake()
	{
		base.OnAwake();
		ply = GameObject.Components.Get<PlayerObject>();

		if ( CC.IsValid() )
		{
			CC.Height = StandHeight;
			setWalkSpeed( baseWalkSpeed );
			setRunSpeed( baseRunSpeed );
			setCrouchSpeed( baseCrouchSpeed );
		}
	}

	public void ControllerStart()
	{
		Animators.Add( ShadowAnimator );
		Animators.Add( AnimationHelper );

		base.OnStart();
	}

	public void ControllerUpdate()
	{
		if ( ply.Ragdoll.IsRagdolled || ply.LifeState == LifeState.Dead )
			return;

		if ( !IsProxy )
		{
			IsRunning = ( !IsCrouching && !ply.IsEncumbered ) ? Input.Down( "Run" ) : false;
		}

		var weapon = ply.Inventory.Deployed;

		foreach ( var animator in Animators )
		{
			animator.HoldType = weapon.IsValid() ? weapon.HoldType : CitizenAnimationHelper.HoldTypes.None;
			animator.WithVelocity( CC.Velocity );
			animator.WithWishVelocity( WishVelocity );
			animator.IsGrounded = CC.IsOnGround;
			animator.MoveRotationSpeed = 0f;
			animator.DuckLevel = IsCrouching ? 1f : 0f;
			animator.WithLook( EyeAngles.Forward );
			animator.MoveStyle = (IsRunning && !IsCrouching) ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
		}
	}

	protected virtual void DoCrouchingInput()
	{
		WantsToCrouch = CC.IsOnGround && Input.Down( "Duck" );

		if ( WantsToCrouch == IsCrouching )
			return;

		if ( WantsToCrouch )
		{

			CC.Height = DuckHeight;
			IsCrouching = true;
		}
		else
		{
			if ( !CanUncrouch() )
				return;

			CC.Height = StandHeight;
			IsCrouching = false;
		}

	}
	/*if (IsEncumbered )
	{
		var overweight = (Inventory.Weight - Inventory.MAX_WEIGHT_IN_GRAMS).ToKilograms();
		var multiplier = MathX.Clamp( 1 - overweight / 15f, 0.2f, 1f );
		wishVelocity *= multiplier;
	}*/
protected virtual void DoMovementInput()
	{
		BuildWishVelocity();

		if ( CC.IsOnGround && Input.Down( "Jump" ) )
		{
			CC.Punch( Vector3.Up * 300f );
			SendJumpMessage();
		}

		MoveSpeed = CC.Velocity.WithZ( 0 ).Length;


		if ( CC.IsOnGround )
		{
			CC.Velocity = CC.Velocity.WithZ( 0f );
			CC.Accelerate( WishVelocity );
			CC.ApplyFriction( 4.0f );

		}
		else
		{
			CC.Velocity -= Gravity * Time.Delta * 0.5f;
			CC.Accelerate( WishVelocity.ClampLength( 50f ) );
			CC.ApplyFriction( 0.1f );
		}

		CC.Move();

		if ( !CC.IsOnGround )
		{
			CC.Velocity -= Gravity * Time.Delta * 0.5f;
			LastUngroundedTime = 0f;
		}
		else
		{
			CC.Velocity = CC.Velocity.WithZ( 0 );
			LastGroundedTime = 0f;
		}

		Transform.Rotation = Rotation.FromYaw( EyeAngles.ToRotation().Yaw() );
	}

	public void ControllerFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( ply.Ragdoll.IsRagdolled || ply.LifeState == LifeState.Dead )
			return;

		DoCrouchingInput();
		DoMovementInput();

	}

	private void BuildWishVelocity()
	{
		var rotation = EyeAngles.ToRotation();

		WishVelocity = rotation * Input.AnalogMove;
		WishVelocity = WishVelocity.WithZ( 0f );

		if ( !WishVelocity.IsNearZeroLength )
			WishVelocity = WishVelocity.Normal;


		if ( IsCrouching )
			WishVelocity *= crouchSpeed;
		else if ( IsRunning )

			WishVelocity *= runSpeed;
		else
			WishVelocity *= walkSpeed;
	}

	[Broadcast]
	private void SendJumpMessage()
	{
		foreach ( var animator in Animators )
		{
			animator.TriggerJump();
		}

		OnJump?.Invoke();
	}
}

