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

		public Tuple<int, double> PickBestMove(List<Move> moves, ReadOnlyBlockadeState state, IBlockadeHeuristic blockadeHeuristic)
		{
			return moves.Select((move, i) => new { nextState = state.MakeMove(move), move, i })
				.Where(a => this._boardCalculator.GetD1Neighbors(a.nextState, a.move.Location, distance: 1)
					.Where(l => !a.nextState.GetCell(l).Player.HasValue)
					.Any())
				.Select(a => Tuple.Create(blockadeHeuristic.EvaluateState(a.nextState, state.CurrentPlayer), a.i))
				.OrderByDescending(t => t)
				.Select(t => Tuple.Create(t.Item2, t.Item1))
				.FirstOrDefault();
		}
	}
}

