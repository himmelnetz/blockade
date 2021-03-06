using System;
using System.Collections.Generic;

namespace blockade
{
	public class ZedPlayer : IBlockadePlayer
	{
		private readonly Random _random;

		public ZedPlayer(Random random)
		{
			this._random = random;
		}

		public static BlockadePlayerDescription GetPlayerDescription()
		{
			return new BlockadePlayerDescription
			{
				Name = "Zed",
				Description = "Plays completely randomly."
			};
		}

		public int PickMove(List<Move> moves, ReadOnlyBlockadeState state)
		{
			return this._random.Next(moves.Count);
		}
	}
}

