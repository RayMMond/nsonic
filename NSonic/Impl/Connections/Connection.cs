using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NSonic.Impl.Connections
{
    abstract class Connection : ISonicConnection
    {
        private readonly ISessionFactory sessionFactory;
        internal readonly IDisposableClient client;
        private readonly Configuration configuration;
        protected readonly ISession session;
        protected readonly ActionBlock<Func<Task>> commandQueue;
        protected readonly CancellationTokenSource source;

        protected Connection(ISessionFactory sessionFactory
            , IRequestWriter requestWriter
            , IDisposableClient client
            , Configuration configuration
            )
        {
            this.sessionFactory = sessionFactory;
            this.RequestWriter = requestWriter;
            this.client = client;
            this.configuration = configuration;

            this.client.Configure(this.configuration.WithMode(this.Mode));

            this.session = this.sessionFactory.Create(this.client);

            source = new CancellationTokenSource();

            commandQueue = new ActionBlock<Func<Task>>(ExecuteCommand, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                CancellationToken = source.Token,
                EnsureOrdered = true
            });
        }

        protected abstract ConnectionMode Mode { get; }

        protected IRequestWriter RequestWriter { get; }

        public void Connect()
        {
            this.client.Connect();
        }

        public async Task ConnectAsync()
        {
            await this.client.ConnectAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.client?.Dispose();
                this.session.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ExecuteCommand(Func<Task> cmd)
        {
            throw new NotImplementedException();
        }
    }
}
