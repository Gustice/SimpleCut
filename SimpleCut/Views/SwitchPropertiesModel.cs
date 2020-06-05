using SimpleCut.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleCut.Views
{
    public delegate void TriggerAction();

    public class SwitchPropertiesModel : BaseModel
    {
        public ObservableCollection<StringObjectPair<long>> Sources { get; } = new ObservableCollection<StringObjectPair<long>>();
        public ObservableCollection<string> Logos { get; } = new ObservableCollection<string>();

        private StringObjectPair<long> _cam1Source;
        public StringObjectPair<long> Cam1Source
        {
            get { return _cam1Source; }
            set { _cam1Source = value; OnPropertyChanged(); }
        }

        private StringObjectPair<long> _cam2Source;
        public StringObjectPair<long> Cam2Source
        {
            get { return _cam2Source; }
            set { _cam2Source = value; OnPropertyChanged(); }
        }

        private StringObjectPair<long> _notebookSource;
        public StringObjectPair<long> NoteBookSource
        {
            get { return _notebookSource; }
            set { _notebookSource = value; OnPropertyChanged(); }
        }

        private StringObjectPair<long> _LogoSource;
        public StringObjectPair<long> LogoSource
        {
            get { return _LogoSource; }
            set { _LogoSource = value; OnPropertyChanged(); }
        }


        public RelayCommand TriggerConfigCommand { get; }

        private string _fadeDuration;
        public string FadeDuration
        {
            get { return _fadeDuration; }
            set { _fadeDuration = value; OnPropertyChanged(); }
        }

        public TriggerAction _trigger { get; }

        public SwitchPropertiesModel()
        {

        }

        public SwitchPropertiesModel(Configuration config, TriggerAction saveTrigger, List<string> logos)
        {
            _trigger = saveTrigger;
            TriggerConfigCommand = new RelayCommand(OnTriggerConfig);
            FadeDuration = config.CrossFadeDuration.ToString("F2");

            if (CheckProperty(config.Cam1Source))
                Cam1Source = config.Cam1Source;

            if (CheckProperty(config.Cam2Source))
                Cam2Source = config.Cam2Source;

            if (CheckProperty(config.NoteBookSource))
                NoteBookSource = config.NoteBookSource;

            if (CheckProperty(config.LogoSource))
                LogoSource = config.LogoSource;

            foreach (string item in logos)
            {
                Logos.Add(item);
            }
        }

        bool CheckProperty(StringObjectPair<long> prop)
        {
            if (string.IsNullOrEmpty(prop.name))
                return false;

            if (prop.value < 0)
                return false;

            return true;
        }

        private void OnTriggerConfig(object obj)
        {
            _trigger?.Invoke();
        }
    }
}
