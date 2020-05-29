using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NSonic.Impl.Connections
{
    sealed class ControlConnection : Connection, ISonicControlConnection
    {
        public ControlConnection(ISessionFactory sessionFactory
            , IRequestWriter requestWriter
            , IDisposableClient client
            , Configuration configuration
            )
            : base(sessionFactory, requestWriter, client, configuration)
        {
            //
        }

        protected override ConnectionMode Mode => ConnectionMode.Control;

        public string Info()
        {
            var tsc = new TaskCompletionSource<string>();

            commandQueue.Post(() =>
            {
                tsc.SetResult(this.RequestWriter.WriteResult(session, "INFO"));
                return Task.CompletedTask;
            });

            return tsc.Task.Result;
        }

        public async Task<string> InfoAsync()
        {
            var tsc = new TaskCompletionSource<string>();

            await commandQueue.SendAsync(async () =>
            {
                var result = await this.RequestWriter.WriteResultAsync(session, "INFO");
                tsc.SetResult(result);
            });

            return await tsc.Task;
        }

        public void Trigger(string action, string data = null)
        {
            var tsc = new TaskCompletionSource<object>();

            commandQueue.Post(() =>
            {
                this.RequestWriter.WriteOk(session, "TRIGGER", action, data);
                tsc.SetResult(null);
                return Task.CompletedTask;
            });

            tsc.Task.Wait();
        }

        public async Task TriggerAsync(string action, string data = null)
        {
            var tsc = new TaskCompletionSource<object>();

            await commandQueue.SendAsync(async () =>
            {
                await this.RequestWriter.WriteOkAsync(session, "TRIGGER", action, data);
                tsc.SetResult(null);
            });

            await tsc.Task;
        }
    }
}
