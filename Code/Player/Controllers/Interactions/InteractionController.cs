

using Microsoft.VisualBasic;
using Sandbox;
using System.Diagnostics;
using static Sandbox.Services.Inventory;

namespace GeneralGame;

public class InteractionController : Component
{
	[RequireComponent] public Player ply { get; set; }
	public SceneTraceResult Trace { get; private set; }

	public Ray ViewRay => new( ply.CameraController.EyePos, ply.Camera.WorldRotation.Forward );

	protected override void OnFixedUpdate()
	{
	
		if (IsProxy) return;
		
		Trace = Scene.Trace.Ray( ViewRay, 9999 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
	
		GameObject obj = Trace.GameObject;

		if ( obj != null && obj.Components.Get<IInteractable>() is IInteractable i && i != null && i.Interactions.Any() )
		{
			foreach ( var interaction in i.Interactions )
			{
				if ( interaction.InputFunction( interaction.Key ) )
				{
					interaction.Action?.Invoke( ply, obj );
				}
			}
		}

	}

}
