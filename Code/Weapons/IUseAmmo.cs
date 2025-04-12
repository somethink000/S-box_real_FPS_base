using Sandbox;

namespace GeneralGame;

public interface IUseAmmo
{
	public AmmoType AmmoType { get; set; }
	public int Clip { get; set; }
}
