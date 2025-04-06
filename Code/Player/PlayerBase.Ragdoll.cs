namespace GeneralGame;

public partial class PlayerBase
{
	[Property] public ModelPhysics RagdollPhysics { get; set; }

	public bool IsRagdolled => RagdollPhysics.Enabled;

	[Rpc.Broadcast]
	public virtual void Ragdoll( Vector3 force, Vector3 forceOrigin )
	{
		ToggleColliders( false );
		RagdollPhysics.Enabled = true;
		//Body.Tags.Add( TagsHelper.Trigger );
		//RagdollPhysics.Tags.Add( TagsHelper.Trigger );

		foreach ( var body in RagdollPhysics.PhysicsGroup.Bodies )
		{
			//body.GetGameObject().Tags.Add( TagsHelper.Trigger );
			body.ApplyImpulseAt( forceOrigin, force );
		}
	}

	public virtual void ToggleColliders( bool enable )
	{
		var colliders = Body.Components.GetAll<Collider>( FindMode.EverythingInSelfAndParent );

		foreach ( var collider in colliders )
		{
			collider.Enabled = enable;
		}
	}

	[Rpc.Broadcast]
	public virtual void Unragdoll()
	{
		RagdollPhysics.Renderer.LocalPosition = Vector3.Zero;
		RagdollPhysics.Renderer.LocalRotation = Rotation.Identity;
		RagdollPhysics.Enabled = false;
		ToggleColliders( true );
	}
}
