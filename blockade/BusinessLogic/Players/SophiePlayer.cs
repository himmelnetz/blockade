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
				Description = "STILL IN DEVELOPMENT"
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
				using (var step = this._myProfiler.Step("sophie evaluate state"))
				{
					var weights = new[] { 20, 15, 10 };
					return weights.Select((weight, i) =>
						state.GetBoardCalculator()
							.GetD1Neighbors(state.GetCurrentLocationOfPlayer(player), distance: i + 1)
							.Sum(l => (state.GetCell(l).Player.HasValue ? -1 : 1) * weight))
						.Sum();
				}
			}
		}
	}
}

