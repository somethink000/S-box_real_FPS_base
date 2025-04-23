
using System;
using Sandbox.Internal;
using static Sandbox.Volumes.VolumeSystem;

namespace Sandbox;


[Title( "Basic Emitter" )]
[Category( "Particles" )]
public sealed class BasicParticleEmiter : Component
{
	[RequireComponent] public ParticleEffect ParticleEffect { get; set; }

	[Property] public Vector3 Angle { get; set; }


	public void Emit( Transform transform )
	{

		Particle particle = ParticleEffect.Emit( transform.Position, 1 );

		particle.Velocity += transform.Rotation * Angle;
	}
}

