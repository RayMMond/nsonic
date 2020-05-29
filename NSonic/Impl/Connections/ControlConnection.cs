using System;
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
                try
                {
                    tsc.SetResult(this.RequestWriter.WriteResult(session, "INFO"));
                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                    return Task.CompletedTask;
                }
            });

            return tsc.Task.Result;
        }

        public async Task<string> InfoAsync()
        {
            var tsc = new TaskCompletionSource<string>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    var result = await this.RequestWriter.WriteResultAsync(session, "INFO");
                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            return await tsc.Task;
        }

        public void Trigger(string action, string data = null)
        {
            var tsc = new TaskCompletionSource<object>();

            commandQueue.Post(() =>
            {
                try
                {
                    this.RequestWriter.WriteOk(session, "TRIGGER", action, data);
                    tsc.SetResult(null);
                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                    return Task.CompletedTask;
                }
            });

            tsc.Task.Wait();
        }

        public async Task TriggerAsync(string action, string data = null)
        {
            var tsc = new TaskCompletionSource<object>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    await this.RequestWriter.WriteOkAsync(session, "TRIGGER", action, data);
                    tsc.SetResult(null);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            await tsc.Task;
        }
    }
}
