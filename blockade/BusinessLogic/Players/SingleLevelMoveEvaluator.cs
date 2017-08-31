using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class SingleLevelMoveEvaluator
	{
		private readonly BoardCalculator _boardCalculator;

		public SingleLevelMoveEvaluator(BoardCalculator boardCalculator)
		{
			this._boardCalculator = boardCalculator;
		}

		public int PickBestMove(List<Move> moves, ReadOnlyBlockadeState state, IBlockadeHeuristic blockadeHeuristic)
		{
			return moves.Select((move, i) => new { nextState = state.MakeMove(move), move, i })
				.Where(a => this._boardCalculator.GetD1Neighbors(a.nextState, a.move.Location, distance: 1)
					.Where(l => !a.nextState.GetCell(l).Player.HasValue)
					.Any())
				.OrderByDescending(a => blockadeHeuristic.EvaluateState(a.nextState, state.CurrentPlayer))
				.Select(a => a.i)
				.FirstOrDefault();
		}
	}
}

