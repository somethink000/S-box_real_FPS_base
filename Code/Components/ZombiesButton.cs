using System;
using System.Linq;
using Sandbox;
namespace GeneralGame;
public sealed class ZombiesButton : Component
{
	[RequireComponent] public ModelRenderer Model { get; set; }
	[Property] public SoundEvent ActSound { get; set; }
	[Property] public SoundEvent DisSound { get; set; }

	private GameObject Target { get; set; }
	private bool ActiveState = false;

	protected override void OnStart()
	{
		var interactions = Components.GetOrCreate<Interactions>();

		interactions.AddInteraction( new Interaction()
		{
			Description = "Pickup",
			Key = "use",
			Action = ( Player player, GameObject obj ) =>
			{
				Toggle();
			},
			//Disabled = () => !PlayerBase.GetLocal().Inventory.HasSpaceInBackpack(),
		} );
	}

	public void Toggle()
	{
		ActiveState = !ActiveState;

		var spawnpoints = Scene.GetAllComponents<ZombieSpawner>();

		foreach ( var spawnpoint in spawnpoints )
		{

			if ( ActiveState )
			{
				spawnpoint.Activate();
			}
			else
			{
				spawnpoint.Disable();
			}
		}


		if ( ActiveState )
		{
			WorldPosition -= WorldRotation.Forward * 2;
			Model.Tint = new Color( 0, 1, 0, 1 );
			GameObject.PlaySound( ActSound );
		}
		else
		{
			WorldPosition += WorldRotation.Forward * 2;
			Model.Tint = new Color( 1, 0, 0, 1 );
			GameObject.PlaySound( DisSound );
		}
	}

	protected override void DrawGizmos()
	{
		const float boxSize = 4f;
		var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );

		Gizmo.Hitbox.BBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.SolidBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );
	}



}
