using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class PlayerProvider
	{
		private readonly Func<ZedPlayer> _zedPlayerFactory;
		private readonly Func<FernandoPlayer> _fernandoPlayerFactory;
		private readonly Func<HelmutPlayer> _helmutPlayerFactory;

		public PlayerProvider(
			Func<ZedPlayer> zedPlayerFactory,
			Func<FernandoPlayer> fernandoPlayerFactory,
			Func<HelmutPlayer> helmutPlayerFactory)
		{
			this._zedPlayerFactory = zedPlayerFactory;
			this._fernandoPlayerFactory = fernandoPlayerFactory;
			this._helmutPlayerFactory = helmutPlayerFactory;
		}

		public BlockadePlayer GetPlayer(string playerName)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(playerName, "zed"))
			{
				return this._zedPlayerFactory();
			}
			if (StringComparer.OrdinalIgnoreCase.Equals(playerName, "fernando"))
			{
				return this._fernandoPlayerFactory();
			}
			if (StringComparer.OrdinalIgnoreCase.Equals(playerName, "helmut"))
			{
				return this._helmutPlayerFactory();
			}
			throw new ArgumentException("Trying to get a player that doesnt exist: '" + playerName + "'");
		}

		public List<BlockadePlayerDescription> GetPlayerDescriptions()
		{
			return new[] 
			{
				ZedPlayer.GetPlayerDescription(),
				FernandoPlayer.GetPlayerDescription(),
				HelmutPlayer.GetPlayerDescription()
			}.ToList();
		}
	}
}

