using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class FernandoPlayer : BlockadePlayer
	{
		private readonly Random _random;

		public FernandoPlayer(Random random)
		{
			this._random = random;
		}

		public static BlockadePlayerDescription GetPlayerDescription()
		{
			return new BlockadePlayerDescription
			{
				Name = "Fernando",
				Description = "Picks any random move that doesn't immediately kill itself the next turn."
			};
		}

		public int PickMove(List<Tuple<int, int>> locations, ReadOnlyBlockadeBoard board, int turn)
		{
			var okayMoves = locations.Select((l, i) => Tuple.Create(l, i))
				.Where(t => board.GetMovesFromLocation(t.Item1).Any())
					.Select(t => t.Item2)
					.ToList();
			return okayMoves.Count > 0
				? okayMoves[this._random.Next(okayMoves.Count)]
				: 0;
		}
	}
}

