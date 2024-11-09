using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;

namespace General;

public interface IPlayer : IValid
{
	public CameraComponent Camera { get; set; }
	public GameObject Body { get; set; }
	public SkinnedModelRenderer BodyRenderer { get; set; }
	public CharacterController CharacterController { get; set; }
	public CitizenAnimationHelper AnimationHelper { get; set; }
	public GameObject GameObject { get; }
	public bool IsFirstPerson { get; }
	public Vector3 Velocity { get; }
	public bool IsCrouching { get; set; }
	public bool IsRunning { get; set; }
	public bool IsOnGround { get; }
	public bool IsAlive { get; }
	public int MaxHealth { get; set; }
	public int Health { get; set; }
	public int Kills { get; set; }
	public int Deaths { get; set; }
	public Guid Id { get; }

	/// <summary>Input sensitivity modifier</summary>
	public float InputSensitivity { get; set; }

	/// <summary>View angles</summary>
	public Angles EyeAngles { get; set; }

	/// <summary>View position</summary>
	public Vector3 EyePos { get; }

	/// <summary>EyeAngles offset (Resets after being applied)</summary>
	public Angles EyeAnglesOffset { get; set; }


	public void ApplyFov( float plusFov, float speed );

}
