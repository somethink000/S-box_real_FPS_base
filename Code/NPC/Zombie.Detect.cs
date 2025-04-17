

using Sandbox;
using Sandbox.Citizen;
using static Sandbox.Connection;
using static Sandbox.Diagnostics.PerformanceStats;
using static Sandbox.PhysicsContact;
using static Sandbox.VertexLayout;

namespace GeneralGame;


public partial class Zombie
{

	public void Detected( GameObject target, bool alertOthers = false )
	{

		if ( target == null ) return;
		target = target.Parent == null || target.Parent == Scene ? target : target.Parent;
		if ( target == TargetObject ) return;

		SetTarget( target );
		BroadcastOnDetect();

		if ( alertOthers && AlertOthers )
		{
			var otherNpcs = Scene.GetAllComponents<Zombie>()
				.Where( x => x.WorldPosition.Distance( WorldPosition ) <= x.VisionRange * x.Scale ) // Find all nearby NPCs
				.Where( x => x.IsAlive ) // Dead or undead
				.Where( x => x.TargetObject == null ) // They don't have a target already
				.Where( x => x != this ) // Not us
				.Where( x => x.GameObject != null ) // And that don't have a target already
				.Where( x => !x.EnemyTags.HasAny( Tags ) ); // And we are friends

			foreach ( var npc in otherNpcs )
				npc.Detected( target, false );
		}

	}

	public void Undetected()
	{
		BroadcastOnEscape();

		TargetObject = null;
		TargetPosition = WorldPosition;
		ReachedDestination = true;
	}

	public void DetectAround()
	{
		if ( TargetObject != null )
		{

			if ( IsWithinRange( TargetObject ) )
			{

				if ( NextAttack )
				{
					BroadcastOnAttack();
					NextAttack = AttackDelay;
				}
			}

			if ( IsWithinRange( TargetObject, 150 ) )
			{
				timeUntilLastDrawHeands = 1;
			}
		}

		var currentTick = (int)(Time.Now / Time.Delta);
		if ( currentTick % 20 != NpcId % 20 ) return; // Check every 20 ticks

		var foundAround = Scene.FindInPhysics( new Sphere( WorldPosition, DetectRange * Scale ) ) // Find gameobjects nearby
			.Where( x => x.Enabled )
			.Where( x => EnemyTags != null && x.Tags.HasAny( EnemyTags ) ) // Do they have any of our enemy tags
			.Where( x => x.Components.GetInAncestorsOrSelf<IHealthComponent>()?.LifeState == LifeState.Alive ); // Are they dead or undead


		if ( TargetObject == null || TargetPrimaryObject == TargetObject )
		{

			if ( foundAround.Any() )
			{

				Detected( foundAround.First(), true ); // If we don't have any target yet, pick the first one around us
			}
			else
			{
				Detected( TargetPrimaryObject, false );
			}


		}
		else
		{
			if ( TargetPrimaryObject == TargetObject ) return;
			var targetDead = TargetObject.Components.GetInAncestorsOrSelf<IHealthComponent>()?.LifeState == LifeState.Dead; // Is our target dead or undead
			var targetEscaped = TargetObject.WorldPosition.Distance( WorldPosition ) > VisionRange * Scale; // Did our target get out of vision range

			if ( targetEscaped || targetDead ) // Did our target die or escape
				Undetected();
		}
	}

	public void SetTarget( GameObject target, bool escapeFrom = false )
	{
		if ( target == null )
		{
			TargetObject = null;
			FollowingTargetObject = false;
			ReachedDestination = true;
			TargetPosition = WorldPosition;
		}
		else
		{
			TargetObject = target;
			FollowingTargetObject = !escapeFrom;
			MoveTo( GetPreferredTargetPosition( TargetObject ) );
			ReachedDestination = false;
		}
	}


	public bool IsWithinRange( GameObject target )
	{
		if ( !GameObject.IsValid() ) return false;

		return IsWithinRange( target, AttackRange * Scale );
	}


	public bool IsWithinRange( GameObject target, float range = 60f )
	{
		if ( !GameObject.IsValid() ) return false;

		return target.WorldPosition.Distance( WorldPosition ) <= range;
	}

	void CheckNewTargetPos()
	{
		if ( TargetObject.IsValid() )
		{
			if ( TargetPosition.Distance( GetPreferredTargetPosition( TargetObject ) ) >= AttackRange / 4f ) // Has our target moved?
			{
				MoveTo( GetPreferredTargetPosition( TargetObject ) );
			}
		}
	}

	public static Vector3 GetRandomPositionAround( Vector3 position, float minRange = 50f, float maxRange = 300f )
	{
		var tries = 0;
		var hitGround = false;
		var hitPosition = position;

		while ( hitGround == false && tries <= 10f )
		{
			var randomDirection = Rotation.FromYaw( Game.Random.Float( 360f ) ).Forward;
			var randomDistance = Game.Random.Float( minRange, maxRange );
			var randomPoint = position + randomDirection * randomDistance;

			var groundTrace = Game.ActiveScene.Trace.Ray( randomPoint + Vector3.Up * 64f, randomPoint + Vector3.Down * 64f )
				.Size( 5f )
				.WithoutTags( "player", "npc", "trigger" )
				.Run();

			if ( groundTrace.Hit && !groundTrace.StartedSolid )
			{
				hitGround = true;
				hitPosition = groundTrace.HitPosition;
			}

			tries++;
		}

		return hitPosition;
	}

	public Vector3 GetPreferredTargetPosition( GameObject target )
	{
		if ( !target.IsValid() )
			return TargetPosition;

		var targetPosition = target.WorldPosition;

		var direction = (WorldPosition - targetPosition).Normal;
		var offset = FollowingTargetObject ? direction * AttackRange * Scale / 2f : direction * VisionRange * Scale;
		var wishPos = targetPosition + offset;

		var groundTrace = Scene.Trace.Ray( wishPos + Vector3.Up * 64f, wishPos + Vector3.Down * 64f )
			.Size( 5f )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "player", "npc", "trigger" )
			.Run();
		//Log.Info( target.Transform.Position.Distance( groundTrace.Hit && !groundTrace.StartedSolid ? groundTrace.HitPosition : (FollowingTargetObject ? targetPosition : targetPosition + offset) ) );
		//Log.Info( FollowingTargetObject );
		return groundTrace.Hit && !groundTrace.StartedSolid ? groundTrace.HitPosition : (FollowingTargetObject ? targetPosition : targetPosition + offset);
	}


}

