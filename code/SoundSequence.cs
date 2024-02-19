using System;
using System.Collections.Generic;
using Sandbox;

namespace Facepunch.Arena;

public class SoundSequence
{
	public SoundSequence( SoundSequenceData data )
	{
		Data = data;
	}
	
	public bool IsActive { get; private set; }
	
	private TimeUntil NextSoundTime { get; set; }
	private int CurrentIndex { get; set; }
	private SoundHandle Handle { get; set; }
	private SoundSequenceData Data { get; set; }

	public void Start( Vector3 position )
	{
		if ( Data.Entries.Count == 0 )
			return;

		Stop();
		
		CurrentIndex = 0;

		var entry = Data.Entries[CurrentIndex];
		Handle = Sound.Play( entry.Sound, position );
		NextSoundTime = entry.Duration;

		IsActive = true;
	}

	public void Stop()
	{
		if ( !IsActive ) return;

		Handle?.Stop();
		Handle = null;

		IsActive = false;
	}
	
	public void Update( Vector3 position )
	{
		if ( !IsActive ) return;
		
		if ( Handle.IsValid() )
		{
			Handle.Position = position;
			Handle.Update();
		}
		
		if ( !NextSoundTime ) return;

		Handle?.Stop();
		Handle = null;

		CurrentIndex++;

		if ( CurrentIndex >= Data.Entries.Count )
		{
			IsActive = false;
			return;
		}

		var entry = Data.Entries[CurrentIndex];
		Handle = Sound.Play( entry.Sound, position );
		NextSoundTime = entry.Duration;
	}
}
