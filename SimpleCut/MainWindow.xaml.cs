using SimpleCut.Utils;
using SimpleCut.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace SimpleCut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        bool _ConfigIsHealthy = false;

        public DeviceConnectModel DeviceConnectionContext { get; }
        public SwitchPropertiesModel SwitchPropertiesContext { get; }
        public ProgramSelectModel ProgramSelectModelContext { get; }

        private bool propFieldEnable = false;
        public bool PropertieFiledEnabled
        {
            get { return propFieldEnable; }
            set { propFieldEnable = value; OnPropertyChanged(); }
        }

        private bool viewFieldEnable = false;
        public bool ViewSelectFieldEnable
        {
            get { return viewFieldEnable; }
            set { viewFieldEnable = value; OnPropertyChanged(); }
        }

        private bool _expandedConfigView = false;
        public bool ExpandedConfigView
        {
            get { return _expandedConfigView; }
            set { _expandedConfigView = value; OnPropertyChanged(); }
        }

        SwitcherControl _videoMixer;

        Configuration LoadConfig()
        {
            Configuration config;

            try
            {
                config = SeDeSerializer.DeSerializeConfig();
                _ConfigIsHealthy = true;
            }
            catch (Exception)
            {
                ExpandedConfigView = true;
                config = new Configuration();
                config.DefaultIpAddress = "192.168.178.xx";
                config.CrossFadeDuration = 2.0;
                config.Cam1Source = new StringObjectPair<long>("", -1);
                config.Cam1Source = new StringObjectPair<long>("", -1);
                config.NoteBookSource = new StringObjectPair<long>("", -1);
                config.LogoSource = new StringObjectPair<long>("", -1);
            }

            return config;
        }

        private void OnCSaveConfigTrigger()
        {
            Configuration config = new Configuration();

            config.DefaultIpAddress = DeviceConnectionContext.IpAddress;
            //config.CrossFadeDuration = SwitchPropertiesContext.FadeDuration.;
            config.Cam1Source = SwitchPropertiesContext.Cam1Source;
            config.Cam2Source = SwitchPropertiesContext.Cam2Source;
            config.NoteBookSource = SwitchPropertiesContext.NoteBookSource;
            config.LogoSource = SwitchPropertiesContext.LogoSource;

            try
            {
                SeDeSerializer.SerializeConfig(config);
                _ConfigIsHealthy = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Error during saving of configuration.");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _videoMixer = new SwitcherControl(Dispatcher, HighlightNewProgrammView, OnReportProgress);

            var startConfig = LoadConfig();

            var appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var IconPath = Path.Combine(appPath, "Logos");

            List<string> iconNames = new List<string>();
            //var iconPaths = Directory.GetFiles(IconPath, "*.png");
            //foreach (var path in iconPaths)
            //{
            //    iconNames.Add( Path.GetFileName(path) );
            //}

            DeviceConnectionContext = new DeviceConnectModel(startConfig, OnConnect);
            SwitchPropertiesContext = new SwitchPropertiesModel(startConfig, OnCSaveConfigTrigger, iconNames);
            ProgramSelectModelContext = new ProgramSelectModel(OnPresetView);


            if (_ConfigIsHealthy)
            {
                try
                {
                    OnConnect(startConfig.DefaultIpAddress);
                }
                catch
                {
                    MessageBox.Show("Automatc connect whent wrong. Please check set IP and retry.");
                }
            }
        }

        private void OnReportProgress(int percent)
        {
            percent = Math.Min(Math.Max(0, percent), 100); // Bound Value

            ProgramSelectModelContext.TransferProgress = percent;
        }

        void HighlightNewProgrammView(long view, bool keyOnAir)
        {
            if (SwitchPropertiesContext.Cam1Source.value == view)
            {
                if (keyOnAir)
                    ProgramSelectModelContext.ChangeSet(SwitchViews.Cam1N);
                else
                    ProgramSelectModelContext.ChangeSet(SwitchViews.Cam1);
            }
            else if (SwitchPropertiesContext.Cam2Source.value == view)
            {
                if (keyOnAir)
                    ProgramSelectModelContext.ChangeSet(SwitchViews.Cam2N);
                else
                    ProgramSelectModelContext.ChangeSet(SwitchViews.Cam2);
            }
            else if (SwitchPropertiesContext.NoteBookSource.value == view)
            {
                ProgramSelectModelContext.ChangeSet(SwitchViews.Notebook);
            }
            else if (SwitchPropertiesContext.LogoSource.value == view)
            {
                ProgramSelectModelContext.ChangeSet(SwitchViews.Logo);
            }
            else if (view == 0)
            {
                // Todo Black??
            }
            else
            {
                ProgramSelectModelContext.ChangeSet(SwitchViews.None);
            }
        }

        private void OnConnect(string address)
        {
            _videoMixer.ConnectToIp(address);
            DeviceConnectionContext.DeviceName = _videoMixer.SwitcherName;

            SwitchPropertiesContext.Sources.Clear();
            foreach (var item in _videoMixer.ComboBoxItems)
                SwitchPropertiesContext.Sources.Add(item);

            PropertieFiledEnabled = true;
            ViewSelectFieldEnable = true;
        }


        SwitchViews _lastSet = SwitchViews.None;

        private void OnPresetView(SwitchViews set, KeyTransparancy transparancy)
        {
            if (!_videoMixer.IsConnected)
                return;

            var cam1srs = SwitchPropertiesContext.Cam1Source.value;
            var cam2srs = SwitchPropertiesContext.Cam2Source.value;
            var booksrs = SwitchPropertiesContext.NoteBookSource.value;
            var logosrs = SwitchPropertiesContext.LogoSource.value;

            var key = SetKey(set, _lastSet);
            var isTransparent = false;
            if (transparancy == KeyTransparancy.Transparent)
                isTransparent = true;

            switch (set)
            {
                case SwitchViews.None:
                    break;

                case SwitchViews.Cam1:
                    _videoMixer.PresetKey(true, key);
                    _videoMixer.SwitchView(cam1srs, false);
                    break;
                case SwitchViews.Cam2:
                    _videoMixer.PresetKey(true, key);
                    _videoMixer.SwitchView(cam2srs, false);
                    break;
                case SwitchViews.Cam1N:
                    _videoMixer.PresetKey(true, key);
                    _videoMixer.SwitchView(cam1srs, false);
                    break;
                case SwitchViews.Cam2N:
                    _videoMixer.PresetKey(true, key, isTransparent);
                    _videoMixer.SwitchView(cam2srs, false);
                    break;
                case SwitchViews.Notebook:
                    _videoMixer.PresetKey(true, key, isTransparent);
                    _videoMixer.SwitchView(booksrs, false);
                    break;
                case SwitchViews.Blank:
                    _videoMixer.BlankOut(false);
                    break;
                case SwitchViews.Logo:
                    _videoMixer.PresetKey(true, key);
                    _videoMixer.SwitchView(logosrs, false);
                    break;

                default:
                    break;
            }

            _lastSet = set;
        }

        bool SetKey(SwitchViews thisSet, SwitchViews lastSet)
        {
            var act = WithKey[thisSet];
            var last = WithKey[lastSet];

            if ((act == false) && (last == false))
                return false;

            if ((act == true) && (last == false))
                return true;

            if ((act == false) && (last == true))
                return true;

            if ((act == true) && (last == true))
                return false;

            return false;
        }

        Dictionary<SwitchViews, bool> WithKey = new Dictionary<SwitchViews, bool>()
        {
            { SwitchViews.Blank, false },
            { SwitchViews.Cam1, false },
            { SwitchViews.Cam2, false },
            { SwitchViews.Cam1N, true },
            { SwitchViews.Cam2N, true },
            { SwitchViews.Notebook, false },
            { SwitchViews.Logo, false },
            { SwitchViews.None, false },
        };

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Automated PropertyChanged Methode:
        /// Calling Member is determined automatically by CallerMemberName-Property
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void ControlBar_Expanded(object sender, RoutedEventArgs e) => ControlBar.Width = 360;

        private void ControlBar_Collapsed(object sender, RoutedEventArgs e) => ControlBar.Width = 20;
    }
}
