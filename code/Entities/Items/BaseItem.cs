using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace MyGame
{



	partial class BaseItem : Item, IUse
	{
		public override string Model => "models/basemodel.vmdl";



		public override void Spawn()
		{
			base.Spawn();
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Tags.Add( "prop", "solid" );

		}


		public bool IsUsable( Entity user )
		{
			return true;
		}

		public bool OnUse( Entity user )
		{
			if ( user is Player player )
			{
				player.Ammo.GiveAmmo( AmmoType.Pistol, 50 );
				//player.Position = new Vector3(0,0,0);//СТАВЛЮ ПОЗИЦИЮ ЁПТА

				Delete();
			}

			return false;
		}


	}
}
