using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BlockadeConfiguration
	{
		public int Rows { get; private set; }
		public int Cols { get; private set; }
		public List<IBlockadePlayer> Players { get; private set; }
		public List<Tuple<int, int>> StartingLocations { get; private set; }

		private BlockadeConfiguration(
			int rows,
			int cols,
			IEnumerable<IBlockadePlayer> players,
			IEnumerable<Tuple<int, int>> startingLocations)
		{
			this.Rows = rows;
			this.Cols = cols;
			this.Players = players.ToList();
			this.StartingLocations = startingLocations.ToList();
		}

		public static bool TryMakeConfiguration(
			int rows,
			int cols,
			List<Tuple<IBlockadePlayer, Tuple<int, int>>> playersWithStartingLocation,
			out BlockadeConfiguration blockadeConfiguration)
		{
			try
			{
				blockadeConfiguration = BlockadeConfiguration.MakeConfiguration(rows, cols, playersWithStartingLocation);
				return true;
			}
			catch 
			{
				blockadeConfiguration = null;
				return false;
			}
		}

		public static BlockadeConfiguration MakeConfiguration(
			int rows,
			int cols,
			List<Tuple<IBlockadePlayer, Tuple<int, int>>> playersWithStartingLocation)
		{
			Throw.IfOutOfRange(rows, 1, 30, "rows");
			Throw.IfOutOfRange(cols, 1, 50, "cols");
			playersWithStartingLocation.ForEach(t => Throw.IfOutOfRange(t.Item2.Item1, 0, rows, "invalid starting location"));
			playersWithStartingLocation.ForEach(t => Throw.IfOutOfRange(t.Item2.Item2, 0, cols, "invalid starting location"));

			return new BlockadeConfiguration(
				rows,
				cols,
				playersWithStartingLocation.Select(t => t.Item1),
				playersWithStartingLocation.Select(t => t.Item2));
		}
	}
}

