using System;
using FluentAssertions;
using Horizon.Core.Abstractions;
using Horizon.PriceEngine.Services;
using Xunit;

namespace Horizon.Tests
{
    public class PriceEngineTests
    {
        private sealed class FixedRandom(double value) : IRandomProvider
        { public double NextDouble() => value; }

        [Fact]
        public void Generator_DeltaWithin2Percent()
        {
            var rnd = new FixedRandom(0.5);
            var gen = new CorrelatedRandomWalkGenerator(rnd);
            var prev = 100m;
            var tick = gen.Next("AAPL", prev, DateTime.UtcNow, 1);
            var diff = (tick.Price - prev) / prev;
            diff.Should().BeInRange(-0.02m, 0.02m);
        }
        [Fact]
        public void Generator_Propagates_Metadata_Correctly()
        {
            var gen = new CorrelatedRandomWalkGenerator(new FixedRandom(0.5));
            var now = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            var tick = gen.Next("MSFT", 200m, now, 42);

            tick.Symbol.Should().Be("MSFT");
            tick.TimestampUtc.Should().Be(now);
            tick.SequenceId.Should().Be(42);
            tick.Price.Should().BeInRange(196m, 204m); 
        }
        [Fact]
        public void Generator_Is_Deterministic_With_FixedRandom()
        {
            var rnd = new FixedRandom(0.75);
            var gen = new CorrelatedRandomWalkGenerator(rnd);
            var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var t1 = gen.Next("TSLA", 150m, now, 1);
            // Aynı koşulları tekrar etmek için yeni generator + aynı FixedRandom
            var t2 = new CorrelatedRandomWalkGenerator(new FixedRandom(0.75))
                        .Next("TSLA", 150m, now, 1);

            t1.Price.Should().Be(t2.Price);
        }
    }
}
