using Sandbox.Citizen;


namespace GeneralGame;

public class MovementController : Component
{
	[RequireComponent] public Player ply { get; set; }
	[Property] public float BaseGroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float RunSpeed { get; set; } = 290f;
	[Property] public float WalkSpeed { get; set; } = 160f;
	[Property] public float CrouchSpeed { get; set; } = 90f;
	[Property] public float JumpForce { get; set; } = 350f;


	[Sync] public Vector3 WishVelocity { get; set; } = Vector3.Zero;
	
	[Sync] public bool IsCrouching { get; set; } = false;
	[Sync] public bool IsRunning { get; set; } = false;
	[Sync] public bool IsSlide { get; set; } = false;
	[Sync] public bool IsGrounded { get; set; } = false;


	private float GroundControl { get; set; }
	public bool IsOnGround => CharacterController.IsOnGround;
	public Vector3 Velocity => CharacterController.Velocity;

	public CharController CharacterController { get; set; }
	public CapsuleCollider BodyCollider { get; set; }
	private GameObject Body => ply.Body;

	TimeSince timeSinceLastFootstep = 0;
	
	float fallVelocity = 0;


	protected override void OnAwake()
	{
		GroundControl = BaseGroundControl;

		CharacterController = Components.Get<CharController>();
		BodyCollider = ply.Body.Components.Get<CapsuleCollider>();

		if ( ply.BodyRenderer is not null )
			ply.BodyRenderer.OnFootstepEvent += OnAnimEventFootstep;
	}

	protected override void OnUpdate()
	{
		if ( !ply.IsAlive ) return;

		if ( !IsProxy )
		{
			IsRunning = Input.Down( "Run" );

			if ( Input.Pressed( "Jump" ) )
				Jump();

			UpdateCrouch();
		}

		RotateBody();
		UpdateAnimations();
		
	}

	protected override void OnFixedUpdate()
	{
		
		if ( IsProxy || !ply.IsAlive ) return;
		BuildWishVelocity();
		Move();
	}

	void BuildWishVelocity()
	{

		

			WishVelocity = 0;
			
			var rot = ply.CameraController.EyeAngles.ToRotation();
			if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
			if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
			if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
			if ( Input.Down( "Right" ) ) WishVelocity += rot.Right;


		
			WishVelocity = WishVelocity.WithZ( 0 );
			if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

			if ( IsCrouching ) WishVelocity *= CrouchSpeed;
			else if ( IsRunning ) WishVelocity *= RunSpeed;
			else WishVelocity *= WalkSpeed;
		


	}

	private void OnGrounded()
	{
		if ( fallVelocity > 650)
		{
			var damage = new DamageInfo( fallVelocity / 10, GameObject, GameObject );

			ply.HealthController.OnDamage( damage );
		}

		fallVelocity = 0;
	}

	
	void Move()
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( IsOnGround )
		{
			// Friction / Acceleration
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( GroundControl );
			
			if ( !IsGrounded ) {
				OnGrounded();
				
				IsGrounded = true;
			}
		}
		else
		{
			// Air control / Gravity
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
			CharacterController.Accelerate( WishVelocity.ClampLength( MaxForce ) );
			CharacterController.ApplyFriction( AirControl );
			IsGrounded = false;
			fallVelocity = CharacterController.Velocity.WithY( 0 ).Length;
		}

		if ( !(CharacterController.Velocity.IsNearZeroLength && WishVelocity.IsNearZeroLength) )
			CharacterController.Move();

		// Second half of gravity after movement (to stay accurate)
		if ( IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
		}
	}

	void RotateBody()
	{
		
		var targetRot = new Angles( 0, ply.CameraController.EyeAngles.ToRotation().Yaw(), 0 ).ToRotation();
		float rotateDiff = Body.Transform.Rotation.Distance( targetRot );

		if ( rotateDiff > 20f || CharacterController.Velocity.Length > 10f )
		{
			Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetRot, Time.Delta * 2f );
		}
	}

	void Jump()
	{
		if ( !IsOnGround ) return;

		CharacterController.Punch( Vector3.Up * JumpForce );

		foreach ( var animator in ply.Animators )
		{
			animator?.TriggerJump();
		}
	}

	void UpdateAnimations()
	{
		foreach ( var animator in ply.Animators )
		{
			animator.WithWishVelocity( WishVelocity );
			animator.WithVelocity( CharacterController.Velocity );
			animator.AimAngle = ply.CameraController.EyeAngles.ToRotation();
			animator.IsGrounded = IsOnGround;
			animator.WithLook( ply.CameraController.EyeAngles.ToRotation().Forward, 1f, 0.75f, 0.5f );
			animator.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
			animator.DuckLevel = IsCrouching ? 1 : 0;
			animator.SpecialMove = IsSlide ? CitizenAnimationHelper.SpecialMoveStyle.Slide : CitizenAnimationHelper.SpecialMoveStyle.None;
		}

	}


	void UpdateCrouch()
	{
		if ( IsSlide ) return;

		if ( Input.Down( "Duck" ) && !IsCrouching && IsOnGround )
		{
			
			
				IsCrouching = true;
				CharacterController.Height /= 2f;
				BodyCollider.End = BodyCollider.End.WithZ( BodyCollider.End.z / 2f );
			 
		}

		if ( IsCrouching && (!Input.Down( "Duck" ) || !IsOnGround) )
		{
			// Check we have space to uncrouch
			var targetHeight = CharacterController.Height * 2f;
			var upTrace = CharacterController.TraceDirection( Vector3.Up * targetHeight );

			if ( !upTrace.Hit )
			{
				IsCrouching = false;
				CharacterController.Height = targetHeight;
				BodyCollider.End = BodyCollider.End.WithZ( BodyCollider.End.z * 2f );
			}
		}
	}

	void OnAnimEventFootstep( SceneModel.FootstepEvent footstepEvent )
	{
		if ( !ply.IsAlive || !IsOnGround ) return;

		// Walk
		var stepDelay = 0.25f;

		// Running
		if ( Velocity.WithZ( 0 ).Length >= 200 )
		{
			stepDelay = 0.2f;
		}
		// Crouching
		else if ( IsCrouching )
		{
			stepDelay = 0.4f;
		}

		if ( timeSinceLastFootstep < stepDelay )
			return;

		var tr = Scene.Trace.Ray( footstepEvent.Transform.Position, footstepEvent.Transform.Position + Vector3.Down * 20 )
			.Radius( 1 )
			.IgnoreGameObject( this.GameObject )
			.Run();

		if ( !tr.Hit ) return;

		var sound = tr.Surface.PlayCollisionSound( footstepEvent.Transform.Position );
		if ( sound is not null ) { 
			if (!IsProxy) { 
				sound.Volume = 0.1f;
			}else
			{
				sound.Volume = footstepEvent.Volume;
			}
		}

		timeSinceLastFootstep = 0;
	}
}
