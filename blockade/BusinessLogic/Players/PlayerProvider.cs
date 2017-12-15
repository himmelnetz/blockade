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
		private readonly Func<SophiePlayer> _sophiePlayerFactory;
		private readonly Func<MiraPlayer> _miraPlayerFactory;

		public PlayerProvider(
			Func<ZedPlayer> zedPlayerFactory,
			Func<FernandoPlayer> fernandoPlayerFactory,
			Func<HelmutPlayer> helmutPlayerFactory,
			Func<SophiePlayer> sophiePlayerFactory,
			Func<MiraPlayer> miraPlayerFactory)
		{
			this._zedPlayerFactory = zedPlayerFactory;
			this._fernandoPlayerFactory = fernandoPlayerFactory;
			this._helmutPlayerFactory = helmutPlayerFactory;
			this._sophiePlayerFactory = sophiePlayerFactory;
			this._miraPlayerFactory = miraPlayerFactory;
		}

		public IBlockadePlayer GetPlayer(string playerName)
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
			if (StringComparer.OrdinalIgnoreCase.Equals(playerName, "sophie"))
			{
				return this._sophiePlayerFactory();
			}
			if (StringComparer.OrdinalIgnoreCase.Equals(playerName, "mira"))
			{
				return this._miraPlayerFactory();
			}
			throw new ArgumentException("Trying to get a player that doesnt exist: '" + playerName + "'");
		}

		public List<BlockadePlayerDescription> GetPlayerDescriptions()
		{
			return new[] 
			{
				ZedPlayer.GetPlayerDescription(),
				FernandoPlayer.GetPlayerDescription(),
				HelmutPlayer.GetPlayerDescription(),
				SophiePlayer.GetPlayerDescription(),
				MiraPlayer.GetPlayerDescription()
			}.ToList();
		}
	}
}

