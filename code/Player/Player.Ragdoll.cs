using Sandbox;
namespace MyGame;

public partial class Player
{
	// TODO - make ragdolls one per entity
	// TODO - make ragdolls dissapear after a load of seconds
	static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 20 };
	[Net] public ModelEntity Corpse { get; set; }
	void BecomeRagdoll( DamageInfo dmg )
	{
		if ( Game.IsClient ) return;
		// TODO - lets not make everyone write this shit out all the time
		// maybe a CreateRagdoll<T>() on ModelEntity?
		var force = dmg.Force;
		var forceBone = dmg.BoneIndex;

		var ent = new Corpse();
		ent.KillDamage = dmg;
		ent.Attacker = dmg.Attacker;
		ent.Weapon = dmg.Weapon;
		ent.OwnerClient = Client;
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.UsePhysicsCollision = true;

		ent.CopyFrom( this );
		ent.TakeDecalsFrom( this );
		ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		ent.CopyBonesFrom( this );
		ent.SetRagdollVelocityFrom( this );
		ent.PhysicsGroup.AddVelocity( Velocity );

		// Copy the clothes over
		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) )
				continue;

			if ( child is ModelEntity e )
			{
				var clothing = new ModelEntity();
				clothing.CopyFrom( e );
				clothing.SetParent( ent, true );
			}
		}

		ent.PhysicsGroup.AddVelocity( force );

		if ( forceBone >= 0 )
		{
			var body = ent.GetBonePhysicsBody( forceBone );
			if ( body != null )
			{
				body.ApplyForce( force * 1000 );
			}
			else
			{
				ent.PhysicsGroup.AddVelocity( force );
			}
		}


		Corpse = ent;

		RagdollLimit.Watch( ent );
	}
}
