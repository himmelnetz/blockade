using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class SingleLevelMoveEvaluator
	{
		public SingleLevelMoveEvaluator()
		{
		}

		public Tuple<int, double> PickBestMove(List<Move> moves, ReadOnlyBlockadeState state, IBlockadeHeuristic blockadeHeuristic)
		{
			return moves.Select((move, i) => new { nextState = state.MakeMove(move), move, i })
				.Where(a => a.nextState.GetBoardCalculator().GetD1Neighbors(a.move.Location, distance: 1)
					.Where(l => a.nextState.GetCell(l).IsEmpty())
					.Any())
				.Select(a => Tuple.Create(this.EvaluateState(a.nextState, state.CurrentPlayer, blockadeHeuristic), a.i))
				.OrderByDescending(t => t)
				.Select(t => Tuple.Create(t.Item2, t.Item1))
				.FirstOrDefault();
		}

		public double EvaluateState(ReadOnlyBlockadeState state, int player, IBlockadeHeuristic blockadeHeuristic)
		{
			return state.IsGameOver()
				? (state.CurrentPlayer == player 
					? blockadeHeuristic.WinScore
					: blockadeHeuristic.LossScore)
				: blockadeHeuristic.EvaluateState(state, player);
		}
	}
}

