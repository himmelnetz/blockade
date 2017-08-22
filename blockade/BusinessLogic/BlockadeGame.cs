using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BlockadeGame
	{
		private const int BoardRows = 10;
		private const int BoardCols = 16;

		private readonly BlockadeConfiguration _configuration;

		private BlockadeBoard _board;
		private Tuple<int, int>[] _playerLocations;
		private List<int> _finalOrdering;

		public BlockadeGame (BlockadeConfiguration configuration)
		{
			this._configuration = configuration;

			this._board = new BlockadeBoard(BoardRows, BoardCols);
		}

		public BlockadeResult Run()
		{
			this.InitializeGame();
			var turn = 1;

			while (this._playerLocations.Where(t => t != null).Count() > 1)
			{
				this.StepGame(turn);
				turn++;
			}

			var winningIndex = Enumerable.Range(0, this._configuration.Players.Count)
				.Where(i => this._playerLocations[i] != null)
					.Select(i => (int?) i)
					.SingleOrDefault();
			if (winningIndex.HasValue)
			{
				this._finalOrdering.Add(winningIndex.Value);
			}
			this._finalOrdering.Reverse();

			return new BlockadeResult(
				this._finalOrdering.ToList(),
				new ReadOnlyBlockadeBoard(this._board));
		}

		private void StepGame(int turn)
		{
			foreach (var i in Enumerable.Range(0, this._configuration.Players.Count).Where(i => this._playerLocations[i] != null))
			{
				var possibleMoves = this._board.GetMovesFromLocation(this._playerLocations[i]);
				if (possibleMoves.Count == 0)
				{
					this._playerLocations[i] = null;
					this._finalOrdering.Add(i);
				}
				else
				{
					var board = new ReadOnlyBlockadeBoard(this._board);
					var move = possibleMoves.Count > 1
						? possibleMoves[this._configuration.Players[i].PickMove(possibleMoves, board, turn)]
						: possibleMoves[0];
					this._board.PlacePlayer(move, i, turn);
					this._playerLocations[i] = move;
				}
			}
		}

		private void InitializeGame()
		{
			this._board = new BlockadeBoard(this._configuration.Rows, this._configuration.Cols);
			this._finalOrdering = new List<int>();
			this._playerLocations = this._configuration.StartingLocations.ToArray();

			foreach (var i in Enumerable.Range(0, this._configuration.Players.Count))
			{
				this._board.PlacePlayer(this._configuration.StartingLocations[i], i, 0);
			}
		}
	}
}

