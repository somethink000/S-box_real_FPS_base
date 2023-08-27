using Sandbox;
using System;

namespace MyGame;

public class FirstPersonCamera : CameraComponent
{
	protected override void OnActivate()
	{
		base.OnActivate();
		// Set field of view to whatever the user chose in options
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
	}
	public override void FrameSimulate( IClient cl )
	{

		var pl = Entity as Player;
		// Update rotation every frame, to keep things smooth  

		pl.EyeRotation = pl.ViewAngles.ToRotation();

		Camera.Position = pl.EyePosition;
		Camera.Rotation = pl.ViewAngles.ToRotation();

		Camera.Main.SetViewModelCamera( Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView ) );

		// Set the first person viewer to this, so it won't render our model
		Camera.FirstPersonViewer = Entity;

		Camera.ZNear = 8 * pl.Scale;

		AddCameraEffects();
	}
	public override void BuildInput()
	{
		if ( Game.LocalClient.Components.TryGet<DevCamera>( out var _ ) )
			return;

		var pl = Entity as Player;
		var viewAngles = (pl.ViewAngles + Input.AnalogLook).Normal;
		pl.ViewAngles = viewAngles.WithPitch( viewAngles.pitch.Clamp( -89f, 89f ) );
		return;
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects()
	{
		var speed = Entity.Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = Entity.Velocity.Normal.Dot( Camera.Rotation.Forward );

		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;

		if ( Entity.GroundEntity != null)
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		Camera.Position += up * MathF.Sin( walkBob ) * speed * 2;
		Camera.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;


		// Camera lean
		lean = lean.LerpTo( Entity.Velocity.Dot( Camera.Rotation.Right ) * 0.01f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.3f;
		Camera.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		if( Entity.Inventory.ActiveChild != null )
		{
			fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 4.0f );

			Camera.FieldOfView += fov;
		}

		
	}
}
