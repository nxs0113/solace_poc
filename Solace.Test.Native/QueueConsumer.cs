using Solace.test.Harness.Common.Models;
using Solace.test.Harness.Common;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Solace.Test.Native
{
    public class QueueConsumer
    {
        IFlow _flow;
        string _region;
        MessageBus _messageBus;
        public QueueConsumer(ISession session, string qName, string region, MessageBus messageBus)
        {
            _region = region;
            _messageBus = messageBus;
            IQueue queue = ContextFactory.Instance.CreateQueue(qName);
            // Set queue permissions to "consume" and access-type to "exclusive"
            EndpointProperties endpointProps = new EndpointProperties()
            {
                Permission = EndpointProperties.EndpointPermission.Consume,
                AccessType = EndpointProperties.EndpointAccessType.NonExclusive
            };
            // Provision it, and do not fail if it already exists
            session.Provision(queue, endpointProps,
                ProvisionFlag.IgnoreErrorIfEndpointAlreadyExists | ProvisionFlag.WaitForConfirm, null);
            Console.WriteLine("Consumer : Queue '{0}' has been created and provisioned for consumption.", qName);

            _flow = session.CreateFlow(new FlowProperties()
            {
                AckMode = MessageAckMode.ClientAck
            },
                queue, null, HandleMessageEvent, HandleFlowEvent);
            _flow.Start();
        }

        private void HandleFlowEvent(object? sender, FlowEventArgs e)
        {
            Console.WriteLine("Received Flow Event '{0}' Type: '{1}' Text: '{2}'",
                e.Event,
                e.ResponseCode.ToString(),
                e.Info);
        }

        private void HandleMessageEvent(object? sender, MessageEventArgs e)
        {
            using (IMessage message = e.Message)
            {
                var payload = JsonSerializer.Deserialize<PayloadMessage>(message.BinaryAttachment);
                payload.RecieverTime = DateTime.UtcNow;
                payload.RecieverTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                payload.RecieverRegion = _region;
                Console.WriteLine($"{this.GetHashCode()} Recieved Queued message {payload.MessageType} and {payload.SenderTimestamp} with a latency of {payload.RecieverTimestamp - payload.SenderTimestamp}. Thread id : {Thread.CurrentThread.ManagedThreadId}");
                // ACK the message
                _flow.Ack(message.ADMessageId);
                // finish the program
                _messageBus.Publish(payload);
            }
        }
    }
}
