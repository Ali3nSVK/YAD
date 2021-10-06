using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using YAD.Audio;
using YAD.Audio.Utils;
using YAD.Visualization;

namespace YAD
{
    public partial class YADWindow : Window
    {
        private int selectedDeviceIndex;

        public AudioEngine AudioHandler { get; private set; }
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
            AudioHandler = new AudioEngine();
            captureDevices = AudioHandler.CaptureDeviceCollection;

            foreach(DeviceContainer cont in captureDevices)
            {
                CaptureDevices.Items.Insert(cont.DeviceNumber, cont.FriendlyName);
            }

            visualizer = new AudioVisualizer(syncContext);
            WaveformGrid.Children.Add(visualizer);

            GainSlider.Value = AudioHandler.Settings.GainLevel * 100;
        }

        #region Recording

        private void StartRecording()
        {
            try
            {
                if (SelectedDevice.ID == YADConstants.LoopbackDevice)
                {
                    AudioHandler.RecordLoopback();
                }
                else
                {
                    AudioHandler.RecordFromDevice(SelectedDevice);
                }

                AudioHandler.AudioDataSubscriber(visualizer.AudioDataHandler);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async void StopRecording()
        {
            try
            {
                if (AudioHandler.Settings.TargetFormat != TargetType.Monitor)
                {
                    string audioFile = await AudioHandler.StopRecording();
                    string outputFile = RunSaveDialog();

                    if (string.IsNullOrWhiteSpace(outputFile))
                    {
                        File.Delete(audioFile);
                    }
                    else
                    {
                        File.Move(audioFile, outputFile);
                    }
                }
                else
                {
                    await AudioHandler.StopRecording();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion

        #region Utility

        private string RunSaveDialog()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "yad_recording_" + DateTime.Now.ToString("yyyyMMddTHHmmss"),
                Filter = AudioHandler.Settings.TargetFormat == TargetType.Wav ? YADConstants.DialogWavFilter : YADConstants.DialogMp3Filter
            };
            bool? result = dlg.ShowDialog();

            return result.HasValue && result.Value ? dlg.FileName : null;
        }

        private void HandleException(Exception ex)
        {
            _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void InitializeDropDowns()
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

        private void UpdateUIState(bool startedRecording)
        {
            if (startedRecording)
            {
                RecordButton.Click -= RecordButton_Start;
                RecordButton.Click += RecordButton_Stop;
                RecordButton.Content = "Stop";
            }
            else
            {
                RecordButton.Click -= RecordButton_Stop;
                RecordButton.Click += RecordButton_Start;
                RecordButton.Content = "Record";
            }

            CaptureDevices.IsEnabled = !startedRecording;
            DeviceChannels.IsEnabled = !startedRecording;
            WavRadio.IsEnabled = !startedRecording;
            Mp3Radio.IsEnabled = !startedRecording;
            MonitorRadio.IsEnabled = !startedRecording;
        }

        #endregion

        #region Event Handlers

        private void CaptureDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeDropDowns();
        }

        private void RecordButton_Start(object sender, RoutedEventArgs e)
        {
            UpdateUIState(true);
            StartRecording();
        }

        private void RecordButton_Stop(object sender, RoutedEventArgs e)
        {
            StopRecording();
            UpdateUIState(false);

            visualizer.Clear();
        }

        private void DeviceChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AudioHandler.Settings.Channel = DeviceChannels.SelectedIndex;
        }

        private void DeviceInfo_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CaptureDevices.SelectedItem != null)
            {
                MessageBox.Show(AudioHandler.GetDeviceCapabilities(SelectedDevice), "Device Information");
            }
        }

        private void GainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AudioHandler != null)
            {
                AudioHandler.Settings.GainLevel = (float)GainSlider.Value / 100;
            }
        }

        #endregion
    }
}
