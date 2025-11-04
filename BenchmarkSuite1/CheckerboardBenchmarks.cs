using System;
using System.Collections.Generic;
using System.Drawing;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace Texel.Benchmarks
{
    [CPUUsageDiagnoser]
    // Benchmarks to compare naive per-cell checkerboard versus batched two-pass approach
    public class CheckerboardBenchmarks
    {
        [Params(64, 128, 256)]
        public int Grid;
        [Params(16, 32, 48)]
        public int CellSize;
        private List<Rectangle> _sink; // prevent optimization
        [GlobalSetup]
        public void Setup()
        {
            _sink = new List<Rectangle>(Grid * Grid);
        }

        [Benchmark]
        public int NaivePerCell()
        {
            _sink.Clear();
            int gw = Grid, gh = Grid;
            int count = 0;
            for (int y = 0; y < gh; y++)
            {
                for (int x = 0; x < gw; x++)
                {
                    bool isAlt = ((x + y) & 1) == 0;
                    // Simulate per-cell color decision and quad emission
                    var rect = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
                    _sink.Add(rect);
                    count += isAlt ? 1 : 2;
                }
            }

            return count + _sink.Count;
        }

        [Benchmark]
        public int BatchedTwoPass()
        {
            _sink.Clear();
            int gw = Grid, gh = Grid;
            int count = 0;
            // First color
            for (int y = 0; y < gh; y++)
            {
                int startX = (y & 1);
                for (int x = ((0 ^ startX) & 1); x < gw; x += 2)
                {
                    var rect = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
                    _sink.Add(rect);
                    count++;
                }
            }

            // Second color
            for (int y = 0; y < gh; y++)
            {
                int startX = (y & 1) ^ 1;
                for (int x = ((0 ^ startX) & 1); x < gw; x += 2)
                {
                    var rect = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
                    _sink.Add(rect);
                    count++;
                }
            }

            return count + _sink.Count;
        }
    }
}