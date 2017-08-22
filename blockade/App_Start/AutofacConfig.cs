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
			builder.RegisterType<PlayerProvider>().InstancePerDependency();

			// we have to explicitly register the controller because for some reason autofac's RegisterControllers isnt working right
			builder.RegisterType<BlockadeController>();
		}
	}
}

