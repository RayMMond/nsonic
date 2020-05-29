using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NSonic.Impl;
using NSonic.Impl.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NSonic.Tests.Stubs
{
    class StubSessionFactory : INonLockingSessionFactory
    {
        public StubSessionFactory(MockSequence sequence, ConnectionMode mode, bool async)
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.TcpClient = new Mock<ITcpClient>();

            if (async)
            {
                this.TcpClient
                    .InSequence(sequence)
                    .Setup(tc => tc.ConnectAsync(StubConstants.Hostname, StubConstants.Port))
                    .Callback((string x, int y) =>
                    {
                        this.TcpClient.Setup(tc => tc.Connected).Returns(true);
                    })
                    .Returns(Task.CompletedTask)
                    ;
            }
            else
            {
                this.TcpClient
                    .InSequence(sequence)
                    .Setup(tc => tc.Connect(StubConstants.Hostname, StubConstants.Port))
                    .Callback((string x, int y) =>
                    {
                        this.TcpClient.Setup(tc => tc.Connected).Returns(true);
                    })
                    ;
            }

            this.TcpClient.Setup(tc => tc.Dispose());

            this.ConnectSession = new Mock<ISession>(MockBehavior.Strict);
            this.ConnectSession.Setup(pcs => pcs.Dispose());

            this.ConnectSession
                .SetupRead(sequence, async, "CONNECTED <sonic-server v1.00>")
                .SetupWrite(sequence, async, "START", mode.ToString().ToLowerInvariant(), StubConstants.Secret)
                .SetupRead(sequence, async, "STARTED control protocol(1) buffer(20002)")
                ;
        }

        public SemaphoreSlim Semaphore { get; }
        public Mock<ITcpClient> TcpClient { get; }
        public Mock<ISession> ConnectSession { get; }

        public ISession Create(IClient tcpClient)
        {
            return this.ConnectSession.Object;
        }
    }
}
