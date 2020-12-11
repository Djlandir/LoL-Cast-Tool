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
using System.Windows.Shapes;

namespace OBS_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string Basepath = @"C:\Users\JanPhilippThies\Desktop\obs_test\";

        public MainWindow()
        {
            InitializeComponent();

            PathInput.Text = Basepath;
            BlueShort.Text = File.ReadAllText(Basepath + "Blue_Short.txt");
            BlueTeam.Text = File.ReadAllText(Basepath + "Blue_Team.txt");
            BlueScore.Text = File.ReadAllText(Basepath + "Blue_Score.txt");
            RedShort.Text = File.ReadAllText(Basepath + "Red_Short.txt");
            RedTeam.Text = File.ReadAllText(Basepath + "Red_Team.txt");
            RedScore.Text = File.ReadAllText(Basepath + "Red_Score.txt");
            Tournament.Text = File.ReadAllText(Basepath + "Tournament.txt");
        }

        private void PathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Basepath = PathInput.Text;
        }

        private void BlueShort_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Blue_Short.txt", BlueShort.Text);
        }

        private void BlueTeam_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Blue_Team.txt", BlueTeam.Text);
        }

        private void BlueScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Blue_Score.txt", BlueScore.Text);
        }

        private void RedShort_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Red_Short.txt", RedShort.Text);
        }

        private void RedTeam_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Red_Team.txt", RedTeam.Text);
        }

        private void RedScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Red_Score.txt", RedScore.Text);
        }

        private void BlueScoreIG_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Blue_Score_IG.txt", BlueScoreIG.Text);
        }

        private void RedScoreIG_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Red_Score_IG.txt", RedScoreIG.Text);
        }

        private void Tournament_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(Basepath + "Tournament.txt", Tournament.Text);
        }
    }
}
