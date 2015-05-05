using System;
using System.Reactive.Linq;
using MongoDB.Driver;

namespace JetBlack.Examples.RxMongoDB
{
    public static class MongoExtensions
    {
        public static IObservable<IAsyncCursor<TProjection>> ToFindObservable<TDocument, TProjection>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null)
        {
            return Observable.Create<IAsyncCursor<TProjection>>(async (observer, token) =>
            {
                try
                {
                    observer.OnNext(await collection.FindAsync(filter, options, token));
                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static IObservable<T> ToObservable<T>(this IAsyncCursor<T> cursor)
        {
            return Observable.Create<T>(async (observer, token) =>
            {
                try
                {
                    while (await cursor.MoveNextAsync(token))
                    {
                        foreach (var document in cursor.Current)
                            observer.OnNext(document);
                    }

                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }
    }
}
