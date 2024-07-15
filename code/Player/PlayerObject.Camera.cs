using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace GeneralGame;

//Camera moves 
public partial class PlayerObject
{
	[Property] public CameraComponent Camera { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public bool SicknessMode { get; set; }

	[Sync] public Angles EyeAngles { get; set; }

	private Vector3 SieatOffset => new Vector3( 0f, 0f, -40f );
	
	public void ResetViewAngles()
	{
		var rotation = Rotation.Identity;
		EyeAngles = rotation.Angles().WithRoll( 0f );
	}

	public void OnCameraAwake()
	{
		
		
		if ( IsProxy )
			return;

		ResetViewAngles();
	}

	public void OnCameraPreRender()
	{
		

		if ( !Scene.IsValid() || !Camera.IsValid() )
			return;

		if ( IsProxy )
			return;

		if ( !Eye.IsValid() )
			return;

		if ( Ragdoll.IsRagdolled )
		{
			Camera.Transform.Position = Camera.Transform.Position.LerpTo( Eye.Transform.Position, Time.Delta * 32f );
			Camera.Transform.Rotation = Rotation.Lerp( Camera.Transform.Rotation, Eye.Transform.Rotation, Time.Delta * 16f );
			return;
		}


		var idealEyePos = Eye.Transform.Position;
		var headPosition = Transform.Position + Vector3.Up * CC.Height;
		var headTrace = Scene.Trace.Ray( Transform.Position, headPosition )
			.UsePhysicsWorld()
			.IgnoreGameObjectHierarchy( GameObject )
			.WithAnyTags( "solid" )
			.Run();

		headPosition = headTrace.EndPosition - headTrace.Direction * 2f;

		var trace = Scene.Trace.Ray( headPosition, idealEyePos )
			.UsePhysicsWorld()
			.IgnoreGameObjectHierarchy( GameObject )
			.WithAnyTags( "solid" )
			.Radius( 2f )
			.Run();

		var deployedWeapon = Weapons.Deployed;
		var hasViewModel = deployedWeapon.IsValid() && deployedWeapon.HasViewModel;

		if ( hasViewModel )
			Camera.Transform.Position = Head.Transform.Position;
		else
			Camera.Transform.Position = trace.Hit ? trace.EndPosition : idealEyePos;

		if ( SicknessMode )
			Camera.Transform.Rotation = Rotation.LookAt( Eye.Transform.Rotation.Left ) * Rotation.FromPitch( -10f );
		else
			Camera.Transform.Rotation = EyeAngles.ToRotation() * Rotation.FromPitch( -10f );


		if ( IsCrouching && hasViewModel )
		{
			Camera.Transform.Position = Camera.Transform.Position + SieatOffset;
		}
	}

	
	public void CameraUpdate()
	{
		if ( Ragdoll.IsRagdolled || LifeState == LifeState.Dead )
			return;

		if ( !IsProxy )
		{
			var angles = EyeAngles.Normal;
			angles += Input.AnalogLook * 0.5f;
			angles.pitch = angles.pitch.Clamp( -60f, 80f );

			EyeAngles = angles.WithRoll( 0f );
		}
	}
}

