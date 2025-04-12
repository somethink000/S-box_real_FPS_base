using Sandbox;
using System;
using System.Numerics;
using static Sandbox.CursorSettings;

namespace GeneralGame;


public partial class ViewModel : Component
{
	public ModelRenderer ViewModelRenderer { get; set; }
	public SkinnedModelRenderer ViewModelHandsRenderer { get; set; }
	public Carriable Carriable { get; set; }
	protected Player ply => Carriable.Owner;
	public CameraComponent Camera { get; set; }
	public bool ShouldDraw { get; set; }

	protected float animSpeed { get; set; } = 1;

	// Target animation values
	protected Vector3 targetVectorPos;
	protected Vector3 targetVectorRot;

	// Finalized animation values
	protected Vector3 finalVectorPos;
	protected Vector3 finalVectorRot;

	// Sway
	protected Rotation lastEyeRot;

	// Jumping Animation
	protected float jumpTime;
	protected float landTime;


	// Helpful values
	protected Vector3 localVel;



	public void OnHolster()
	{

		Destroy();

	}

	protected override void OnUpdate()
	{

		if ( ply == null ) return;

		var renderType = ShouldDraw ? ModelRenderer.ShadowRenderType.Off : ModelRenderer.ShadowRenderType.ShadowsOnly;
		ViewModelRenderer.Enabled = ply.IsFirstPerson;
		ViewModelRenderer.RenderType = renderType;

		if ( ViewModelHandsRenderer is not null )
		{
			ViewModelHandsRenderer.Enabled = ply.IsFirstPerson;
			ViewModelHandsRenderer.RenderType = renderType;
		}

		if ( !ply.IsFirstPerson ) return;


		// For particles & lighting
		Camera.Transform.Position = Scene.Camera.Transform.Position;
		Camera.Transform.Rotation = Scene.Camera.Transform.Rotation;

		Transform.Position = Camera.Transform.Position;
		Transform.Rotation = Camera.Transform.Rotation;

		// Smoothly transition the vectors with the target values
		finalVectorPos = finalVectorPos.LerpTo( targetVectorPos, animSpeed * RealTime.Delta );
		finalVectorRot = finalVectorRot.LerpTo( targetVectorRot, animSpeed * RealTime.Delta );

		animSpeed = 10 * Carriable.AnimSpeed;

		// Change the angles and positions of the viewmodel with the new vectors
		Transform.Rotation *= Rotation.From( finalVectorRot.x, finalVectorRot.y, finalVectorRot.z );
		// Position has to be set after rotation!
		Transform.Position += finalVectorPos.z * Transform.Rotation.Up + finalVectorPos.y * Transform.Rotation.Forward + finalVectorPos.x * Transform.Rotation.Right;
		//player.CurFOV = finalPlayerFOV;

		// Initialize the target vectors for this frame
		targetVectorPos = Vector3.Zero;
		targetVectorRot = Vector3.Zero;

		// I'm sure there's something already that does this for me, but I spend an hour
		// searching through the wiki and a bunch of other garbage and couldn't find anything...
		// So I'm doing it manually. Problem solved.
		var eyeRot = ply.CameraController.EyeAngles.ToRotation();
		localVel = new Vector3( eyeRot.Right.Dot( ply.MovementController.Velocity ), eyeRot.Forward.Dot( ply.MovementController.Velocity ), ply.MovementController.Velocity.z );

		HandleIdleAnimation();
		HandleWalkAnimation();
		HandleJumpAnimation();

		// Tucking
		var shouldTuck = Carriable.ShouldTuck( out var tuckDist );
		if ( Carriable.RunAnimData != AngPos.Zero && shouldTuck )
		{
			var animationCompletion = Math.Min( 1, ((Carriable.TuckRange - tuckDist) / Carriable.TuckRange) + 0.5f );
			targetVectorPos += Carriable.RunAnimData.Pos * animationCompletion;
			targetVectorRot += MathUtil.ToVector3( Carriable.RunAnimData.Angle * animationCompletion );
			return;
		}

		HandleSwayAnimation();
		HandleSprintAnimation();
	}

	void HandleIdleAnimation()
	{

		// Perform a "breathing" animation
		var breatheTime = RealTime.Now * 2.0f;
		targetVectorPos -= new Vector3( MathF.Cos( breatheTime / 4.0f ) / 8.0f, 0.0f, -MathF.Cos( breatheTime / 4.0f ) / 32.0f );
		targetVectorRot -= new Vector3( MathF.Cos( breatheTime / 5.0f ), MathF.Cos( breatheTime / 4.0f ), MathF.Cos( breatheTime / 7.0f ) );

		// Crouching animation
		if ( Input.Down( InputButtonHelper.Duck ) && ply.MovementController.IsOnGround )
			targetVectorPos += new Vector3( -1.0f, -1.0f, 0.5f );
	}


