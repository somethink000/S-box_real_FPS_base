using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;


namespace MyGame
{
	public partial class Fists : Melee
	{
		public override string ViewModelPath => "models/first_person/first_person_arms.vmdl";
		public override float PrimaryRate => 2.0f;
		public override float SecondaryRate => 2.0f;


		private void Attack( bool leftHand )
		{
			if ( MeleeAttack() )
			{
				OnMeleeHit( leftHand );
			}
			else
			{
				OnMeleeMiss( leftHand );
			}

			(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );
		}

		public override void PrimaryAttack()
		{
			Attack( true );
		}

		public override void SecondaryAttack()
		{
			Attack( false );
		}


		[ClientRpc]
		public override void CreateViewModel()
		{
			Game.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new PlayerViewModel( this )
			{
				Position = Position,
				Owner = Owner,
				EnableViewmodelRendering = true,
			};

			ViewModelEntity.SetModel( ViewModelPath );
			ViewModelEntity.SetAnimGraph( "models/first_person/first_person_arms_punching.vanmgrph" );
		}


		private bool MeleeAttack()
		{
			var ray = Owner.AimRay;

			var forward = ray.Forward;
			forward = forward.Normal;

			bool hit = false;

			foreach ( var tr in TraceMelee( ray.Position, ray.Position + forward * 80, 20.0f ) )
			{
				if ( !tr.Entity.IsValid() ) continue;

				tr.Surface.DoBulletImpact( tr );

				hit = true;

				if ( !Game.IsServer ) continue;

				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100, 25 )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}

			return hit;
		}

		[ClientRpc]
		private void OnMeleeMiss( bool leftHand )
		{
			Game.AssertClient();

			ViewModelEntity?.SetAnimParameter( "b_attack_has_hit", false );
			ViewModelEntity?.SetAnimParameter( "b_attack", true );
			ViewModelEntity?.SetAnimParameter( "holdtype_attack", leftHand ? 2 : 1 );
		}

		[ClientRpc]
		private void OnMeleeHit( bool leftHand )
		{
			Game.AssertClient();

			ViewModelEntity?.SetAnimParameter( "b_attack_has_hit", true );
			ViewModelEntity?.SetAnimParameter( "b_attack", true );
			ViewModelEntity?.SetAnimParameter( "holdtype_attack", leftHand ? 2 : 1 );
		}
	}
}
