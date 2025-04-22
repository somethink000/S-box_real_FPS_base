using System.Runtime.InteropServices;

namespace GeneralGame;


public enum InputMode
{
	Pressed,
	Released,
	Down,
}

public class Interaction
{
	[Property] public string Description { get; set; } = "";
	[Property] public string Key { get; set; } = "use";
	[Property] public InputMode InputMode { get; set; } = InputMode.Pressed;
	[Property] public Func<bool> Disabled { get; set; } = () => false;
	[Property] public float InteractDistance { get; set; } = 150f;

	[Property] public InteractionEvent Action { get; set; }
	public delegate void InteractionEvent( Player interactor, GameObject obj );

	[Property] public bool HasBounds { get; set; } = false;
	[Property, ShowIf( "HasBounds", true )] public Vector3 Position { get; set; }
	[Property, ShowIf( "HasBounds", true )] public Vector3 Extents { get; set; }

	public bool InputFunction( string action )
	{
		switch ( InputMode )
		{
			case InputMode.Pressed:
				return Input.Pressed( action );
			case InputMode.Down:
				return Input.Down( action );
			case InputMode.Released:
				return Input.Pressed( action );
		};

		return false;
	}



}
