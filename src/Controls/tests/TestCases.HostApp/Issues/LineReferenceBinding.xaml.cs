using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
    public partial class LineReferenceBinding : ContentPage
    {
        private int _changeCount = 0;
        
        public LineReferenceBinding()
        {
            InitializeComponent();
            
            // Update debug info periodically
            Loaded += OnLoaded;
            
            // Listen for size changes
            lblDistanceText.SizeChanged += OnLabelSizeChanged;
            testLine.PropertyChanged += OnLinePropertyChanged;
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            UpdateDebugInfo();
        }

        private void OnLabelSizeChanged(object sender, EventArgs e)
        {
            UpdateDebugInfo();
        }

        private void OnLinePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Line.X2))
            {
                UpdateDebugInfo();
            }
        }

        private void OnChangeWidthClicked(object sender, EventArgs e)
        {
            _changeCount++;
            
            // Toggle between different widths
            switch (_changeCount % 3)
            {
                case 0:
                    lblDistanceText.Text = "Short";
                    statusLabel.Text = "Status: Changed to Short";
                    break;
                case 1:
                    lblDistanceText.Text = "Medium Text Width";
                    statusLabel.Text = "Status: Changed to Medium";
                    break;
                case 2:
                    lblDistanceText.Text = "Very Long Text That Should Make The Line Longer";
                    statusLabel.Text = "Status: Changed to Long";
                    break;
            }
            
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            if (lblDistanceText != null && lineX2Label != null && labelWidthLabel != null)
            {
                var labelWidth = lblDistanceText.Width;
                var lineX2 = testLine.X2;
                
                labelWidthLabel.Text = $"Label Width: {labelWidth:F1}";
                lineX2Label.Text = $"Line X2: {lineX2:F1}";
            }
        }
    }
}