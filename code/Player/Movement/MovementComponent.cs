using Sandbox;
using System;
using System.Collections.Generic;

namespace MyGame;

/// <summary>
/// Component designed for movement, only 1 per pawn.
/// </summary>
public class MovementComponent : EntityComponent<Player>, ISingletonComponent
{

	public virtual void Simulate( IClient cl )
	{

	}
	public virtual void FrameSimulate( IClient cl )
	{

	}
	public virtual void BuildInput()
	{

	}
	public Vector3 WishVelocity { get; set; }

	internal HashSet<string> Events = new( StringComparer.OrdinalIgnoreCase );

	internal HashSet<string> Tags;
	/// <summary>
	/// Call OnEvent for each event
	/// </summary>
	public virtual void RunEvents( MovementComponent additionalController )
	{
		if ( Events == null ) return;

		foreach ( var e in Events )
		{
			OnEvent( e );
			additionalController?.OnEvent( e );
		}
	}

	/// <summary>
	/// An event has been triggered - maybe handle it
	/// </summary>
	public virtual void OnEvent( string name )
	{

	}

	/// <summary>
	/// Returns true if we have this event
	/// </summary>
	public bool HasEvent( string eventName )
	{
		if ( Events == null ) return false;
		return Events.Contains( eventName );
	}

	/// <summary>
	/// </summary>
	public bool HasTag( string tagName )
	{
		if ( Tags == null ) return false;
		return Tags.Contains( tagName );
	}


	/// <summary>
	/// Allows the controller to pass events to other systems
	/// while staying abstracted.
	/// For example, it could pass a "jump" event, which could then
	/// be picked up by the playeranimator to trigger a jump animation,
	/// and picked up by the player to play a jump sound.
	/// </summary>
	public void AddEvent( string eventName )
	{
		// TODO - shall we allow passing data with the event?

		if ( Events == null ) Events = new HashSet<string>();

		if ( Events.Contains( eventName ) )
			return;

		Events.Add( eventName );
	}


	/// <summary>
	/// </summary>
	public void SetTag( string tagName )
	{
		// TODO - shall we allow passing data with the event?

		Tags ??= new HashSet<string>();

		if ( Tags.Contains( tagName ) )
			return;

		Tags.Add( tagName );
	}
}
