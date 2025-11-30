using Xunit;

namespace vNext.Tests
{
    public class Run
    {
        [Fact]
        public async Task CheckRunThread()
        {
            var run = false; 
            var host = App.Create()
                .Workflow<Start>()
                .Entry<Start>()
                .Run(cancellation =>
                {
                    run = true;

                    return Task.CompletedTask;
                })
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(run);
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

        [Fact]
        public void CheckDuplicatedRunRegistration()
        {
            Assert.Throws<Exception>(() =>
            App.Create()
                .Run((cancellation) => { return Task.CompletedTask; })
                .Run((cancellation) => { return Task.CompletedTask; }));
        }
    }
}