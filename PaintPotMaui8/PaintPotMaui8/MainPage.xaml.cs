using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PaintPotMaui8;
public partial class MainPage : ContentPage
{
    // State variables - list of dots that are drawn on the image, currentColor get the color selected - Default value is Red.

    List<(float x, float y, SKColor color)> dots = new();
    SKColor currentColor = SKColors.Red;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCanvasTouched(object sender, SKTouchEventArgs e)
    {
        // Handle mouse click and drag event
        if (e.DeviceType == SKTouchDeviceType.Mouse)
        {
            // On Windows (Mouse), only draw on Pressed or when left button is held
            if (e.ActionType == SKTouchAction.Pressed ||
                (e.ActionType == SKTouchAction.Moved && e.InContact))
            {
                dots.Add((e.Location.X, e.Location.Y, currentColor));
                CanvasView.InvalidateSurface();
            }
        }
        else
        {
            // On touch devices, just handle Pressed and Moved normally
            if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
            {
                dots.Add((e.Location.X, e.Location.Y, currentColor));
                CanvasView.InvalidateSurface();
            }
        }

        e.Handled = true;
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        // We call this to draw on the canvas, defining the paint properties, and adding dot to the canvas on the image
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            StrokeWidth = 4
        };

        foreach (var dot in dots)
        {
            paint.Color = dot.color;
            canvas.DrawCircle(dot.x, dot.y, 10, paint);
        }
    }

    private void OnColorClicked(object sender, EventArgs e)
    {
        // Event handler to handle color button click (to change colors)
        if (sender is Button button)
        {
            var mauiColor = button.BackgroundColor;
            currentColor = new SKColor(
                (byte)(mauiColor.Red * 255),
                (byte)(mauiColor.Green * 255),
                (byte)(mauiColor.Blue * 255));
        }
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        // Handler for clear button (clears the drawing from the canvas)
        dots.Clear();
        CanvasView.InvalidateSurface();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Handler to save the image
        // This handler involves File I/O, and Image processing which are time intensive tasks. Hence, it is important to declare
        // this method as async-await method 

        var imageInfo = new SKImageInfo((int)CanvasView.CanvasSize.Width, (int)CanvasView.CanvasSize.Height);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        var paint = new SKPaint { Style = SKPaintStyle.Fill, StrokeWidth = 4 };

        foreach (var dot in dots)
        {
            paint.Color = dot.color;
            canvas.DrawCircle(dot.x, dot.y, 10, paint);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        var filename = Path.Combine(FileSystem.AppDataDirectory, $"drawing_{DateTime.Now.Ticks}.png");
        using var stream = File.OpenWrite(filename);
        data.SaveTo(stream);

        await DisplayAlert("Saved", $"Drawing saved to:\n{filename}", "OK");
    }
}

