using System.Text.Json;

namespace Solace.test.Harness.Common
{
    public class EnvVars
    {
        private string _region;
        private int _topicPublishRate;
        private int _requestReplyPublishRate;
        private int _queuePublishRate;
        private string _userName;
        private string _password;
        private string _mongoConnectionString;
        private string _solaceConnectionString;
        private string _vpn;


        public static EnvVars LoadAppSettings(string region, string userName, string password)
        {
            EnvVars envVars = new EnvVars();
            envVars._region = region;
            envVars._userName = userName;
            envVars._password = password;
            string text = File.ReadAllText("appsettings.json");
            var vars = JsonSerializer.Deserialize<Dictionary<string, Object>>(text);
            envVars._queuePublishRate = int.Parse(vars["queuePublishRate"].ToString());
            envVars._topicPublishRate = int.Parse(vars["topicPublishRate"].ToString());
            envVars._requestReplyPublishRate = int.Parse(vars["requestReplyPublishRate"].ToString());
            envVars._mongoConnectionString = vars["mongoConnectionString"].ToString();
            envVars._solaceConnectionString = vars["solaceConnectionString"].ToString();
            envVars._vpn = vars["vpn"].ToString();
            return envVars; 
        }

        public string Region => _region;

        public int TopicPublishRate => _topicPublishRate;

        public int RequestReplyPublishRate => _requestReplyPublishRate;

        public int QueuePublishRate => _queuePublishRate;

        public string UserName => _userName;

        public string Password => _password;

        public string SolaceConnectionString => _solaceConnectionString;
        public string MongoConnectionString => _mongoConnectionString;
        public string Vpn => _vpn;
    }
}
