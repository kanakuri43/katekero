using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace sale.Models
{
    public static class UpdateKintoneOrder
    {
        private static dynamic _config = LoadConfig();

        static dynamic LoadConfig()
        {
            var doc = XDocument.Load("config.xml");

            return new
            {
                //ConnectionString = doc.Root.Element("Database").Element("ConnectionString").Value.Trim(),
                BaseUrl = doc.Root.Element("Kintone").Element("BaseUrl").Value.Trim(),
                AppId = doc.Root.Element("Kintone").Element("AppId").Value.Trim(),
                ApiToken = doc.Root.Element("Kintone").Element("ApiToken").Value.Trim(),
                BasicPassword = doc.Root.Element("Kintone").Element("BasicPassword").Value.Trim(),
                Document = doc,
            };
        }

        static string UrlEncode(Dictionary<string, string> parameters)
        {
            var encodedParams = new List<string>();
            foreach (var param in parameters)
            {
                encodedParams.Add($"{param.Key}={Uri.EscapeDataString(param.Value)}");
            }
            return string.Join("&", encodedParams);
        }

        public static async void Update(int OrderNo)
        {
            // サンプルデータ
            var fetchedData = new List<(string 年月, string 担当者, double 売上実績, double 粗利実績)>
            {
                ("202410", "s-motoki@netoffice.obisan.co.jp",  10, 10),
                //("202410", "m-sugawara@netoffice.obisan.co.jp", 20, 20)
            };

            // 
            //foreach (var d in fetchedData)
            //{
                string query = $"order_no = \"{OrderNo}\" ";

                var getParams = new Dictionary<string, string>
                {
                    { "app", _config.AppId },
                    { "query", query }
                };

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-Cybozu-API-Token", _config.ApiToken);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + _config.BasicPassword);

                    var response = await client.GetAsync(_config.BaseUrl + "records.json?" + UrlEncode(getParams));
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Fetch Kintone Error");
                        return;
                    }

                    var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
                    var recordsArray = (Newtonsoft.Json.Linq.JArray)responseData["records"];
                    var records = recordsArray.ToObject<List<object>>();

                    if (records.Count > 0)
                    {
                        foreach (var record in records)
                        {
                            var recordDict = ((Newtonsoft.Json.Linq.JObject)record).ToObject<Dictionary<string, object>>();
                            //string recordId = ((Newtonsoft.Json.Linq.JObject)recordDict["$id"])["value"].ToString();
                            string recordId = OrderNo.ToString();

                            var postParams = new Dictionary<string, object>
                            {
                                { "app", _config.AppId },
                                { "id", recordId },
                                { "record", new Dictionary<string, object>
                                    {
                                        { "sold", new Dictionary<string, object> { { "value", 1 } } }   // 売上済みを1に更新
                                    }
                                }
                            };

                            var postContent = new StringContent(JsonConvert.SerializeObject(postParams), Encoding.UTF8, "application/json");
                            var postResponse = await client.PutAsync(_config.BaseUrl + "record.json", postContent);

                            if (!postResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Register Kintone Error" + postResponse.StatusCode);
                            }

                        }
                    }
                //}
            }
        }
    }
}
