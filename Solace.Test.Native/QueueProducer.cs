using Solace.test.Harness.Common.Models;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Solace.Test.Native
{
    public class QueueProducer
    {
        ISession _session = null;
        string _qName = null;
        string _region = null;
        private int _delay;

        public QueueProducer(ISession session, string qName, string region, int delay)
        {
            _session = session;
            _qName = qName;
            _region = region;
            _delay = delay;
        }

        public async void Start()
        {
            var contents = await File.ReadAllTextAsync("Sample.txt");
            IQueue queue = ContextFactory.Instance.CreateQueue(_qName);
            // Set queue permissions to "consume" and access-type to "exclusive"
            EndpointProperties endpointProps = new EndpointProperties()
            {
                Permission = EndpointProperties.EndpointPermission.Consume,
                AccessType = EndpointProperties.EndpointAccessType.NonExclusive
            };
            // Provision it, and do not fail if it already exists
            _session.Provision(queue, endpointProps,
                ProvisionFlag.IgnoreErrorIfEndpointAlreadyExists | ProvisionFlag.WaitForConfirm, null);
            Console.WriteLine("Queue '{0}' has been created and provisioned.", _qName);


            _ = Task.Factory.StartNew(() =>
            {
                int count = 0;
                while (true)
                {
                    using (IMessage message = ContextFactory.Instance.CreateMessage())
                    {
                        // Message's destination is the queue and the message is persistent
                        message.Destination = queue;
                        message.DeliveryMode = MessageDeliveryMode.Persistent;
                        // Create the message content as a binary attachment
                        var payloadMessage = new PayloadMessage()
                        {
                            MessageType = MessageType.P2P,
                            Payload = contents,
                            SenderRegion = _region,
                            SenderTime = DateTime.UtcNow,
                            SenderTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                        };

                        message.BinaryAttachment = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(payloadMessage));

                        // Send the message to the queue on the Solace messaging router
                        Console.WriteLine($"Sending message {count} to queue {0}...", _qName);
                        ReturnCode returnCode = _session.Send(message);
                        if (returnCode == ReturnCode.SOLCLIENT_OK)
                        {
                            // Delivery not yet confirmed. See ConfirmedPublish.cs
                            Console.WriteLine("Published to Queue.");
                            count++;
                        }
                        else
                        {
                            Console.WriteLine("Sending failed, return code: {0}", returnCode);
                        }
                    }
                    Thread.Sleep(new Random().Next(0, _delay));
                }
            });

        }
    }
}

