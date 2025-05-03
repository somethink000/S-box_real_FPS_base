using GeneralGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralGame;

public class EntThrow : Component
{
	[Property] public GameObject ExplosionEffect { get; set; }
	[Property] public SoundEvent explodeSound { get; set; }
	[Property] public float range { get; set; }
	[Property] public float Damage { get; set; } = 50;
	[Sync] public Player Owner { get; set; }
	public TimeUntil explodeTime { get; set; }
	public TimeUntil removeTime { get; set; }
	public bool isExploded { get; set; }

	protected override void OnStart()
	{
		removeTime = explodeTime + 5;
	}

	protected override void OnUpdate()
	{

		if ( explodeTime && !isExploded )
		{
			isExploded = true;

			GameObject.Components.Get<ModelRenderer>().Destroy();

			var radSphere = new Sphere( WorldPosition, range );
			var targets = Scene.FindInPhysics( radSphere );
			foreach ( var target in targets )
			{
				
				var trace = Scene.Trace.Ray( WorldPosition + Vector3.Up * 10, target.WorldPosition )
					.IgnoreGameObjectHierarchy( GameObject )
					.UsePhysicsWorld()
					.UseHitboxes()
					.Run();

				
				if ( trace.GameObject != null ) {
					
					var damage = new DamageInfo( Damage, Owner.GameObject, GameObject, trace.Hitbox );
					damage.Position = trace.HitPosition;
					damage.Shape = trace.Shape;


					if ( trace.GameObject.Components.GetInAncestorsOrSelf<IHealthComponent>() is IHealthComponent damagable )
					{
						damagable.OnDamage( damage );
					}
				}

				
			}

			IEnumerable<BasicParticleEmiter> emiters = ExplosionEffect.Components.GetAll<BasicParticleEmiter>( FindMode.EnabledInSelfAndChildren );

			foreach ( var emiter in emiters )
			{

				emiter.Emit( WorldTransform, null );

			}

			Sound.Play( explodeSound, WorldPosition );
		}

		if ( removeTime ) { GameObject.Destroy(); }

		base.OnUpdate();
	}




}
