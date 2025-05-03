
using System;
using GeneralGame;
using Sandbox.Internal;
using static Sandbox.PhysicsContact;
using static Sandbox.Volumes.VolumeSystem;

namespace Sandbox;


[Title( "Basic Emitter" )]
[Category( "Particles" )]
public sealed class BasicParticleEmiter : Component
{
	[RequireComponent] public ParticleEffect ParticleEffect { get; set; }

	[Property] public Vector3 Angle { get; set; }

	[Property]
	[Range( 0, 1000, 1, true, true )]
	[Title( "Initial Burst" )]
	[DefaultValue( 100f )]
	public int Burst { get; set; } = 1;

	public void Emit( Transform transform, Vector3? velocity )
	{

		
		for ( int i = 0; i < Burst; i++ ) 
		{
			
			Particle particle = ParticleEffect.Emit( transform.Position, 1 );
		
			if ( velocity != null)
			{
				particle.Velocity += transform.Rotation * Angle + (Vector3)velocity;

			}
			else
			{
				particle.Velocity += transform.Rotation * Angle;
			}


		}

	}
}

