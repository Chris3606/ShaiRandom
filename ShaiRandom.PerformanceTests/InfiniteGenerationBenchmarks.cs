using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using ShaiRandom.Generators;

namespace ShaiRandom.PerformanceTests
{
    public struct ULongGenerationIterator
    {
        private IEnhancedRandom _rng;

        private ulong _current;
        public ulong Current => _current;

        public ULongGenerationIterator(IEnhancedRandom rng)
        {
            _rng = rng;
            _current = 0;
        }

        public bool MoveNext()
        {
            _current = _rng.NextULong();
            return true;
        }

        public ULongGenerationIterator GetEnumerator() => this;

        public IEnumerable<ulong> ToEnumerable()
        {
            foreach (ulong val in this)
                yield return val;
        }
    }

    public ref struct ULongGenerationIteratorRef
    {
        private IEnhancedRandom _rng;

        private ulong _current;
        public ulong Current => _current;

        public ULongGenerationIteratorRef(IEnhancedRandom rng)
        {
            _rng = rng;
            _current = 0;
        }

        public bool MoveNext()
        {
            _current = _rng.NextULong();
            return true;
        }

        public ULongGenerationIteratorRef GetEnumerator() => this;
    }

    [SuppressMessage("ReSharper", "IteratorNeverReturns")]
    public static class InfiniteGenerationExtensions
    {
        public static IEnumerable<ulong> NextULongIEnumerable(this IEnhancedRandom rng)
        {
            while (true)
                yield return rng.NextULong();
        }

        public static ULongGenerationIterator ULongIterator(this IEnhancedRandom rng)
            => new ULongGenerationIterator(rng);

        public static ULongGenerationIteratorRef ULongRefIterator(this IEnhancedRandom rng)
            => new ULongGenerationIteratorRef(rng);
    }

    // |                                 Method | NumValues |        Mean |     Error |    StdDev |
    // |--------------------------------------- |---------- |------------:|----------:|----------:|
    // |                   GenULongsIEnumerable |         5 |    45.89 ns |  0.880 ns |  0.864 ns |
    // | GenULongsIEnumerableFromCustomIterator |         5 |    58.38 ns |  0.643 ns |  0.601 ns |
    // |                GenULongsCustomIterator |         5 |    12.06 ns |  0.053 ns |  0.050 ns |
    // |             GenULongsCustomRefIterator |         5 |    12.07 ns |  0.076 ns |  0.067 ns |
    // |                    GenULongsManualLoop |         5 |    10.52 ns |  0.069 ns |  0.061 ns |
    // |                   GenULongsIEnumerable |        10 |    83.88 ns |  0.595 ns |  0.497 ns |
    // | GenULongsIEnumerableFromCustomIterator |        10 |    95.14 ns |  0.458 ns |  0.428 ns |
    // |                GenULongsCustomIterator |        10 |    23.91 ns |  0.201 ns |  0.179 ns |
    // |             GenULongsCustomRefIterator |        10 |    26.35 ns |  0.231 ns |  0.216 ns |
    // |                    GenULongsManualLoop |        10 |    22.55 ns |  0.206 ns |  0.182 ns |
    // |                   GenULongsIEnumerable |        50 |   330.48 ns |  1.364 ns |  1.276 ns |
    // | GenULongsIEnumerableFromCustomIterator |        50 |   327.82 ns |  3.182 ns |  2.977 ns |
    // |                GenULongsCustomIterator |        50 |   138.04 ns |  1.576 ns |  1.474 ns |
    // |             GenULongsCustomRefIterator |        50 |   136.61 ns |  0.829 ns |  0.647 ns |
    // |                    GenULongsManualLoop |        50 |   112.17 ns |  1.289 ns |  1.076 ns |
    // |                   GenULongsIEnumerable |       100 |   636.57 ns |  1.072 ns |  0.837 ns |
    // | GenULongsIEnumerableFromCustomIterator |       100 |   601.21 ns |  8.839 ns |  7.835 ns |
    // |                GenULongsCustomIterator |       100 |   266.70 ns |  2.285 ns |  2.138 ns |
    // |             GenULongsCustomRefIterator |       100 |   243.15 ns |  2.539 ns |  2.375 ns |
    // |                    GenULongsManualLoop |       100 |   217.85 ns |  1.183 ns |  0.988 ns |
    // |                   GenULongsIEnumerable |      1000 | 6,150.19 ns | 20.113 ns | 18.814 ns |
    // | GenULongsIEnumerableFromCustomIterator |      1000 | 5,778.75 ns | 60.695 ns | 56.774 ns |
    // |                GenULongsCustomIterator |      1000 | 2,605.59 ns | 18.084 ns | 16.031 ns |
    // |             GenULongsCustomRefIterator |      1000 | 2,602.60 ns | 11.695 ns | 10.367 ns |
    // |                    GenULongsManualLoop |      1000 | 2,361.67 ns | 14.343 ns | 13.417 ns |
    public class InfiniteGenerationBenchmarks
    {
        [Params(5, 10, 50, 100, 1000)]
        public int NumValues;

        private IEnhancedRandom _rng = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _rng = new MizuchiRandom(1UL);
        }

        // Tests will purposely avoid using LINQ's Take, to make it fair for the other iterator tests, and also to
        // avoid incurring the overhead of Linq in the general-case IEnumerable test.
        [Benchmark]
        public ulong GenULongsIEnumerable()
        {
            ulong lastNum = 0;
            int numbersGenerated = 1;
            foreach (ulong num in _rng.NextULongIEnumerable())
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public ulong GenULongsIEnumerableFromCustomIterator()
        {
            ulong lastNum = 0;
            int numbersGenerated = 1;
            foreach (ulong num in _rng.ULongIterator().ToEnumerable())
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public ulong GenULongsCustomIterator()
        {
            ulong lastNum = 0;
            int numbersGenerated = 1;
            foreach (ulong num in _rng.ULongIterator())
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public ulong GenULongsCustomRefIterator()
        {
            ulong lastNum = 0;
            int numbersGenerated = 1;
            foreach (ulong num in _rng.ULongRefIterator())
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public ulong GenULongsManualLoop()
        {
            ulong lastNum = 0;
            for (int i = 1; i <= NumValues; i++)
                lastNum = _rng.NextULong();

            return lastNum;
        }
    }
}
