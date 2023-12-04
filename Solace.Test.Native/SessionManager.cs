using Solace.test.Harness.Common.Models;
using Solace.test.Harness.Common;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Text;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;

namespace Solace.Test.Native
{
    public class SessionManager : IDisposable
    {
        IContext _context = null;
        ISession _session = null;
        MessageHandler _messageHandler;
        public SessionManager(MessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
            ContextFactoryProperties cfp = new ContextFactoryProperties()
            {
                SolClientLogLevel = SolLogLevel.Warning
            };
            cfp.LogToConsoleError();
            ContextFactory.Instance.Init(cfp);

            _context = ContextFactory.Instance.CreateContext(new ContextProperties(), (source,h) => { });
        }

        public ISession CreateSession(string host, string vpnName, string userName, string password)
        {
            SessionProperties sessionProps = new SessionProperties()
            {
                Host = host,
                VPNName = vpnName,
                UserName = userName,
                Password = password,
                ReconnectRetries = 5,
                GdWithWebTransport = true
            };
            //_session = _context.CreateSession(sessionProps, _messageHandler.HandleMessage, null);
            _session = _context.CreateSession(sessionProps, _messageHandler.HandleMessage, null);
            
            ReturnCode returnCode = _session.Connect();
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                return _session;
            }
            return null;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
