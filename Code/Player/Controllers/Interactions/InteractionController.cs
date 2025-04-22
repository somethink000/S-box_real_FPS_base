

using Microsoft.VisualBasic;
using Sandbox;
using System.Diagnostics;
using static Sandbox.Services.Inventory;

namespace GeneralGame;

public class InteractionController : Component
{
	private const float _interactDistance = 650f;
	private const float _interactSize = 25f;

	[RequireComponent] public Player ply { get; set; }
	
	public Ray ViewRay => new( ply.CameraController.EyePos, ply.Camera.WorldRotation.Forward );
	public GameObject TargetedGameObject { get; private set; }
	public SceneTraceResult InteractionTrace { get; private set; }
	public BBox? InteractionBounds { get; private set; }

	protected override void OnFixedUpdate()
	{

		var thinTrace = Scene.Trace.Ray( ViewRay, _interactDistance )
			.Size( _interactSize )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			//.WithoutTags( "world" )
			.Run();


		if ( thinTrace.GameObject != null && thinTrace.GameObject.GetInteractions() != null )
		{

			InteractionTrace = thinTrace;
			InteractionBounds = thinTrace.GameObject != TargetedGameObject ? null : InteractionBounds;
			TargetedGameObject = thinTrace.GameObject;

			if ( InteractionBounds == null && TargetedGameObject.IsValid() )
			{
				if ( TargetedGameObject.Components.TryGet<BoxCollider>( out var box ) )
					InteractionBounds = new BBox( box.Center - box.Scale / 2f, box.Center + box.Scale / 2f );

				if ( TargetedGameObject.Components.TryGet<CapsuleCollider>( out var capsule ) )
					InteractionBounds = new BBox( capsule.Start - capsule.Radius, capsule.End + capsule.Radius );

				if ( TargetedGameObject.Components.TryGet<ModelCollider>( out var model ) )
					InteractionBounds = model.Model.PhysicsBounds;
			}
		}
		else
		{
			InteractionTrace = Scene.Trace.Ray( ViewRay, _interactDistance )
						.Size( _interactSize )
						.IgnoreGameObject( GameObject )
						//.WithoutTags( "world" )
						.Run();

			var obj = InteractionTrace.GameObject;
			obj = obj?.GetInteractions() == null ? null : obj;

			InteractionBounds = InteractionTrace.GameObject != TargetedGameObject ? null : InteractionBounds;
			TargetedGameObject = obj;

			if ( InteractionBounds == null && TargetedGameObject.IsValid() )
			{
				if ( TargetedGameObject.Components.TryGet<BoxCollider>( out var box ) )
					InteractionBounds = new BBox( box.Center - box.Scale / 2f, box.Center + box.Scale / 2f );

				if ( TargetedGameObject.Components.TryGet<CapsuleCollider>( out var capsule ) )
					InteractionBounds = new BBox( capsule.Start - capsule.Radius, capsule.End + capsule.Radius );

				if ( TargetedGameObject.Components.TryGet<ModelCollider>( out var model ) )
					InteractionBounds = model.Model.PhysicsBounds;
			}
		}

		// Get bounds again.
		if ( InteractionBounds == null && TargetedGameObject != null )
		{
			InteractionBounds = BBox.FromPositionAndSize( 0, 50f );
		}

	}

}
