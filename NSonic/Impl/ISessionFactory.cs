namespace NSonic.Impl
{
    interface ISessionFactory
    {
        ISession Create(IClient client);
    }
}
