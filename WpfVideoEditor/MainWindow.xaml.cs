using WpfVideoEditor.Models;
using WpfVideoEditor.Ffmpeg;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Path = System.IO.Path;
using Timer = System.Timers.Timer;

namespace WpfVideoEditor
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "WPF Window is never disposed. We handle it manually in the Closed event.")]
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty CurrentFileProperty = DependencyProperty.Register(nameof(CurrentFile), typeof(FileInfo), typeof(MainWindow), new PropertyMetadata());
        public static readonly DependencyProperty ClipsProperty = DependencyProperty.Register(nameof(Clips), typeof(ClipsCollection), typeof(MainWindow), new PropertyMetadata(new ClipsCollection()));
        public static readonly DependencyProperty TrimMinMsProperty = DependencyProperty.Register(nameof(TrimMinMs), typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));
        public static readonly DependencyProperty TrimMaxMsProperty = DependencyProperty.Register(nameof(TrimMaxMs), typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));
        public static readonly DependencyProperty CurrentPosMsProperty = DependencyProperty.Register(nameof(CurrentPosMs), typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));
        public static readonly DependencyProperty IsPlayerRepeatOnProperty = DependencyProperty.Register(nameof(IsPlayerRepeatOn), typeof(bool), typeof(MainWindow), new PropertyMetadata(defaultValue: true));
        public static readonly DependencyProperty IsPlayerMutedProperty = DependencyProperty.Register(nameof(IsPlayerMuted), typeof(bool), typeof(MainWindow), new PropertyMetadata(defaultValue: false));
        public static readonly DependencyProperty PlayerVolumeProperty = DependencyProperty.Register(nameof(PlayerVolume), typeof(double), typeof(MainWindow), new PropertyMetadata(defaultValue: 0.5));
        public static readonly DependencyProperty SettingContainerFormatProperty = DependencyProperty.Register(nameof(SettingContainerFormat), typeof(VideoFormat), typeof(MainWindow));
        public static readonly DependencyProperty SettingVideoCodecProperty = DependencyProperty.Register(nameof(SettingVideoCodec), typeof(VideoCodec), typeof(MainWindow));
        public static readonly DependencyProperty SettingAudioCodecProperty = DependencyProperty.Register(nameof(SettingAudioCodec), typeof(AudioCodec), typeof(MainWindow));



        public FileInfo CurrentFile { get => (FileInfo)GetValue(CurrentFileProperty); set => SetValue(CurrentFileProperty, value); }
        internal ClipsCollection Clips { get => (ClipsCollection)GetValue(ClipsProperty); set => SetValue(ClipsProperty, value); }
        public double TrimMinMs { get => (double)GetValue(TrimMinMsProperty); set => SetValue(TrimMinMsProperty, value); }
        public double TrimMaxMs { get => (double)GetValue(TrimMaxMsProperty); set => SetValue(TrimMaxMsProperty, value); }
        public double CurrentPosMs { get => (double)GetValue(CurrentPosMsProperty); set => SetValue(CurrentPosMsProperty, value); }
        public bool IsPlayerRepeatOn { get => (bool)GetValue(IsPlayerRepeatOnProperty); set => SetValue(IsPlayerRepeatOnProperty, value); }
        public bool IsPlayerMuted { get => (bool)GetValue(IsPlayerMutedProperty); set => SetValue(IsPlayerMutedProperty, value); }
        public double PlayerVolume { get => (double)GetValue(PlayerVolumeProperty); set => SetValue(PlayerVolumeProperty, value); }
        public VideoFormat SettingContainerFormat { get => (VideoFormat)GetValue(SettingContainerFormatProperty); set => SetValue(SettingContainerFormatProperty, value); }
        public VideoCodec SettingVideoCodec { get => (VideoCodec)GetValue(SettingVideoCodecProperty); set => SetValue(SettingVideoCodecProperty, value); }
        public AudioCodec SettingAudioCodec { get => (AudioCodec)GetValue(SettingAudioCodecProperty); set => SetValue(SettingAudioCodecProperty, value); }

        private bool IsPlaying { get; set; }
        private DirectoryInfo CurrentDir { get; set; }
        private MediaTimeline MediaTimeline;
        private readonly Timer UiUpdateTimer = new Timer(200);

        private MediaClock Clock { get => cMediaElement.Clock; }
        private ClockController MediaController { get => Clock.Controller; }



        private readonly FfmpegInterface Ffmpeg = new FfmpegInterface();

        public MainWindow()
        {
            InitializeComponent();

            cMediaElement.MediaOpened += CMediaElement_MediaOpened;

            if (!File.Exists("ffmpeg.exe"))
            {
                var wantDownloadRes = MessageBox.Show("ffmpeg.exe is required to extract and combine video, but it was not found.\nDo you want to download it now? (This is automated.)\nIf you choose no you will be able to open, view and prepare videos, but not encode them.", "No ffmpeg - download", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (wantDownloadRes == MessageBoxResult.Yes)
                {
                    FfmpegDownloader.Download();
                }
            }

            Ffmpeg.ActiveCountChanged += (sender, e) => Dispatcher.Invoke(() => sStatus.Content = $"Active exports: {Ffmpeg.ActiveCount}");
            Closed += (sender, e) => { if (CurrentFile != null) { Clips.Save(CurrentFile); }; };
            UiUpdateTimer.Elapsed += (sender, e) => UpdateSeekers();

            // TODO: Solve this through bindings now
            //cFrom.ValueChanged += (sender, e) => { if (TimeSpan.FromMilliseconds(cPosition.Minimum) > Clock.CurrentTime.Value) { JumpTo(TimeSpan.FromMilliseconds(cPosition.Minimum)); } };
            // TODO: Solve this through bindings now
            //cTo.ValueChanged += (sender, e) => { if (Clock.CurrentTime.Value > TimeSpan.FromMilliseconds(cPosition.Maximum)) { JumpTo(TimeSpan.FromMilliseconds(TrimMaxMs)); } };
            // TODO: This is currently in conflict with the UpdateLoopThread which auto updates the value according to the current position, but in a separate thread so the value will be outdated because playback went on once the value is changed on the event thread and the change event triggers.
            //cPosition.ValueChanged += (sender, e) => JumpTo(TimeSpan.FromMilliseconds(cPosition.Value));

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                OpenFile(args[1]);
            }
            else if (args.Length > 2)
            {
                throw new ArgumentException("Invalid program arguments. Currently only accepts no or a single filepath parameter.");
            }

            UiUpdateTimer.Start();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            UiUpdateTimer?.Dispose();
            Ffmpeg?.Dispose();
        }


        /// <summary>
        /// Opens file from disk
        /// </summary>
        /// <param name="filePath">file path</param>
        public void OpenFile(string filePath)
        {
            var newPath = new FileInfo(filePath);
            if (newPath?.FullName == CurrentFile?.FullName)
            {
                return;
            }
            if (CurrentFile != null)
            {
                Clips.Save(CurrentFile);
            }

            Console.WriteLine($"Opening file {filePath}");
            MediaTimeline = new MediaTimeline(new Uri(filePath))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            cMediaElement.Clock = MediaTimeline.CreateClock();
            //cFileInfo.FilePath = new Uri(filePath);
            CurrentFile = new FileInfo(filePath);
            sFilename.Content = CurrentFile.Name;
            sFileSize.Content = CurrentFile.LengthAsPrettyString();
            IsPlaying = true;

            if (CurrentDir?.FullName != CurrentFile.Directory?.FullName)
            {
                var dirFiles = GetVideoFiles();
                cFilesList.ItemsSource = dirFiles;
            }
            cFilesList.SelectedValue = CurrentFile.Name;

            UpdateWindowTitle();

            //Clips.Clear();
            //Clips.AddRange();
            Clips = ClipsCollection.LoadFor(new FileInfo(filePath));
            //cClipsList.Clips = Clips.LoadFor(filePath);
        }


        /// <summary>
        /// Set video clip 
        /// </summary>
        /// <param name="clip"></param>
        public void SetTrim(Clip clip = null)
        {
            TrimMinMs = clip?.StartMs ?? 0.0;
            TrimMaxMs = clip?.EndMs ?? cTo.Maximum;

            JumpTo(TimeSpan.FromMilliseconds(TrimMinMs));
        }

        /// <summary>
        /// Get video files list in directory
        /// </summary>
        /// <returns></returns>
        private IOrderedEnumerable<string> GetVideoFiles()
        {
            CurrentDir = CurrentFile.Directory;
            var dirFiles = CurrentDir.GetFiles().Select(x => x.Name).Where(IsVideoFile).OrderBy(x => x);
            return dirFiles;
        }

        /// <summary>
        /// Returns true if specified file is a video file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static bool IsVideoFile(string filename) => filename.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase) || filename.EndsWith(".mkv", StringComparison.InvariantCultureIgnoreCase);


        /// <summary>
        /// Update Main Windows title
        /// </summary>
        private void UpdateWindowTitle() => Title = "WpfVideoEditor" + TitlePostfix;

        private string TitlePostfix { get => CurrentFile != null ? " - " + CurrentFile.Name + " in " + CurrentDir.FullName : string.Empty; }

        private void CMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            cTo.Maximum = cMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            TrimMinMs = 0.0;
            TrimMaxMs = cTo.Maximum;
            CurrentPosMs = 0.0;
        }



        private int CurrentTimeMsPrecise => (int)(Clock.CurrentTime ?? TimeSpan.Zero).TotalMilliseconds;

        /// <summary>
        /// Update seekers
        /// </summary>
        private void UpdateSeekers()
        {
            if (Clock == null)
            {
                return;
            }
            Dispatcher.Invoke(() => SetCurrentTime(CurrentTimeMsPrecise));
            Dispatcher.Invoke(() => { if (cPosition.Value >= TrimMaxMs) { if (IsPlayerRepeatOn) { JumpToStart(); } else { Stop(); } } });
        }

        /// <summary>
        /// Sets current time
        /// </summary>
        /// <param name="ms">value in milliseconds</param>
        private void SetCurrentTime(double ms) => CurrentPosMs = ms;
        private void BtnJumpStart_Click(object sender, RoutedEventArgs e) => JumpToStart();

        private void Play() { MediaController.Resume(); IsPlaying = true; }
        private void Pause() { MediaController.Pause(); IsPlaying = false; }
        private void Stop() { MediaController.Stop(); IsPlaying = false; }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e) { if (IsPlaying) { Pause(); } else { Play(); } }
        private void BtnStop_Click(object sender, RoutedEventArgs e) { Stop(); }

        /// <summary>
        /// Manage media element(s) drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filePaths.Length == 1)
            {
                OpenFile(filePaths[0]);
            }
            else
            {
                throw new NotImplementedException($"Multiple files were dropped ({filePaths.Length}).");
            }
        }


        /// <summary>
        /// Manages Media element(s) drag over
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = false;
        }

        private void BtnSkipForward_Click(object sender, RoutedEventArgs e) => JumpRelative(TimeSpan.FromSeconds(5));
        private void BtnSkipBackward_Click(object sender, RoutedEventArgs e) => JumpRelative(-TimeSpan.FromSeconds(3));
        private void JumpRelative(TimeSpan distance) => JumpTo(cMediaElement.Position + distance);
        private void JumpTo(TimeSpan target) => cMediaElement.Clock.Controller.Seek(TimeSpanValueRangeLimited(target, TimeSpan.FromMilliseconds(TrimMinMs), TimeSpan.FromMilliseconds(TrimMaxMs)), TimeSeekOrigin.BeginTime);
        private static TimeSpan TimeSpanValueRangeLimited(TimeSpan value, TimeSpan min, TimeSpan max)
        {
            return value > max ? max : (value < min ? min : value);
        }
        private void JumpToStart() => MediaController.Seek(TimeSpan.FromMilliseconds(TrimMinMs), TimeSeekOrigin.BeginTime);

        private void CFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cFilesList.SelectedValue != null)
            {
                OpenFile(Path.Combine(CurrentDir.FullName, (string)cFilesList.SelectedValue));
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var Trim = new Clip
            {
                StartMs = (int)cFrom.Value,
                EndMs = (int)cTo.Value,
            };
            ExportTrim(Trim);
        }

        /// <summary>
        /// Exports trimmed clip
        /// </summary>
        /// <param name="Trim"></param>
        private void ExportTrim(Clip Trim)
        {
            Ffmpeg.ExportTrim(CurrentFile, Trim, SettingContainerFormat, SettingVideoCodec, SettingAudioCodec);
        }



        #region Clips collection

        /// <summary>
        /// Plays specified clip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CClipsList_Play(object sender, Controls.ClipEventArgs e) { SetTrim(e.Clip); Play(); }

        /// <summary>
        /// Exports specified clip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CClipsList_Export(object sender, Controls.ClipEventArgs e) => ExportTrim(e.Clip);
        // If new value is after end time, assume we just passed the end time and the user wanted to set the end time as the start time as well (possibly in preparation to change the end time afterwards).
        private void CClipsList_SetBegin(object sender, Controls.ClipEventArgs e) => e.Clip.StartMs = CurrentTimeMsPrecise > e.Clip.EndMs ? e.Clip.EndMs : CurrentTimeMsPrecise;
        // If new value is before start time, expect the playback looped around from the end to the start, and use the end (Trim max value) instead.
        private void CClipsList_SetEnd(object sender, Controls.ClipEventArgs e) => e.Clip.EndMs = CurrentTimeMsPrecise < e.Clip.StartMs ? (IsPlayerRepeatOn ? (int)TrimMaxMs : e.Clip.StartMs) : CurrentTimeMsPrecise;
        private void BtnAddClip_Click(object sender, RoutedEventArgs e) => Clips.Add(new Clip() { StartMs = (int)CurrentPosMs, EndMs = (int)CurrentPosMs, });
        private void BtnAddClip5s_Click(object sender, RoutedEventArgs e) => Clips.Add(new Clip() { StartMs = (int)CurrentPosMs - 5000, EndMs = (int)CurrentPosMs, });
        private void BtnExportAllClips_Click(object sender, RoutedEventArgs e) { foreach (var item in Clips) { ExportTrim(item); } }


        #endregion

    }
}
