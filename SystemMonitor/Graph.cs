using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SystemMonitor
{
    public class Graph : Canvas
    {
        double _x;
        Polyline _line;
        Polygon _area;

        public Graph()
        {
            _line = new Polyline();
            _line.StrokeThickness = 3;
            _area = new Polygon();

            Children.Add(_line);
            Children.Add(_area);

            LineColor = Brushes.White;
            BackColor = Brushes.Black;
        }

        public Brush LineColor
        {
            get => _line.Stroke;
            set => _line.Stroke = value;
        }

        public Brush BackColor
        {
            get => _area.Fill;
            set => _area.Fill = value;
        }

        public void SetNewValue(double val)
        {
            double x2 = _x + 5,
                y2 = ActualHeight - (ActualHeight / 100 * val);

            var punto = new Point(x2, y2);

            _line.Points.Add(punto);

            if (_area.Points.Count > 1)
                _area.Points.RemoveAt(_area.Points.Count - 1);
            _area.Points.Add(punto);
            _area.Points.Add(new Point(x2, ActualHeight));

            // Save 200 points max
            if (_line.Points.Count > 200)
                _line.Points.RemoveAt(0);
            if (_area.Points.Count > 200)
                _area.Points.RemoveAt(0);

            if (x2 > ActualWidth)
                RenderTransform = new TranslateTransform(-(x2 - ActualWidth), 0);

            _x = x2;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Add the start point of the graph
            var startPoint = new Point(0, sizeInfo.NewSize.Height);
            _line.Points.Add(startPoint);
            _area.Points.Add(startPoint);
        }
    }
}