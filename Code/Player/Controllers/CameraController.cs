

using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Sandbox.CursorSettings;

namespace GeneralGame;


public class CameraController : Component
{
	[RequireComponent] public Player ply { get; set; }

	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public Vector3 EyeOffset { get; set; } = Vector3.Zero;
	public Vector3 EyePos => ply.Head.Transform.Position + EyeOffset;


	public float CurFOV { get; set; }
	public float TargetFov { get; set; } = 0;

	public Vector3 CamPos { get; set; } = Vector3.Zero;
	public Rotation ZeroRotation { get; set; }
	public Angles RotationMultyplier { get; set; }
	public Rotation TargetRotation { get; set; }


	public float InputSensitivity { get; set; } = 1f;
	public Angles EyeAnglesOffset { get; set; }

	public bool IsFirstPerson = true;
	private float fovSpeed = 5f;


	TimeSince timeSinceShake;
	float shakeSpeed;
	private Vector3 CurShakePos { get; set; }
	private Angles CurShakeRot { get; set; } 


	protected override void OnAwake()
	{
		CurFOV = Preferences.FieldOfView;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;


		ApplyFov( ply.MovementController.Velocity.WithZ( 0 ).Length / 40 );


		// Rotate the head based on mouse movement
		var eyeAngles = EyeAngles;

		Input.AnalogLook *= InputSensitivity;
		eyeAngles.pitch += Input.AnalogLook.pitch;
		eyeAngles.yaw += Input.AnalogLook.yaw;
		eyeAngles += EyeAnglesOffset;
		EyeAnglesOffset = Angles.Lerp( EyeAnglesOffset, Angles.Zero, 0.4f );
		InputSensitivity = 1;

		eyeAngles.roll = 0;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.9f, 89.9f );

		EyeAngles = eyeAngles;

		// Set the current camera offset
		var targetOffset = Vector3.Zero;
		//if ( IsCrouching || IsSlide ) targetOffset += Vector3.Down * 32f;
		EyeOffset = Vector3.Lerp( EyeOffset, targetOffset, Time.Delta * 10f );

		// Set position of the camera
		if ( Scene.Camera is not null )
		{
			var camPos = EyePos;
			if ( !IsFirstPerson )
			{

				var camForward = eyeAngles.ToRotation().Forward + (new Vector3( 0.2f, 0.5f, 0 ) * eyeAngles);
				var camTrace = Scene.Trace.Ray( camPos, camPos - (camForward) ) //* Distance
					.WithoutTags( TagsHelper.Player, TagsHelper.Trigger, TagsHelper.ViewModel, TagsHelper.Weapon )
					.Run();

				if ( camTrace.Hit )
				{
					// Add normal to prevent clipping
					camPos = camTrace.HitPosition + camTrace.Normal;
				}
				else
				{
					camPos = camTrace.EndPosition;
				}

				CamPos = camPos;
			}

			//Camera.Transform.Local = Camera.Transform.Local.RotateAround( EyePos, EyeAngles.WithYaw( 0f ) );

			//EyeAnglesOffset
			eyeAngles += CurShakeRot;
			ZeroRotation = eyeAngles.ToRotation();

			
			HandleCameraFov();

			//Log.Info(CurShakePos);
			ply.Camera.WorldRotation = ZeroRotation;
			ply.Camera.WorldPosition = camPos + CurShakePos; 

			HandleScreenShake();
		}

	}

	public void ApplyFov( float multiplyer )
	{
		TargetFov += multiplyer;


	}

	public void ApplyShake( float multiplyer, float duration )
	{
		var random = new Random();
		CurShakePos = new Vector3( random.Float( 0, multiplyer ), random.Float( 0, multiplyer ), random.Float( 0, multiplyer ) );
		CurShakeRot = new Angles( random.Float( 0, 2 ), random.Float( 0, 2 ), 0 );

	}

	private void HandleScreenShake()
	{
		CurShakePos = Vector3.Lerp( CurShakePos, Vector3.Zero, 0.4f );
		CurShakeRot = Rotation.Lerp( CurShakeRot, Angles.Zero, 0.4f );
	}
	private void HandleCameraFov()
	{
		CurFOV = MathX.LerpTo( CurFOV, TargetFov, fovSpeed * RealTime.Delta );

		TargetFov = Preferences.FieldOfView;


		ply.Camera.FieldOfView = CurFOV;
	}


}
