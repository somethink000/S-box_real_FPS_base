namespace GeneralGame;

public class RagdollManager
{
	private ModelPhysics RagdollModel { get; set; }

	public RagdollManager (ModelPhysics model) 
	{
		RagdollModel = model;
		
	}

	public bool IsRagdolled => RagdollModel.Enabled;


	public void Ragdoll( Vector3 force, Vector3 forceOrigin )
	{
		ToggleColliders( false );
		RagdollModel.Enabled = true;
		

		foreach ( var body in RagdollModel.PhysicsGroup.Bodies )
		{
			
			body.ApplyImpulseAt( forceOrigin, force );
		}
	}

	public void ToggleColliders( bool enable )
	{
		var colliders = RagdollModel.GameObject.Components.GetAll<Collider>( FindMode.EverythingInSelfAndParent );

		foreach ( var collider in colliders )
		{
			collider.Enabled = enable;
		}
	}


	public void Unragdoll()
	{
		
		RagdollModel.Renderer.LocalPosition = Vector3.Zero;
		RagdollModel.Renderer.LocalRotation = Rotation.Identity;
		RagdollModel.Enabled = false;
		ToggleColliders( true );
		
	}
}
