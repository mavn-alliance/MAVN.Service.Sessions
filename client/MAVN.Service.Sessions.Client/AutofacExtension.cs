using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;

namespace MAVN.Service.Sessions.Client
{
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        ///     Registers <see cref="ISessionsServiceClient" /> in Autofac container using
        ///     <see cref="SessionsServiceClientSettings" />.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings"><see cref="SessionsServiceClientSettings" /> client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder" /> configure handler.</param>
        public static void RegisterSessionsServiceClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] SessionsServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(SessionsServiceClientSettings.ServiceUrl));

            var clientBuilder = HttpClientGenerator.HttpClientGenerator.BuildForUrl(settings.ServiceUrl)
                .WithoutCaching()
                .WithoutRetries()
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder;

            builder.RegisterInstance(new SessionsServiceClient(clientBuilder.Create()))
                .As<ISessionsServiceClient>()
                .SingleInstance();
        }
    }
}
