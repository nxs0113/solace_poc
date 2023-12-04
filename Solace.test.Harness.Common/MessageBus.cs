using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Solace.test.Harness.Common
{
    public class MessageBus
    {
        readonly Subject<object> _bus = new Subject<object>();
        public void Publish(object message)=> _bus.OnNext(message);
        public IObservable<T> Subscribe<T>() => _bus.OfType<T>();
    }
}
