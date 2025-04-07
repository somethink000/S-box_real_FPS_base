namespace GeneralGame;

partial class Player
{
	//Setup Player Singleton
	public static IReadOnlyList<Player> All => _internalPlayers;
	public static List<Player> _internalPlayers = new List<Player>();

	public static Player Local { get; set; }

	private Guid _guid;

	[Sync( SyncFlags.FromHost )]
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

	public static Player CPly()
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

	public static Player GetByID( Guid id )
		=> _internalPlayers.FirstOrDefault( x => x.ConnectionID == id );
}
