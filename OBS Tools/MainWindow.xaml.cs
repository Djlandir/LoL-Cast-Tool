using LCUSharp;
using OBS_Tools.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace OBS_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Basepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"TextFiles\");
        public string Imagepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"Images\");
        public List<Team> Teams;

        public MainWindow()
        {
            InitializeComponent();

            BlueShort.Text = File.ReadAllText(Basepath + "Blue_Short.txt");
            BlueTeam.Text = File.ReadAllText(Basepath + "Blue_Team.txt");
            BlueScore.Text = File.ReadAllText(Basepath + "Blue_Score.txt");
            RedShort.Text = File.ReadAllText(Basepath + "Red_Short.txt");
            RedTeam.Text = File.ReadAllText(Basepath + "Red_Team.txt");
            RedScore.Text = File.ReadAllText(Basepath + "Red_Score.txt");
            Tournament.Text = File.ReadAllText(Basepath + "Tournament.txt");

            Teams = GenerateTeams();

            Teams.ForEach(t =>
            {
                TeamsBlue.Items.Add(t.Name);
                TeamsRed.Items.Add(t.Name);
            });
        }

        private void BlueShort_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Blue_Short.txt", BlueShort.Text);
        }

        private void BlueTeam_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Blue_Team.txt", BlueTeam.Text);
        }

        private void BlueScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Blue_Score.txt", BlueScore.Text);
        }

        private void RedShort_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Red_Short.txt", RedShort.Text);
        }

        private void RedTeam_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Red_Team.txt", RedTeam.Text);
        }

        private void RedScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Red_Score.txt", RedScore.Text);
        }

        private void BlueScoreIG_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Blue_Score_IG.txt", BlueScoreIG.Text);
        }

        private void RedScoreIG_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Red_Score_IG.txt", RedScoreIG.Text);
        }

        private void Tournament_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditFiles(Basepath + "Tournament.txt", Tournament.Text);
        }

        public void EditFiles(string file, string content)
        {
            File.WriteAllText(file, content);
        }

        public void SetImage(int id, int side)
        {
            Team team = Teams.SingleOrDefault(t => t.Id == id);

            if (side == 1)
            {
                string bluePath = Imagepath + @"ActiveImageBlueSide\";
                DirectoryInfo dBlue = new DirectoryInfo(bluePath);
                FileInfo[] dBlueFiles = dBlue.GetFiles("*.png");

                if (dBlueFiles.Length > 0)
                {
                    File.Move(dBlueFiles[0].FullName, Imagepath + $"AllImages\\{dBlueFiles[0].Name}");
                    File.Move(Imagepath + $"AllImages\\{team.Name}", bluePath + team.Name);
                }
                else
                {
                    File.Move(Imagepath + $"AllImages\\{team.Name}", bluePath + team.Name);
                }

                
            }
            else if (side == 2)
            {
                string redPath = Imagepath + @"ActiveImageRedSide\";
                DirectoryInfo dRed = new DirectoryInfo(redPath);
                FileInfo[] dRedFiles = dRed.GetFiles("*.png");

                if (dRedFiles.Length > 0)
                {
                    File.Move(dRedFiles[0].FullName, Imagepath + $"AllImages\\{dRedFiles[0].Name}");
                    File.Move(Imagepath + $"AllImages\\{team.Name}", redPath + team.Name);
                }
                else
                {
                    File.Move(Imagepath + $"AllImages\\{team.Name}", redPath + team.Name);
                }
            }
            else
                throw new Exception("Ungültige Eingabe");
        }

        public List<Team> GenerateTeams()
        {
            List<Team> teams = new List<Team>();
            int index = 0;

            string pathGeneral = Imagepath + @"AllImages\";
            string pathBlue = Imagepath + @"ActiveImageBlueSide\";
            string pathRed = Imagepath + @"ActiveImageRedSide\";

            DirectoryInfo dGeneral = new DirectoryInfo(pathGeneral);
            DirectoryInfo dBlue = new DirectoryInfo(pathBlue);
            DirectoryInfo dRed = new DirectoryInfo(pathRed);

            foreach (var file in dGeneral.GetFiles("*.png"))
            {
                Team team = new Team
                {
                    Id = index,
                    Name = file.Name
                };

                teams.Add(team);
                index++;
            }

            foreach (var file in dBlue.GetFiles("*.png"))
            {
                Team team = new Team
                {
                    Id = index,
                    Name = file.Name
                };

                teams.Add(team);
                index++;
            }

            foreach (var file in dRed.GetFiles("*.png"))
            {
                Team team = new Team
                {
                    Id = index,
                    Name = file.Name
                };

                teams.Add(team);
                index++;
            }

            return teams;
        }

        private void ChangeImageBlue(object sender, RoutedEventArgs e)
        {
            SetImage(TeamsBlue.SelectedIndex, 1);
        }

        private void ChangeImageRed(object sender, RoutedEventArgs e)
        {
            SetImage(TeamsBlue.SelectedIndex, 2);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Overlay overlay = new Overlay();
            overlay.Show();
        }
    }
}
