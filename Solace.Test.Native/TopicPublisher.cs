using Solace.test.Harness.Common.Models;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Solace.Test.Native
{
    public class TopicPublisher
    {
        int _delay;
        string _topicName;
        ISession _session;
        string _region;
        public TopicPublisher(int delay, ISession session, string topicName, string region)
        {
            _delay = delay;
            _topicName = topicName;
            _session = session;
            _region = region;
        }

        public async Task StartAsync()
        {
            var topic = ContextFactory.Instance.CreateTopic(_topicName);
            var contents = await File.ReadAllTextAsync("Sample.txt");
            
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    using (IMessage message = _session.CreateMessage())
                    {
                        message.Destination = topic;
                        var payloadMessage = new PayloadMessage()
                        {
                            MessageType = MessageType.Broadcast,
                            Payload = contents,
                            SenderRegion = _region,
                            SenderTime = DateTime.UtcNow,
                            SenderTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                        };

                        message.BinaryAttachment = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(payloadMessage));

                        
                        ReturnCode returnCode = _session.Send(message);
                        if (returnCode == ReturnCode.SOLCLIENT_OK)
                        {
                            Console.WriteLine($"Published message {payloadMessage.SenderTimestamp} - {payloadMessage.GetHashCode()} - {Thread.CurrentThread.ManagedThreadId}");
                        }
                        else
                        {
                            Console.WriteLine("Publishing failed, return code: {0}", returnCode);
                        }
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(0,_delay)));
                }
            });
        }
    }
}
