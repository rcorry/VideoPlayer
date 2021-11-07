using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;

namespace WpfTutorialSamples.Audio_and_Video
{
	public partial class AudioVideoPlayerCompleteSample : Window
	{
		private bool mediaPlayerIsPlaying = false;
		private bool userIsDraggingSlider = false;
		private List<double[]> SkipTimes = new List<double[]>();
		private string SkipFile = "";
		private Boolean isPlaying = false;

		public AudioVideoPlayerCompleteSample()
		{
            InitializeComponent();

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += timer_Tick;
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
			{
				sliProgress.Minimum = 0;
				sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
				sliProgress.Value = mePlayer.Position.TotalSeconds;
				ApplyTimeFilters();
				if(mePlayer.Position.TotalSeconds >= mePlayer.NaturalDuration.TimeSpan.TotalSeconds)
                {
					mePlayer.Position = TimeSpan.FromSeconds(0);
                }
			}
		}

		private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.mp4)|*.mp3;*.mpg;*.mpeg;*.mp4|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true) {
				mePlayer.Source = new Uri(openFileDialog.FileName);
		//		mePlayer.Pause();
			}
		}

		private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = mediaPlayerIsPlaying;
		}

		private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			mePlayer.Stop();
			mediaPlayerIsPlaying = false;
		}

		private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
		{
			userIsDraggingSlider = true;
		}

		private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			userIsDraggingSlider = false;
			mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
		}

		private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
		}

		private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
		}

        private void mePlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
			MessageBox.Show(e.ErrorException.Message);
        }

        private void OpenSkipFile(object sender, RoutedEventArgs e)
        {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Media files (*.txt)|*.txt|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{
				SkipFile = openFileDialog.FileName;
				BuildSkipList();
			}
        }

		private void BuildSkipList()
        {
			if (File.Exists(SkipFile)){
				SkipTimes.Clear();
				StreamReader file = new StreamReader(SkipFile);
				string line;

				while ((line = file.ReadLine()) != null)
				{
					string[] tmp = line.Split(",");
					double[] add = new double[] { 0, 0 };
					double.TryParse(tmp[0], out add[0]);
					double.TryParse(tmp[1], out add[1]);
					SkipTimes.Add(add);
				}
				file.Close();
			}
			AddTimesToMenu();
        }
        private void AddNewSkip(object sender, RoutedEventArgs e)
        {
			
			string newSkip = "";
			//checks for blank inputs
			if(StartSkip.Text != "" && StopSkip.Text != "")
            {
				newSkip = String.Concat(StartSkip.Text, ",", StopSkip.Text, "\n");	
            }
			if (newSkip == "")
				return;

            if (File.Exists(SkipFile))
            {
				File.AppendAllText(SkipFile, newSkip);
            }
			StartSkip.Text = "";
			StopSkip.Text = "";
			BuildSkipList();
			AddTimesToMenu();
        }

		private void ApplyTimeFilters()
        {

			for (int i=0; i<SkipTimes.Count; i++)
            {
				double start = SkipTimes[i][0];
				double stop = SkipTimes[i][1];
				if (mePlayer.Position.TotalSeconds >= start &&
					mePlayer.Position.TotalSeconds <= stop)
                {
					mePlayer.Position = TimeSpan.FromSeconds(stop);
                }
            }
        }

		private void AddTimesToMenu()
        {
			SkipMenu.Items.Clear();
			if (File.Exists(SkipFile)){
				StreamReader file = new StreamReader(SkipFile);
				string line;
				int i = 0;
				while ((line = file.ReadLine()) != null)
				{
					SkipMenu.Items.Insert(i, line);
					i++;
				}
				file.Close();
			}
        }

        private void SkipMenu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
        private void DelSkip(object sender, RoutedEventArgs e)
        {
			if (SkipMenu.Text != "Skip Times:")
            {
				string tmp = SkipMenu.SelectedItem.ToString();
				string[] skip = tmp.Split(",");
				for (int i=0; i<SkipTimes.Count; i++)
                {
					if (SkipTimes[i][0].ToString() == skip[0] && SkipTimes[i][1].ToString() == skip[1])
                    {
						SkipTimes.RemoveAt(i);
                    }

                }
				SkipMenu.Items.Remove(SkipMenu.SelectedItem);
				UpdateSkipFile();
				SkipMenu.Text = "Skip Times:";
            }

        }

		private void UpdateSkipFile()
        {
			if (File.Exists(SkipFile))
            {
				File.WriteAllText(SkipFile, "");	
				for (int i = 0; i<SkipTimes.Count; i++)
                {
					string time = String.Concat(SkipTimes[i][0].ToString(), ",", SkipTimes[i][1].ToString(), "\n");
					File.AppendAllText(SkipFile, time);
                }
            }
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                mePlayer.Pause();
                isPlaying = false;
				PlayButtonText.Text = "Play";
            }
            else
            {
                mePlayer.Play();
                isPlaying = true;
                mediaPlayerIsPlaying = true;
				PlayButtonText.Text = "Pause";
            }
        }

        private void PlayButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			mePlayer.Stop();
			isPlaying = false;
			mediaPlayerIsPlaying = false;
			PlayButtonText.Text = "Pause";
        }
        private void ToggleSlowMo(object sender, RoutedEventArgs e)
        {
            mePlayer.Position = TimeSpan.FromSeconds(mePlayer.Position.TotalSeconds + 10);
        }
    }
}

	
