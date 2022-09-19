using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YAMP2.ViewModels;
using LibVLCSharp.Avalonia.Unofficial;

namespace YAMP2.Views
{
    public partial class VideoPlayerViewControl : UserControl
    {
        public VideoPlayerViewControlModel viewModel = new VideoPlayerViewControlModel();
        static VideoPlayerViewControl? _this;
        //public static ControlsPanelView? ControlsView = new ControlsPanelView();
        public static ControlsPanelViewControl? ControlsView;

        //public Panel mpContainer;

        public VideoView? _videoViewer;


        public string? videoUrl { get; set; }
        public string? coverUrl { get; set; }
        public string? videoDuration { get; set; }
        public string? videoTitle { get; set; }
        public int videoWidth { get; set; }
        public int videoHeight { get; set; }

        public string videoAspectRatio { get; set; }
        public VideoPlayerViewControl()
        {
            InitializeComponent();

            _this = this;

            DataContext = viewModel;

            _videoViewer = this.Get<VideoView>("VideoViewer");            

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static VideoPlayerViewControl GetInstance()
        {
            return _this;
        }

        public void SetPlayerHandle()
        {
            if (_videoViewer != null && viewModel.MediaPlayer != null)
            {
                _videoViewer.MediaPlayer = viewModel.MediaPlayer;
                _videoViewer.MediaPlayer.Hwnd = _videoViewer.hndl.Handle;

                viewModel.IsStopped = true;
            }

        }


        private void VideoPlayerViewControl_Init()
        {

            ControlsView = ControlsPanelViewControl.GetInstance();

            ControlsView.timeSlider.Maximum = 100.0;
            ControlsView.timeSlider.Minimum = 0.0;
            ControlsView.timeSlider.Value = 0.0;
            ControlsView.volumeSlider.Value = 50.0;

            ControlsView.viewModel.XVolume = 50.0;
            ControlsView.viewModel.XTime = 0.0;            

        }

        public void VideoPlayerViewControl_Play()
        {
            if (null != videoUrl)
            {
                VideoPlayerViewControl_Init();

                ControlsView.viewModel.VideoDurationString = videoDuration;
                ControlsView.viewModel.VideoTitle = videoTitle;

                viewModel.StartPlay(videoUrl, coverUrl);
            }
        }
    }
}
