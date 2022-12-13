using Autofac;
using MyJetWallet.B2C2.Client.Settings;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyJetWallet.B2C2.Client.Autofac
{
    public static class AutofacHelper
    {
        /// <summary>
        /// Registers <see cref="IB2С2RestClient"/> in Autofac container using <see cref="B2C2ClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">MarketMakerArbitrageDetector client settings.</param>
        public static void RegisterB2С2RestClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] B2C2ClientSettings settings)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            builder.RegisterType<B2С2RestClient>()
                .As<IB2С2RestClient>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(settings));
        }

        /// <summary>
        /// Registers <see cref="IB2С2WebSocketClient"/> in Autofac container using <see cref="B2C2ClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">MarketMakerArbitrageDetector client settings.</param>
        public static void RegisterB2С2WebSocketClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] B2C2ClientSettings settings)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            builder.RegisterType<B2С2WebSocketClient>()
                .As<IB2С2WebSocketClient>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(settings));
        }
    }
}
