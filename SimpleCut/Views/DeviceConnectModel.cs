using SimpleCut.Utils;
using System;
using System.Windows;

namespace SimpleCut.Views
{
    public delegate void ConnectEvent(string address);

    public class DeviceConnectModel : BaseModel
    {
        private string _ipAddress;

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(); }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; OnPropertyChanged(); }
        }

        private bool _canConnect = true;
        public bool CanConnect
        {
            get { return _canConnect; }
            set { _canConnect = value; OnPropertyChanged(); }
        }
        
        public RelayCommand ConnectCommand { get; }
        public ConnectEvent BubbleConnect { get; }

        public DeviceConnectModel()
        {
            IpAddress = "192.168.178.xx";
        }

        public DeviceConnectModel(Configuration config, ConnectEvent connect)
        {
            IpAddress = config.DefaultIpAddress;
            ConnectCommand = new RelayCommand(OnConnect);
            BubbleConnect = connect;
        }

        private void OnConnect(object obj)
        {
            try
            {
                var ip = System.Net.IPAddress.Parse(this.IpAddress);
                CanConnect = false;
                BubbleConnect(ip.ToString());
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("The given IP-Address is not valid.");
            }
        }
    }
}
