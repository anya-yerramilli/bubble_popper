using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Threading;
using Microsoft.Win32; 

namespace Lesson5ProgrammingProjectWPF
{
    //  namespace enums
    internal enum GameMode { Stack, Queue };

    public partial class MainWindow : Window
    {
        // class constants
        private int STARTING_BUBBLES = 10;
        private const int QUEUE_START_NUM = 100;

        // properties
        private GameMode GameMode { get; set; }
        private int GameScore { get; set; }
        private int HighScore { get; set; }
        private int GameSpeed { get; set; }
        private int GameTime { get; set; }

        private bool OkClicked { get; set; }

        // fields
        private DispatcherTimer _gameTimer;
        private Queue<Button> _queue;
        private Stack<Button> _stack;
        private Random _rng;

        public string GAME_SETTINGS_FILE_PATH;
        public string HIGH_SCORE_FILE_PATH;


        // constructor
        public MainWindow()
        {
            InitializeComponent();

            // initialize timer
            _gameTimer = new DispatcherTimer();
            _gameTimer.Tick += TimerGame_Tick;
            _gameTimer.Interval = new TimeSpan(50);

            this.GameTime = 0;

            // initialize other fields
            _queue = new Queue<Button>();
            _stack = new Stack<Button>();
            _rng = new Random();


            // READ HIGH SCORE FILE AND DISPLAY 
            RegistryKey highScoreKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Bubble");

            if(highScoreKey != null)
            {
                this.HighScore = int.Parse(highScoreKey.GetValue("High Score").ToString());
            }

            this.labelHighScore.Content = "High Score: " + this.HighScore; 


            // reset high score
            //this.HighScore = 0;

            // load game settings
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataPath = System.IO.Path.Combine(appDataPath, "BubblePopGame");
            System.IO.Directory.CreateDirectory(appDataPath);
            GAME_SETTINGS_FILE_PATH = System.IO.Path.Combine(appDataPath, "GameSettings.txt");

            

            GameSettings window = new GameSettings();
            window.ShowDialog();

            LoadSettings();

            // initialize the game 
            this.OkClicked = window.OkClicked;

            if (OkClicked)
            {
                ResetGame();
            }

        }

        // instance methods
        private void LoadSettings()
        {

            // initalize settings to defaults
            this.GameMode = GameMode.Stack;
            this.GameSpeed = 1000;  // new bubble every 1 second
            this.GameTime = 30000;  // game length is 30 seconds

            // load saved user preference and override defaults

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
                        this.GameMode = GameMode.Stack;
                    }
                    else
                    {
                        this.GameMode = GameMode.Queue;
                    }

                    line = fileReader.ReadLine();
                    // read game speed 
                    if (line.Substring(12) == "Hard")
                    {
                        this.GameSpeed = 500;
                    }
                    else if (line.Substring(12) == "Medium")
                    {
                        this.GameSpeed = 750;
                    } else
                    {
                        this.GameSpeed = 1000; 
                    }
                    

                    line = fileReader.ReadLine();
                    // read starting bubble
                    STARTING_BUBBLES = Int16.Parse(line.Substring(18));

                    line = fileReader.ReadLine();
                    // read game time 
                    GameTime = Int16.Parse(line.Substring(15)) *1000;

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

        private Button CreateBubbleButton(int num)
        {
            // construct the button control for this bubble
            Button button = new Button();
            // set the text for the button
            button.Content = num.ToString();
            button.FontWeight = FontWeights.Bold;
            // set the background image for the button
            string uri = "pack://application:,,,/Resources/SmallBubble.png";
            if (_rng.Next(0, 10) == 0)
            {
                // 10% chance to get large bubble
                uri = "pack://application:,,,/Resources/LargeBubble.png";
            }
            BitmapImage image = new BitmapImage(new Uri(uri));
            button.Background = new ImageBrush(image);
            // button dimensions
            button.Height = image.Height;
            button.Width = image.Width;
            // remove the border
            button.BorderThickness = new Thickness(0.0);
            // place the bubble on the screen
            int left = _rng.Next((int)button.Width, (int)(this.Width - button.Width * 2));
            int top = _rng.Next((int)button.Height, (int)(this.Height - button.Height * 2));
            Canvas.SetLeft(button, left);
            Canvas.SetTop(button, top);
            // set the style of the button
            button.Style = (Style)FindResource("ButtonStyleNoMouseOver");
            // add the event handler
            button.Click += BubbleButton_Click;
            // return the new button
            return button;
        }

