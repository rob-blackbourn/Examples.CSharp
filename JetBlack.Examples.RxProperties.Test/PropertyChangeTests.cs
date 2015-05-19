using System;
using NUnit.Framework;

namespace JetBlack.Examples.RxProperties.Test
{
    [TestFixture]
    public class PropertyChangeTests
    {
        [Test]
        public void Test1()
        {
            var person = new Person {Name = "Rob Blackbourn", DateOfBirth = new DateTime(1967, 8, 12)};

            DateTime dateOfBirth = default (DateTime);
            person.ToObservable(x => x.DateOfBirth)
                .Subscribe(x => dateOfBirth = x);
            person.DateOfBirth += TimeSpan.FromDays(1);
            Assert.AreEqual(person.DateOfBirth, dateOfBirth);
        }
    }
}
