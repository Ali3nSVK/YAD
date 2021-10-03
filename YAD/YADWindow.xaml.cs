using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using YAD.Audio;
using YAD.Visualization;

namespace YAD
{
    public partial class YADWindow : Window
    {
        private int selectedDeviceIndex;
        private int selectedChannel;
        private string audioFile;

        private AudioWrapper audioHandler;
        private AudioVisualizer visualizer;
        private List<DeviceContainer> captureDevices;

        private readonly SynchronizationContext syncContext;

        private DeviceContainer SelectedDevice => captureDevices.Single(d => d.DeviceNumber == selectedDeviceIndex);

        public YADWindow()
        {
            InitializeComponent();
            syncContext = SynchronizationContext.Current;

            InitializeYAD();
        }

        private void InitializeYAD()
        {
            audioHandler = new AudioWrapper();
            captureDevices = audioHandler.CaptureDeviceCollection;

            foreach(DeviceContainer cont in captureDevices)
            {
                CaptureDevices.Items.Insert(cont.DeviceNumber, cont.FriendlyName);
            }

            visualizer = new AudioVisualizer(syncContext);
            WaveformGrid.Children.Add(visualizer);
        }

        private string RunSaveDialog()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "yad_recording_" + DateTime.Now.ToString("yyyyMMddTHHmmss"),
                Filter = "Waveform Audio Files (*.wav)|*.wav"
            };
            bool? result = dlg.ShowDialog();

            return result.HasValue && result.Value ? dlg.FileName : null;
        }

        private void HandleException(Exception ex)
        {
            _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Event Handlers

        private void CaptureDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDeviceIndex = CaptureDevices.SelectedIndex;
            int selectedDeviceChannels = captureDevices.Single(d => d.DeviceNumber == selectedDeviceIndex).Channels;

            DeviceChannels.Items.Clear();
            DeviceChannels.Items.Insert(0, "All");

            if (selectedDeviceChannels > 1)
            {
                for (int i = 1; i < selectedDeviceChannels + 1; i++)
                {
                    DeviceChannels.Items.Insert(i, i.ToString());
                }
                DeviceChannels.IsEnabled = true;
            }
            else
            {
                DeviceChannels.IsEnabled = false;
            }

            if (selectedDeviceIndex != -1)
            {
                RecordButton.IsEnabled = true;
                DeviceChannels.SelectedIndex = 0;
            }
        }

        private void RecordButton_Start(object sender, RoutedEventArgs e)
        {
            RecordButton.Click -= RecordButton_Start;
            RecordButton.Click += RecordButton_Stop;
            RecordButton.Content = "Stop";

            CaptureDevices.IsEnabled = false;
            DeviceChannels.IsEnabled = false;

            try
            {
                if (SelectedDevice.ID == YADConstants.LoopbackDevice)
                {
                    audioHandler.RecordLoopback();
                }
                else
                {
                    audioHandler.RecordFromDevice(SelectedDevice, selectedChannel);
                }

                audioHandler.AudioDataSubscriber(visualizer.AudioDataHandler);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async void RecordButton_Stop(object sender, RoutedEventArgs e)
        {
            RecordButton.Click -= RecordButton_Stop;
            RecordButton.Click += RecordButton_Start;
            RecordButton.Content = "Record";

            try
            {
                audioFile = await audioHandler.StopRecording();

                string outputFile = RunSaveDialog();
                File.Move(audioFile, outputFile);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            CaptureDevices.IsEnabled = true;
            DeviceChannels.IsEnabled = true;

            visualizer.Clear();
        }

        private void DeviceChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedChannel = DeviceChannels.SelectedIndex;
        }

        #endregion

        private void DeviceInfo_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CaptureDevices.SelectedItem != null)
            {
                MessageBox.Show(audioHandler.GetDeviceCapabilities(SelectedDevice), "Device Information");
            }
        }
    }
}
