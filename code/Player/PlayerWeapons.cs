
using Sandbox;
using System;
using System.Collections.Generic;
using FPSGame.Weapons;


namespace FPSGame
{
	public partial class FPSPlayer
	{
		[Net, Predicted] public Weapon ActiveWeapon { get; set; }
		[Net] public Weapon FirstWeapon { get; set; }
		[Net] public Weapon SecondWeapon { get; set; }


		public void SwitchWeapon()
		{
			if ( ActiveWeapon == FirstWeapon )
			{
				SetActiveWeapon( SecondWeapon );
			}
			else
			{
				SetActiveWeapon( FirstWeapon );
			}
		}

		public void SetActiveWeapon( Weapon weapon )
		{
			if (ActiveWeapon != null)
			{
				ActiveWeapon.OnHolster();
			}
			
			ActiveWeapon = weapon;
			ActiveWeapon.OnEquip( this );
		}
	}
}
