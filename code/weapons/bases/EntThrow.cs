using System.Collections.Generic;
using System.Diagnostics;
using Sandbox;

namespace GeneralGame;


public class EntThrow : Component
{
	[Property] public ParticleSystem explosionEffect { get; set; }
	[Property] public SoundEvent explodeSound { get; set; }
	[Property] public SoundEvent colideSound { get; set; }
	[Property] public float range { get; set; }
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
			isExploded  = true;
			
			GameObject.Components.Get<ModelRenderer>().Destroy();

			var radSphere = new Sphere( Transform.Position, range );
			var targets = Scene.FindInPhysics( radSphere );
			foreach ( var target in targets )
			{
				
				var trace = Scene.Trace.Ray( Transform.Position, target.Transform.Position )
					.IgnoreGameObjectHierarchy( GameObject.Root )
					.UsePhysicsWorld()
					.UseHitboxes()
					.Run();

				IHealthComponent damageable = null;

				if ( trace.Component.IsValid() )
					damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

				Log.Info( target );
				if ( damageable is not null )
				{
					damageable.TakeDamage( DamageType.Bullet, 100, trace.EndPosition, trace.Direction * 5, GameObject.Id );
				}
			}

			var p = new SceneParticles( Scene.SceneWorld, explosionEffect );
			p.SetControlPoint( 0, Transform.Position );
			p.PlayUntilFinished( Task );

			Sound.Play( explodeSound, Transform.Position );
		}

		if ( removeTime ) { GameObject.Destroy(); }

		base.OnUpdate();
	}


	

}
