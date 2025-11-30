using Xunit;

namespace vNext.Tests
{
    public class NoStartup
    {
        static bool Activated = false;

        [Fact]
        public async Task RunWithoutStartup()
        {
            var host = App.Create()
                .Workflow<Start>()
                .Entry<Start>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(Activated);
        }

        class Start : State
        {
            readonly App app;

            public Start(App app)
            {
                this.app = app;
            }

            public override Result Activate(object? data)
            {
                Activated = true;

                app.Stop();

                return base.Activate(data);
            }
        }
    }
}