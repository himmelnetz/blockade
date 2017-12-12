using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class FernandoPlayer : IBlockadePlayer
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

		public int PickMove(List<Move> moves, ReadOnlyBlockadeState state)
		{
			var okayMoves = moves.Select((m, i) => 
				{
					var nextState = state.MakeMove(m);
					return nextState.GetBoardCalculator()
							.GetD1Neighbors(m.Location, distance: 1)
							.Where(l => nextState.GetCell(l).IsEmpty())
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

