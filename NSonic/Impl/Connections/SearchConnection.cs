﻿using NSonic.Utils;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NSonic.Impl.Connections
{
    sealed class SearchConnection : Connection, ISonicSearchConnection
    {
        public SearchConnection(ISessionFactory sessionFactory
            , IRequestWriter requestWriter
            , IDisposableClient client
            , Configuration configuration
            )
            : base(sessionFactory, requestWriter, client, configuration)
        {
        }

        protected override ConnectionMode Mode => ConnectionMode.Search;

        public string[] Query(string collection
            , string bucket
            , string terms
            , int? limit = null
            , int? offset = null
            , string locale = null
            )
        {
            var tsc = new TaskCompletionSource<string[]>();

            commandQueue.Post(() =>
            {
                if (source.IsCancellationRequested)
                {
                    tsc.SetCanceled();
                }

                try
                {
                    var request = new QueryRequest(terms, limit, offset, locale);
                    session.Write("QUERY"
                        , collection
                        , bucket
                        , request.Terms
                        , request.Limit
                        , request.Offset
                        , request.Locale
                    );

                    var response = session.Read();
                    Assert.IsTrue(response.StartsWith("PENDING"), "Expected pending marker", response);

                    var marker = response.Substring("PENDING ".Length);

                    var result = this.ParseQueryResponse(marker, session.Read());

                    tsc.SetResult(result);

                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                    throw;
                }
            });

            return tsc.Task.Result;
        }

        public async Task<string[]> QueryAsync(string collection, string bucket, string terms, int? limit = null, int? offset = null, string locale = null)
        {
            var tsc = new TaskCompletionSource<string[]>();

            await commandQueue.SendAsync(async () =>
            {
                if (source.IsCancellationRequested)
                {
                    tsc.SetCanceled();
                }

                try
                {
                    var request = new QueryRequest(terms, limit, offset, locale);

                    await session.WriteAsync("QUERY"
                        , collection
                        , bucket
                        , request.Terms
                        , request.Limit
                        , request.Offset
                        , request.Locale
                    );

                    var response = await session.ReadAsync();
                    Assert.IsTrue(response.StartsWith("PENDING"), "Expected pending marker", response);

                    var marker = response.Substring("PENDING ".Length);

                    var result = this.ParseQueryResponse(marker, await session.ReadAsync());

                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                    throw;
                }
            });

            return await tsc.Task;
        }

        public string[] Suggest(string collection, string bucket, string word, int? limit = null)
        {

            var tsc = new TaskCompletionSource<string[]>();

            commandQueue.Post(() =>
            {
                if (source.IsCancellationRequested)
                {
                    tsc.SetCanceled();
                }

                try
                {
                    var request = new SuggestRequest(word, limit);

                    session.Write("SUGGEST"
                        , collection
                        , bucket
                        , request.Word
                        , request.Limit
                    );

                    var response = session.Read();
                    Assert.IsTrue(response.StartsWith("PENDING"), "Expected pending marker", response);

                    var marker = response.Substring("PENDING ".Length);

                    var result = this.ParseSuggestResponse(marker, session.Read());

                    tsc.SetResult(result);

                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                    throw;
                }
            });

            return tsc.Task.Result;
        }

        public async Task<string[]> SuggestAsync(string collection, string bucket, string word, int? limit = null)
        {
            var tsc = new TaskCompletionSource<string[]>();

            await commandQueue.SendAsync(async () =>
            {
                if (source.IsCancellationRequested)
                {
                    tsc.SetCanceled();
                }

                try
                {
                    var request = new SuggestRequest(word, limit);

                    await session.WriteAsync("SUGGEST"
                        , collection
                        , bucket
                        , request.Word
                        , request.Limit
                    );

                    var response = await session.ReadAsync();
                    Assert.IsTrue(response.StartsWith("PENDING"), "Expected pending marker", response);

                    var marker = response.Substring("PENDING ".Length);

                    var result = this.ParseSuggestResponse(marker, await session.ReadAsync());

                    tsc.SetResult(result);
                }
                catch (Exception e)
                {
                    tsc.SetException(e);
                    throw;
                }
            });

            return await tsc.Task;
        }

        private string[] ParseQueryResponse(string marker, string response)
        {
            Assert.IsTrue(response.StartsWith($"EVENT QUERY {marker}"), "Expected query result", response);

            return response
                .Substring($"EVENT QUERY {marker} ".Length)
                .Split(' ');
        }

        private string[] ParseSuggestResponse(string marker, string response)
        {
            Assert.IsTrue(response.StartsWith($"EVENT SUGGEST {marker}"), "Expected suggest result", response);

            return response
                .Substring($"EVENT SUGGEST {marker} ".Length)
                .Split(' ');
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source.Cancel();
            }

            base.Dispose(disposing);
        }

        class QueryRequest
        {
            private readonly string terms;
            private readonly int? limit;
            private readonly int? offset;
            private readonly string locale;

            public QueryRequest(string terms, int? limit, int? offset, string locale)
            {
                this.terms = terms;
                this.limit = limit;
                this.offset = offset;
                this.locale = locale;
            }

            public string Terms => $"\"{terms}\"";
            public string Limit => limit.HasValue ? $"LIMIT({limit})" : "";
            public string Offset => offset.HasValue ? $"OFFSET({offset})" : "";
            public string Locale => !string.IsNullOrEmpty(locale) ? $"LANG({locale})" : "";
        }

        class SuggestRequest
        {
            private readonly string word;
            private readonly int? limit;

            public SuggestRequest(string word, int? limit)
            {
                this.word = word;
                this.limit = limit;
            }

            public string Word => $"\"{word}\"";
            public string Limit => limit.HasValue ? $"LIMIT({limit})" : "";
        }
    }
}
