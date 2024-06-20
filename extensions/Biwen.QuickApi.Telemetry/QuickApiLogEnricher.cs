using Microsoft.AspNetCore.Diagnostics.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.Enrichment;


namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// QuickApiLogEnricher
    /// </summary>
    internal class QuickApiLogEnricher : IStaticLogEnricher
    {
        public void Enrich(IEnrichmentTagCollector collector)
        {
            collector.Add("UserName", "vipwan");
        }
    }
}