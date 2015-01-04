using NUnit.Framework;
using Isop.Gui;

namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class SingleScopeOnlyTests
    {
        [Test]
        public void Will_only_execute_the_first_scope()
        {
            var scope1 = false;
            var scope2 = false;
            var scope = new SingleScopeOnly();
            scope.Try(() => {
                scope.Try(() => {
                    scope2 = true;
                });
                scope1 = true;
            });
            Assert.That(!scope2);
            Assert.That(scope1);
        }
        [Test]
        public void Will_execute_separate_scopes()
        {
            var scope1 = false;
            var scope2 = false;
            var scope = new SingleScopeOnly();
            scope.Try(() => { scope1 = true; });
            scope.Try(() => { scope2 = true; });
            Assert.That(scope2);
            Assert.That(scope1);
        }
    }
}