	void HandleWalkAnimation()
	{
		var breatheTime = RealTime.Now * 16.0f;
		var walkSpeed = new Vector3( ply.MovementController.Velocity.x, ply.MovementController.Velocity.y, 0.0f ).Length;
		var maxWalkSpeed = 200.0f;
		var roll = 0.0f;
		var yaw = 0.0f;

		// Check if on the ground
		if ( !ply.MovementController.IsOnGround )
			return;

		// Check if sprinting
		if ( ply.MovementController.IsRunning )
		{
			breatheTime = RealTime.Now * 18.0f;
			maxWalkSpeed = 100.0f;
		}

		// Check for sideways velocity to sway the gun slightly
		if ( localVel.x > 0.0f )
			roll = -7.0f * (localVel.x / maxWalkSpeed);
		else if ( localVel.x < 0.0f )
			yaw = 3.0f * (localVel.x / maxWalkSpeed);


		// Perform walk cycle
		targetVectorPos -= new Vector3( (-MathF.Cos( breatheTime / 2.0f ) / 5.0f) * walkSpeed / maxWalkSpeed - yaw / 4.0f, 0.0f, 0.0f );
		targetVectorRot -= new Vector3( (Math.Clamp( MathF.Cos( breatheTime ), -0.3f, 0.3f ) * 2.0f) * walkSpeed / maxWalkSpeed, (-MathF.Cos( breatheTime / 2.0f ) * 1.2f) * walkSpeed / maxWalkSpeed - yaw * 1.5f, roll );
	}


	void HandleSwayAnimation()
	{
		var swayspeed = 5;


		// Lerp the eye position
		lastEyeRot = Rotation.Lerp( lastEyeRot, ply.Camera.Transform.Rotation, swayspeed * RealTime.Delta );

		// Calculate the difference between our current eye angles and old (lerped) eye angles
		var angDif = ply.Camera.Transform.Rotation.Angles() - lastEyeRot.Angles();
		angDif = new Angles( angDif.pitch, MathX.RadianToDegree( MathF.Atan2( MathF.Sin( MathX.DegreeToRadian( angDif.yaw ) ), MathF.Cos( MathX.DegreeToRadian( angDif.yaw ) ) ) ), 0 );

		// Perform sway
		targetVectorPos += new Vector3( Math.Clamp( angDif.yaw * 0.04f, -1.5f, 1.5f ), 0.0f, Math.Clamp( angDif.pitch * 0.04f, -1.5f, 1.5f ) );
		targetVectorRot += new Vector3( Math.Clamp( angDif.pitch * 0.2f, -4.0f, 4.0f ), Math.Clamp( angDif.yaw * 0.2f, -4.0f, 4.0f ), 0.0f );
	}


	void HandleSprintAnimation()
	{
		if ( Carriable.IsRunning && Carriable.RunAnimData != AngPos.Zero )
		{
			targetVectorPos += Carriable.RunAnimData.Pos;
			targetVectorRot += MathUtil.ToVector3( Carriable.RunAnimData.Angle );
		}
	}

	void HandleJumpAnimation()
	{
		// If we're not on the ground, reset the landing animation time
		if ( !ply.MovementController.IsOnGround )
			landTime = RealTime.Now + 0.31f;

		// Reset the timers once they elapse
		if ( landTime < RealTime.Now && landTime != 0.0f )
		{
			landTime = 0.0f;
			jumpTime = 0.0f;
		}

		// If we jumped, start the animation
		if ( Input.Down( InputButtonHelper.Jump ) && jumpTime == 0.0f )
		{
			jumpTime = RealTime.Now + 0.31f;
			landTime = 0.0f;
		}

		// If we're not ironsighting, do a fancy jump animation
		if ( jumpTime > RealTime.Now )
		{
			// If we jumped, do a curve upwards
			var f = 0.31f - (jumpTime - RealTime.Now);
			var xx = MathUtil.BezierY( f, 0.0f, -4.0f, 0.0f );
			var yy = 0.0f;
			var zz = MathUtil.BezierY( f, 0.0f, -2.0f, -5.0f );
			var pt = MathUtil.BezierY( f, 0.0f, -4.36f, 10.0f );
			var yw = xx;
			var rl = MathUtil.BezierY( f, 0.0f, -10.82f, -5.0f );
			targetVectorPos += new Vector3( xx, yy, zz ) / 4.0f;
			targetVectorRot += new Vector3( pt, yw, rl ) / 4.0f;
			animSpeed = 20.0f;
		}
		else if ( !ply.MovementController.IsOnGround )
		{
			// Shaking while falling
			var breatheTime = RealTime.Now * 30.0f;
			targetVectorPos += new Vector3( MathF.Cos( breatheTime / 2.0f ) / 16.0f, 0.0f, -5.0f + (MathF.Sin( breatheTime / 3.0f ) / 16.0f) ) / 4.0f;
			targetVectorRot += new Vector3( 10.0f - (MathF.Sin( breatheTime / 3.0f ) / 4.0f), MathF.Cos( breatheTime / 2.0f ) / 4.0f, -5.0f ) / 4.0f;
			animSpeed = 20.0f;
		}
		else if ( landTime > RealTime.Now )
		{
			// If we landed, do a fancy curve downwards
			var f = landTime - RealTime.Now;
			var xx = MathUtil.BezierY( f, 0.0f, -4.0f, 0.0f );
			var yy = 0.0f;
			var zz = MathUtil.BezierY( f, 0.0f, -2.0f, -5.0f );
			var pt = MathUtil.BezierY( f, 0.0f, -4.36f, 10.0f );
			var yw = xx;
			var rl = MathUtil.BezierY( f, 0.0f, -10.82f, -5.0f );
			targetVectorPos += new Vector3( xx, yy, zz ) / 2.0f;
			targetVectorRot += new Vector3( pt, yw, rl ) / 2.0f;
			animSpeed = 20.0f;
		}
	}
}
