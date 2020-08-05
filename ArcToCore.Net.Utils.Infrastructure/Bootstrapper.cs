using ArcToCore.Net.Utils.Core.Interface;
using ArcToCore.Net.Utils.Repository.Client;
using ArcToCore.Net.Utils.Repository.Converter;
using ArcToCore.Net.Utils.Repository.Output;
using ArcToCore.Net.Utils.Repository.Reflection;
using Autofac;

namespace ArcToCore.Net.Utils.Infrastructure
{
    public static class Bootstrapper
    {
        public static ContainerBuilder ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ObjectHandler>().As<IObjectHandler>().AsSelf();
            builder.RegisterType<ConvertJsonToPoco>().As<IConvertJsonToPoco>().AsSelf();
            builder.RegisterType<RestClientJsonCore>().As<IRestClientJsonCore>().AsSelf();
            builder.RegisterType<OutputHelper>().As<IOutputHelper>().AsSelf();
            builder.RegisterType<ConvertWsdlToCode>().As<IConvertWsdlToCode>().AsSelf();

            return builder;
        }
    }
}