using SimpleCut.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleCut.Views
{
    public delegate void SetSwitchView(SwitchViews set, KeyTransparancy key);

    public class ProgramSelectModel : BaseModel
    {
        SetSwitchView _presetView;

        public ViewState ViewState { get; } = new ViewState();

        public RelayCommand ChangePresetCommand { get; }
        public RelayCommand SetOpacityCommand { get; }

        private KeyTransparancy _keyTransparancy = KeyTransparancy.Solid;
        public KeyTransparancy KeyTransparancy
        {
            get { return _keyTransparancy; }
            set { _keyTransparancy = value; OnPropertyChanged(); }
        }

        private int _transferProgress;

        public int TransferProgress
        {
            get { return _transferProgress; }
            set { _transferProgress = value; OnPropertyChanged(); }
        }

        private Visibility _progressVisible = Visibility.Collapsed;

        public Visibility ProgressVisible
        {
            get { return _progressVisible; }
            set { _progressVisible = value; }
        }


        SwitchViews _activeView = SwitchViews.None;

        public ProgramSelectModel()
        {
        }


        public ProgramSelectModel(SetSwitchView presetView)
        {
            _presetView = presetView;

            ChangePresetCommand = new RelayCommand(OnChangePreset);
            SetOpacityCommand = new RelayCommand(OnSetOpacity);
        }

        private void OnSetOpacity(object obj)
        {
            var opacity = (string)obj;

            switch (opacity)
            {
                case "Opaque":
                    KeyTransparancy = KeyTransparancy.Solid;
                    _presetView?.Invoke(_activeView, KeyTransparancy);
                    break;

                case "Transparent":
                    KeyTransparancy = KeyTransparancy.Transparent;
                    _presetView?.Invoke(_activeView, KeyTransparancy);
                    break;

                default:
                    break;
            }
        }

        private void OnChangePreset(object obj)
        {
            ProgressVisible = Visibility.Visible;
            SwitchViews next;
            Enum.TryParse<SwitchViews>((string)obj, out next);
            ViewState.PresetView = next;
            _presetView?.Invoke(next, _keyTransparancy);
            OnPropertyChanged("ViewState");
        }

        public void ChangeSet(SwitchViews view)
        {
            ViewState.SetView = view;
            _activeView = view;
            OnPropertyChanged("ViewState");
            ProgressVisible = Visibility.Collapsed;
        }
    }

    public enum SwitchViews
    {
        None = 0,
        Cam1,
        Cam2,
        Cam1N,
        Cam2N,
        Notebook,
        Blank,
        Logo
    }

    public enum KeyTransparancy
    {
        Solid, 
        Transparent,
    }

    public class ViewState : BaseModel
    {
        private SwitchViews presetView = SwitchViews.None;
        private SwitchViews setView = SwitchViews.None;

        public SwitchViews PresetView
        {
            get => presetView;
            set
            {
                presetView = value;
                OnPropertyChanged();
            }
        }
        public SwitchViews SetView
        {
            get => setView;
            set
            {
                setView = value;
                OnPropertyChanged();
            }
        }
    }


    public class ViewStateToBorderBrushConverter : IValueConverter
    {
        SolidColorBrush _neutralBorder = new SolidColorBrush(Colors.Black);
        SolidColorBrush _presetBorder = new SolidColorBrush(Colors.Green);
        SolidColorBrush _activeBorder = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var setViewName = ((ViewState)value).SetView.ToString();
            var presetViewName = ((ViewState)value).PresetView.ToString();
            var param = (string)parameter;

            if (setViewName == param)
                return _activeBorder;

            if (presetViewName == param)
                return _presetBorder;

            return _neutralBorder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TransparancyValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = ((KeyTransparancy)value);
            
            if (key == KeyTransparancy.Transparent)
                return 0.5;

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
