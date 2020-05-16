﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NSonic.Impl;
using NSonic.Tests.Stubs;

namespace NSonic.Tests.Connections
{
    public abstract class TestBase
    {
        internal ISonicRequestWriter RequestWriter { get; private set; }
        internal MockSequence Sequence { get; private set; }
        internal StubSessionFactory SessionFactory { get; private set; }
        internal IDisposableSonicClient Client { get; private set; }

        internal Mock<ISonicSession> Session => this.SessionFactory.PostConnectSession;

        internal abstract ConnectionMode Mode { get; }
        protected abstract bool Async { get; }

        [TestInitialize]
        public virtual void Initialize()
        {
            this.RequestWriter = new SonicRequestWriter();
            this.Sequence = new MockSequence();
            this.SessionFactory = new StubSessionFactory(this.Sequence, this.Mode, this.Async);

            var connector = new SonicClientConnector(this.SessionFactory, this.RequestWriter);
            this.Client = new SonicClient(connector, this.SessionFactory.TcpClient.Object);
        }

        protected void SetupWriteWithOk(params string[] args)
        {
            this.Session.SetupWriteWithOk(this.Sequence, this.Async, args);
        }

        protected void SetupWriteWithResult(string result, params string[] args)
        {
            this.Session.SetupWriteWithResult(this.Sequence, this.Async, result, args);
        }

        protected void VerifyAll()
        {
            this.Session.VerifyAll();
        }
    }
}
