using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class HelmutPlayer : IBlockadePlayer
	{
		private readonly BoardCalculator _boardCalculator;
		private readonly SingleLevelMoveEvaluator _singleLevelMoveEvaluator;
		private readonly Random _random;

		public HelmutPlayer(
			BoardCalculator boardCalculator,
			SingleLevelMoveEvaluator singleLevelMoveEvaluator,
			Random random)
		{
			this._boardCalculator = boardCalculator;
			this._singleLevelMoveEvaluator = singleLevelMoveEvaluator;
			this._random = random;
		}

		public static BlockadePlayerDescription GetPlayerDescription()
		{
			return new BlockadePlayerDescription
			{
				Name = "Helmut",
				Description = "Maximizes heuristic of board after next move. Heuristic prefers locations that give the most amount of availabe spaces nearby."
			};
		}

		public int PickMove(List<Move> moves, ReadOnlyBlockadeState state)
		{
			var tuple = this._singleLevelMoveEvaluator.PickBestMove(moves, state, new HelmutBlockadeHeuristic(this._boardCalculator));
			return tuple != null
				? tuple.Item1
				// all moves kill me, so pick a random one
				: this._random.Next(moves.Count);
		}

		private class HelmutBlockadeHeuristic : IBlockadeHeuristic
		{
			private readonly BoardCalculator _boardCalculator;

			public HelmutBlockadeHeuristic(BoardCalculator boardCalculator)
			{
				this._boardCalculator = boardCalculator;
			}

			public double WinScore { get { return 100.0; } }
			public double LossScore { get { return -100.0; } }

			public double EvaluateState(ReadOnlyBlockadeState state, int player)
			{
				var weights = new[] { 20, 15, 10 };
				return weights.Select((weight, i) => 
					this._boardCalculator.GetD1Neighbors(state, state.GetCurrentLocationOfPlayer(player), distance: i + 1)
						.Sum(l => (state.GetCell(l).Player.HasValue ? -1 : 1) * weight))
					.Sum();
			}
		}
	}
}

