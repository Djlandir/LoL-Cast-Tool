using OBS_Tools.Interfaces;
using RiotSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LCUSharp;
using System.Net.Http;
using RiotSharp.Endpoints.SummonerEndpoint;
using LCUSharp.Websocket;
using System.Windows.Threading;
using System.Net;
using Newtonsoft.Json;
using RiotSharp.Endpoints.StaticDataEndpoint.Champion;
using System.Windows.Media.Imaging;

namespace OBS_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        public string Basepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"TextFiles\");
        public string Imagepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"Images\");
        public RiotApi Api = RiotApi.GetDevelopmentInstance("RGAPI-ae3ff4a1-a358-490b-a054-4971d19030a4", 20, 100);
        public LeagueClientApi LCUApi;
        public List<string> SummonerNames = new List<string>();
        private DispatcherTimer Timer = new DispatcherTimer();
        public State State;
        public CSAction Actions;
        public string LatestVersion;
        public ChampionListStatic Champions;

        public event EventHandler<LeagueEvent> GameFlowChanged;

        public Overlay()
        {
            InitializeComponent();

            LatestVersion = File.ReadAllText(Basepath + "Latest_Version.txt");
            var versions = Api.StaticData.Versions.GetAllAsync().Result;

            if (versions[0] != LatestVersion)
                File.WriteAllText(Basepath + "Latest_Version.txt", versions[0]);

            GetChampions();

            Timer.Interval = TimeSpan.FromMilliseconds(500);
            Timer.Tick += TimerTick;

            Status.Content = "Connected";

            Timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (State == State.ChampSelect)
            {
                GetChampSelectData();
            }

            if (State == State.ChampSelect && SummonerNames.Count == 0)
            {
                GetSummonerNames();
            }
                

            if (State == State.ChampSelect && SummonerNames.Count > 0)
            {
                SetSummonerNames();
            }

            EventExampleAsync().ConfigureAwait(true);
        }

        public void GetChampions()
        {
            Champions = Api.StaticData.Champions.GetAllAsync(LatestVersion).Result;

            foreach (var champion in Champions.Champions)
            {
#warning TODO
                //object pickImage, banImage;

                //string imagePick = champion.Value.Image.Full;
                //string imageBan = champion.Value.Name + "_0.jpg";

                //WebRequest wrPick = WebRequest.Create($"http://ddragon.leagueoflegends.com/cdn/{LatestVersion}/img/champion/{imagePick}");
                //WebRequest wrBan = WebRequest.Create($"http://ddragon.leagueoflegends.com/cdn/img/champion/loading/{imageBan}");

                //WebResponse resPick = wrPick.GetResponse();
                //WebResponse resBan = wrBan.GetResponse();

                //using (StreamReader sr = new StreamReader(resPick.GetResponseStream()))
                //{
                //    string json = sr.ReadToEnd();
                //    pickImage = JsonConvert.DeserializeObject(json);
                //}

                //using (StreamReader sr = new StreamReader(resBan.GetResponseStream()))
                //{
                //    string json = sr.ReadToEnd();
                //    banImage = JsonConvert.DeserializeObject(json);
                //}

                //PickImages.Add(pickImage);
                //BanImages.Add(banImage);
            }
        }

        public async Task EventExampleAsync()
        {
            LCUApi = await LeagueClientApi.ConnectAsync();

            GameFlowChanged += OnGameFlowChanged;
            LCUApi.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", GameFlowChanged);
        }

        private void OnGameFlowChanged(object sender, LeagueEvent e)
        {
            var result = e.Data.ToString();

            if (result == "None")
                State = State.MainMenu;
            else if (result == "ChampSelect")
                State = State.ChampSelect;
            else if (result == "Lobby")
                State = State.Lobby;
            else if (result == "InProgress")
                State = State.InProgress;

            // Print new state and set work to complete.
            this.Dispatcher.Invoke(() =>
            {
                Status.Content = $"Status update: Entered {State}.";
            });
        }

        public async void GetChampSelectData()
        {
            var queryParameters = Enumerable.Empty<string>();
            var json = await LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "lol-champ-select/v1/session",
                                                                     queryParameters);

            var jsonResult = JsonConvert.DeserializeObject<CSActions>(json);

            if (jsonResult.actions.Last().First().championId != 0)
            {
                var champion = Champions.Champions.SingleOrDefault(c => c.Value.Id == jsonResult.actions.Last().First().championId);

                this.Dispatcher.Invoke(() =>
                {
                    Champion.Content = champion.Value.Name;

                    Uri source = new Uri(Imagepath + $"ChampionIcons\\{champion.Value.Image.Full}");
                    Icon.Source = new BitmapImage(source);
                });
            }
        }

        public async void GetSummonerNames()
        {
            SummonerNames = new List<string>();

            var queryParameters = Enumerable.Empty<string>();
            var json = await LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "lol-champ-select/v1/session",
                                                                     queryParameters);

            var jsonResults = JsonConvert.DeserializeObject<CSPlayers>(json);

            if (jsonResults != null)
            {
                jsonResults.myTeam.ForEach(async mt =>
                {
                    var jsonSummonerResponse = await LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, $"lol-summoner/v1/summoners/{mt.summonerId}", queryParameters);
                    SummonerName summonerName = JsonConvert.DeserializeObject<SummonerName>(jsonSummonerResponse);
                    SummonerNames.Add(summonerName.displayName);
                });

                jsonResults.theirTeam.ForEach(async tt =>
                {
                    var jsonSummonerResponse = await LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, $"lol-summoner/v1/summoners/{tt.summonerId}", queryParameters);
                    SummonerName summonerName = JsonConvert.DeserializeObject<SummonerName>(jsonSummonerResponse);
                    SummonerNames.Add(summonerName.displayName);
                });
            }
        }

        public async void SetSummonerNames()
        {
            await Task.Run(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < SummonerNames.Count; i++)
                    {
                        if (i == 0)
                            SummonerName1.Text = SummonerNames[i];
                        else if (i == 1)
                            SummonerName2.Text = SummonerNames[i];
                        else if (i == 2)
                            SummonerName3.Text = SummonerNames[i];
                        else if (i == 3)
                            SummonerName4.Text = SummonerNames[i];
                        else if (i == 4)
                            SummonerName5.Text = SummonerNames[i];
                        else if (i == 5)
                            SummonerName6.Text = SummonerNames[i];
                        else if (i == 6)
                            SummonerName7.Text = SummonerNames[i];
                        else if (i == 7)
                            SummonerName8.Text = SummonerNames[i];
                        else if (i == 8)
                            SummonerName9.Text = SummonerNames[i];
                        else if (i == 9)
                            SummonerName10.Text = SummonerNames[i];
                    }
                });                
            });
        }
    }
}
