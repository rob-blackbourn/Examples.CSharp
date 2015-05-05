using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JetBlack.Examples.RxMongoDB
{
    class Program
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;

        static void Main(string[] args)
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("local");
            //Test1();
            //Test1Async().Wait();
            //Test3();
            Test4();

            Console.ReadLine();
        }

        static void Test1()
        {
            Test1Async().Wait();
        }

        static async Task Test1Async()
        {
            var collection = _database.GetCollection<BsonDocument>("oplog.rs");

            var filter = new BsonDocument();
            //var findOptions = new FindOptions<BsonDocument, BsonDocument> { CursorType = CursorType.TailableAwait };
            var findOptions = new FindOptions<BsonDocument, BsonDocument>();

            using (var cursor = await collection.FindAsync(filter, findOptions))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        Debug.Print("Document={0}", document);
                    }
                }
            }

            Debug.Print("Done");
        }

        static async Task Test2Async()
        {
            var collection = _database.GetCollection<BsonDocument>("oplog.rs");
            var filter = new BsonDocument();
            var findOptions = new FindOptions<BsonDocument, BsonDocument> { CursorType = CursorType.TailableAwait };
            //var findOptions = new FindOptions<BsonDocument, BsonDocument>();

            (await collection.FindAsync(filter, findOptions))
                .ToObservable()
                .Subscribe(x => Debug.Print("OnNext: {0}", x),
                error => Debug.Print("OnError: {0}", error),
                () => Debug.Print("OnCompleted"));

            Debug.Print("Done");
        }

        private static void Test3()
        {
            var collection = _database.GetCollection<BsonDocument>("oplog.rs");
            var filter = new BsonDocument();
            //var findOptions = new FindOptions<BsonDocument, BsonDocument> {CursorType = CursorType.TailableAwait};
            var findOptions = new FindOptions<BsonDocument, BsonDocument>();

            collection.FindAsync((x => true));
            collection.ToFindObservable(filter, findOptions)
                .Subscribe(
                    cursor =>
                        cursor.ToObservable()
                            .Subscribe(
                                document => Debug.Print("OnNext: {0}", document),
                                error => Debug.Print("OnError: {0}", error),
                                () => Debug.Print("OnCompleted")),
                    error => Debug.Print("ToFindObservable: OnError: {0}", error),
                    () => Debug.Print("ToFindObservable: OnCompleted"));
        }

        private static void Test4()
        {
            var collection = _database.GetCollection<OpLog>("oplog.rs");
            var filter = new BsonDocumentFilterDefinition<OpLog>(new BsonDocument());
            //var filter = new OpLog();
            //var findOptions = new FindOptions<BsonDocument, BsonDocument> {CursorType = CursorType.TailableAwait};
            var findOptions = new FindOptions<OpLog, OpLog>();

            collection.FindAsync((x => true));
            collection.ToFindObservable(filter, findOptions)
                .Subscribe(
                    cursor =>
                        cursor.ToObservable()
                            .Subscribe(
                                document => Debug.Print("OnNext: {0}", document),
                                error => Debug.Print("OnError: {0}", error),
                                () => Debug.Print("OnCompleted")),
                    error => Debug.Print("ToFindObservable: OnError: {0}", error),
                    () => Debug.Print("ToFindObservable: OnCompleted"));
        }
    }
}
