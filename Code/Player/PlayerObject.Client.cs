
namespace GeneralGame;

partial class PlayerObject
{
	public static IReadOnlyList<PlayerObject> All => _internalPlayers;
	public static List<PlayerObject> _internalPlayers = new List<PlayerObject>();

	public static PlayerObject Local { get; set; }

	private Guid _guid;

	[HostSync]
	public Guid ConnectionID
	{
		get => _guid;
		set
		{
			_guid = value;
			Connection = Connection.Find( _guid );

			if ( _guid == Connection.Local.Id )
				Local = this;

			if ( !_internalPlayers.Contains( this ) )
				_internalPlayers.Add( this );
		}
	}

	public static PlayerObject GetLocal()
	{
		return Local;
	}

	public Connection Connection { get; private set; }

	public ulong SteamID => Connection.SteamId;
	public string Name => Connection.DisplayName;

	public void SetupConnection( Connection connection )
	{
		ConnectionID = connection.Id;
		GameObject.Name = $"{Name} / {SteamID}";
	}

	public static PlayerObject GetByID( Guid id )
		=> _internalPlayers.FirstOrDefault( x => x.ConnectionID == id );
}
