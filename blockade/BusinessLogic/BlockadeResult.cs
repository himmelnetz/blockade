using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace blockade
{
	public class BlockadeResult
	{
		public List<int> PlayerOrdering { get; private set; }
		public ReadOnlyBlockadeBoard Board { get; private set; }

		public BlockadeResult (List<int> playerOrdering, ReadOnlyBlockadeBoard board)
		{
			this.PlayerOrdering = playerOrdering;
			this.Board = board;
		}

		/*
		public Task WriteToStreamAsync(StreamWriter streamWriter)
		{
			var jsonObject = new
			{
				Results = this.PlayerOrdering.ToArray(),
				Board = this.Board.To2dArray((player, turn) => new[] { player, turn })
			};
			var json = JsonConvert.SerializeObject(jsonObject);
			return streamWriter.WriteAsync(json);
		}
		*/
	}
}

