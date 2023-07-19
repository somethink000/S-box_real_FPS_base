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
		
		public BaseItem()
		{
			var Item = this;
		// world panels creating only on client
		if (Game.IsClient )
		{
		var WorldPanel = new MyWorldPanel();
		WorldPanel.Item = Item;
		WorldPanel.Title = "Pistol ammo: 50";
		}
		}

		

		public override void Spawn()
		{
			base.Spawn();
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Tags.Add( "prop", "solid" );



		}
		public override void Simulate( IClient player )
		{


			Log.Info( "ada" );


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
				//player.Position = new Vector3(0,0,0);

				Delete();
				
			}

			return false;
		}


	}
}
