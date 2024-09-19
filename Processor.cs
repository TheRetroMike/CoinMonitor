using Discord;
using Discord.Webhook;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinReleaseMonitor
{
    public class Processor
    {
        static string discordWebhookUrl = "";
        public static void Process(string _discordWebhook)
        {
            discordWebhookUrl = _discordWebhook;
            MiningPoolStats();
            RPlant();
            YiimpStyle("https://www.zpool.ca/api/currencies", "ZergPool");
            YiimpStyle("https://www.zpool.ca/api/currencies", "ZPool");
        }

        static void MiningPoolStats()
        {
            string source = "MiningPoolStats";
            string url = "https://miningpoolstats.stream/data/coin_list.1726257384.min.js";

            Console.WriteLine(String.Format("[{0}] Running {1}...", DateTime.Now, source));
            var datFileName = String.Format("{0}.dat", source.ToLower());
            List<CoinData> sourceCoins = GetExistingCoins(datFileName);
            List<CoinData> newCoins = new List<CoinData>();
            List<CoinData> allCoins = new List<CoinData>();
            var client = new RestClient(url);
            var request = new RestRequest("");
            var response = client.Get(request);
            var trimmedResponseContent = response.Content.Replace("var coin_list = ", String.Empty);
            trimmedResponseContent = trimmedResponseContent.Remove(trimmedResponseContent.Length - 1, 1);
            dynamic responseData = JsonConvert.DeserializeObject(trimmedResponseContent ?? "");
            if (responseData != null)
            {
                foreach (var item in responseData)
                {
                    var grouping = Convert.ToString(item.Name);
                    if (grouping != "time")
                    {
                        foreach (var subItem in item)
                        {
                            foreach (var nestedSubItem in subItem)
                            {
                                var coinTicker = Convert.ToString(nestedSubItem.Value["s"]);
                                var coinName = Convert.ToString(nestedSubItem.Value["n"]);
                                var coinAlgo = grouping;
                                allCoins.Add(new CoinData() { Name = coinName, Algo = coinAlgo });

                                if (sourceCoins.Where(x => x.Name == coinName).FirstOrDefault() == null)
                                {
                                    newCoins.Add(new CoinData() { Name = coinName, Algo = coinAlgo, Ticker = coinTicker });
                                }
                            }

                        }
                    }
                }
            }

            foreach (var item in newCoins)
            {
                DiscordNotify(item.Name, item.Algo, item.Ticker, source);
                Console.WriteLine(item.Name);
            }

            using (var writer = new StreamWriter(datFileName))
            {
                writer.Write(JsonConvert.SerializeObject(allCoins));
            }
        }

        static void YiimpStyle(string url, string source)
        {
            Console.WriteLine(String.Format("[{0}] Running {1}...", DateTime.Now, source));
            var datFileName = String.Format("{0}.dat", source.ToLower());
            List<CoinData> sourceCoins = GetExistingCoins(datFileName);
            List<CoinData> newCoins = new List<CoinData>();
            List<CoinData> allCoins = new List<CoinData>();
            var client = new RestClient(url);
            var request = new RestRequest("");
            var response = client.Get(request);
            dynamic responseData = JsonConvert.DeserializeObject(response.Content ?? "");
            if (responseData != null)
            {
                foreach (var item in responseData)
                {
                    var coinTicker = Convert.ToString(item.Name);
                    var coinName = Convert.ToString(item.Value["name"]);
                    var coinAlgo = Convert.ToString(item.Value["algo"]);
                    allCoins.Add(new CoinData() { Name = coinName, Algo = coinAlgo });

                    if (sourceCoins.Where(x => x.Name == coinName).FirstOrDefault() == null)
                    {
                        newCoins.Add(new CoinData() { Name = coinName, Algo = coinAlgo, Ticker = coinTicker });
                    }
                }
            }

            foreach (var item in newCoins)
            {
                DiscordNotify(item.Name, item.Algo, item.Ticker, source);
                Console.WriteLine(item.Name);
            }

            using (var writer = new StreamWriter(datFileName))
            {
                writer.Write(JsonConvert.SerializeObject(allCoins));
            }
        }

        static void RPlant()
        {
            Console.WriteLine(String.Format("[{0}] Running RPlant...", DateTime.Now));
            List<CoinData> rplantCoins = GetExistingCoins("rplant.dat");
            List<CoinData> newCoins = new List<CoinData>();
            List<CoinData> allCoins = new List<CoinData>();
            string requestUrl = "https://pool.rplant.xyz/api/currencies";
            var client = new RestClient(requestUrl);
            var request = new RestRequest("");
            var response = client.Get(request);
            dynamic responseData = JsonConvert.DeserializeObject(response.Content ?? "");
            if (responseData != null)
            {
                foreach (var item in responseData)
                {
                    var coinTicker = Convert.ToString(item.Name);
                    var coinName = Convert.ToString(item.Value["name"]);
                    var coinAlgo = Convert.ToString(item.Value["algo"]);
                    allCoins.Add(new CoinData() { Name=coinName, Algo=coinAlgo });

                    if (rplantCoins.Where(x => x.Name==coinName).FirstOrDefault() == null)
                    {
                        newCoins.Add(new CoinData() {  Name=coinName, Algo=coinAlgo, Ticker=coinTicker });
                    }
                }
            }

            foreach (var item in newCoins)
            {
                DiscordNotify(item.Name, item.Algo, item.Ticker, "RPlant");
                Console.WriteLine(item.Name);
            }

            using (var writer = new StreamWriter("rplant.dat"))
            {
                writer.Write(JsonConvert.SerializeObject(allCoins));
            }
        }

        static List<CoinData> GetExistingCoins(string source)
        {
            List<CoinData> result = new List<CoinData>();
            if (File.Exists(source))
            {
                var fileData = File.ReadAllText(source);
                result = JsonConvert.DeserializeObject<List<CoinData>>(fileData) ?? new List<CoinData>();
            }
            return result;
        }

        static async void DiscordNotify(string coinName, string algo, string ticker, string source)
        {
            try
            {
                using (var client = new DiscordWebhookClient(discordWebhookUrl))
                {
                    var embed = new EmbedBuilder
                    {
                        Title = String.Format("New Mineable Coin Alert: {0}", coinName),
                        Description = String.Format("Ticker: {3}\r\nCoin: {0}\r\nAlgo: {1}\r\nSource: {2}", coinName, algo, source, ticker)
                    };

                    await client.SendMessageAsync(text: "", embeds: new[] { embed.Build() });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error trying to notify discord: " + ex.Message);
            }
        }
    }
}
