namespace GeneralGame;

public static class RandomExtensions
{
	public static float NextFloat(
		this Random random,
		float minValue,
		float maxValue )
	{
		return random.Float() * (maxValue - minValue) + minValue;
	}

	public static IReadOnlyList<T> Shuffle<T>( this IEnumerable<T> items, Random random = null )
	{
		var copy = items.ToArray();

		random ??= Random.Shared;

		for ( var i = 0; i < copy.Length - 1; ++i )
		{
			var j = random.Next( i, copy.Length );

			(copy[i], copy[j]) = (copy[j], copy[i]);
		}

		return copy;
	}
}
