using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using IExtLogger = Microsoft.Extensions.Logging.ILogger;
using ISeriLogger = Serilog.ILogger;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Program>();
        }

        private ISeriLogger _serilogger;
        private IExtLogger _extLogger;
        private object _value;

        private const string TestMessage = "Test message {p1} {@p2} {$p3} Test message.";

        [GlobalSetup]
        public void GlobalSetup()
        {
            _serilogger = Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .WriteTo.Sink(new NoopSink())
              .CreateLogger();

            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            var provider = services.BuildServiceProvider();
            _extLogger = provider.GetRequiredService<ILogger<Program>>();

            _value = new { a = 1, b = true };
        }

        [Benchmark(Baseline = true)]
        public void Serilog()
        {
            _serilogger.Information(TestMessage, _value, _value, _value);
        }

        [Benchmark]
        public void Extension()
        {
            _extLogger.LogInformation(TestMessage, _value, _value, _value);
        }

        [Benchmark]
        public void ExtensionWithEventId()
        {
            _extLogger.LogInformation(new EventId(1), TestMessage, _value, _value, _value);
        }
    }

    public class NoopSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent) { }
    }
}
