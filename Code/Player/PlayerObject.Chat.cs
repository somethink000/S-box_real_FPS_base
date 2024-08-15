
namespace GeneralGame;


public record ChatEntry( string author, string message, RealTimeSince timeSinceAdded );


public partial class PlayerObject
{
	public List<ChatEntry> StoredChat { get; set; } = new();

	[Broadcast]
	public void NewEntry( string author, string message )
	{	
		UI.Chat.Instance.AddTextLocal( author, message );
	}


	public void AddEntry( string author, string message )
	{
		if ( IsProxy ) return;
		
		StoredChat.Add( new( author, message, 0f ) );
		
	}


}
