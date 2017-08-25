using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace blockade.Controllers
{
	public class BlockadeController : Controller
	{
		private readonly PlayerProvider _playerProvider;

		public BlockadeController(PlayerProvider playerProvider)
		{
			this._playerProvider = playerProvider;
		}

		public ActionResult Index()
		{
			return this.View("blockade", new BlockadeModel 
			{ 
				Players = this._playerProvider.GetPlayerDescriptions()
			});
		}

		[HttpPost]
		public JsonResult PlayOneGame()
		{
			// have to use this.Request["data"] instead of parameters to extract data from the post because mono
			var data = JsonConvert.DeserializeObject<PlayOneGameData>(this.Request["data"]);

			var configuration = BlockadeConfiguration.MakeConfiguration(
				rows: data.Rows,
				cols: data.Cols,
				playersWithStartingLocation: data.PlayersWithStartingPosition
					.Select(a => Tuple.Create(this._playerProvider.GetPlayer(a.Name), a.StartingLocation))
					.ToList());

			var game = new BlockadeGame(configuration);
			var result = game.Run();

			return this.Json(new
			{
				Board = result.Board.To2dArray((player, turn) => Tuple.Create(player, turn))
			});
		}

		private class PlayOneGameData
		{
			public int Rows { get; set; }
			public int Cols { get; set; }
			public PlayerWithStartingPosition[] PlayersWithStartingPosition { get; set; }

			public class PlayerWithStartingPosition
			{
				public string Name { get; set; }
				public Tuple<int, int> StartingLocation { get; set; }
			}
		}
	}
}
