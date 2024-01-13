using Axis.Nix.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Nix.Tests.Unit.Event
{
    [TestClass]
    public class DomainEventTests
    {
        #region Constructor
        [TestMethod]
        public void Constructor_ShouldReturnValidInstance()
        {
            var @event = new DomainEvent<int>("some-name", 5);
            Assert.IsNotNull(@event);
        }

        [TestMethod]
        public void Constructor_WithInvalidArgs_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentException>(() => new DomainEvent<int>(null, 5));
            Assert.ThrowsException<ArgumentException>(() => new DomainEvent<int>("", 5));
            Assert.ThrowsException<ArgumentException>(() => new DomainEvent<int>(" ", 5));
            Assert.ThrowsException<ArgumentException>(() => new DomainEvent<int>(" \t\r\n", 5));
            Assert.ThrowsException<ArgumentNullException>(() => new DomainEvent<string>("some-event", null));
        }
        #endregion

        [TestMethod]
        public void Name_ShouldContainInitializedValue()
        {
            var eventName = "event-name";
            var @event = new DomainEvent<Guid>(eventName, Guid.NewGuid());
            Assert.AreEqual(eventName, @event.Name);
        }

        [TestMethod]
        public void Data_ShouldContainInitializedValue()
        {
            var data = Guid.NewGuid();
            var @event = new DomainEvent<Guid>("the event", data);
            Assert.AreEqual(data, @event.Data);
        }

        [TestMethod]
        public void EqualityTest()
        {
            var date = DateTimeOffset.Now;
            var event1 = new DomainEvent<DateTimeOffset>("theEvent", date);
            var event2 = new DomainEvent<int>("theEvent", 5);
            var event3 = new DomainEvent<DateTimeOffset>("theEvent", date - TimeSpan.FromDays(43));
            var event4 = new DomainEvent<DateTimeOffset>("theEvent", date);
            var event5 = new DomainEvent<DateTimeOffset>("theEvent-x", date);

            Assert.IsTrue(event1.Equals(event1));
            Assert.IsTrue(event1.Equals(event4));
            Assert.IsFalse(event1.Equals(event2));
            Assert.IsFalse(event1.Equals(event3));
            Assert.IsFalse(event1.Equals(event5));

            Assert.IsTrue(event1 == event4);
            Assert.IsTrue(event1 != event3);
            Assert.IsTrue(event1 != event5);
        }
    }
}
