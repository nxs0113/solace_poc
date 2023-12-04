using Solace.test.Harness.Common;
using Solace.test.Harness.Common.Models;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Solace.Test.Native
{
    public class MessageHandler
    {
        MessageBus _messageBus = null;
        string _region;
        public MessageHandler(MessageBus messageBus, EnvVars envar)
        {
            _messageBus = messageBus;
            _region = envar.Region;
        } 
        public void HandleMessage(object source, MessageEventArgs args)
        {
            // Received a message
            using (IMessage message = args.Message)
            {
                var payload = JsonSerializer.Deserialize<PayloadMessage>(message.BinaryAttachment);
                payload.RecieverTime = DateTime.UtcNow;
                payload.RecieverTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                payload.RecieverRegion = _region;
                Console.WriteLine($"Recieved message {payload.MessageType} and {payload.SenderTimestamp} with a latency of {payload.RecieverTimestamp - payload.SenderTimestamp}. Thread id : {Thread.CurrentThread.ManagedThreadId}");
                _messageBus.Publish(payload);
            }
        }
    }
}
