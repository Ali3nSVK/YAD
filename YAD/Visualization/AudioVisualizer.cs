using NAudio.Wave;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YAD.Visualization
{
    public class AudioVisualizer : ScrollViewer
    {
        private readonly SynchronizationContext parentThread;
        private const int waveFormOffset = 20;

        private double xPosition = 2;
        private double yTranslate = 0;
        private double yScale = 1;

        public Grid Waveform { get; private set; }
        public EventHandler<WaveInEventArgs> AudioDataHandler => OnAudioDataAvailable;

        public AudioVisualizer(SynchronizationContext context)
        {
            VisualizerInit();

            Content = Waveform;
            parentThread = context;
        }

        private void VisualizerInit()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            Background = Brushes.LightSalmon;
            SizeChanged += AudioVisualizer_SizeChanged;
            ScrollChanged += AudioVisualizer_ScrollChanged;

            Waveform = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
        }

        private void AudioVisualizer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollToRightEnd();
        }

        #region Helpers

        public void Clear()
        {
            Waveform.Children.Clear();
            xPosition = 2;
        }

        private Line NewLine()
        {
            Line newLine = new Line
            {
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };

            return newLine;
        }

        private void AddNewWaveFormLine(double max)
        {
            if (max == 0d)
            {
                max = .5d;
            }

            Line newLine = NewLine();
            newLine.X1 = xPosition;
            newLine.Y1 = max * yScale;
            newLine.X2 = xPosition;
            newLine.Y2 = -max * yScale;

            Waveform.Children.Add(newLine);

            xPosition += 2;
        }

        #endregion

        #region Event Handlers

        private void OnAudioDataAvailable(object sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded > 0)
            {
                double average = e.Buffer.Mean(e.BytesRecorded);
                parentThread.Post(s => AddNewWaveFormLine(average), null);
            }
        }

        private void AudioVisualizer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            yTranslate = ActualHeight / 2;
            yScale = ActualHeight / (byte.MaxValue + waveFormOffset);
            Waveform.RenderTransform = new TranslateTransform(0, yTranslate);
        }

        #endregion
    }
}
