using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
			var configuration = BlockadeConfiguration.MakeConfiguration(
				rows: 10,
				cols: 16,
				playersWithStartingLocation: new[]
				{
					Tuple.Create(this._playerProvider.GetPlayer("helmut"), Tuple.Create(0, 0)),
					Tuple.Create(this._playerProvider.GetPlayer("fernando"), Tuple.Create(9, 15))
				}.ToList());

			var game = new BlockadeGame(configuration);
			var result = game.Run();

			return this.View("blockade", new BlockadeModel 
			{ 
				Board = result.Board.To2dArray((player, turn) => Tuple.Create(player, turn))
			});
		}

		[HttpPost]
		public JsonResult PlayOneGame()
		{
			var configuration = BlockadeConfiguration.MakeConfiguration(
				rows: 10,
				cols: 16,
				playersWithStartingLocation: new[]
				{
					Tuple.Create(this._playerProvider.GetPlayer("helmut"), Tuple.Create(0, 0)),
					Tuple.Create(this._playerProvider.GetPlayer("fernando"), Tuple.Create(9, 15))
				}.ToList());

			var game = new BlockadeGame(configuration);
			var result = game.Run();

			return this.Json(new
			{
				Board = result.Board.To2dArray((player, turn) => Tuple.Create(player, turn))
			});
		}
	}
}