        private void ResetGame()
        {
            // reset the scoreboard
            this.GameScore = 0;
            labelScore.Content = $"Score: {this.GameScore}";
            labelHighScore.Content = $"High Score: {this.HighScore}";

            // set the queue and the stack
            this._queue.Clear();
            this._stack.Clear();

            // remove all bubble buttons
            List<UIElement> bubbles = new List<UIElement>();
            foreach (UIElement e in gameCanvas.Children)
            {
                if (e is Button)
                {
                    bubbles.Add(e);
                }
            }
            foreach (UIElement e in bubbles)
            {
                gameCanvas.Children.Remove(e);
            }

            // create the starting number of bubbles
            if (this.GameMode == GameMode.Stack)
            {
                for (int num = 1; num <= STARTING_BUBBLES; num++)
                {
                    Button b = CreateBubbleButton(num);
                    // add the button to the stack
                    _stack.Push(b);
                    // add the button to the canvas
                    gameCanvas.Children.Add(b);
                }
            }
            else
            {
                // create a list of buttons
                Button[] arr = new Button[STARTING_BUBBLES];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = CreateBubbleButton(QUEUE_START_NUM + i);
                    // add the button to the queue
                    _queue.Enqueue(arr[i]);
                }
                // add the buttons to the game canvas in revere order
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    gameCanvas.Children.Insert(0, arr[i]);
                }
            }

            _gameTimer.Start();
        }




        // event handlers
        private void BubbleButton_Click(object sender, RoutedEventArgs e)
        {

            Button b = sender as Button;

            if (GameMode.ToString() == "Stack")
            {
                if (b.Content.ToString() == _stack.Count.ToString())
                {
                    GameScore++;
                    labelScore.Content = "Score: " + GameScore.ToString();

                    gameCanvas.Children.Remove(b);
                    _stack.Pop();
                }
            }
            else
            {
                if (b == _queue.Peek())
                {
                    GameScore++;
                    labelScore.Content = "Score: " + GameScore.ToString();

                    gameCanvas.Children.Remove(b);
                    _queue.Dequeue();
                }

            }

        }

        private void EndGame()
        {

            _gameTimer.Stop();
            


            MessageBox.Show("Game Over");

            // set high scores
            this.HighScore = 0;
            RegistryKey highScoreKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Bubble");

            if (highScoreKey != null)
            {
                this.HighScore = int.Parse(highScoreKey.GetValue("High Score").ToString());
            }
            
            if(this.HighScore < this.GameScore)
            {

                this.labelHighScore.Content = "High Score: " + this.GameScore;
                this.HighScore = this.GameScore;

                highScoreKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Bubble");
                highScoreKey.SetValue("High Score", this.GameScore);
                highScoreKey.Close(); 

                
               
            }

            ResetGame();
            LoadSettings(); 
            
        }

        private void TimerGame_Tick(object sender, EventArgs e)
        {
            GameTime--;

            if (GameTime == 0)
            {
                EndGame();
            }

            if (GameTime / 1000 < 10)
            {
                labelTimer.Content = "Timer: 0:0" + (GameTime / 1000).ToString();
            }
            else
            {
                labelTimer.Content = "Timer: 0:" + (GameTime / 1000).ToString();
            }

            if (GameTime % GameSpeed == 0)
            {
                if (GameMode.ToString() == "Stack")
                {
                    // if stack empty, end game 
                    if (_stack.Count <= 0)
                    {
                        EndGame();
                    }

                    // add button to bottom of stack 
                    Button newButton = CreateBubbleButton((Int16.Parse(_stack.Peek().Content.ToString()) + 1));
                    _stack.Push(newButton);
                    gameCanvas.Children.Add(newButton);


                }
                else
                {

                    // if queue empty, end game
                    if (_queue.Count <= 0)
                    {
                        EndGame();
                    }

                    // add buttom to back of queue
                    Button newButton = CreateBubbleButton(Int16.Parse(_queue.Peek().Content.ToString()) + _queue.Count());
                    _queue.Enqueue(newButton);
                    gameCanvas.Children.Insert(0, newButton);

                }
            }
        }

    }
}