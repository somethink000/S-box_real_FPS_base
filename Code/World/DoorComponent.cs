
namespace GeneralGame;

public enum DoorState
{
	Close,
	Open,
	Closing,
	Opening
}

public class DoorComponent : Component, IInteractable
{
	[Sync, Property] public DoorState State { get; set; } = DoorState.Close;
	[Sync] public bool Inverted { get; set; }

	[Property] public bool BothSides { get; set; } = true;
	[Property] public float OpenTime { get; set; } = 0.25f;
	[Property] public float Angle { get; set; } = 100f;
	[Property] public Vector3 Pivot { get; set; }
	[Property] public SoundEvent Sound { get; set; }

	[Sync( SyncFlags.FromHost )] private Transform InitialTransform { get; set; }
	public List<Interaction> Interactions { get; set; } = new List<Interaction>();

	protected override void OnAwake()
	{
		InitialTransform = Transform.World;
	}

	protected override void OnStart()
	{

		Interactions.Add(
			new Interaction()
			{
				Key = "use",
				Action = ( Player player, GameObject obj ) =>
				{
					var dot = Vector3.Dot( InitialTransform.Rotation.Forward, (WorldPosition - player.WorldPosition).Normal );
					Inverted = BothSides && dot <= 0;

					State = State == DoorState.Close ? DoorState.Opening : DoorState.Closing;
				},

			}
		); 

	}

	protected override void DrawGizmos()
	{
		if ( GameObject != Scene )
			return;

		Gizmo.Draw.IgnoreDepth = true;

		var pivot = Pivot.WithZ( 0f );
		var dir = Vector3.Right * WorldRotation.Inverse * 20;

		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Draw.LineThickness = 5;
		Gizmo.Draw.Line( pivot, pivot + dir );
		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.Line( pivot, pivot - dir );

		if ( !Gizmo.HasSelected )
			return;

		using ( Gizmo.Scope( $"Door", new Transform( pivot, Rotation.Identity ) ) )
		{
			Gizmo.Hitbox.DepthBias = 0.01f;

			if ( Gizmo.Control.Position( "position", Vector3.Zero, out var pos ) )
				Pivot = Pivot + pos;
		}
	}

	protected override void OnUpdate()
	{
		if ( State == DoorState.Open || State == DoorState.Close )
			return;

		// todo take into account direction of player
		var direction = State == DoorState.Opening ? 1 : -1;
		var defaultAngles = InitialTransform.Rotation.Angles();
		var targetYaw = direction == 1
			? !Inverted ? Angle : -Angle
			: 0;

		var targetRotation = defaultAngles
			.WithYaw( targetYaw )
			.ToRotation();
		var inversed = WorldRotation * InitialTransform.Rotation.Inverse;
		var difference = inversed.Distance( targetRotation );

		if ( difference.AlmostEqual( 0, 1f ) )
		{
			State = State == DoorState.Opening
				? DoorState.Open
				: DoorState.Close;

			Transform.World = InitialTransform;
			Transform.World = Transform.World.RotateAround( WorldPosition + Pivot * InitialTransform.Rotation, targetRotation );

			if ( State == DoorState.Close )
			{
				// todo: play close sound
			}

			return;
		}

		// todo: Prevent colliding with players.

		// Rotate around the hinge by a tiny amount every tick.
		var rot = Rotation.Lerp( inversed, targetRotation, 1f / OpenTime * Time.Delta );
		Transform.World = InitialTransform;
		Transform.World = Transform.World.RotateAround( WorldPosition + Pivot * InitialTransform.Rotation, rot );
	}
}
