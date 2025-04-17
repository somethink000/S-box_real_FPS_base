namespace GeneralGame;

public interface IBulletBase
{
	public void Shoot( Gun weapon, Vector3 spreadOffset );

	public Vector3 GetRandomSpread( float spread );
}
