using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using ShaiRandom.Distributions.Continuous;
using ShaiRandom.Generators;
using Troschuetz.Random;
using IContinuousDistribution = ShaiRandom.Distributions.IContinuousDistribution;

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


    public struct BetaSamplingIterator
    {
        private IEnhancedRandom _rng;
        private double _alpha;
        private double _beta;

        private double _current;
        public double Current => _current;

        public BetaSamplingIterator(IEnhancedRandom rng, double alpha, double beta)
        {
            _rng = rng;
            _alpha = alpha;
            _beta = beta;
            _current = 0;
        }

        public bool MoveNext()
        {
            _current = BetaDistribution.Sample(_rng, _alpha, _beta);
            return true;
        }

        public BetaSamplingIterator GetEnumerator() => this;
    }

    public struct KumaraswamySamplingIterator
    {
        private IEnhancedRandom _rng;
        private double _alpha;
        private double _beta;

        private double _current;
        public double Current => _current;

        public KumaraswamySamplingIterator(IEnhancedRandom rng, double alpha, double beta)
        {
            _rng = rng;
            _alpha = alpha;
            _beta = beta;
            _current = 0;
        }

        public bool MoveNext()
        {
            _current = KumaraswamyDistribution.Sample(_rng, _alpha, _beta);
            return true;
        }

        public KumaraswamySamplingIterator GetEnumerator() => this;
    }

    public struct GenericAlphaBetaSamplingIterator
    {
        private IEnhancedRandom _rng;
        private double _alpha;
        private double _beta;
        private Func<IEnhancedRandom, double, double, double> _sampleFunc;

        private double _current;
        public double Current => _current;

        public GenericAlphaBetaSamplingIterator(Func<IEnhancedRandom, double, double, double> sampleFunc, IEnhancedRandom rng, double alpha, double beta)
        {
            _rng = rng;
            _alpha = alpha;
            _beta = beta;
            _current = default;
            _sampleFunc = sampleFunc;
        }

        public bool MoveNext()
        {
            _current = _sampleFunc(_rng, _alpha, _beta);
            return true;
        }

        public GenericAlphaBetaSamplingIterator GetEnumerator() => this;
    }

    [SuppressMessage("ReSharper", "IteratorNeverReturns")]
    public class TRandomMock
    {
        private IEnhancedRandom _rng;

        public TRandomMock(IEnhancedRandom rng)
        {
            _rng = rng;
        }

        public IEnumerable<double> BetaSamplesIEnumerable(double alpha, double beta)
        {
            if (!BetaDistribution.AreValidParams(alpha, beta)) throw new ArgumentException("Ye be given me invalid params!");
            while (true)
                yield return BetaDistribution.Sample(_rng, alpha, beta);
        }

        public BetaSamplingIterator BetaSamplesCustomIterator(double alpha, double beta)
        {
            if (!BetaDistribution.AreValidParams(alpha, beta)) throw new ArgumentException("Ye be given me invalid params!");
            return new BetaSamplingIterator(_rng, alpha, beta);
        }

        public GenericAlphaBetaSamplingIterator BetaSamplesCustomGenericIterator(double alpha, double beta)
        {
            if (!BetaDistribution.AreValidParams(alpha, beta)) throw new ArgumentException("Ye be given me invalid params!");
            return new GenericAlphaBetaSamplingIterator(BetaDistribution.Sample, _rng, alpha, beta);
        }

        public IEnumerable<double> KumaraswamySamplesIEnumerable(double alpha, double beta)
        {
            if (!KumaraswamyDistribution.AreValidParams(alpha, beta)) throw new ArgumentException("Ye be given me invalid params!");
            while (true)
                yield return KumaraswamyDistribution.Sample(_rng, alpha, beta);
        }

        public KumaraswamySamplingIterator KumaraswamySamplesCustomIterator(double alpha, double beta)
        {
            if (!KumaraswamyDistribution.AreValidParams(alpha, beta)) throw new ArgumentException("Ye be given me invalid params!");
            return new KumaraswamySamplingIterator(_rng, alpha, beta);
        }

        public GenericAlphaBetaSamplingIterator KumaraswamySamplesCustomGenericIterator(double alpha, double beta)
        {
            if (!KumaraswamyDistribution.AreValidParams(alpha, beta)) throw new ArgumentException("Ye be given me invalid params!");
            return new GenericAlphaBetaSamplingIterator(KumaraswamyDistribution.Sample, _rng, alpha, beta);
        }
    }

    // |                                 Method | NumValues | Alpha | Beta |        Mean |     Error |    StdDev |
    // |--------------------------------------- |---------- |------ |----- |------------:|----------:|----------:|
    // |                  BetaSampleIEnumerable |         5 |     2 |  2.5 |    451.2 ns |   1.83 ns |   1.62 ns |
    // |               BetaSampleCustomIterator |         5 |     2 |  2.5 |    419.3 ns |   2.17 ns |   2.03 ns |
    // |        BetaSampleCustomGenericIterator |         5 |     2 |  2.5 |    425.8 ns |   2.19 ns |   2.05 ns |
    // |           KumaraswamySampleIEnumerable |         5 |     2 |  2.5 |    442.5 ns |   3.08 ns |   2.73 ns |
    // |        KumaraswamySampleCustomIterator |         5 |     2 |  2.5 |    393.9 ns |   1.48 ns |   1.39 ns |
    // | KumaraswamySampleCustomGenericIterator |         5 |     2 |  2.5 |    391.1 ns |   0.97 ns |   0.86 ns |
    // |                  BetaSampleIEnumerable |        10 |     2 |  2.5 |    895.2 ns |   4.06 ns |   3.39 ns |
    // |               BetaSampleCustomIterator |        10 |     2 |  2.5 |    837.5 ns |   9.28 ns |   8.23 ns |
    // |        BetaSampleCustomGenericIterator |        10 |     2 |  2.5 |    830.6 ns |   5.40 ns |   5.05 ns |
    // |           KumaraswamySampleIEnumerable |        10 |     2 |  2.5 |    868.8 ns |   2.12 ns |   1.88 ns |
    // |        KumaraswamySampleCustomIterator |        10 |     2 |  2.5 |    762.0 ns |   3.87 ns |   3.23 ns |
    // | KumaraswamySampleCustomGenericIterator |        10 |     2 |  2.5 |    764.0 ns |   2.54 ns |   2.38 ns |
    // |                  BetaSampleIEnumerable |        50 |     2 |  2.5 |  4,323.5 ns |  35.99 ns |  33.66 ns |
    // |               BetaSampleCustomIterator |        50 |     2 |  2.5 |  4,099.8 ns |  23.45 ns |  21.94 ns |
    // |        BetaSampleCustomGenericIterator |        50 |     2 |  2.5 |  4,111.2 ns |  35.92 ns |  33.60 ns |
    // |           KumaraswamySampleIEnumerable |        50 |     2 |  2.5 |  4,216.0 ns |  13.56 ns |  11.33 ns |
    // |        KumaraswamySampleCustomIterator |        50 |     2 |  2.5 |  3,748.8 ns |   6.39 ns |   5.33 ns |
    // | KumaraswamySampleCustomGenericIterator |        50 |     2 |  2.5 |  3,755.1 ns |  22.48 ns |  19.92 ns |
    // |                  BetaSampleIEnumerable |       100 |     2 |  2.5 |  8,649.9 ns |  78.54 ns |  73.47 ns |
    // |               BetaSampleCustomIterator |       100 |     2 |  2.5 |  8,250.5 ns |  78.96 ns |  69.99 ns |
    // |        BetaSampleCustomGenericIterator |       100 |     2 |  2.5 |  8,077.9 ns |  31.30 ns |  26.14 ns |
    // |           KumaraswamySampleIEnumerable |       100 |     2 |  2.5 |  8,301.9 ns |  54.16 ns |  45.23 ns |
    // |        KumaraswamySampleCustomIterator |       100 |     2 |  2.5 |  7,478.7 ns |  16.22 ns |  15.17 ns |
    // | KumaraswamySampleCustomGenericIterator |       100 |     2 |  2.5 |  7,528.9 ns |  75.38 ns |  70.51 ns |
    // |                  BetaSampleIEnumerable |      1000 |     2 |  2.5 | 85,646.1 ns | 471.86 ns | 394.03 ns |
    // |               BetaSampleCustomIterator |      1000 |     2 |  2.5 | 80,705.7 ns | 229.42 ns | 191.57 ns |
    // |        BetaSampleCustomGenericIterator |      1000 |     2 |  2.5 | 81,416.8 ns | 181.54 ns | 160.93 ns |
    // |           KumaraswamySampleIEnumerable |      1000 |     2 |  2.5 | 83,631.6 ns | 355.59 ns | 315.22 ns |
    // |        KumaraswamySampleCustomIterator |      1000 |     2 |  2.5 | 74,567.7 ns | 294.73 ns | 246.12 ns |
    // | KumaraswamySampleCustomGenericIterator |      1000 |     2 |  2.5 | 75,578.8 ns | 262.72 ns | 232.90 ns |
    public class InfiniteDistributionSampleBenchmarks
    {
        [Params(5, 10, 50, 100, 1000)]
        public int NumValues;

        [Params(2.0)]
        public double Alpha;

        [Params(2.5)]
        public double Beta;

        private TRandomMock _rng = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _rng = new TRandomMock(new MizuchiRandom(1UL));
        }

        // Tests will purposely avoid using LINQ's Take, to make it fair for the other iterator tests, and also to
        // avoid incurring the overhead of Linq in the general-case IEnumerable test.
        [Benchmark]
        public double BetaSampleIEnumerable()
        {
            double lastNum = 0.0;
            int numbersGenerated = 1;
            foreach (double num in _rng.BetaSamplesIEnumerable(Alpha, Beta))
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public double BetaSampleCustomIterator()
        {
            double lastNum = 0.0;
            int numbersGenerated = 1;
            foreach (double num in _rng.BetaSamplesCustomIterator(Alpha, Beta))
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public double BetaSampleCustomGenericIterator()
        {
            double lastNum = 0.0;
            int numbersGenerated = 1;
            foreach (double num in _rng.BetaSamplesCustomIterator(Alpha, Beta))
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        // Tests will purposely avoid using LINQ's Take, to make it fair for the other iterator tests, and also to
        // avoid incurring the overhead of Linq in the general-case IEnumerable test.
        [Benchmark]
        public double KumaraswamySampleIEnumerable()
        {
            double lastNum = 0.0;
            int numbersGenerated = 1;
            foreach (double num in _rng.KumaraswamySamplesIEnumerable(Alpha, Beta))
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public double KumaraswamySampleCustomIterator()
        {
            double lastNum = 0.0;
            int numbersGenerated = 1;
            foreach (double num in _rng.KumaraswamySamplesCustomIterator(Alpha, Beta))
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

        [Benchmark]
        public double KumaraswamySampleCustomGenericIterator()
        {
            double lastNum = 0.0;
            int numbersGenerated = 1;
            foreach (double num in _rng.KumaraswamySamplesCustomIterator(Alpha, Beta))
            {
                lastNum = num;
                if (numbersGenerated == NumValues)
                    break;
                numbersGenerated++;
            }

            return lastNum;
        }

    }
}
