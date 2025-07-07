using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MoleMash8
{
    public partial class MainPage : ContentPage
    {
        private SKBitmap moleBitmap; // Mole image
        private SKPoint molePosition; // current position of the mole
        private const float moleSize = 100f; // size of the image in the app

        private int score = 0;
        private int highScore = 0;
        private int timeLeft = 30;

        private readonly Random rand = new(); // For mole's random movement across the canvas
        private IDispatcherTimer moleTimer; // For mole movement
        private Timer gameTimer; // For 800ms countdown

        public MainPage()
        {
            // Initialize the UI component defined in XAML
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            // Loads the game on app start 
            base.OnAppearing(); 

            moleBitmap = LoadEmbeddedBitmap("MoleMash8.Resources.Images.mole.png");
            StartGame();
        }

        private SKBitmap LoadEmbeddedBitmap(string resourcePath)
        {
            // Loads the image from embedded resources into SKBitmap
            var assembly = typeof(MainPage).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            return SKBitmap.Decode(stream);
        }

        private void StartGame()
        {
            // Start countdown timer, resets score, 
            score = 0;
            timeLeft = 30;
            ScoreLabel.Text = $"Score: {score}";
            TimeLabel.Text = $"Time: {timeLeft}";

            moleTimer = Dispatcher.CreateTimer();
            moleTimer.Interval = TimeSpan.FromMilliseconds(800);
            moleTimer.Tick += MoveMole;
            moleTimer.Start();

            gameTimer = new Timer(1000);
            gameTimer.Elapsed += OnGameTimerElapsed;
            gameTimer.Start();
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            // Stop existing timers and restart the game
            moleTimer?.Stop();
            gameTimer?.Stop();
            StartGame();
        }

        private void MoveMole(object sender, EventArgs e)
        {
            // Moves the mole in the canvas and redraw in canvas to show the changes in the canvas
            float maxX = (float)GameCanvas.CanvasSize.Width - moleSize;
            float maxY = (float)GameCanvas.CanvasSize.Height - moleSize;

            molePosition = new SKPoint(
                (float)(rand.NextDouble() * maxX),
                (float)(rand.NextDouble() * maxY)
            );

            GameCanvas.InvalidateSurface();
        }

        private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // Clears the canvas and draws the mole image at the current position
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.LightGreen);

            if (moleBitmap != null)
            {
                var destRect = new SKRect(
                    molePosition.X, molePosition.Y,
                    molePosition.X + moleSize, molePosition.Y + moleSize
                );
                canvas.DrawBitmap(moleBitmap, destRect);
            }
        }

        private void OnCanvasTouched(object sender, SKTouchEventArgs e)
        {
            // Checks if the user tapped on the mole, and adds up score if touched
            var touch = e.Location;

            if (touch.X >= molePosition.X && touch.X <= molePosition.X + moleSize &&
                touch.Y >= molePosition.Y && touch.Y <= molePosition.Y + moleSize)
            {
                score++;
                ScoreLabel.Text = $"Score: {score}";

                // Vibrate
                try { Vibration.Default.Vibrate(); } catch { }

                // TODO: Add sound effect
            }

            e.Handled = true;
        }

        private void OnGameTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // decrements time, shows game over, high score, current score after finish
            timeLeft--;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimeLabel.Text = $"Time: {timeLeft}";

                if (timeLeft <= 0)
                {
                    moleTimer.Stop();
                    gameTimer.Stop();

                    if (score > highScore)
                    {
                        highScore = score;
                        HighScoreLabel.Text = $"High Score: {highScore}";
                    }

                    DisplayAlert("Game Over", $"Your score: {score}", "OK");
                }
            });
        }
    }

}
