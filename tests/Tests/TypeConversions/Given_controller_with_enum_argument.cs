using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.TypeConversions;

[TestFixture]
public class Given_controller_with_enum_argument
{
    class WithEnumController
    {
        public enum WithEnum
        {
            Param1,
            ParamWithCasing
        }

        public WithEnumController()
        {
            OnIndex = p1 => string.Empty;
        }
        public Func<WithEnum?, string> OnIndex { get; set; }
        public string Index(WithEnum? value = null) { return OnIndex(value); }
    }

    [Test]
    public async Task It_can_handle_different_casing_for_enum()
    {
        foreach (var pair in new[] {
                new { value = "param1", expected = WithEnumController.WithEnum.Param1 },
                new { value = "paramwithcasing", expected = WithEnumController.WithEnum.ParamWithCasing },
            })
        {
            var parameters = new List<WithEnumController.WithEnum?>();
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new WithEnumController
            {
                OnIndex = p1 =>
                {
                    parameters.Add(p1);
                    return "";
                }
            });

            var arguments = AppHostBuilder.Create(sc)
                .Recognize(typeof(WithEnumController))
                .BuildAppHost()
                .Parse(new[] { "WithEnum", /*"Index", */"--value", pair.value });

            Assert.That(arguments.Unrecognized.Select(u => u.Value), Is.Empty);
            await arguments.InvokeAsync(new StringWriter());
            Assert.That(parameters, Is.EquivalentTo(new[] { pair.expected }));
        }
    }
}
