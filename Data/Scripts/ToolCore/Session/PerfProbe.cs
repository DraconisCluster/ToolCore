using System;
using System.Diagnostics;
using VRage.Utils;

namespace ToolCore.Session
{
    //TEST ONLY - CompLoop timing for A/B measurement, never merge
    internal partial class ToolSession
    {
        private const string PerfBuild = "baseline";
        private const int PerfWindow = 3600;

        private readonly Stopwatch _perfWatch = new Stopwatch();
        private readonly long[] _perfSamples = new long[PerfWindow];
        private readonly long[] _perfSorted = new long[PerfWindow];
        private int _perfIndex;
        internal long PerfVisits;

        private void PerfRecord(long elapsedTicks)
        {
            _perfSamples[_perfIndex++] = elapsedTicks;
            if (_perfIndex < PerfWindow)
                return;

            _perfIndex = 0;
            Array.Copy(_perfSamples, _perfSorted, PerfWindow);
            Array.Sort(_perfSorted);

            long sum = 0;
            for (int i = 0; i < PerfWindow; i++)
                sum += _perfSorted[i];

            int activated = 0, shooting = 0, hitting = 0, broken = 0;
            foreach (var comp in ToolMap.Values)
            {
                if (comp._activated) activated++;
                if (comp.GunBase.Shooting) shooting++;
                if (comp.WasHitting) hitting++;
                if (comp.Broken) broken++;
            }

            var toMs = 1000.0 / Stopwatch.Frequency;
            MyLog.Default.WriteLineAndConsole(
                $"[ToolCorePerf] build={PerfBuild} ds={IsDedicated} tick={Tick} tools={ToolMap.Count} grids={GridList.Count} " +
                $"activated={activated} shooting={shooting} hitting={hitting} broken={broken} " +
                $"visits/tick={PerfVisits / (double)PerfWindow:0.00} " +
                $"compLoopMs avg={sum * toMs / PerfWindow:0.0000} p50={_perfSorted[PerfWindow / 2] * toMs:0.0000} " +
                $"p95={_perfSorted[PerfWindow * 95 / 100] * toMs:0.0000} p99={_perfSorted[PerfWindow * 99 / 100] * toMs:0.0000} " +
                $"max={_perfSorted[PerfWindow - 1] * toMs:0.0000}");

            PerfVisits = 0;
        }
    }
}
