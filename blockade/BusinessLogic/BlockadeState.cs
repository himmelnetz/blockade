using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BlockadeState
	{
		private readonly BoardCalculator.Factory _boardCalculatorFactory;

		private readonly Grid<Cell> _board;
		private readonly Location[] _playerLocations;
		private readonly List<int> _finalResults;

		private int _currentPlayer;
		private int _turn;

		public int Rows { get { return this._board.Rows; } }
		public int Cols { get { return this._board.Cols; } }
		public int PlayerCount { get { return this._playerLocations.Length; } }

		public int CurrentPlayer { get { return this._currentPlayer; } }

		private BlockadeState(
			BoardCalculator.Factory boardCalculatorFactory,
			Grid<Cell> board,
			Location[] playerLocations,
			List<int> finalResults,
			int currentPlayer,
			int turn)
		{
			this._boardCalculatorFactory = boardCalculatorFactory;
			this._board = board;
			this._playerLocations = playerLocations;
			this._finalResults = finalResults;
			this._currentPlayer = currentPlayer;
			this._turn = turn;
		}

		private static BlockadeState CreateFromConfiguration(
			BlockadeConfiguration configuration,
			BoardCalculator.Factory boardCalculatorFactory)
		{
			var board = Grid.Create(configuration.Rows, configuration.Cols, (row, col) => Cell.EmptyCell);
			var playerLocations = configuration.StartingLocations
				.Select(t => Location.Create(t.Item1, t.Item2))
				.ToArray();
			var finalResults = new List<int>(capacity: configuration.Players.Count);
			var currentPlayer = 0;
			var turn = 1;

			// place players at their initial positions
			foreach (var i in Enumerable.Range(0, playerLocations.Length))
			{
				var location = playerLocations[i];
				board[location] = Cell.MakeOccupiedCell(player: i, turn: 0);
			}

			return new BlockadeState(boardCalculatorFactory, board, playerLocations, finalResults, currentPlayer, turn);
		}

		public BlockadeState Clone()
		{
			// these rely on the underlying values (cell, location) to be immutable
			var newBoard = this._board.Clone();
			var newPlayerLocations = this._playerLocations.ToArray();
			var newFinalResults = this._finalResults.ToList();

			return new BlockadeState(this._boardCalculatorFactory, newBoard, newPlayerLocations, newFinalResults, this._currentPlayer, this._turn);
		}

		public IEnumerable<Move> GetMoves()
		{
			return this.GetMoves(this._currentPlayer);
		}

		public IEnumerable<Move> GetMoves(int player)
		{
			Throw.InvalidIf(this.IsGameOver(), "game is over");

			return this.GetBoardCalculator()
				.GetD1Neighbors(this._playerLocations[player], distance: 1)
				.Where(location => this._board[location].IsEmpty())
				.Select(Move.Create);
		}

		public void MakeMove(Move move)
		{
			Throw.InvalidIf(this.IsGameOver(), "game is over");

			this._board[move.Location] = Cell.MakeOccupiedCell(this._currentPlayer, this._turn);
			this._playerLocations[this._currentPlayer] = move.Location;

			var originalPlayer = this._currentPlayer;
			while (!this.IsGameOver())
			{
				this._currentPlayer++;
				if (this._currentPlayer == this._playerLocations.Length)
				{
					this._currentPlayer = 0;
					this._turn++;
				}

				// we've made it back to the beginning, so the game is over
				if (this._currentPlayer == originalPlayer)
				{
					this._finalResults.Add(this._currentPlayer);
					Throw.InvalidIf(this._finalResults.Count != this._playerLocations.Length, "expected game to be over");
					return;
				}

				// we found a still alive player who has a move
				if (this._playerLocations[this._currentPlayer] != null && this.GetMoves().Any())
				{
					return;
				}

				// this is a player who was alive but now has no moves
				if (this._playerLocations[this._currentPlayer] != null)
				{
					this._playerLocations[this._currentPlayer] = null;
					this._finalResults.Add(this._currentPlayer);
				}

				// this is an already or freshly dead player, so loop
			}
		}

		public bool IsGameOver()
		{
			return this._finalResults.Count == this._playerLocations.Length;
		}

		public bool IsPlayerOut(int player)
		{
			return this._playerLocations[player] == null;
		}

		public BlockadeResult ToResult()
		{
			Throw.InvalidIf(!this.IsGameOver(), "game is not over");

			return new BlockadeResult(
				finalOrdering: this._finalResults.AsEnumerable().Reverse(),
				board: this._board);
		}

		public ReadOnlyBlockadeState AsReadOnly()
		{
			return new ReadOnlyBlockadeState(this);
		}

		public Cell GetCell(Location location)
		{
			return this._board[location];
		}

		public Location GetCurrentLocationOfPlayer(int player)
		{
			return this._playerLocations[player];
		}

		public BoardCalculator GetBoardCalculator()
		{
			return this._boardCalculatorFactory(this.AsReadOnly());
		}

		public Grid<Cell> GetBoard()
		{
			return this._board.Clone();
		}

		public class BlockadeStateFactory
		{
			private readonly BoardCalculator.Factory _boardCalculatorFactory;

			public BlockadeStateFactory(BoardCalculator.Factory boardCalculatorFactory)
			{
				this._boardCalculatorFactory = boardCalculatorFactory;
			}

			public BlockadeState CreateFromConfiguration(BlockadeConfiguration configuration)
			{
				return BlockadeState.CreateFromConfiguration(configuration, this._boardCalculatorFactory);
			}
		}
	}

	public class Move : IEqualityComparer<Move>
	{
		public Location Location { get; private set; }

		private Move(Location location)
		{
			this.Location = location;
		}

		public static Move Create(Location location)
		{
			return new Move(location);
		}

		public override string ToString()
		{
			return this.Location.ToString();
		}

		public bool Equals(Move @this, Move other)
		{
			return @this.Location.Equals(other.Location);
		}

		public int GetHashCode(Move @this)
		{
			return @this.Location.GetHashCode();
		}
	}

	public class Location : IEqualityComparer<Location>
	{
		public int Row { get; private set; }
		public int Col { get; private set; }

		private Location(int row, int col)
		{
			this.Row = row;
			this.Col = col;
		}

		public static Location Create(int row, int col)
		{
			return new Location(row, col);
		}

		public override string ToString()
		{
			return Tuple.Create(this.Row, this.Col).ToString();
		}

		public bool Equals(Location @this, Location other)
		{
			return Tuple.Create(@this.Row, @this.Col)
				.Equals(Tuple.Create(other.Row, other.Col));
		}

		public int GetHashCode(Location @this)
		{
			return Tuple.Create(@this.Row, @this.Col).GetHashCode();
		}
	}

	public class Cell : IEqualityComparer<Cell>
	{
		public int? Player { get; private set; }
		public int? Turn { get; private set; }

		private Cell(int? player, int? turn)
		{
			this.Player = player;
			this.Turn = turn;
		}

		public bool IsEmpty()
		{
			return !this.Player.HasValue;
		}

		public static Cell MakeOccupiedCell(int player, int turn)
		{
			return new Cell(player, turn);
		}

		public static Cell EmptyCell = new Cell(player: default(int?), turn: default(int?));

		public override string ToString()
		{
			return Tuple.Create(this.Player, this.Turn).ToString();
		}

		public bool Equals(Cell @this, Cell other)
		{
			return Tuple.Create(@this.Player, @this.Turn)
				.Equals(Tuple.Create(other.Player, other.Turn));
		}

		public int GetHashCode(Cell @this)
		{
			return Tuple.Create(@this.Player, @this.Turn).GetHashCode();
		}
	}
}

