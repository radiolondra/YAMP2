using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using YAMP2.Utils;
using YAMP2.Views;

namespace YAMP2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _mediaFile;
        private bool _isLoading;

        string Output = String.Empty;
        StringBuilder outputString = new();
        double probedDuration = 0.0;
        double probedWidth = 0.0;
        double probedHeight = 0.0;
        string probedAspectRatio = String.Empty;

        string tempMediaFile { get; set; }

        public MainWindowViewModel()
        {
            _mediaFile = String.Empty;
            IsLoading = false;
            UpdateDl();
        }

        /// <summary>
        /// Updates yt-dlp
        /// </summary>
        private void UpdateDl()
        {
            Process dlProcess = new Process();
            dlProcess.StartInfo.CreateNoWindow = true;
            dlProcess.StartInfo.UseShellExecute = false;
            dlProcess.StartInfo.FileName = Settings.BackendYtDlpPath;
            dlProcess.StartInfo.ArgumentList.Clear();

            try
            {
                dlProcess.StartInfo.ArgumentList.Add($"-U");
                dlProcess.Start();
            }
            catch (Exception ex)
            {

            }
        }

        public string MediaFile
        {
            get => _mediaFile;
            set => this.RaiseAndSetIfChanged(ref _mediaFile, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private async void SelectFile()
        {
            var dlg = new OpenFileDialog();

            dlg.Directory = String.IsNullOrWhiteSpace(MediaFile) || MediaFile.StartsWith("http") ? Utilities.ApplicationFolder() : Path.GetDirectoryName(MediaFile);
            //dlg.Directory = @"C:\Users\Robi\AppData\Local\WeTubeDownloads";

            dlg.Title = "Media file selection";

            var result = await dlg.ShowAsync(MainWindow.GetInstance());
            if (result != null)
            {
                if (!string.IsNullOrWhiteSpace(result[0]))
                {
                    MediaFile = result[0];
                }
            }
        }

        private void ProbeProcess_Exited(object? sender, EventArgs e)
        {
            var p = sender as Process;
            probedWidth = 0.0;
            probedHeight = 0.0;
            probedDuration = 0.0;
            probedAspectRatio = String.Empty;

            if (null != p)
            {
                p.CancelErrorRead();
                p.CancelOutputRead();

                var obj = JsonConvert.DeserializeObject<MovieData>(Output);

                if (null != obj)
                {
                    if (obj.Format != null)
                    {
                        probedDuration = double.Parse(obj.Format.Duration, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    if (obj.Streams != null)
                    {
                        foreach (var stream in obj.Streams)
                        {
                            if (stream.CodecType.Contains("video"))
                            {
                                string? tmpAR;

                                var tmpW = double.Parse(stream.Width, System.Globalization.CultureInfo.InvariantCulture);
                                var tmpH = double.Parse(stream.Height, System.Globalization.CultureInfo.InvariantCulture);

                                try
                                {
                                    tmpAR = stream.AspectRatio.Replace("\"", "");
                                }
                                catch { tmpAR = String.Empty; }

                                if (probedWidth < tmpW)
                                {
                                    probedWidth = tmpW;
                                    probedHeight = tmpH;
                                    if (String.IsNullOrWhiteSpace(tmpAR))
                                    {
                                        probedAspectRatio = $"{probedWidth}:{probedHeight}";
                                    }
                                    else
                                    {
                                        probedAspectRatio = tmpAR;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void EphemeralProcess_Exited(object? sender, EventArgs e)
        {
            var p = sender as Process;
            if (null != p)
            {
                p.CancelErrorRead();
                p.CancelOutputRead();
            }
            if (!String.IsNullOrWhiteSpace(Output) && Output.StartsWith("http"))
            {
                tempMediaFile = Output.Replace("\n", "").Replace("\r", "");
            }
            else
            {
                // Errors happened
                tempMediaFile = String.Empty;
            }
        }

        private void ProcessOutputHandler(object? sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                outputString.Append(outLine.Data);
                outputString.Append(Environment.NewLine);
                Output = outputString.ToString();
            }
        }

        private void EphemeralOutputHandler(object? sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                outputString.Append(outLine.Data);
                outputString.Append(Environment.NewLine);
                Output = outputString.ToString();
            }
        }

        public void CallOpenPlayer()
        {
            Thread thread = new Thread(() => OpenPlayer());
            thread.Start();
        }

        public async void OpenPlayer()
        {
            await Dispatcher.UIThread.InvokeAsync(() => { IsLoading = true; }).ConfigureAwait(false);

            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string? assemblyPath = Path.GetDirectoryName(assembly.Location);

            tempMediaFile = MediaFile;


            if (!String.IsNullOrEmpty(assemblyPath))
            {

                if (MediaFile.ToLower().StartsWith("http"))
                {
                    // try to get the ephemeral link.
                    var eProcess = new Process();
                    eProcess.StartInfo.CreateNoWindow = true;
                    eProcess.StartInfo.UseShellExecute = false;
                    eProcess.StartInfo.RedirectStandardError = true;
                    eProcess.StartInfo.RedirectStandardOutput = true;
                    eProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    eProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    eProcess.EnableRaisingEvents = true;

                    eProcess.ErrorDataReceived += EphemeralOutputHandler;
                    eProcess.OutputDataReceived += EphemeralOutputHandler;
                    eProcess.Exited += EphemeralProcess_Exited;

                    outputString.Clear();
                    Output = String.Empty;

                    eProcess.StartInfo.FileName = Settings.BackendYtDlpPath;
                    eProcess.StartInfo.ArgumentList.Clear();
                    // get best video and audio up to 4K or best available
                    // if yt-dlp returns separate video and audio ephemeral urls, a new ffmpeg process will be used to mux
                    eProcess.StartInfo.Arguments = $"-s -f \"best[width<7680]/b\" --no-check-certificates --geo-bypass --print urls \"{tempMediaFile}\"";

                    try
                    {
                        eProcess.Start();
                        eProcess.BeginErrorReadLine();
                        eProcess.BeginOutputReadLine();
                        eProcess.WaitForExit();
                    }
                    catch { }

                }


                outputString.Clear();
                Output = String.Empty;

                if (!String.IsNullOrWhiteSpace(tempMediaFile))
                {

                    var pProcess = new Process();
                    pProcess.StartInfo.CreateNoWindow = true;
                    pProcess.StartInfo.UseShellExecute = false;
                    pProcess.StartInfo.RedirectStandardError = true;
                    pProcess.StartInfo.RedirectStandardOutput = true;
                    pProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    pProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    pProcess.EnableRaisingEvents = true;

                    pProcess.ErrorDataReceived += ProcessOutputHandler;
                    pProcess.OutputDataReceived += ProcessOutputHandler;
                    pProcess.Exited += ProbeProcess_Exited;

                    pProcess.StartInfo.FileName = Path.Combine(Settings.FfmpegPath, "ffprobe.exe");
                    pProcess.StartInfo.ArgumentList.Clear();
                    pProcess.StartInfo.Arguments = $"-loglevel quiet -print_format json -show_format -show_streams \"{tempMediaFile}\"";

                    try
                    {
                        pProcess.Start();
                        pProcess.BeginErrorReadLine();
                        pProcess.BeginOutputReadLine();
                        pProcess.WaitForExit();
                    }
                    catch { }
                }

            }

            if (!String.IsNullOrWhiteSpace(tempMediaFile))
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    IsLoading = false;

                    var playerView = VideoPlayerViewControl.GetInstance();

                    playerView.videoUrl = tempMediaFile;
                    playerView.coverUrl = String.Empty;
                    TimeSpan dur = TimeSpan.FromSeconds(probedDuration);
                    playerView.videoDuration = dur.ToString(@"hh\:mm\:ss\:fff");
                    playerView.videoTitle = MediaFile.StartsWith("http") ? MediaFile : Path.GetFileNameWithoutExtension(MediaFile);
                    playerView.videoWidth = Convert.ToInt32(probedWidth);
                    playerView.videoHeight = Convert.ToInt32(probedHeight);
                    playerView.videoAspectRatio = probedAspectRatio;

                    
                    playerView.VideoPlayerViewControl_Play();

                }).ConfigureAwait(false);
            }
            else
            {
                // Some errors happened
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsLoading = false;
                    MediaFile = "ERROR! Actually unable to play this video. Please report the used link to the support team.";
                }).ConfigureAwait(false);
            }
        }
    }

    public class MovieData
    {
        [JsonProperty("streams")]
        public MovieStream[] Streams { get; set; }
        public MovieFormat Format { get; set; }
    }

    public class MovieStream
    {
        [JsonProperty("codec_type")]
        public string CodecType { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("display_aspect_ratio")]
        public string? AspectRatio { get; set; }
    }

    public class MovieFormat
    {
        [JsonProperty("duration")]
        public string Duration { get; set; }
    }
}
