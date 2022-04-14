using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAMP2.Utils;
using YAMP2.Views;

namespace YAMP2.ViewModels
{
    public class ControlsPanelViewControlModel: ViewModelBase
    {
        // Properties
        private bool _isPlaying;

        private bool _isLoading;
        private double _volume;
        private double _duration;
        private double _instantTimeValue;

        private string? _title;
        private string? _durationString;

        public ControlsPanelViewControlModel()
        {
            _title = "No media selected";
            _durationString = "--:--:--:--";
        }

        #region PROPERTIES
        public string VideoTitle
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string VideoDurationString
        {
            get => _durationString;
            set => this.RaiseAndSetIfChanged(ref _durationString, value);
        }

        public double VideoDuration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public double XTime
        {
            get => _instantTimeValue;
            set => this.RaiseAndSetIfChanged(ref _instantTimeValue, value);
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
        }

        public double XVolume
        {
            get => _volume;
            set => this.RaiseAndSetIfChanged(ref _volume, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        #endregion

        public void Rewind()
        {
            try
            {
                var tmp = ControlsPanelViewControl.GetInstance();
                var mediaPlayer = tmp.playerViewModel.MediaPlayer;
                if (mediaPlayer == null)
                {
                    return;
                }

                mediaPlayer.Time -= Settings.SEEK_OFFSET;
            }
            catch { }
        }

        public void Seek()
        {
            try
            {
                var tmp = ControlsPanelViewControl.GetInstance();
                var mediaPlayer = tmp.playerViewModel.MediaPlayer;
                if (mediaPlayer == null)
                {
                    return;
                }

                mediaPlayer.Time += Settings.SEEK_OFFSET;
            }
            catch { }

        }
        public void Play()
        {
            try
            {
                IsPlaying = true;

                var tmp = ControlsPanelViewControl.GetInstance();
                tmp.playerViewModel.IsStopped = false;
                tmp.playerViewModel.MediaPlayer.Play();

            }
            catch { }
        }

        public void ChangeTime(long time)
        {
            try
            {
                var tmp = ControlsPanelViewControl.GetInstance();
                var mediaPlayer = tmp.playerViewModel.MediaPlayer;
                if (mediaPlayer == null)
                {
                    return;
                }

                //XTime = time / 1000;
                mediaPlayer.Time = time;
            }
            catch { }

        }

        public void Pause()
        {
            try
            {
                IsPlaying = false;
                var tmp = ControlsPanelViewControl.GetInstance();
                tmp.playerViewModel.MediaPlayer.Pause();
            }
            catch { }
        }

        public void Stop()
        {
            try
            {
                IsPlaying = false;

                XTime = 0.0;
                var tmp = ControlsPanelViewControl.GetInstance();
                tmp.playerViewModel.IsStopped = true;
                tmp.playerViewModel.MediaPlayer.Stop();

            }
            catch { }
        }
    }
}
