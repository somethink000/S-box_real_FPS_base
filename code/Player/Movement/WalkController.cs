
using Sandbox;
using System;
using System.Collections.Generic;

namespace MyGame;
[Library]
public partial class WalkController : MovementComponent
{
	[Net] public float SprintSpeed { get; set; } = 320.0f;
	[Net] public float WalkSpeed { get; set; } = 150.0f;
	[Net] public float CrouchSpeed { get; set; } = 80.0f;
	[Net] public float DefaultSpeed { get; set; } = 190.0f;
	[Net] public float Acceleration { get; set; } = 10.0f;
	[Net] public float AirAcceleration { get; set; } = 50.0f;
	[Net] public float FallSoundZ { get; set; } = -30.0f;
	[Net] public float GroundFriction { get; set; } = 4.0f;
	[Net] public float StopSpeed { get; set; } = 100.0f;
	[Net] public float Size { get; set; } = 20.0f;
	[Net] public float DistEpsilon { get; set; } = 0.03125f;
	[Net] public float GroundAngle { get; set; } = 46.0f;
	[Net] public float Bounce { get; set; } = 0.0f;
	[Net] public float MoveFriction { get; set; } = 1.0f;
	[Net] public float StepSize { get; set; } = 18.0f;
	[Net] public float MaxNonJumpVelocity { get; set; } = 140.0f;
	[Net] public float BodyGirth { get; set; } = 32.0f;
	[Net] public float BodyHeight { get; set; } = 72.0f;
	[Net] public float EyeHeight { get; set; } = 64.0f;
	[Net] public float DuckHeight { get; set; } = 32.0f;
	[Net] public float DuckEyeHeight { get; set; } = 24.0f;
	[Net] public float Gravity { get; set; } = 800.0f;
	[Net] public float AirControl { get; set; } = 30.0f;
	[ConVar.Replicated( "walkcontroller_showbbox" )] public bool ShowBBox { get; set; } = false;
	public bool Swimming { get; set; } = false;
	[Net] public bool AutoJump { get; set; } = false;



	public WalkController()
	{
	}

	/// <summary>
	/// This is temporary, get the hull size for the player's collision
	/// </summary>
	public BBox GetHull()
	{
		var girth = BodyGirth * 0.5f;
		var height = BodyHeight;
		if ( IsDucking ) height = 32;
		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, BodyHeight );

