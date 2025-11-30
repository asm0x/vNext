using Xunit;

namespace vNext.Tests
{
    public class Exit
    {
        [Fact]
        public async Task CheckExitEvent()
        {
            var exit = false;
            var host = App.Create()
                .Workflow<Start>()
                .Entry<Start>()
                .Build();

            host.Exit += () => exit = true;

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(exit);
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
                app.Stop();

                return base.Activate(data);
            }
        }
    }
}