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
using System.Windows.Shapes;

namespace Lesson5ProgrammingProjectWPF
{
    /// <summary>
    /// Interaction logic for GameSettings.xaml
    /// </summary>
    public partial class GameSettings : Window
    {

        internal GameMode GameMode { get; set; }
        internal int GameScore { get; set; }
        internal int GameSpeed { get; set; }
        internal int GameTime { get; set; }
        internal bool OkClicked { get; set; }

        string GAME_SETTINGS_FILE_PATH;


        public GameSettings()
        {
            InitializeComponent();

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataPath = System.IO.Path.Combine(appDataPath, "BubblePopGame");
            System.IO.Directory.CreateDirectory(appDataPath);
            GAME_SETTINGS_FILE_PATH = System.IO.Path.Combine(appDataPath, "GameSettings.txt");

            // read game settings and display on form 

            try
            {
                // load saved setting to game settings window 

                StreamReader fileReader = new StreamReader(GAME_SETTINGS_FILE_PATH);

                using (fileReader)
                {
                    // SET GAME SETTINGS AFTER READING FILE 
                    string line = fileReader.ReadLine();
                    // read game mode 
                    if (line.Substring(11) == "Stack")
                    {
                        StackButton.IsChecked = true; 
                    }
                    else
                    {
                        QueueButton.IsChecked = true; 
                    }

                    line = fileReader.ReadLine();
                    // read game speed 
                    if (line.Substring(12) == "Hard")
                    {
                        HardButton.IsChecked = true; 
                    }
                    else if (line.Substring(12) == "Medium")
                    {
                        MediumButton.IsChecked = true; 
                    } 
                    else
                    {
                        EasyButton.IsChecked = true; 
                    }

                    line = fileReader.ReadLine();
                    // read starting bubble
                    StartBubblesBox.Text = line.Substring(18);

                    line = fileReader.ReadLine();
                    // read game time 
                    GameLengthBox.Text = line.Substring(15);

                }

            }
            catch (FileNotFoundException)
            {

            }
            catch (IOException exception)
            {
                MessageBox.Show($"Something went wrong: {exception.Message}");
            }

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {


            // write file with all game settings data 

            try
            {
                StreamWriter fileWriter = new StreamWriter(GAME_SETTINGS_FILE_PATH);
                using (fileWriter)
                {
                    // check game mode and write 
                    if ((bool)QueueButton.IsChecked)
                    {
                        fileWriter.WriteLine("Game Mode: Queue");
                    } else
                    {
                        fileWriter.WriteLine("Game Mode: Stack");
                    }

                    // check game difficulty and write speed
                    if ((bool)HardButton.IsChecked)
                    {
                        fileWriter.WriteLine("Game Speed: 500");
                    }
                    else if ((bool)MediumButton.IsChecked)
                    {
                        fileWriter.WriteLine("Game Speed: 750");
                    } else
                    {
                        fileWriter.WriteLine("Game Speed: 1000");
                    }

                    // parse starting number of bubbles and write 
                    // set default starting bubbles if left empty 
                    if(StartBubblesBox.Text.Length <= 0)
                    {
                        fileWriter.WriteLine("Starting Bubbles: 10");
                    } else
                    {
                        fileWriter.WriteLine("Starting Bubbles: " + StartBubblesBox.Text);
                    }



                    // parse game length and write 
                    // set default game length if left empty 
                    if (GameLengthBox.Text.Length <= 0)
                    {
                        fileWriter.WriteLine("Starting Time: 60 ");
                    }
                    else
                    {
                        fileWriter.WriteLine("Starting Time: " + GameLengthBox.Text);
                    }

                }
            }
            catch (IOException j)
            {
                MessageBox.Show(j.Message);
            }

            OkClicked = true;

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OkClicked = false; 
        }
    }
}
