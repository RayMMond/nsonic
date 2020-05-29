using NSonic.Impl.Net;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NSonic.Impl.Connections
{
    sealed class IngestConnection : Connection, ISonicIngestConnection
    {
        public IngestConnection(ISessionFactory sessionFactory
            , IRequestWriter requestWriter
            , IDisposableClient client
            , Configuration configuration
            )
            : base(sessionFactory, requestWriter, client, configuration)
        {
            //
        }

        protected override ConnectionMode Mode => ConnectionMode.Ingest;

        public int Count(string collection, string bucket = null, string @object = null)
        {
            var tsc = new TaskCompletionSource<int>();

            commandQueue.Post(() =>
            {
                try
                {
                    var response = this.RequestWriter.WriteResult(session, "COUNT", collection, bucket, @object);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
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

        public async Task<int> CountAsync(string collection, string bucket = null, string @object = null)
        {
            var tsc = new TaskCompletionSource<int>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    var response = await this.RequestWriter.WriteResultAsync(session, "COUNT", collection, bucket, @object);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            return await tsc.Task;
        }

        public int FlushBucket(string collection, string bucket)
        {
            var tsc = new TaskCompletionSource<int>();

            commandQueue.Post(() =>
            {
                try
                {
                    var response = this.RequestWriter.WriteResult(session, "FLUSHB", collection, bucket);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
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

        public async Task<int> FlushBucketAsync(string collection, string bucket)
        {
            var tsc = new TaskCompletionSource<int>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    var response = await this.RequestWriter.WriteResultAsync(session, "FLUSHB", collection, bucket);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            return await tsc.Task;
        }

        public int FlushCollection(string collection)
        {
            var tsc = new TaskCompletionSource<int>();

            commandQueue.Post(() =>
            {
                try
                {
                    var response = this.RequestWriter.WriteResult(session, "FLUSHC", collection);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
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

        public async Task<int> FlushCollectionAsync(string collection)
        {
            var tsc = new TaskCompletionSource<int>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    var response = await this.RequestWriter.WriteResultAsync(session, "FLUSHC", collection);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            return await tsc.Task;
        }

        public int FlushObject(string collection, string bucket, string @object)
        {
            var tsc = new TaskCompletionSource<int>();

            commandQueue.Post(() =>
            {
                try
                {
                    var response = this.RequestWriter.WriteResult(session, "FLUSHO", collection, bucket, @object);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
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

        public async Task<int> FlushObjectAsync(string collection, string bucket, string @object)
        {
            var tsc = new TaskCompletionSource<int>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    var response = await this.RequestWriter.WriteResultAsync(session, "FLUSHO", collection, bucket, @object);

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            return await tsc.Task;
        }

        public int Pop(string collection, string bucket, string @object, string text)
        {
            var tsc = new TaskCompletionSource<int>();

            commandQueue.Post(() =>
            {
                try
                {
                    var response = this.RequestWriter.WriteResult(session, "POP", collection, bucket, @object, $"\"{text}\"");

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
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

        public async Task<int> PopAsync(string collection, string bucket, string @object, string text)
        {
            var tsc = new TaskCompletionSource<int>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    var response = await this.RequestWriter.WriteResultAsync(session, "POP", collection, bucket, @object, $"\"{text}\"");

                    var result = Convert.ToInt32(response);

                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            return await tsc.Task;
        }

        public void Push(string collection, string bucket, string @object, string text, string locale = null)
        {
            var tsc = new TaskCompletionSource<object>();

            commandQueue.Post(() =>
            {
                try
                {
                    var request = new PushRequest(text, locale);

                    this.RequestWriter.WriteOk(session
                        , "PUSH"
                        , collection
                        , bucket
                        , @object
                        , request.Text
                        , request.Locale
                    );

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

        public async Task PushAsync(string collection, string bucket, string @object, string text, string locale = null)
        {
            var tsc = new TaskCompletionSource<object>();

            await commandQueue.SendAsync(async () =>
            {
                try
                {
                    var request = new PushRequest(text, locale);

                    await this.RequestWriter.WriteOkAsync(session
                        , "PUSH"
                        , collection
                        , bucket
                        , @object
                        , request.Text
                        , request.Locale
                    );

                    tsc.SetResult(null);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                }
            });

            await tsc.Task;
        }

        class PushRequest
        {
            private readonly string text;
            private readonly string locale;

            public PushRequest(string text, string locale)
            {
                this.text = text;
                this.locale = locale;
            }

            public string Text => $"\"{this.text}\"";
            public string Locale => !string.IsNullOrEmpty(this.locale) ? $"LANG({this.locale})" : "";
        }
    }
}
