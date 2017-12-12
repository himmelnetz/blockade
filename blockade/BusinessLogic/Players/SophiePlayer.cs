using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class SophiePlayer : IBlockadePlayer
	{
		private readonly SimpleMultiLevelMoveEvaluator _simpleMultiLevelEvaluator;
		private readonly MyProfiler _myProfiler;

		public SophiePlayer(
			SimpleMultiLevelMoveEvaluator simpleMultiLevelEvaluator,
			MyProfiler myProfiler)
		{
			this._simpleMultiLevelEvaluator = simpleMultiLevelEvaluator;
			this._myProfiler = myProfiler;
		}

		public static BlockadePlayerDescription GetPlayerDescription()
		{
			return new BlockadePlayerDescription
			{
				Name = "Sophie",
				Description = "Maximizes heuristic of board after everyone's next move. Heuristic"
					+ " prefers going towards areas with most available cells when possible, breaking"
					+ " ties by avoiding neighboring occupied cells"
			};
		}

		public int PickMove(List<Move> moves, ReadOnlyBlockadeState state)
		{
			var tuple = this._simpleMultiLevelEvaluator.PickBestMove(moves, state, new SophieBlockadeHeuristic(this._myProfiler), levels: 1);
			return tuple.Item1;
		}

		private class SophieBlockadeHeuristic : IBlockadeHeuristic
		{
			private readonly MyProfiler _myProfiler;

			public SophieBlockadeHeuristic(
				MyProfiler myProfiler)
			{
				this._myProfiler = myProfiler;
			}

			public double WinScore { get { return 1000.0; } }
			public double LossScore { get { return -1000.0; } }

			public double EvaluateState(ReadOnlyBlockadeState state, int player)
			{
				return GetReachableCellScore(state, player) * 1000.0
					+ GetNeighborAversionScore(state, player) * 1.0;
			}

			public double GetReachableCellScore(ReadOnlyBlockadeState state, int player)
			{
				using (var step = this._myProfiler.Step("sophie evaluate state - reachable"))
				{
					// so we cant check the reachable count for the current position of the player because that space is
					// occupied, but we can check all the places they can go and get the max of that as an analog
					return state.GetMoves(player)
						.Select(move => state.GetBoardCalculator().GetReachableCellCount(move.Location))
							.Max();
				}
			}

			public double GetNeighborAversionScore(ReadOnlyBlockadeState state, int player)
			{
				using (var step = this._myProfiler.Step("sophie evaluate state - neighbor"))
				{
					var weights = new[] { 20, 15, 10 };
					return weights.Select((weight, i) =>
						state.GetBoardCalculator()
							.GetD1Neighbors(state.GetCurrentLocationOfPlayer(player), distance: i + 1)
							.Sum(l => (state.GetCell(l).IsEmpty() ? 1 : -1) * weight))
						.Sum();
				}
			}
		}
	}
}

