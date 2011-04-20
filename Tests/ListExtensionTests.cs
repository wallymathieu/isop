using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Helpers.Tests
{
    [TestFixture]
    internal class ListExtensionTests
    {
        [Test]
        public void GetForIndexOrDefault_Can_return_default_for_values_outside_of_range()
        {
            Assert.That(new[] { 1, 2, 3 }.ToList().GetForIndexOrDefault(3), Is.EqualTo(0));
        }
        [Test]
        public void GetForIndexOrDefault_Can_return_value_for_index_inside_range()
        {
            Assert.That(new[] { 1, 2, 3 }.ToList().GetForIndexOrDefault(1), Is.EqualTo(2));
        }
        [Test]
        public void GetForIndexOrDefault_Can_return_value_for_first_index_inside_range()
        {
            Assert.That(new[] { 1, 2, 3 }.ToList().GetForIndexOrDefault(0), Is.EqualTo(1));
        }
        [Test]
        public void FindIndexAndValues_Can_return_correct_index()
        {
            Assert.That(new[] { "1", "2", "3" }.ToList().FindIndexAndValues(value => value == "1"), 
                        Is.EquivalentTo(new[] { new KeyValuePair<int,string>(0,"1") }));
        }
        [Test]
        public void FindIndexAndValues_Can_find_all()
        {
            Assert.That(new[] { "1", "2", "3" }.ToList().FindIndexAndValues(value => true),
                        Is.EquivalentTo(new[] {
                                                  new KeyValuePair<int, string>(0, "1"),
                                                  new KeyValuePair<int, string>(1, "2"),
                                                  new KeyValuePair<int, string>(2, "3")
                                              }));
        }
    }
}