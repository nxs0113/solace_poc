using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solace.test.Harness.Common.Models
{
    public class BaseMessage
    {
        public string SenderRegion {  get; set; }
        public long SenderTimestamp { get; set; }
        public string RecieverRegion { get; set; }
        public long RecieverTimestamp { get; set; }
        public MessageType MessageType { get; set; }
        public DateTime? SenderTime { get; set; }
        public DateTime? RecieverTime { get; set; }
    }
}
