using System.Runtime.InteropServices;

namespace GeneralGame;

public interface IInteractable
{
	public List<Interaction> Interactions { get; set; }
}

public enum InputMode
{
	Pressed,
	Released,
	Down,
}

public class Interaction
{

	public string Key { get; set; } = "use";
	public SoundEvent Sound { get; set; }
	public InteractionEvent Action { get; set; }
	public delegate void InteractionEvent( Player interactor, GameObject obj );
	public InputMode InputMode { get; set; } = InputMode.Pressed;

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
