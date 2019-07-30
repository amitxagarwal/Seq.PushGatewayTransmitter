using Prometheus.Client;
using Prometheus.Client.MetricPusher;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushgatewayTransmitter
{
    [SeqApp("Seq.PushgatewayReactor",
        Description = "Uses a Handlebars template to send events to Pushgateway.")]
    public class PushgatewayReactor : SeqApp, ISubscribeTo<LogEventData>
    {
        //private readonly object pushGatewayNotification;

        [SeqAppSetting(
            DisplayName = "Pushgateway URL",
            HelpText = "The URL of the Pushgateway")]
        public static string PushgatewayUrl { get; set; }

        public void On(Event<LogEventData> evt)
        {
            var applicationName = FormatTemplate(evt);
            var pushGatewayUrl = "https://kmd-shareddev-monitoring.westeurope.cloudapp.azure.com/pushgateway";
            var seqPushgatewayWorkerName = "pushgateway-testworker";
            var instanceName = "default";

            var defaultPusher = new MetricPusher(pushGatewayUrl, seqPushgatewayWorkerName, instanceName);
            IMetricPushServer server = new MetricPushServer(new IMetricPusher[]
            {
                defaultPusher
            });
            server.Start();
            var counter = Metrics.CreateCounter("webjobcounter", "helptext", new[] { "ApplicationName" });
            counter.Labels(applicationName).Inc();
            server.Stop();
        }

        public static string FormatTemplate(Event<LogEventData> evt)
        {
            var properties = (IDictionary<string, object>)ToDynamic(evt.Data.Properties ?? new Dictionary<string, object>());
            var resourceName = string.Empty;

            foreach (var property in properties)
            {
                if (property.Key == "Application")
                {
                    resourceName = property.Value.ToString();
                }
            }
            return resourceName;
        }

        private static IDictionary<string, object> ToDynamic(IReadOnlyDictionary<string, object> readOnlyDictionary)
        {
            throw new NotImplementedException();
        }
    }
}
