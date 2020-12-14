using OBS_Tools.Interfaces;
using RiotSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using LCUSharp;
using System.Net.Http;
using Newtonsoft.Json;
using RiotSharp.Endpoints.SummonerEndpoint;

namespace OBS_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        public RiotApi Api;
        public LeagueClientApi LCUApi;

        public Overlay()
        {
            InitializeComponent();

            Api = RiotApi.GetDevelopmentInstance("RGAPI-e8cbd757-7167-44cd-87cd-7fc989cfa5f5", 20, 100);

            var summoner = Api.Summoner.GetSummonerBySummonerIdAsync(RiotSharp.Misc.Region.Euw, "142943159").Result;

            ConnecttoClient();
        }

        public void UpdatePlayers()
        {
            var summoner = Api.Summoner.GetSummonerByNameAsync(RiotSharp.Misc.Region.Euw, "DVN Djlandir").Result;
            var summonerId = summoner.Id;

            var n = Api.Spectator.GetCurrentGameAsync(RiotSharp.Misc.Region.Euw, summonerId).Result;
            var p = n.Participants;

            
        }

        private void RefreshSummonners_Click(object sender, RoutedEventArgs e)
        {
            UpdatePlayers();
        }

        public async void ConnecttoClient()
        {
            LCUApi = await LeagueClientApi.ConnectAsync();

            //var body = new { profileIconId = 23 };
            var queryParameters = Enumerable.Empty<string>();
            var json = await LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "lol-champ-select/v1/session",
                                                                     queryParameters);

            var jsonResults = JsonConvert.DeserializeObject<CSPlayers>(json);

            if (jsonResults != null)
            {
                List<Summoner> p = new List<Summoner>();

                jsonResults.myTeam.ForEach(mt =>
                {
                    string summnonerId = mt.summonerId.ToString();
                    p.Add(Api.Summoner.GetSummonerBySummonerIdAsync(RiotSharp.Misc.Region.Euw, summnonerId).Result);
                });

                jsonResults.theirTeam.ForEach(tt =>
                {
                    string summnonerId = tt.summonerId.ToString();
                    p.Add(Api.Summoner.GetSummonerBySummonerIdAsync(RiotSharp.Misc.Region.Euw, summnonerId).Result);
                });

                SummonerName1.Text = p[0].Name;
                SummonerName2.Text = p[1].Name;
                SummonerName3.Text = p[2].Name;
                SummonerName4.Text = p[3].Name;
                SummonerName5.Text = p[4].Name;
                SummonerName6.Text = p[5].Name;
                SummonerName7.Text = p[6].Name;
                SummonerName8.Text = p[7].Name;
                SummonerName9.Text = p[8].Name;
                SummonerName10.Text = p[9].Name;
            }
        }
    }
}
