using Sandbox;
using System;
using System.Numerics;

namespace GeneralGame;


public sealed class ViewModel : Component
{
	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }
	[Property] public bool UseSprintAnimation { get; set; }
	
	private Rotation CurRotation { get; set; }
	private Vector3 CurPos { get; set; }
	private float CurFOV { get; set; }


	private float InertiaDamping => 15.0f;


	private Vector3 swingOffset;
	private float lastPitch;
	private float lastYaw;
	private float bobAnim;
	private float bobSpeed;

	private float SwingInfluence => 0.05f;
	private float ReturnSpeed => 20.0f;
	private float MaxOffsetLength => 0.5f;
	private float BobCycleTime => 10;
	
	private Vector3 BobDirection => new Vector3( 0.0f, 0.1f, 0.09f );
	private Rotation CurSmoothRotate { get; set; }
	private Rotation lastCameraCalc { get; set; }

	public float YawInertia { get; private set; }
	public float PitchInertia { get; private set; }
	

	private PlayerController PlayerController => Weapon.Components.GetInAncestors<PlayerController>();
	private CameraComponent Camera { get; set; }
	private WeaponComponent Weapon { get; set; }
	private BaseGun Gun { get; set; }

	public void SetWeaponComponent( WeaponComponent weapon )
	{
		Weapon = weapon;
		Gun = Weapon.Components.GetInDescendantsOrSelf<BaseGun>( true );
	}
	
	public void SetCamera( CameraComponent camera )
	{
		Camera = camera;
	}
	
	protected override void OnStart()
	{

		ModelRenderer.Set( "b_deploy", true );


		Transform.LocalPosition = Vector3.Zero;
		CurRotation = Rotation.Identity;
		CurSmoothRotate = Rotation.Identity;
		lastCameraCalc = Camera.Transform.Rotation;
		
	}

	protected override void OnDestroy()
	{
		if ( IsProxy )
		{
			return;
		}
		
		base.OnDestroy();
	}

	protected override void OnAwake()
	{
		if ( IsProxy )
		{
			GameObject.Enabled = false;
			ModelRenderer.Enabled = false;
			return;
		}

		base.OnAwake();
	}


	protected override void OnFixedUpdate()
	{
		Vector3 plusPos = Vector3.Zero + Weapon.idlePos;

		if ( Gun.IsValid() )
		{ 

			float plySettingsFov = Preferences.FieldOfView;


			if ( PlayerController.IsAiming )
			{ 
				CurPos = CurPos.LerpTo( plusPos + Gun.aimPos, Time.Delta * 10f );
				CurFOV = CurFOV.LerpTo( Screen.CreateVerticalFieldOfView( plySettingsFov - Gun.AimFOVDec ), Time.Delta * 10f );
			
			}
			else
			{
				CurPos = CurPos.LerpTo( plusPos, Time.Delta * 10f );
				CurFOV = CurFOV.LerpTo( Screen.CreateVerticalFieldOfView( plySettingsFov ), Time.Delta * 10f );
			}
			ModelRenderer.Set( "b_aiming", PlayerController.IsAiming );
		

		
		



		
			if ( PlayerController.MoveSpeed > 150f )
			{
				CurRotation = Rotation.Lerp( CurRotation, Rotation.Identity * Gun.runRotation, Time.Delta * 10f );
			}
			else
			{
				CurRotation = Rotation.Lerp( CurRotation, Rotation.Identity, Time.Delta * 10f );
			}

		

			
			Camera.FieldOfView = CurFOV;

		}
		else 
		{
			CurRotation = Rotation.Lerp( CurRotation, Rotation.Identity, Time.Delta * 10f );
			CurPos = CurPos.LerpTo( plusPos, Time.Delta * 10f );
		}


		

		CalcShakeMoves();
		CalcRotateSmooth();

		Transform.LocalRotation = CurRotation;
		Transform.LocalPosition = CurPos;

	}

	private void CalcRotateSmooth()
	{
		//TODO make this beter
		float CurX;
		float CurY;

		Rotation curCameraCalc = Camera.Transform.Rotation;


		CurX = lastCameraCalc.Yaw() - curCameraCalc.Yaw();
		CurY = lastCameraCalc.Pitch() - curCameraCalc.Pitch();


		if ( PlayerController.IsAiming )
		{
			CurSmoothRotate = Rotation.From( 0, 0, 0 );
		}
		else
		{
			CurSmoothRotate = Rotation.From( Math.Clamp( CurY, -1, 1 ), Math.Clamp( CurX, -1, 1 ), 0 );
		}
		
		CurRotation *= CurSmoothRotate;

		lastCameraCalc = Rotation.Lerp( lastCameraCalc, curCameraCalc, Time.Delta * 30f );
	}

	private void CalcShakeMoves()
	{
		var newPitch = CurRotation.Pitch(); 
		var newYaw = CurRotation.Yaw();

		var pitchDelta = Angles.NormalizeAngle( newPitch - lastPitch );
		var yawDelta = Angles.NormalizeAngle( lastYaw - newYaw );

		PitchInertia += pitchDelta;
		YawInertia += yawDelta;


		var playerVelocity = PlayerController.CharacterController.Velocity;


		var verticalDelta = playerVelocity.z * Time.Delta;
		var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
		verticalDelta *= 1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y );
		pitchDelta -= verticalDelta * 1.0f;

		var speed = playerVelocity.WithZ( 0 ).Length;
		speed = speed > 10.0 ? speed : 0.0f;


		if ( speed > 0f && PlayerController.IsAiming )
		{
			speed = 10f;
		}


		bobSpeed = bobSpeed.LerpTo( speed, Time.Delta * InertiaDamping );


		var offset = CalcBobbingOffset( bobSpeed );
		offset += CalcSwingOffset( pitchDelta, yawDelta );

		CurPos += offset;


		lastPitch = newPitch;
		lastYaw = newYaw;

		YawInertia = YawInertia.LerpTo( 0, Time.Delta * InertiaDamping );
		PitchInertia = PitchInertia.LerpTo( 0, Time.Delta * InertiaDamping );
	}
	private Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		var swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

		swingOffset -= swingOffset * ReturnSpeed * Time.Delta;
		swingOffset += (swingVelocity * SwingInfluence);

		if ( swingOffset.Length > MaxOffsetLength )
		{
			swingOffset = swingOffset.Normal * MaxOffsetLength;
		}

		return swingOffset;
	}

	private Vector3 CalcBobbingOffset( float speed )
	{
		bobAnim += Time.Delta * BobCycleTime;

		var twoPI = System.MathF.PI * 2.0f;

		if ( bobAnim > twoPI )
		{
			bobAnim -= twoPI;
		}

		var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
		offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

		return offset;
	}

}
