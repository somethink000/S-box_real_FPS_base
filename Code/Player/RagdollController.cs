namespace GeneralGame;

public class RagdollController
{
	private ModelPhysics RagdollModel { get; set; }

	public RagdollController (ModelPhysics model) 
	{
		RagdollModel = model; 
	}

	public bool IsRagdolled => RagdollModel.Enabled;


	public virtual void Ragdoll( Vector3 force, Vector3 forceOrigin )
	{
		ToggleColliders( false );
		RagdollModel.Enabled = true;
		

		foreach ( var body in RagdollModel.PhysicsGroup.Bodies )
		{
			
			body.ApplyImpulseAt( forceOrigin, force );
		}
	}

	public virtual void ToggleColliders( bool enable )
	{
		var colliders = RagdollModel.GameObject.Components.GetAll<Collider>( FindMode.EverythingInSelfAndParent );

		foreach ( var collider in colliders )
		{
			collider.Enabled = enable;
		}
	}


	public virtual void Unragdoll()
	{
		RagdollModel.Renderer.LocalPosition = Vector3.Zero;
		RagdollModel.Renderer.LocalRotation = Rotation.Identity;
		RagdollModel.Enabled = false;
		ToggleColliders( true );
	}
}
