using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class FernandoPlayer : IBlockadePlayer
	{
		private readonly Random _random;
		private readonly BoardCalculator _boardCalculator;

		public FernandoPlayer(
			Random random,
			BoardCalculator boardCalculator)
		{
			this._random = random;
			this._boardCalculator = boardCalculator;
		}

		public static BlockadePlayerDescription GetPlayerDescription()
		{
			return new BlockadePlayerDescription
			{
				Name = "Fernando",
				Description = "Picks any random move that doesn't immediately kill itself the next turn."
			};
		}

		public int PickMove(List<Move> moves, ReadOnlyBlockadeState state)
		{
			var okayMoves = moves.Select((m, i) => 
				{
					var nextState = state.MakeMove(m);
					return this._boardCalculator.GetD1Neighbors(nextState, m.Location, distance: 1)
							.Where(l => !nextState.GetCell(l).Player.HasValue)
							.Any()
						? i
						: default(int?);
				})
				.Where(i => i.HasValue)
				.Select(i => i.Value)
				.ToList();
			return okayMoves.Count > 0
				? okayMoves[this._random.Next(okayMoves.Count)]
				: 0;
		}
	}
}

