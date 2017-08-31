using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class SimpleMultiLevelMoveEvaluator
	{
		private readonly SingleLevelMoveEvaluator _singleLevelMoveEvaluator;
		private readonly Random _random;

		public SimpleMultiLevelMoveEvaluator(
			SingleLevelMoveEvaluator singleLevelMoveEvaluator,
			Random random)
		{
			this._singleLevelMoveEvaluator = singleLevelMoveEvaluator;
			this._random = random;
		}

		public Tuple<int, double> PickBestMove(List<Move> moves, ReadOnlyBlockadeState state, IBlockadeHeuristic heuristic, int levels)
		{
			if (levels == 0)
			{
				var singleLevelResult = this._singleLevelMoveEvaluator.PickBestMove(moves, state, heuristic);
				return singleLevelResult != null
					? singleLevelResult
					: Tuple.Create(0, heuristic.LossScore);
			}

			return moves.Select((m, i) => Tuple.Create(i, this.TryAllOtherPlayerMoves(state.MakeMove(m), heuristic, levels, state.CurrentPlayer)))
				.OrderByDescending(t => Tuple.Create(t.Item2, this._random.Next()))
				.First();
		}

		public double TryAllOtherPlayerMoves(ReadOnlyBlockadeState state, IBlockadeHeuristic heuristic, int levels, int myselfPlayer)
		{
			if (state.IsGameOver())
			{
				return state.CurrentPlayer == myselfPlayer
					? heuristic.WinScore
					: heuristic.LossScore;
			}

			if (state.CurrentPlayer == myselfPlayer)
			{
				return this.PickBestMove(state.GetMoves().ToList(), state, heuristic, levels - 1).Item2;
			}

			return state.GetMoves()
				.Select(state.MakeMove)
				.Select(newState => newState.GetCurrentLocationOfPlayer(myselfPlayer) == null
					? heuristic.LossScore
					: newState.CurrentPlayer == myselfPlayer
						? this.PickBestMove(newState.GetMoves().ToList(), newState, heuristic, levels - 1).Item2
						: this.TryAllOtherPlayerMoves(newState, heuristic, levels, myselfPlayer))
				.Average();
		}
	}
}

