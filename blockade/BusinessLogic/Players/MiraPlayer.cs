using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class MiraPlayer : IBlockadePlayer
	{
		private readonly SimpleMultiLevelMoveEvaluator _simpleMultiLevelEvaluator;
		private readonly MyProfiler _myProfiler;

		public MiraPlayer(
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
				Name = "Mira",
				Description = "Same as Sophie, but also scores other players the same way (applying negatively to overal value)"
			};
		}

		public int PickMove(List<Move> moves, ReadOnlyBlockadeState state)
		{
			var tuple = this._simpleMultiLevelEvaluator.PickBestMove(moves, state, new MiraBlockadeHeuristic(this._myProfiler), levels: 1);
			return tuple.Item1;
		}

		private class MiraBlockadeHeuristic : IBlockadeHeuristic
		{
			private readonly MyProfiler _myProfiler;

			public MiraBlockadeHeuristic(
				MyProfiler myProfiler)
			{
				this._myProfiler = myProfiler;
			}

			public double WinScore { get { return 1000.0; } }
			public double LossScore { get { return -1000.0; } }

			public double EvaluateState(ReadOnlyBlockadeState state, int player)
			{
				return Enumerable.Range(0, state.PlayerCount)
					.Where(thisPlayer => !state.IsPlayerOut(thisPlayer))
					.Select(thisPlayer => (thisPlayer == player ? 1.0 : -1.0)
						* (GetReachableCellScore(state, thisPlayer) * 100.0
							+ GetNeighborAversionScore(state, thisPlayer) * 1.0))
					.Sum();
			}

			public double GetReachableCellScore(ReadOnlyBlockadeState state, int player)
			{
				using (var step = this._myProfiler.Step("mira evaluate state - reachable"))
				{
					// so we cant check the reachable count for the current position of the player because that space is
					// occupied, but we can check all the places they can go and get the max of that as an analog
					return state.GetMoves(player)
						.Select(move => state.GetBoardCalculator().GetReachableCellCount(move.Location))
						.OrderByDescending(s => s)
						.FirstOrDefault();
				}
			}

			public double GetNeighborAversionScore(ReadOnlyBlockadeState state, int player)
			{
				using (var step = this._myProfiler.Step("mira evaluate state - neighbor"))
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

