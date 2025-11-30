namespace vNext
{
    public interface IApp
    {
        /// <summary>
        /// Event of application stopping.
        /// </summary>
        event Action Exit;

        Task RunAsync(CancellationToken token);

        Task RunAsync();

        void Stop();
    }
}
