using System.Collections.Generic;
using Sandbox;

namespace Facepunch.Arena;

[GameResource( "Sound Sequence", "sndseq", "A sequence of sounds to play." )]
public class SoundSequenceData : GameResource
{
	public struct Entry
	{
		[ResourceType( "sound")] public string Sound { get; set; }
		public float Duration { get; set; }
	}
	
	public List<Entry> Entries { get; set; }
}
