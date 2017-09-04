using System;
using Autofac;
using blockade.Controllers;

namespace blockade
{
	public static class AutofacConfig
	{
		public static void Configure(ContainerBuilder builder)
		{
			var random = new Random();
			builder.Register(cc => new Random(random.Next())).InstancePerDependency();
			builder.RegisterType<ZedPlayer>().InstancePerDependency();
			builder.RegisterType<FernandoPlayer>().InstancePerDependency();
			builder.RegisterType<HelmutPlayer>().InstancePerDependency();
			builder.RegisterType<SophiePlayer>().InstancePerDependency();
			builder.RegisterType<PlayerProvider>().InstancePerDependency();
			builder.RegisterType<BlockadeState.BlockadeStateFactory>().InstancePerDependency();
			builder.RegisterType<BlockadeGame>().InstancePerDependency();
			builder.RegisterType<BoardCalculator>().InstancePerDependency();
			builder.RegisterType<SingleLevelMoveEvaluator>().InstancePerDependency();
			builder.RegisterType<SimpleMultiLevelMoveEvaluator>().InstancePerDependency();

			// needs to be single instance to work properly
			builder.RegisterType<MyProfiler>().SingleInstance();

			// we have to explicitly register the controller because for some reason autofac's RegisterControllers isnt working right
			builder.RegisterType<BlockadeController>();
		}
	}
}

