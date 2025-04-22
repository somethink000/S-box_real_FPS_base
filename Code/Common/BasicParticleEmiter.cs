
using System;
using Sandbox.Internal;
using static Sandbox.Volumes.VolumeSystem;

namespace Sandbox;


[Title( "Basic Emitter" )]
[Category( "Particles" )]
public sealed class BasicParticleEmiter : Component
{

	[RequireComponent] public ParticleEffect ParticleEffect { get; set; }


	public void Emit( Vector3 pos )
	{

		ParticleEffect.Emit( pos, 1 );
	}
}