		return new BBox( mins, maxs );
	}


	// Duck body height 32
	// Eye Height 64
	// Duck Eye Height 28

	public Vector3 mins;
	public Vector3 maxs;

	/// <summary>
	/// Any bbox traces we do will be offset by this amount.
	/// todo: this needs to be predicted
	/// </summary>
	public Vector3 TraceOffset;

	public virtual void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	/// <summary>
	/// Update the size of the bbox. We should really trigger some shit if this changes.
	/// </summary>
	public virtual void UpdateBBox( int forceduck = 0 )
	{
		var girth = BodyGirth * 0.5f;
		var height = (EyeHeight + DuckAmount) + (BodyHeight - EyeHeight);
		if ( forceduck == 1 ) height = DuckHeight;
		if ( forceduck == -1 ) height = BodyHeight;

		var mins = new Vector3( -girth, -girth, 0 ) * Entity.Scale;
		var maxs = new Vector3( +girth, +girth, height ) * Entity.Scale;

		SetBBox( mins, maxs );
	}

	protected float SurfaceFriction;


	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		if ( ShowBBox ) DebugOverlay.Box( Entity.Position, mins, maxs, Color.Yellow );
		RestoreGroundAngles();
		var pl = Entity as Player;
		SaveGroundAngles();
		DuckFrameSimulate();
	}

	public override void BuildInput()
	{

		var pl = Entity as Player;
		pl.InputDirection = Input.AnalogMove;
	}
	public override void Simulate( IClient cl )
	{
		var pl = Entity as Player;

		Events?.Clear();
		Tags?.Clear();

		pl.EyeLocalPosition = Vector3.Up * EyeHeight;
		pl.EyeRotation = pl.ViewAngles.ToRotation();


		CheckDuck();
		UpdateBBox();


		RestoreGroundPos();
		//Entity.Velocity += Entity.BaseVelocity * (1 + Time.Delta * 0.5f);
		//Entity.BaseVelocity = Vector3.Zero;

		//Rot = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );


		// Check Stuck
		// Unstuck - or return if stuck

		// Set Ground Entity to null if  falling faster then 250

		// store water level to compare later

		// if not on ground, store fall velocity

		// player->UpdateStepSound( player->m_pSurfaceData, mv->GetAbsOrigin(), mv->m_vecVelocity )


		// RunLadderMode

		CheckLadder();
		Swimming = Entity.GetWaterLevel() > 0.5f;

		//
		// Start Gravity
		//
		if ( !Swimming && !IsTouchingLadder )
		{
			Entity.Velocity -= new Vector3( 0, 0, (Gravity * Entity.Scale) * 0.5f ) * Time.Delta;
			Entity.Velocity += new Vector3( 0, 0, Entity.BaseVelocity.z ) * Time.Delta;

			Entity.BaseVelocity = Entity.BaseVelocity.WithZ( 0 );
		}


		/*
		 if (player->m_flWaterJumpTime)
			{
				WaterJump();
				TryPlayerMove();
				// See if we are still in water?
				CheckWater();
				return;
			}
		*/

		// if ( underwater ) do underwater movement 
		if ( Entity.GetWaterLevel() > 0.1 )
		{
			WaterSimulate();
		}
		else
		if ( AutoJump ? Input.Down( "Jump" ) : Input.Pressed( "Jump" ) )
		{
			CheckJumpButton();
		}

		// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor,
		//  we don't slow when standing still, relative to the conveyor.
		bool bStartOnGround = Entity.GroundEntity != null;
		//bool bDropSound = false;
		if ( bStartOnGround )
		{
			//if ( Velocity.z < FallSoundZ ) bDropSound = true;

			Entity.Velocity = Entity.Velocity.WithZ( 0 );
			//player->m_Local.m_flFallVelocity = 0.0f;

			if ( Entity.GroundEntity != null )
			{
				ApplyFriction( GroundFriction * SurfaceFriction );
			}
		}

		//
		// Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
		//
		WishVelocity = new Vector3( pl.InputDirection.x.Clamp( -1f, 1f ), pl.InputDirection.y.Clamp( -1f, 1f ), 0 );
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );

		if ( Swimming || IsTouchingLadder )
		{

			WishVelocity *= pl.ViewAngles.ToRotation();
		}
		else
		{

			WishVelocity *= pl.ViewAngles.WithPitch( 0 ).ToRotation();
		}


		if ( !Swimming && !IsTouchingLadder )
		{
			WishVelocity = WishVelocity.WithZ( 0 );
		}

		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= GetWishSpeed();
		WishVelocity *= Entity.Scale;


		bool bStayOnGround = false;
		if ( Swimming )
		{
			ApplyFriction( 1 );
			WaterMove();
		}
		else if ( IsTouchingLadder )
		{
			Entity.Tags.Add( "climbing" );
			LadderMove();
		}
		else if ( Entity.GroundEntity != null )
		{
			bStayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( bStayOnGround );

		// FinishGravity
		if ( !Swimming && !IsTouchingLadder )
		{
			Entity.Velocity -= new Vector3( 0, 0, (Gravity * Entity.Scale) * 0.5f ) * Time.Delta;
		}


		if ( Entity.GroundEntity != null )
		{
			Entity.Velocity = Entity.Velocity.WithZ( 0 );
		}
		DoPushingStuff();
		if ( Entity == null ) return;

		// CheckFalling(); // fall damage etc

		// Land Sound
		// Swim Sounds

		SaveGroundPos();
		LatchOntoLadder();
		PreviousGroundEntity = Entity.GroundEntity;

		bool Debug = false;
		if ( Debug )
		{
			DebugOverlay.Box( Entity.Position + TraceOffset, mins, maxs, Color.Red );
			DebugOverlay.Box( Entity.Position, mins, maxs, Color.Blue );

			var lineOffset = 0;
			if ( Game.IsServer ) lineOffset = 10;

			DebugOverlay.ScreenText( $"        Position: {Entity.Position}", lineOffset + 0 );
			DebugOverlay.ScreenText( $"        Velocity: {Entity.Velocity}", lineOffset + 1 );
			DebugOverlay.ScreenText( $"    BaseVelocity: {Entity.BaseVelocity}", lineOffset + 2 );
			DebugOverlay.ScreenText( $"    GroundEntity: {Entity.GroundEntity} [{Entity.GroundEntity?.Velocity}]", lineOffset + 3 );
			DebugOverlay.ScreenText( $" SurfaceFriction: {SurfaceFriction}", lineOffset + 4 );
			DebugOverlay.ScreenText( $"    WishVelocity: {WishVelocity}", lineOffset + 5 );
			DebugOverlay.ScreenText( $"    Speed: {Entity.Velocity.Length}", lineOffset + 6 );
		}

	}

	public virtual float GetWishSpeed()
	{
		var ws = -1;// Duck.GetWishSpeed();
		if ( ws >= 0 ) return ws;

		if ( Input.Down( "Duck" ) || IsDucking ) return CrouchSpeed;
		if ( Input.Down( "Run" ) ) return SprintSpeed; ;
		if ( Input.Down( "Walk" ) ) return WalkSpeed;

		return DefaultSpeed;
	}

	public virtual void WalkMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * wishspeed;

		Entity.Velocity = Entity.Velocity.WithZ( 0 );
		Accelerate( wishdir, wishspeed, 0, Acceleration );
		Entity.Velocity = Entity.Velocity.WithZ( 0 );

		//   Player.SetAnimParam( "forward", Input.Forward );
		//   Player.SetAnimParam( "sideward", Input.Right );
		//   Player.SetAnimParam( "wishspeed", wishspeed );
		//    Player.SetAnimParam( "walkspeed_scale", 2.0f / 190.0f );
		//   Player.SetAnimParam( "runspeed_scale", 2.0f / 320.0f );

		//  DebugOverlay.Text( 0, Pos + Vector3.Up * 100, $"forward: {Input.Forward}\nsideward: {Input.Right}" );

		// Add in any base velocity to the current velocity.
		Entity.Velocity += Entity.BaseVelocity;

		try
		{
			if ( Entity.Velocity.Length < 1.0f )
			{
				Entity.Velocity = Vector3.Zero;
				return;
			}

			// first try just moving to the destination
			var dest = (Entity.Position + Entity.Velocity * Time.Delta).WithZ( Entity.Position.z );

			var pm = TraceBBox( Entity.Position, dest );

			if ( pm.Fraction == 1 )
			{
				Entity.Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{

			// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
			Entity.Velocity -= Entity.BaseVelocity;
		}

		StayOnGround();

		Entity.Velocity = Entity.Velocity.Normal * MathF.Min( Entity.Velocity.Length, (GetWishSpeed() * Entity.Scale) );
	}

	public virtual void StepMove()
	{
		MoveHelper mover = new MoveHelper( Entity.Position, Entity.Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Entity );
		mover.MaxStandableAngle = GroundAngle;

		mover.TryMoveWithStep( Time.Delta, StepSize * Entity.Scale );

		Entity.Position = mover.Position;
		Entity.Velocity = mover.Velocity;
	}

	public virtual void Move()
	{
		MoveHelper mover = new MoveHelper( Entity.Position, Entity.Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Entity );
		mover.MaxStandableAngle = GroundAngle;

		mover.TryMove( Time.Delta );

		Entity.Position = mover.Position;
		Entity.Velocity = mover.Velocity;
	}

	/// <summary>
	/// Add our wish direction and speed onto our velocity
	/// </summary>
	public virtual void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		// This gets overridden because some games (CSPort) want to allow dead (observer) players
		// to be able to move around.
		// if ( !CanAccelerate() )
		//     return; 
		speedLimit *= Entity.Scale;
		acceleration /= Entity.Scale;
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		// See if we are changing direction a bit
		var currentspeed = Entity.Velocity.Dot( wishdir );

		// Reduce wishspeed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = (acceleration * Entity.Scale) * Time.Delta * wishspeed * SurfaceFriction;

		// Cap at addspeed
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Entity.Velocity += wishdir * accelspeed;
	}

	/// <summary>
	/// Remove ground friction from velocity
	/// </summary>
	public virtual void ApplyFriction( float frictionAmount = 1.0f )
	{
		// If we are in water jump cycle, don't apply friction
		//if ( player->m_flWaterJumpTime )
		//   return; 
		// Not on ground - no friction   
		// Calculate speed 
		var speed = Entity.Velocity.Length;
		if ( speed < 0.1f ) return;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		float control = (speed < StopSpeed * Entity.Scale) ? (StopSpeed * Entity.Scale) : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Entity.Velocity *= newspeed;
		}

		// mv->m_outWishVel -= (1.f-newspeed) * mv->m_vecVelocity;
	}

	[Net, Predicted] public bool IsDucking { get; set; } // replicate
	[Net, Predicted] public float DuckAmount { get; set; } = 0;
	public virtual void CheckDuck()
	{
		var pl = Entity as Player;
		bool wants = Input.Down( "Duck" );

		if ( wants != IsDucking )
		{
			if ( wants ) TryDuck();
			else TryUnDuck();
		}

		if ( IsDucking )
		{
			var delta = DuckAmount;
			DuckAmount = DuckAmount.LerpTo( (EyeHeight - DuckEyeHeight) * -1, 8 * Time.Delta );
			delta -= DuckAmount;
			SetTag( "ducked" );
			if ( pl.GroundEntity is null )
			{
				pl.Position += Vector3.Up * (delta * Entity.Scale);
				var pm = TraceBBox( Entity.Position, Entity.Position );
				if ( pm.StartedSolid )
				{
					pl.Position -= Vector3.Up * (delta * Entity.Scale);
				}
			}
			FixPlayerCrouchStuck( true );
			CategorizePosition( false );
		}
		else
		{
			var delta = DuckAmount;
			DuckAmount = DuckAmount.LerpTo( 0, 8 * Time.Delta );
			delta -= DuckAmount;

			if ( pl.GroundEntity is null )
			{
				pl.Position += Vector3.Up * (delta * Entity.Scale);

				var pm = TraceBBox( Entity.Position, Entity.Position );
				if ( pm.StartedSolid )
				{
					pl.Position -= Vector3.Up * (delta * Entity.Scale);
				}
			}
			CategorizePosition( false );
		}
		pl.EyeLocalPosition = pl.EyeLocalPosition.WithZ( EyeHeight + (DuckAmount) );

	}
	public float LocalDuckAmount { get; set; } = 0;
	void DuckFrameSimulate()
	{

		var pl = Entity as Player;
		if ( IsDucking )
		{
			LocalDuckAmount = LocalDuckAmount.LerpTo( (EyeHeight - DuckEyeHeight) * -1, 8 * Time.Delta );
		}
		else
		{
			LocalDuckAmount = LocalDuckAmount.LerpTo( 0, 8 * Time.Delta );
		}
		pl.EyeLocalPosition = pl.EyeLocalPosition.WithZ( EyeHeight + LocalDuckAmount );
	}
	public virtual void TryDuck()
	{
		IsDucking = true;
	}

	public virtual void TryUnDuck()
	{
		UpdateBBox( -1 );
		var pm = TraceBBox( Entity.Position, Entity.Position, DuckHeight - 4 );
		if ( pm.StartedSolid )
		{
			UpdateBBox();
			return;
		}
		IsDucking = false;
	}

	public virtual void FixPlayerCrouchStuck( bool upward )
	{
		int direction = upward ? 1 : 0;

		var trace = TraceBBox( Entity.Position, Entity.Position );
		if ( trace.Entity == null )
			return;

		var test = Entity.Position;
		for ( int i = 0; i < (DuckHeight - 4); i++ )
		{
			var org = Entity.Position;
			org.z += direction;

			Entity.Position = org;
			trace = TraceBBox( Entity.Position, Entity.Position );
			if ( trace.Entity == null )
				return;
		}

		Entity.Position = test;
	}
	public virtual void CheckJumpButton()
	{
		//if ( !player->CanJump() )
		//    return false;


		/*
		if ( player->m_flWaterJumpTime )
		{
			player->m_flWaterJumpTime -= gpGlobals->frametime();
			if ( player->m_flWaterJumpTime < 0 )
				player->m_flWaterJumpTime = 0;

			return false;
		}*/



		// If we are in the water most of the way...
		if ( Swimming )
		{
			// swimming, not jumping
			ClearGroundEntity();

			Entity.Velocity = Entity.Velocity.WithZ( 100 );

			// play swimming sound
			//  if ( player->m_flSwimSoundTime <= 0 )
			{
				// Don't play sound again for 1 second
				//   player->m_flSwimSoundTime = 1000;
				//   PlaySwimSound();
			}

			return;
		}

		if ( Entity.GroundEntity == null )
			return;

		/*
		if ( player->m_Local.m_bDucking && (player->GetFlags() & FL_DUCKING) )
			return false;
		*/

		/*
		// Still updating the eye position.
		if ( player->m_Local.m_nDuckJumpTimeMsecs > 0u )
			return false;
		*/

		ClearGroundEntity();

		// player->PlayStepSound( (Vector &)mv->GetAbsOrigin(), player->m_pSurfaceData, 1.0, true );

		// MoveHelper()->PlayerSetAnimation( PLAYER_JUMP );

		float flGroundFactor = 1.0f;
		//if ( player->m_pSurfaceData )
		{
			//   flGroundFactor = g_pPhysicsQuery->GetGameSurfaceproperties( player->m_pSurfaceData )->m_flJumpFactor;
		}

		float flMul = (268.3281572999747f * Entity.Scale) * 1.2f;
		float startz = Entity.Velocity.z;

		Entity.Velocity = Entity.Velocity.WithZ( startz + flMul * flGroundFactor );

		Entity.Velocity -= new Vector3( 0, 0, (Gravity * Entity.Scale) * 0.5f ) * Time.Delta;

		// mv->m_outJumpVel.z += mv->m_vecVelocity[2] - startz;
		// mv->m_outStepHeight += 0.15f;

		// don't jump again until released
		//mv->m_nOldButtons |= IN_JUMP;

		AddEvent( "jump" );

	}
	public virtual void WaterSimulate()
	{
		if ( Entity.GetWaterLevel() > 0.4 )
		{
			CheckWaterJump();
		}

		// If we are falling again, then we must not trying to jump out of water any more.
		if ( (Entity.Velocity.z < 0.0f) && IsJumpingFromWater )
		{
			WaterJumpTime = 0.0f;
		}

		// Was jump button pressed?
		if ( Input.Down( "Jump" ) )
		{
			CheckJumpButton();
		}
		SetTag( "swimming" );
	}
	protected float WaterJumpTime { get; set; }
	protected Vector3 WaterJumpVelocity { get; set; }
	protected bool IsJumpingFromWater => WaterJumpTime > 0;
	protected TimeSince TimeSinceSwimSound { get; set; }
	protected float LastWaterLevel { get; set; }

	public virtual float WaterJumpHeight => 8;
	protected void CheckWaterJump()
	{
		// Already water jumping.
		if ( IsJumpingFromWater )
			return;

		// Don't hop out if we just jumped in
		// only hop out if we are moving up
		if ( Entity.Velocity.z < -180 )
			return;

		// See if we are backing up
		var flatvelocity = Entity.Velocity.WithZ( 0 );

		// Must be moving
		var curspeed = flatvelocity.Length;
		flatvelocity = flatvelocity.Normal;

		// see if near an edge
		var flatforward = Entity.Rotation.Forward.WithZ( 0 ).Normal;

		// Are we backing into water from steps or something?  If so, don't pop forward
		if ( curspeed != 0 && Vector3.Dot( flatvelocity, flatforward ) < 0 )
			return;

		var vecStart = Entity.Position + (mins + maxs) * .5f;
		var vecEnd = vecStart + flatforward * 24;

		var tr = TraceBBox( vecStart, vecEnd );
		if ( tr.Fraction == 1 )
			return;

		vecStart.z = Entity.Position.z + EyeHeight + WaterJumpHeight;
		vecEnd = vecStart + flatforward * 24;
		WaterJumpVelocity = tr.Normal * -50;

		tr = TraceBBox( vecStart, vecEnd );
		if ( tr.Fraction < 1.0 )
			return;

		// Now trace down to see if we would actually land on a standable surface.
		vecStart = vecEnd;
		vecEnd.z -= 1024;

		tr = TraceBBox( vecStart, vecEnd );
		if ( tr.Fraction < 1 && tr.Normal.z >= 0.7f )
		{
			Entity.Velocity = Entity.Velocity.WithZ( 256 * Entity.Scale );
			Entity.Tags.Add( "waterjump" );
			WaterJumpTime = 2000;
		}
	}
	public virtual void AirMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );

		Entity.Velocity += Entity.BaseVelocity;

		Move();

		Entity.Velocity -= Entity.BaseVelocity;
	}

	public virtual void WaterMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		wishspeed *= 0.8f;

		Accelerate( wishdir, wishspeed, 100 * Entity.Scale, Acceleration );

		Entity.Velocity += Entity.BaseVelocity;

		Move();

		Entity.Velocity -= Entity.BaseVelocity;
	}

	bool IsTouchingLadder = false;
	Vector3 LadderNormal;

	Vector3 LastNonZeroWishLadderVelocity;
	public virtual void CheckLadder()
	{
		var pl = Entity as Player;

		var wishvel = new Vector3( pl.InputDirection.x.Clamp( -1f, 1f ), pl.InputDirection.y.Clamp( -1f, 1f ), 0 );
		if ( wishvel.Length > 0 )
		{
			LastNonZeroWishLadderVelocity = wishvel;
		}
		if ( TryLatchNextTickCounter > 0 ) wishvel = LastNonZeroWishLadderVelocity * -1;
		wishvel *= pl.ViewAngles.WithPitch( 0 ).ToRotation();
		wishvel = wishvel.Normal;


		if ( IsTouchingLadder )
		{
			if ( Input.Pressed( "Jump" ) )
			{
				var sidem = (Math.Abs( Entity.ViewAngles.ToRotation().Forward.Abs().z - 1 ) * 3).Clamp( 0, 1 );
				var upm = Entity.ViewAngles.ToRotation().Forward.z;

				var Eject = new Vector3();

				Eject.x = LadderNormal.x * sidem;
				Eject.y = LadderNormal.y * sidem;
				Eject.z = (3 * upm).Clamp( 0, 1 );

				Entity.Velocity += (Eject * 180.0f) * Entity.Scale;
				IsTouchingLadder = false;

				return;

			}
			else if ( Entity.GroundEntity != null && LadderNormal.Dot( wishvel ) > 0 )
			{
				IsTouchingLadder = false;

				return;
			}
		}

		const float ladderDistance = 1.0f;
		var start = Entity.Position;
		Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithTag( "ladder" )
					.Ignore( Entity )
					.Run();

		IsTouchingLadder = false;

		if ( pm.Hit )
		{
			IsTouchingLadder = true;
			LadderNormal = pm.Normal;
		}
	}
	public virtual void LadderMove()
	{
		var velocity = WishVelocity;
		float normalDot = velocity.Dot( LadderNormal );

		var cross = LadderNormal * normalDot;
		Entity.Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

		Move();
	}

	Entity PreviousGroundEntity;
	int TryLatchNextTickCounter = 0;
	Vector3 LastNonZeroWishVelocity;
	[ConVar.Replicated( "sv_ladderlatchdebug" )]
	public static bool LatchDebug { get; set; } = false;
	public virtual void LatchOntoLadder()
	{
		if ( !WishVelocity.Normal.IsNearlyZero( 0.001f ) )
		{
			LastNonZeroWishVelocity = WishVelocity;
		}
		if ( TryLatchNextTickCounter > 0 )
		{

			Entity.Velocity = (LastNonZeroWishVelocity.Normal * -100).WithZ( Entity.Velocity.z );
			TryLatchNextTickCounter++;
		}
		if ( TryLatchNextTickCounter >= 10 )
		{
			TryLatchNextTickCounter = 0;
		}

		if ( Entity.GroundEntity != null ) return;
		if ( PreviousGroundEntity == null ) return;
		var pos = Entity.Position + (Vector3.Down * 16);
		//var tr = TraceBBox( pos, pos );

		var tr = Trace.Ray( pos, pos - (LastNonZeroWishVelocity.Normal * 8) )
					.Size( mins, maxs )
					.WithTag( "ladder" )
					.Ignore( Entity )
					.Run();

		if ( LatchDebug ) DebugOverlay.Line( Entity.Position, pos, 10 );
		if ( LatchDebug ) DebugOverlay.Line( tr.StartPosition, tr.EndPosition, 10 );
		if ( tr.Hit )
		{
			Entity.Velocity = Vector3.Zero.WithZ( Entity.Velocity.z );
			TryLatchNextTickCounter++;
		}

	}


	public virtual void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		// Doing this before we move may introduce a potential latency in water detection, but
		// doing it after can get us stuck on the bottom in water if the amount we move up
		// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
		// this several times per frame, so we really need to avoid sticking to the bottom of
		// water on each call, and the converse case will correct itself if called twice.
		//CheckWater();

		var point = Entity.Position - Vector3.Up * (2 * Entity.Scale);
		var vBumpOrigin = Entity.Position;

		//
		//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
		//
		bool bMovingUpRapidly = Entity.Velocity.z > MaxNonJumpVelocity;
		bool bMovingUp = Entity.Velocity.z > 0;

		bool bMoveToEndPos = false;

		if ( Entity.GroundEntity != null ) // and not underwater
		{
			bMoveToEndPos = true;
			point.z -= StepSize * Entity.Scale;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize * Entity.Scale;
		}

		if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Entity.Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Entity.Position = pm.EndPosition;
		}

	}

	public Vector3 GroundNormal { get; set; }
	/// <summary>
	/// We have a new ground entity
	/// </summary>
	public virtual void UpdateGroundEntity( TraceResult tr )
	{
		GroundNormal = tr.Normal;

		// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
		// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
		// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		//if ( tr.Entity == GroundEntity ) return;

		Vector3 oldGroundVelocity = default;
		if ( Entity.GroundEntity != null ) oldGroundVelocity = Entity.GroundEntity.Velocity;

		bool wasOffGround = Entity.GroundEntity == null;

		Entity.GroundEntity = tr.Entity;

		if ( Entity.GroundEntity != null )
		{
			Entity.BaseVelocity = Entity.GroundEntity.Velocity;
		}
	}

	/// <summary>
	/// We're no longer on the ground, remove it
	/// </summary>
	public virtual void ClearGroundEntity()
	{
		if ( Entity.GroundEntity == null ) return;

		Entity.GroundEntity = null;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1.0f;
	}

	/// <summary>
	/// Traces the current bbox and returns the result.
	/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, mins, maxs, liftFeet );
	}

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			liftFeet *= Entity.Scale;
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
					.Ignore( Entity )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	/// <summary>
	/// Try to keep a walking player on the ground when running down slopes etc
	/// </summary>
	public virtual void StayOnGround()
	{
		var start = Entity.Position + Vector3.Up * (2 * Entity.Scale);
		var end = Entity.Position + Vector3.Down * (StepSize * Entity.Scale);

		// See how far up we can go without getting stuck
		var trace = TraceBBox( Entity.Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		// This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
		// float flDelta = fabs( mv->GetAbsOrigin().z - trace.m_vEndPos.z );
		// if ( flDelta > 0.5f * DIST_EPSILON )

		Entity.Position = trace.EndPosition;
	}

	public Transform? GroundTransform;
	public Sandbox.Entity? OldGroundEntity;
	void RestoreGroundPos()
	{
		if ( Entity.GroundEntity == null || Entity.GroundEntity.IsWorld || GroundTransform == null || Entity.GroundEntity != OldGroundEntity )
			return;

		var worldTrns = Entity.GroundEntity.Transform.ToWorld( GroundTransform.Value );
		if ( Prediction.FirstTime )
		{
			Entity.BaseVelocity = ((Entity.Position - worldTrns.Position) * -1) / Time.Delta;
		}
		//Entity.Position = (Entity.Position.WithZ( worldTrns.Position.z ));
	}

	void SaveGroundPos()
	{
		var ply = Entity as Player;
		if ( Entity.GroundEntity == null || Entity.GroundEntity.IsWorld )
		{
			OldGroundEntity = null;
			GroundTransform = null;
			return;
		}

		if ( Entity.GroundEntity == OldGroundEntity )
		{
			GroundTransform = Entity.GroundEntity.Transform.ToLocal( Entity.Transform );
		}
		else
		{
			GroundTransform = null;
			GroundTransformViewAngles = null;
		}
		OldGroundEntity = Entity.GroundEntity;
	}

	public Transform? GroundTransformViewAngles;
	public Angles? PreviousViewAngles;
	void RestoreGroundAngles()
	{
		if ( Entity.GroundEntity == null || Entity.GroundEntity.IsWorld || GroundTransformViewAngles == null || PreviousViewAngles == null )
			return;

		var ply = Entity as Player;
		var worldTrnsView = Entity.GroundEntity.Transform.ToWorld( GroundTransformViewAngles.Value );
		ply.ViewAngles -= (PreviousViewAngles.Value.ToRotation() * worldTrnsView.Rotation.Inverse).Angles().WithPitch( 0 ).WithRoll( 0 );
	}
	void SaveGroundAngles()
	{

		if ( Entity.GroundEntity == null || Entity.GroundEntity.IsWorld )
		{
			GroundTransformViewAngles = null;
			return;
		}

		var ply = Entity as Player;
		GroundTransformViewAngles = Entity.GroundEntity.Transform.ToLocal( new Transform( Vector3.Zero, ply.ViewAngles.ToRotation() ) );
		PreviousViewAngles = ply.ViewAngles;
	}
	bool PushDebug = false;
	[SkipHotload] Dictionary<int, Transform> OldTransforms;
	Transform OldTransform;
	void DoPushingStuff()
	{
		var tr = TraceBBox( Entity.Position, Entity.Position );
		if ( tr.StartedSolid
			&& tr.Entity != null
			&& tr.Entity != OldGroundEntity
			&& tr.Entity != Entity.GroundEntity
			&& !tr.Entity.IsWorld
			&& OldTransforms != null
			&& OldTransforms.TryGetValue( tr.Entity.NetworkIdent, out var oldTransform ) )
		{
			if ( tr.Entity is BasePhysics ) return;
			var oldPosition = Entity.Position;
			var oldTransformLocal = oldTransform.ToLocal( Entity.Transform );
			var newTransform = tr.Entity.Transform.ToWorld( oldTransformLocal );

			// this used to be just the direction of the tr delta however pushing outwards a llittle seems more appropriate
			var direction = ((Entity.Position - newTransform.Position) * -1);
			direction += (Entity.Position - tr.Entity.Position).Normal.WithZ( 0 ) * 0.8f;

			FindIdealMovementDirection( newTransform.Position, direction, out var outOffset, out var outDirection );


			var newPosition = newTransform.Position + (outDirection * outOffset) + (outDirection * 0.1f);

			// Check if we're being crushed, if not we set our position.
			if ( IsBeingCrushed( newPosition ) )
			{
				OnCrushed( tr.Entity );
			}
			else
			{
				Entity.Velocity += (outDirection / Time.Delta);

				// insurance we dont instantly get stuck again add a little extra.
				Entity.Position = newPosition;

			}
		}

		// In order to get the delta of transforms we have not yet touched, we grab the transforms of EVERYTHING with in a radius.
		// The radius we look in is determined by player speed and velocity 
		GetPossibleTransforms();
	}

	void FindIdealMovementDirection( Vector3 Position, Vector3 Direction, out float OutOffset, out Vector3 OutDirection )
	{
		OutDirection = Direction;
		OutOffset = 0;
		// ------------------------ shit ------------------------
		//			look into doing this nicer at somepoint
		// ------------------------------------------------------
		// brute force our way into finding how much extra we need to be pushed in the case of AABB edges being still inside of the object
		for ( int i = 0; i < 512; i++ )
		{
			var possibleoffset = (float)(i) / 16f;
			var pos = Position + (Direction * possibleoffset);

			var offsettr = TraceBBox( pos, pos );

			if ( !offsettr.StartedSolid )
			{
				if ( PushDebug ) DebugOverlay.Line( Entity.Position, pos, Color.Green, 5 );
				OutDirection = Direction;
				OutOffset = possibleoffset;
				break;
			}

			//sidewards test, for things moving sideways and upwards or downwards
			var posside = Position + (Direction.WithZ( 0 ) * possibleoffset);
			var offsettrside = TraceBBox( posside, posside );

			if ( !offsettrside.StartedSolid )
			{
				if ( PushDebug ) DebugOverlay.Line( Entity.Position, pos, Color.Green, 5 );
				OutDirection = Direction.WithZ( 0 );
				OutOffset = possibleoffset;

				break;
			}

			if ( PushDebug ) DebugOverlay.Line( Entity.Position, pos, Color.Red, 5 );
		}
		// ------------------------------------------------------
	}

	bool IsBeingCrushed( Vector3 NewPosition )
	{
		//do a trace that will decide whether or not we're being crushed
		var crushtrace = Trace.Ray( Entity.Position, NewPosition )
				.Ignore( Entity )
				.Run();

		if ( PushDebug ) DebugOverlay.Line( crushtrace.StartPosition, crushtrace.EndPosition, Color.Blue, 5 );

		return crushtrace.Fraction != 1;
	}

	void OnCrushed( Entity CurshingEntity )
	{
		// deal crush damage!
		if ( !Game.IsServer ) return;
		if ( CurshingEntity is DoorEntity || CurshingEntity is PlatformEntity || CurshingEntity is PathPlatformEntity )
		{
			Entity.TakeDamage( DamageInfo.Generic( 5 ).WithTag( "crush" ) );
		}

		// if we get crushed by a door, change its direction.
		if ( CurshingEntity is DoorEntity door )
		{
			if ( door.State == DoorEntity.DoorState.Opening )
			{
				door.Close();
				door.Close();
			}
			else if ( door.State == DoorEntity.DoorState.Closing )
			{
				door.Open();
				door.Open();
			}
		}
	}
	/*void DoPushingStuff()
	{
		var tr = TraceBBox( Entity.Position, Entity.Position );
		if ( tr.StartedSolid && tr.Entity != null && !tr.Entity.IsWorld && tr.Entity != OldGroundEntity && tr.Entity != Entity.GroundEntity )
		{
			var tf = new Transform();
			if ( OldTransforms != null && OldTransforms.TryGetValue( tr.Entity.NetworkIdent, out tf ) )
			{
				var x = tf.ToLocal( Entity.Transform );
				var x2 = tr.Entity.Transform.ToWorld( x );

				// grab the delta between the last ticks position and this ticks position
				var x3 = ((Entity.Position - x2.Position) * -1);//.WithZ( 0 );
				var x4 = ((Entity.Position - x2.Position) * -1).WithZ( 0 );

				// grab the delta between the last ticks position and this ticks position
				var oldpos = Entity.Position;
				var oldvel = Entity.Velocity;
				bool unstuck = false;
				for ( int i = 0; i < 2048; i++ )
				{
					var ch = (x3 * (i / 16.0f));
					var pos = Entity.Position + ch;


					tr.Entity.ResetInterpolation();
					Entity.ResetInterpolation();
					var tr2 = TraceBBox( pos, pos );

					if ( !tr2.StartedSolid )
					{
						var x5 = ((oldpos - pos) * -1);
						var tr3 = Trace.Ray( Entity.Position, pos + x3 ).Ignore( tr.Entity ).Ignore( Entity ).Run();


						if ( tr3.Fraction != 1 )
						{
							var ch2 = (x4 * (i / 16.0f));
							var pos2 = Entity.Position + ch2;
							var tr4 = TraceBBox( pos2, pos2 );
							var tr5 = Trace.Ray( Entity.Position, pos2 + x4 ).Ignore( tr.Entity ).Ignore( Entity ).Run();
							if ( !tr4.StartedSolid && tr5.Fraction == 1 )
							{
								x3 = x4;
								ch = ch2;
								pos = pos2;
							}
							else
							{
								//crush / stuck 
								if ( tr.Entity is DoorEntity door )
								{
									if ( Game.IsServer )
									{
										if ( door.State == DoorEntity.DoorState.Opening )
											door.Close();
										if ( door.State == DoorEntity.DoorState.Closing )
											door.Open();

										Entity.TakeDamage( DamageInfo.Generic( 10 ).WithTag( "crush" ) );
									}
									GetPossibleTransforms();
									OldTransforms.Remove( tr.Entity.NetworkIdent );
									if ( PushDebug ) DebugOverlay.Line( Entity.Position, pos, Color.Red, 5 );
									return;
								}
								else
								{
									if ( Game.IsServer )
									{
										Entity.TakeDamage( DamageInfo.Generic( 10 ).WithTag( "crush" ) );
									}
									if ( PushDebug ) DebugOverlay.Line( Entity.Position, pos, Color.Red, 5 );
									continue;
								}
							}
						}
						if ( PushDebug ) DebugOverlay.Line( Entity.Position, pos, Color.Green, 5 );
						if ( !(x3 / Time.Delta).AlmostEqual( Vector3.Zero, 0.01f ) )
						{
							Entity.Velocity += (x3 / Time.Delta);
							Entity.Position = pos + x3;
						}
						//Entity.Velocity = (x5 / Time.Delta);
						unstuck = true;
						break;
					}
					else
					{
						if ( PushDebug ) DebugOverlay.Line( Entity.Position, pos, Color.Red, 5 );
					}
				}
				if ( unstuck )
				{
					if ( PushDebug ) DebugOverlay.Text( "unstuck", Entity.Position, Color.Green );
				}
				else
				{

					if ( PushDebug ) DebugOverlay.Text( "failed", Entity.Position, Color.Red );
				}
			}
		}
		GetPossibleTransforms();
	}*/

	void GetPossibleTransforms()
	{

		var a = Sandbox.Entity.FindInSphere( Entity.Position, 512 + Entity.Velocity.Length );
		var b = new Dictionary<int, Transform>();
		foreach ( var i in a )
		{
			b.Add( i.NetworkIdent, i.Transform );
		}
		OldTransforms = b;
		OldTransform = Entity.Transform;
	}
}
