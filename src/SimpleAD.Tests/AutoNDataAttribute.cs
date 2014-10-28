using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Ploeh.AutoFixture.Xunit;

namespace SimpleAD.Tests
{
    public class AutoNAttribute : AutoDataAttribute
    {
        public AutoNAttribute()
            : base(new Fixture()
                .Customize(new AutoConfiguredNSubstituteCustomization()))
        {
        }
    }
}