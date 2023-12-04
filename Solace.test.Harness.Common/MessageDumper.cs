using MongoDB.Driver;
using Solace.test.Harness.Common.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solace.test.Harness.Common
{
    public class MessageDumper
    {
        MongoClient _client;
        IMongoDatabase _database;
        IMongoCollection<PayloadMessage> _payload_messages;
        BlockingCollection<List<PayloadMessage>> payloads = new BlockingCollection<List<PayloadMessage>>();

        public MessageDumper(MessageBus messageBus, EnvVars envVars)
        {
            messageBus.Subscribe<PayloadMessage?>().Buffer(TimeSpan.FromSeconds(5)).Subscribe((x) => {
                var p = x.ToList();
                foreach (var message in p) { message.Payload = null; }
                if(p.Any())
                    payloads.Add(p);
            });
            //messageBus.Subscribe<PayloadMessage>().Subscribe(message => { Console.WriteLine($"dumper needs to dump {message}"); });

            var settings = MongoClientSettings.FromConnectionString(envVars.MongoConnectionString);
            _client = new MongoClient(settings);
            _database = _client.GetDatabase("solace_poc");
            _payload_messages = _database.GetCollection<PayloadMessage>("payload_messages");

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var p_loads = payloads.Take();
                    _payload_messages.InsertMany(p_loads);
                }
            });
        }
    }
}
