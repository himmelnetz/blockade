using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class HelmutPlayer : BlockadePlayer
	{
		public HelmutPlayer()
		{
		}

		public static BlockadePlayerDescription GetPlayerDescription()
		{
			return new BlockadePlayerDescription
			{
				Name = "Helmut",
				Description = "Uses heuristic of board after next move. Heuristic prefers locations that give the most amoung of availabe spaces nearby."
			};
		}

		public int PickMove(List<Tuple<int, int>> locations, ReadOnlyBlockadeBoard board, int turn)
		{
			return locations.Select((l, i) => Tuple.Create(l, i))
				// immediately dont consider any moves with no moves next turn
				.Where(t => board.GetMovesFromLocation(t.Item1).Any())
					// use heuristic to evaluate moves
					// TODO PLAYER HERE IS WRONG
					.OrderByDescending(t => this.EvaluateBoard(t.Item1, board.CloneAndPlacePlayer(t.Item1, player: 0, turn: turn)))
					.Select(t => t.Item2)
					.FirstOrDefault();
		}

		private int EvaluateBoard(Tuple<int, int> location, ReadOnlyBlockadeBoard board)
		{
			var weights = new[] { 20, 15, 10 };
			return weights.Select((weight, i) => 
			                      board.GetD1Neighbors(location, distance: i + 1)
			                      .Sum(l => (board.GetPlayerAtLocation(l).HasValue ? -1 : 1) * weight))
				.Sum();
		}
	}
}

