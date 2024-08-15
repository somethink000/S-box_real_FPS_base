using static Sandbox.Material;

namespace GeneralGame;

public static class EnumExtensions
{
	public static string GetIcon( this EquipSlot slot )
	{
		var path = "ui/hud/" + slot switch
		{
			EquipSlot.FirstWeapon => "first_wep.png",
			EquipSlot.SeccondWeapon => "seccond_wep.png",
			_ => ""
		};

		return path;
	}
}
