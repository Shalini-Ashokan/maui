using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 30007, "Line Drawing Stroke Extension Issue on Windows", PlatformAffected.Windows)]
    public partial class Issue30007 : ContentPage
    {
        private float _currentStrokeWidth = 1.0f;
        private LineDrawingTestDrawable _drawable;

        public Issue30007()
        {
            InitializeComponent();
            _drawable = new LineDrawingTestDrawable(_currentStrokeWidth);
            LineTestGraphicsView.Drawable = _drawable;
        }

        private void OnIncreaseStrokeClicked(object sender, EventArgs e)
        {
            _currentStrokeWidth += 1.0f;
            if (_currentStrokeWidth > 10.0f)
                _currentStrokeWidth = 10.0f;
            UpdateStroke();
        }

        private void OnDecreaseStrokeClicked(object sender, EventArgs e)
        {
            _currentStrokeWidth -= 1.0f;
            if (_currentStrokeWidth < 1.0f)
                _currentStrokeWidth = 1.0f;
            UpdateStroke();
        }

        private void UpdateStroke()
        {
            _drawable.StrokeWidth = _currentStrokeWidth;
            StrokeWidthLabel.Text = $"Stroke Width: {_currentStrokeWidth}";
            LineTestGraphicsView.Invalidate();
        }
    }

    public class LineDrawingTestDrawable : IDrawable
    {
        public float StrokeWidth { get; set; }

        public LineDrawingTestDrawable(float strokeWidth)
        {
            StrokeWidth = strokeWidth;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Green;
            canvas.StrokeSize = StrokeWidth;
            canvas.StrokeLineCap = LineCap.Butt; // This should prevent line extension

            // Draw reference rectangles to show expected line endpoints
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(50, 50, 100, 100); // Rectangle 1
            canvas.DrawRectangle(200, 50, 100, 100); // Rectangle 2
            canvas.DrawRectangle(50, 200, 100, 100); // Rectangle 3
            canvas.DrawRectangle(200, 200, 100, 100); // Rectangle 4

            // Reset stroke properties for test line
            canvas.StrokeColor = Colors.Green;
            canvas.StrokeSize = StrokeWidth;
            canvas.StrokeLineCap = LineCap.Butt;

            // Draw test lines that should end exactly at rectangle boundaries
            // Horizontal line from left edge of rect 1 to left edge of rect 2
            canvas.DrawLine(50, 100, 200, 100);
            
            // Vertical line from top edge of rect 1 to top edge of rect 3  
            canvas.DrawLine(100, 50, 100, 200);
            
            // Diagonal line from bottom-right of rect 1 to top-left of rect 4
            canvas.DrawLine(150, 150, 200, 200);

            // Add labels to show expected vs actual behavior
            canvas.FontColor = Colors.Black;
            canvas.FontSize = 12;
            canvas.DrawString("Expected: Line ends at rectangle edges", 20, 320, HorizontalAlignment.Left);
            canvas.DrawString("Issue: Lines extend beyond edges on Windows", 20, 340, HorizontalAlignment.Left);
            canvas.DrawString($"Current stroke width: {StrokeWidth}", 20, 360, HorizontalAlignment.Left);
        }
    }
}