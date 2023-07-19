using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
	public abstract partial class Item : AnimatedEntity
	{

		public virtual string Model => "models/citizen/citizen.vmdl";



		public override void Spawn()
		{
			base.Spawn();

			SetModel(Model);
		}

	}
}
