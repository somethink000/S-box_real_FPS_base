using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Arena;

[Group( "Arena" )]
[Title( "Ragdoll Controller" )]
public sealed class RagdollController : Component
{
	[Property] public ModelPhysics Physics { get; private set; }

	public bool IsRagdolled => Physics.Enabled;

	[Broadcast]
	public void Ragdoll( Vector3 position, Vector3 force )
	{
		Physics.Enabled = true;

		foreach ( var body in Physics.PhysicsGroup.Bodies )
		{
			body.ApplyImpulseAt( position, force * 200f );
		}
	}

	[Broadcast]
	public void Unragdoll()
	{
		Physics.Renderer.Transform.LocalPosition = Vector3.Zero;
		Physics.Renderer.Transform.LocalRotation = Rotation.Identity;
		Physics.Enabled = false;
	}
}
