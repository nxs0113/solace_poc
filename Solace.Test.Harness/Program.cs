using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Solace.test.Harness.Common;
using Solace.Test.Native;
using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace Solace.Test.Harness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Om namah shivaya! Thread id : {Thread.CurrentThread.ManagedThreadId}\"");
            var cVars = GetCommandVars(args);
            var envar = EnvVars.LoadAppSettings(cVars["region"], cVars["user"], cVars["pwd"]);
            var services = new ServiceCollection();

            // add necessary services
            services.AddSingleton<EnvVars>(envar);
            services.AddSingleton<MessageBus>();
            services.AddSingleton<SessionManager>();
            services.AddTransient<MessageHandler>();
            services.AddSingleton<MessageDumper>();
            // build the pipeline
            var provider = services.BuildServiceProvider();
            var env = provider.GetService<EnvVars>();

            var dumper = provider.GetService<MessageDumper>();
            var contexts = new List<SessionManager>();
            if (cVars["tproducer"] == "yes")
            {
                for (int i = 0; i < 1; i++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var sessionManager1 = provider.GetService<SessionManager>();
                        contexts.Add(sessionManager1);
                        var session1 = sessionManager1.CreateSession(env.SolaceConnectionString, env.Vpn, env.UserName, env.Password);
                        TopicPublisher rxTopicPublisher1 = new TopicPublisher(envar.TopicPublishRate, session1, $"poc/topic/{envar.Region}", envar.Region);
                        rxTopicPublisher1.StartAsync();
                    });
                }
            }

            if (cVars["qproducer"] == "yes")
            {
                for (int i = 0; i < 1; i++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var sessionManager1 = provider.GetService<SessionManager>();
                        contexts.Add(sessionManager1);
                        var session1 = sessionManager1.CreateSession(env.SolaceConnectionString, env.Vpn, env.UserName, env.Password);
                        QueueProducer qProducer = new QueueProducer(session1, "poc/queue", env.Region, env.QueuePublishRate);
                        qProducer.Start();
                    });
                    Thread.Sleep(1000);
                }
            }

            if (cVars["tconsumer"] == "yes")
            {
                for (int i = 0; i < 1; i++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var sessionManager1 = provider.GetService<SessionManager>();
                        contexts.Add(sessionManager1);
                        var session1 = sessionManager1.CreateSession(env.SolaceConnectionString, env.Vpn, env.UserName, env.Password);
                        session1.Subscribe(ContextFactory.Instance.CreateTopic($"poc/topic/*"), true);
                    });
                }
            }

            if (cVars["qconsumer"] == "yes")
            {
                for (int i = 0; i < 1; i++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var sessionManager1 = provider.GetService<SessionManager>();
                        contexts.Add(sessionManager1);
                        var session1 = sessionManager1.CreateSession(env.SolaceConnectionString, env.Vpn, env.UserName, env.Password);
                        QueueConsumer qConsumer = new QueueConsumer(session1, "poc/queue", env.Region, provider.GetService<MessageBus>());
                    });
                    Thread.Sleep(1000);
                }
            }
            Console.ReadKey();
            Console.ReadKey();
        }


        static Dictionary<string, string> GetCommandVars(string[] args)
        {
            Dictionary<string, string> cVars = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                var sArgs = arg.Split('=');
                cVars[sArgs[0]] = sArgs[1];
            }
            return cVars;
        }
    }


}
