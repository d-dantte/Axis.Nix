using Axis.Nix.Builder;
using Axis.Nix.Configuration;
using Axis.Nix.Tests.Types;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Axis.Nix.Tests.Unit.Builder
{
    [TestClass]
    public class NotifierBuilderTest
    {
        private Mock<IResolverContract> mockResolver = new Mock<IResolverContract>();
        private Mock<IRegistrarContract> mockRegistrar = new Mock<IRegistrarContract>();


        #region NewBuilder()
        [TestMethod]
        public void NewBuilder_ShouldReturnValidInstance()
        {
            var instance = NotifierBuilder.NewBuilder();
            Assert.IsNotNull(instance);
        }
        #endregion

        [TestMethod]
        public void Configure_AssignsOptions()
        {
            var instance = NotifierBuilder.NewBuilder();

            Options options = default;
            _ = instance.Configure(options);
            Assert.AreEqual(options, instance.BuilderOptions());

            options = new Options(AsyncOptions.DefaultOptions());
            _ = instance.Configure(options);
            Assert.AreEqual(options, instance.BuilderOptions());
        }


        #region WithEventHandler<..>(IRegistrarContract)
        [TestMethod]
        public void WithEventHandler_WithValidTypes_ShouldRegisterHandler()
        {
            // arrange
            var builder = NotifierBuilder.NewBuilder();
            var registrationMap = new Dictionary<Type, RegistrationInfo>();
            mockRegistrar
                .Setup(r => r.Register<Event1Handler>(
                    It.IsAny<RegistryScope>(),
                    It.IsAny<InterceptorProfile>(),
                    It.IsAny<IBindContext[]>()))
                .Returns(mockRegistrar.Object)
                .Verifiable();

            mockRegistrar
                .Setup(r => r.RootManifest())
                .Returns(new ReadOnlyDictionary<Type, RegistrationInfo>(registrationMap))
                .Verifiable();

            // test
            _ = builder.WithEventHandler<Event1Data, Event1Handler>(mockRegistrar.Object);

            // assert
            Assert.AreEqual(1, builder.EventTypes().Length);
            Assert.AreEqual(typeof(Event1Data), builder.EventTypes()[0]);
            Assert.AreEqual(typeof(Event1Handler), builder.HandlerTypesFor(typeof(Event1Data))[0]);
        }

        [TestMethod]
        public void WithEventHandler_WithDuplicateHandlers_ShouldThrowException()
        {
            // arrange
            var builder = NotifierBuilder.NewBuilder();
            var registrationMap = new Dictionary<Type, RegistrationInfo>();
            mockRegistrar
                .Setup(r => r.Register<Event1Handler>(
                    It.IsAny<RegistryScope>(),
                    It.IsAny<InterceptorProfile>(),
                    It.IsAny<IBindContext[]>()))
                .Returns(mockRegistrar.Object)
                .Verifiable();

            mockRegistrar
                .Setup(r => r.RootManifest())
                .Returns(new ReadOnlyDictionary<Type, RegistrationInfo>(registrationMap))
                .Verifiable();

            // test and assert
            _ = builder.WithEventHandler<Event1Data, Event1Handler>(mockRegistrar.Object);
            Assert.ThrowsException<ArgumentException>(() => builder.WithEventHandler<Event1Data, Event1Handler>(mockRegistrar.Object));
        }

        [TestMethod]
        public void WithEventHandler_WithNullRegistrar_ShouldThrowException()
        {
            // arrange
            var builder = NotifierBuilder.NewBuilder();

            // test and assert
            Assert.ThrowsException<ArgumentNullException>(() => builder.WithEventHandler<Event1Data, Event1Handler>((IRegistrarContract)null));
        }
        #endregion


        #region WithEventHandler<..>(THandler)
        [TestMethod]
        public void WithEventHandler2_WithValidTypes_ShouldRegisterHandler()
        {
            // arrange
            var builder = NotifierBuilder.NewBuilder();
            var instance = new Event1Handler();

            // test
            _ = builder.WithEventHandler<Event1Data, Event1Handler>(instance);

            // assert
            Assert.AreEqual(1, builder.EventTypes().Length);
            Assert.AreEqual(typeof(Event1Data), builder.EventTypes()[0]);
            Assert.AreEqual(instance, builder.HandlerInstancesFor(typeof(Event1Data))[0]);
        }

        [TestMethod]
        public void WithEventHandler2_WithNullRegistrar_ShouldThrowException()
        {
            // arrange
            var builder = NotifierBuilder.NewBuilder();

            // test and assert
            Assert.ThrowsException<ArgumentNullException>(() => builder.WithEventHandler<Event1Data, Event1Handler>((Event1Handler)null));
        }
        #endregion

        #region Build(IResolverContract)
        [TestMethod]
        public void Build_ShouldReturnValidNotifier()
        {
            var builder = NotifierBuilder.NewBuilder();
            var notifier = builder.Build(mockResolver.Object);
            Assert.IsNotNull(notifier);
            Assert.AreEqual(0, notifier.RegisteredEvents().Length);


            // setup
            mockRegistrar
                .Setup(r => r.RootManifest())
                .Returns(new ReadOnlyDictionary<Type, RegistrationInfo>(new Dictionary<Type, RegistrationInfo>()))
                .Verifiable();

            builder.Configure(
                new Options(
                    new AsyncOptions(
                        task => { })));

            builder.WithEventHandler<Event2Data, Event2Handler>(new Event2Handler());
            builder.WithEventHandler<Event1Data, Event1Handler>(mockRegistrar.Object);
            builder.WithEventHandler<Event1Data, Event1Handler2>(mockRegistrar.Object);

            // test
            notifier = builder.Build(mockResolver.Object);
            Assert.IsNotNull(notifier);
            Assert.AreEqual(2, notifier.RegisteredEvents().Length);
        }
        #endregion
    }
}
