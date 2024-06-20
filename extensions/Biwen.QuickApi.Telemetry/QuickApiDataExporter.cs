using Microsoft.Extensions.Diagnostics.Latency;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace Biwen.QuickApi.Telemetry;

/// <summary>
/// 自定义的LatencyDataExporter
/// </summary>
internal sealed class QuickApiDataExporter : ILatencyDataExporter
{
    private const int MillisPerSecond = 1000;
    private static readonly CompositeFormat _title = CompositeFormat.Parse("QuickApi #{0}: {1}ms, {2} checkpoints, {3} tags, {4} measures" + Environment.NewLine);
    private readonly bool _outputCheckpoints;
    private readonly bool _outputTags;
    private readonly bool _outputMeasures;
    private long _sampleCount = -1;

    public QuickApiDataExporter(IOptions<LatencyConsoleOptions> options)
    {
        var o = options.Value;
        _outputCheckpoints = o.OutputCheckpoints;
        _outputTags = o.OutputTags;
        _outputMeasures = o.OutputMeasures;
    }

    public Task ExportAsync(LatencyData latencyData, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        try
        {
            var cnt = Interlocked.Increment(ref _sampleCount);

            _ = sb.AppendFormat(
                CultureInfo.InvariantCulture,
                _title,
                cnt,
                (double)latencyData.DurationTimestamp / latencyData.DurationTimestampFrequency * MillisPerSecond,
                latencyData.Checkpoints.Length,
                latencyData.Tags.Length,
                latencyData.Measures.Length);

            bool needBlankLine = false;
            if (_outputCheckpoints && latencyData.Checkpoints.Length > 0)
            {
                int nameColumnWidth = 0;
                for (int i = 0; i < latencyData.Checkpoints.Length; i++)
                {
                    nameColumnWidth = Math.Max(nameColumnWidth, latencyData.Checkpoints[i].Name.Length);
                }

                var fmt = StartTable(sb, "Checkpoint", "Value (ms)", nameColumnWidth, ref needBlankLine);
                for (int i = 0; i < latencyData.Checkpoints.Length; i++)
                {
                    var c = latencyData.Checkpoints[i];
                    _ = sb.AppendFormat(CultureInfo.InvariantCulture, fmt, c.Name, (double)c.Elapsed / c.Frequency * MillisPerSecond);
                }
            }

            if (_outputTags && latencyData.Tags.Length > 0)
            {
                int nameColumnWidth = 0;
                for (int i = 0; i < latencyData.Tags.Length; i++)
                {
                    nameColumnWidth = Math.Max(nameColumnWidth, latencyData.Tags[i].Name.Length);
                }

                var fmt = StartTable(sb, "Tag", "Value", nameColumnWidth, ref needBlankLine);
                for (int i = 0; i < latencyData.Tags.Length; i++)
                {
                    var t = latencyData.Tags[i];
                    _ = sb.AppendFormat(CultureInfo.InvariantCulture, fmt, t.Name, t.Value);
                }
            }

            if (_outputMeasures && latencyData.Measures.Length > 0)
            {
                int nameColumnWidth = 0;
                for (int i = 0; i < latencyData.Measures.Length; i++)
                {
                    nameColumnWidth = Math.Max(nameColumnWidth, latencyData.Measures[i].Name.Length);
                }

                var fmt = StartTable(sb, "Measure", "Value", nameColumnWidth, ref needBlankLine);
                for (int i = 0; i < latencyData.Measures.Length; i++)
                {
                    var m = latencyData.Measures[i];
                    _ = sb.AppendFormat(CultureInfo.InvariantCulture, fmt, m.Name, m.Value);
                }
            }

            // the whole sample is output in a single shot so it won't be interrupted with conflicting output
            return Console.Out.WriteAsync(sb.ToString());
        }
        finally
        {
            sb = null;

        }
    }

    private static CompositeFormat StartTable(StringBuilder sb, string nameHeader, string valueHeader, int nameColumnWidth, ref bool needBlankLine)
    {
        if (needBlankLine)
        {
            _ = sb.AppendLine();
        }
        else
        {
            needBlankLine = true;
        }

        return CompositeFormat.Parse("{0,-" + nameColumnWidth + "}  {1}" + Environment.NewLine);
    }
}
