using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace YSGM
{
    public class MUIPManager
    {
        public static MUIPManager Instance = new();

        private MUIPManager() { }

        public string GM(string uid, string cmd)
        {
            return GET(1116, new Dictionary<string, string>()
            {
                { "uid", uid },
                { "msg", cmd }
            });
        }

        public string FetchPlayerBin(string uid)
        {
            return GET(1004, new Dictionary<string, string>()
            {
                { "uid", uid }
            });
        }

        public string GET(int cmd, Dictionary<string, string> param) // These both are numbers, but string for convenience
        {
#if DEBUG
            var builder = new UriBuilder("http://10.0.0.2:22111/api");
#else
            var builder = new UriBuilder(ConfigurationManager.AppSettings.Get("MUIP_HOST")!);
#endif
            var query = HttpUtility.ParseQueryString(builder.Query);
            
            foreach(KeyValuePair<string, string> entry in param) {
                query[entry.Key] = entry.Value;
            }

            
            query["cmd"] = cmd.ToString();
            query["region"] = ConfigurationManager.AppSettings.Get("MUIP_TARGET_REGION");
            query["ticket"] = $"YSGM";

#if DEBUG
            string muip_sign = "20e45af70e490adc44d01f174ea6c23ebb74d3ec";

#else
            string muip_sign = ConfigurationManager.AppSettings.Get("MUIP_SIGN");
#endif
            
            String query_str = query.ToString();
            var dict = new Dictionary<string, string>();

            query_str = query_str.Replace("+", " ");
            string [] a = query_str.Split("&");
            
            List<string> new_query_parm = new List<string>();
            foreach (var c in a) {
                var f = c.Split("=");
                dict.Add(f[0], f[1]);
            }
            foreach (var key in dict.Keys) { 
                if (dict[key]=="")
                {
                    continue;
                }
                new_query_parm.Add( key+"="+dict[key]);
                
            }

            string []new_query_parm_arr = new_query_parm.ToArray();

            Array.Sort(new_query_parm_arr);



            query_str = string.Join("&",new_query_parm_arr);
            string sign_str = $"{query_str}{muip_sign}";
            
            query["sign"] = SHA(sign_str);

 
            builder.Query = query.ToString();

            var client = new HttpClient();
#if DEBUG
            Console.WriteLine();
            Console.WriteLine("query_str");
            Console.WriteLine(query_str);
            Console.WriteLine();

            Console.WriteLine("sign_str");
            Console.WriteLine(sign_str);
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("signed_sign");
            Console.WriteLine(query["sign"]);
            Console.WriteLine();
#endif
            string url = builder.ToString();
            url = url.Replace("%2c", ",");
            url = url.Replace("%3d", "=");
            url = url.Replace("%3a", ":");
#if DEBUG


            Console.WriteLine(url);
#endif
            var webRequest = new HttpRequestMessage(HttpMethod.Get, url);

            var response = client.Send(webRequest);

            using var reader = new StreamReader(response.Content.ReadAsStream());

            return reader.ReadToEnd();
        }

        private string SHA(string str)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(str));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
