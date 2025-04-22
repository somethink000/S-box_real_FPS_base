
using System.Runtime.InteropServices;

namespace GeneralGame;


public class Interactions : Component
{
	[Property]
	public List<Interaction> ObjectInteractions { get; set; }

	[Property]
	public bool HideOnEmpty { get; set; } = false;

	public IEnumerable<Interaction> AllInteractions => ObjectInteractions.Concat( programmedInteractions ?? new List<Interaction>() );

	private List<Interaction> programmedInteractions;

	public void AddInteraction( Interaction interaction )
	{
		programmedInteractions ??= new();
		programmedInteractions.Add( interaction );
	}

	public void AddInteractions( List<Interaction> interactions )
	{
		programmedInteractions ??= new();
		programmedInteractions.AddRange( interactions );
	}

	protected override void OnAwake()
	{
		ObjectInteractions ??= new();
	}

	protected override void DrawGizmos()
	{
		var interactions = ObjectInteractions;
		if ( interactions == null )
			return;

		for ( int i = 0; i < interactions.Count; i++ )
		{
			var interaction = interactions[i];
			if ( !interaction.HasBounds )
				continue;

			var bbox = new BBox( interaction.Position - interaction.Extents / 2, interaction.Position + interaction.Extents / 2 );
			Gizmo.Draw.Color = Color.Yellow;
			Gizmo.Draw.LineThickness = 0.5f;
			Gizmo.Draw.LineBBox( bbox );

			Gizmo.Draw.Color = Color.White;
			Gizmo.Draw.Text( $"{interaction.Description}", new Transform( interaction.Position ), "Consolas", 12 );

			if ( !Gizmo.HasSelected || GameObject != Game.ActiveScene )
				continue;

			using ( Gizmo.Scope( $"{interaction.Description}", new Transform( interaction.Position ) ) )
			{
				Gizmo.Hitbox.BBox( bbox );
				Gizmo.Hitbox.DepthBias = 0.01f;

				if ( Gizmo.IsShiftPressed )
				{
					if ( Gizmo.Control.Scale( "scale", Vector3.Zero, out var scale ) )
					{
						interaction.Extents += scale * 50;
						ObjectInteractions[ObjectInteractions.IndexOf( interaction )] = interaction;
					}
					continue;
				}

				if ( Gizmo.Control.Position( "position", Vector3.Zero, out var pos ) )
				{
					interaction.Position += pos;
					ObjectInteractions[ObjectInteractions.IndexOf( interaction )] = interaction;
				}
			}
		}
	}
}
