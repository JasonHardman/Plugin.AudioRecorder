using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.AudioRecorder;

namespace AudioRecord.Forms
{
	public partial class MainPage : ContentPage
	{
		AudioRecorderService recorder;
		AudioPlayer player;
		bool _isPlaying;
		bool _playerIsPaused;
		bool _recorderIsPaused;

		public MainPage ()
		{
			InitializeComponent ();

			recorder = new AudioRecorderService
			{
				StopRecordingAfterTimeout = true,
				TotalAudioTimeout = TimeSpan.FromSeconds (15),
				AudioSilenceTimeout = TimeSpan.FromSeconds (2)
			};

			player = new AudioPlayer ();
			player.FinishedPlaying += Player_FinishedPlaying;
		}

		async void Record_Clicked (object sender, EventArgs e)
		{
			StopButton.IsEnabled = true;
			PlayButton.IsEnabled = false;
			if (_recorderIsPaused)
			{
				_recorderIsPaused = false;
				await recorder.StartRecording();
			}
			else if (recorder.IsRecording)
			{
				_recorderIsPaused = true;
				recorder.PauseRecording();
			}
			else
			{
				await RecordAudio();
				PlayButton.IsEnabled = true;
			}

			UpdateRecordButtonText();
		}

		async Task RecordAudio ()
		{
			try
			{
				if (!recorder.IsRecording) //Record button clicked
				{
					recorder.StopRecordingOnSilence = TimeoutSwitch.IsToggled;

					//start recording audio
					var audioRecordTask = await recorder.StartRecording ();
					UpdateRecordButtonText();
					await audioRecordTask;
				}
				else
				{
					recorder.PauseRecording();
				}
			}
			catch (Exception ex)
			{
				//blow up the app!
				throw ex;
			}
		}

		void Play_Clicked (object sender, EventArgs e)
		{
			StopButton.IsEnabled = true;

			if (_playerIsPaused)
			{
				//resume
				_playerIsPaused = false;
				player.Play();
			}
			else if (_isPlaying)
			{
				player.Pause();
				_playerIsPaused = true;
			}
			else
			{
				PlayAudio();
			}

			_isPlaying = true;
			UpdatePlayButtonText();
		}

		void PlayAudio ()
		{
			try
			{
				var filePath = recorder.GetAudioFilePath ();

				if (filePath != null)
				{
					RecordButton.IsEnabled = false;

					player.Play (filePath);
				}
			}
			catch (Exception ex)
			{
				//blow up the app!
				throw ex;
			}
		}

		void Player_FinishedPlaying (object sender, EventArgs e)
		{
			PlayButton.IsEnabled = true;
			RecordButton.IsEnabled = true;
			StopButton.IsEnabled = false;
			_isPlaying = false;
			_playerIsPaused = false;
			UpdatePlayButtonText();
		}

		private async void Stop_Clicked(object sender, EventArgs e)
		{
			if (recorder.IsRecording)
			{
				RecordButton.IsEnabled = false;

				//stop the recording...
				await recorder.StopRecording();

				_recorderIsPaused = false;
				RecordButton.IsEnabled = true;
				PlayButton.IsEnabled = true;
			}
			else
			{
				player.Pause();
				_playerIsPaused = false;
				_isPlaying = false;
				UpdatePlayButtonText();
			}

			RecordButton.IsEnabled = true;			
			StopButton.IsEnabled = false;
		}

		private void UpdatePlayButtonText()
		{
			PlayButton.Text = _isPlaying &&  !_playerIsPaused ? "Pause" : "Play";
		}

		private void UpdateRecordButtonText()
		{
			RecordButton.Text = recorder.IsRecording && !_recorderIsPaused ? "Pause" : "Record";
		}
	}
}
