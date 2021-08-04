using System;
using FluentAssertions;
using NUnit.Framework;

namespace FirstAzureFunction.UnitTests
{
    public class RequestTest
    {
        [Test]
        public void TheSumOfAnEmptyListIsZero()
        {
            var request = new Request() { Addends = System.Array.Empty<int>()};

            var result = request.GetSumOfIntegers();

            result.Should().Be(0);
        }

        [Test]
        public void MultipleIntegersAreSummedUp()
        {
            var request = new Request() { Addends = new []{12, -12, 13, -14} };

            var result = request.GetSumOfIntegers();

            result.Should().Be(-1);
        }

        [Test]
        public void WhenTheResultIsBiggerThanTheMaxSafeIntegerItThrowsAnError()
        {
            var request = new Request() { Addends = new[] { int.MaxValue, 2 } };

            request.Invoking(r => r.GetSumOfIntegers()).Should().Throw<OverflowException>();
        }
    }
}
