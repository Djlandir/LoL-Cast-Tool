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
using System.Diagnostics;

namespace OBS_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        public string Basepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"TextFiles\");
        public string Imagepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"Images\");
        public RiotApi Api = RiotApi.GetDevelopmentInstance(GlobalValues.RiotAPIKey, 20, 100);
        public LeagueClientApi LCUApi;
        public List<string> SummonerNames = new List<string>();
        private DispatcherTimer Timer = new DispatcherTimer();
        private DispatcherTimer TimerClientStateCheck = new DispatcherTimer();
        public State State;
        public CSAction Actions;
        public string LatestVersionString;
        public ChampionListStatic Champions;
        public bool GoldGraphOpened = false;
        public LatestVersionService LatestVersionService = new LatestVersionService();
        public List<System.Windows.Controls.Image> PickPlaceholders = new List<System.Windows.Controls.Image>();
        public List<System.Windows.Controls.Image> BanPlaceholders = new List<System.Windows.Controls.Image>();

        public event EventHandler<LeagueEvent> GameFlowChanged;

        public Overlay()
        {
            InitializeComponent();
            
            AddPlaceholders();

            LatestVersionString = LatestVersionService.GetLatestVersion();

            BlueTeam.Content = File.ReadAllText(Basepath + "Blue_Team.txt") + "    " + File.ReadAllText(Basepath + "Blue_Score.txt");
            RedTeam.Content = File.ReadAllText(Basepath + "Red_Team.txt") + "    " + File.ReadAllText(Basepath + "Red_Score.txt");

            GetChampions();

            Timer.Interval = TimeSpan.FromMilliseconds(500);
            Timer.Tick += TimerTick;

            TimerClientStateCheck.Interval = TimeSpan.FromMilliseconds(500);
            TimerClientStateCheck.Tick += TimerTickClientStateCheck;

            Status.Content = "Connected";

            TimerClientStateCheck.Start();
        }

        private void TimerTickClientStateCheck(object sender, EventArgs e)
        {
            EventExampleAsync().ConfigureAwait(true);
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

            if (State == State.InProgress && GoldGraphOpened == false)
            {
                GoldGraph goldGraph = new GoldGraph();
                goldGraph.Show();

                GoldGraphOpened = true;
            }

            if (State != State.InProgress && GoldGraphOpened == true)
                GoldGraphOpened = false;
        }

        public void GetChampions()
        {
            Champions = Api.StaticData.Champions.GetAllAsync(LatestVersionString).Result;
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

                if (State == State.ChampSelect && !Timer.IsEnabled)
                    Timer.Start();
                else if (State != State.ChampSelect && Timer.IsEnabled)
                    Timer.Stop();
            });
        }

        public void GetChampSelectData()
        {
            var queryParameters = Enumerable.Empty<string>();
            var json = LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "lol-champ-select/v1/session",
                                                                     queryParameters).Result;

            var jsonResult = JsonConvert.DeserializeObject<CSActions>(json);

            var parsedJson = new List<CSAction>();
            this.Dispatcher.Invoke(() =>
            {
                foreach (var actions in jsonResult.actions)
                {
                    parsedJson.Add(actions.First());
                }
            });
            

            if (parsedJson.Count > 1)
            {
                var lastAction = parsedJson.Where(x => x.championId != 0).Last();

                if (lastAction.championId != 0)
                {
                    var champion = Champions.Champions.SingleOrDefault(c => c.Value.Id == lastAction.championId);

                    this.Dispatcher.Invoke(() =>
                    {
                        if (lastAction.type == "pick")
                        {
                            Uri source = new Uri(Imagepath + $"ChampionSplash\\{champion.Value.Key}_0.jpg");
                            PickPlaceholders[lastAction.actorCellId].Source = new BitmapImage(source);
                        }
                        else if (lastAction.type == "ban")
                        {
                            Uri source = new Uri(Imagepath + $"ChampionIcons\\{champion.Value.Key}.png");
                            BanPlaceholders[lastAction.pickTurn - 1].Source = new BitmapImage(source);
                        }
                    });
                }
            }
        }

        public void GetSummonerNames()
        {
            SummonerNames = new List<string>();

            var queryParameters = Enumerable.Empty<string>();
            var json = LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "lol-champ-select/v1/session",
                                                                     queryParameters).Result;

            var jsonResults = JsonConvert.DeserializeObject<CSPlayers>(json);

            if (jsonResults != null)
            {
                jsonResults.myTeam.ForEach(mt =>
                {
                    var jsonSummonerResponse = LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, $"lol-summoner/v1/summoners/{mt.summonerId}", queryParameters).Result;
                    SummonerName summonerName = JsonConvert.DeserializeObject<SummonerName>(jsonSummonerResponse);
                    SummonerNames.Add(summonerName.displayName);
                });

                jsonResults.theirTeam.ForEach(tt =>
                {
                    var jsonSummonerResponse = LCUApi.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, $"lol-summoner/v1/summoners/{tt.summonerId}", queryParameters).Result;
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
                            SummonerName1.Content = SummonerNames[i];
                        else if (i == 1)
                            SummonerName2.Content = SummonerNames[i];
                        else if (i == 2)
                            SummonerName3.Content = SummonerNames[i];
                        else if (i == 3)
                            SummonerName4.Content = SummonerNames[i];
                        else if (i == 4)
                            SummonerName5.Content = SummonerNames[i];
                        else if (i == 5)
                            SummonerName6.Content = SummonerNames[i];
                        else if (i == 6)
                            SummonerName7.Content = SummonerNames[i];
                        else if (i == 7)
                            SummonerName8.Content = SummonerNames[i];
                        else if (i == 8)
                            SummonerName9.Content = SummonerNames[i];
                        else if (i == 9)
                            SummonerName10.Content = SummonerNames[i];
                    }
                });                
            });
        }

        private void AddPlaceholders()
        {
            PickPlaceholders.Add(PickTurn1);
            PickPlaceholders.Add(PickTurn2);
            PickPlaceholders.Add(PickTurn3);
            PickPlaceholders.Add(PickTurn4);
            PickPlaceholders.Add(PickTurn5);
            PickPlaceholders.Add(PickTurn6);
            PickPlaceholders.Add(PickTurn7);
            PickPlaceholders.Add(PickTurn8);
            PickPlaceholders.Add(PickTurn9);
            PickPlaceholders.Add(PickTurn10);

            BanPlaceholders.Add(BanTurn1);
            BanPlaceholders.Add(BanTurn2);
            BanPlaceholders.Add(BanTurn3);
            BanPlaceholders.Add(BanTurn4);
            BanPlaceholders.Add(BanTurn5);
            BanPlaceholders.Add(BanTurn6);
            BanPlaceholders.Add(BanTurn7);
            BanPlaceholders.Add(BanTurn8);
            BanPlaceholders.Add(BanTurn9);
            BanPlaceholders.Add(BanTurn10);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TimerClientStateCheck.Stop();
            Timer.Stop();
        }
    }
}
