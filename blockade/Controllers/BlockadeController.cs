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
			var players = new BlockadePlayer[]
			{
				this._playerProvider.GetPlayer("helmut"),
				this._playerProvider.GetPlayer("fernando")
				//this._fernandoPlayerFactory(),
				//this._fernandoPlayerFactory(),
				//this._fernandoPlayerFactory(),
				//this._fernandoPlayerFactory(),
				//new FernandoPlayer(new Random(Seed: random.Next())),
				//new ZedPlayer(new Random(Seed: random.Next()))
				//new HelmutPlayer(),
				//new HelmutPlayer()
				//new HelmutPlayer()
				//this._fernandoPlayerFactory()
			}.ToList();
			var game = new BlockadeGame(players);
			var result = game.Run();

			return this.View("blockade", new BlockadeModel 
			{ 
				Board = result.Board.To2dArray((player, turn) => Tuple.Create(player, turn))
			});
		}
	}
}
