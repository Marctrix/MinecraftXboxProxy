using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MinecraftXboxProxy.WpfApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static string AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinecraftXboxProxy", "appstate.json");

        private MinecraftXboxProxyService _minecraftXboxProxy = new MinecraftXboxProxyService();

        public MainViewModel()
        {
            RestoreViewModelState();

            Start = new RelayCommand(async o => await StartProxy(), o => CanStartProxy());
            Stop = new RelayCommand(async o => await StopProxy(), o => CanStopProxy());
            PropertyChanged += (s, e) => SaveViewModelState();
        }

        private bool _isRunning;
        [JsonIgnore]
        public Boolean IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _xboxHost;
        public string XboxHost
        {
            get => _xboxHost;
            set
            {
                if (_xboxHost != value)
                {
                    _xboxHost = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _minecraftServerHost;
        public string MinecraftServerHost
        {
            get => _minecraftServerHost;
            set
            {
                if (_minecraftServerHost != value)
                {
                    _minecraftServerHost = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _gamePort = 19132;
        public int GamePort
        {
            get => _gamePort;
            set
            {
                if (_gamePort != value)
                {
                    _gamePort = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public ICommand Start { get; set; }
        [JsonIgnore]
        public ICommand Stop { get; set; }

        private bool CanStartProxy()
        {
            return !IsRunning;
        }

        private Task StartProxy()
        {
            return Task.Factory.StartNew(() => {
                IsRunning = true;

                var xBoxIp = IPAdressFromHostname(XboxHost);
                var serverIp = IPAdressFromHostname(MinecraftServerHost);

                _minecraftXboxProxy.Start(xBoxIp, serverIp, GamePort);
            });
        }

        private bool CanStopProxy()
        {
            return IsRunning;
        }

        private Task StopProxy()
        {
            return Task.Factory.StartNew(() =>
            {
                _minecraftXboxProxy.Stop();
                IsRunning = false;
            });
        }

        private void SaveViewModelState()
        {
            var viewModelJson = JsonConvert.SerializeObject(this);
            var folder = Path.GetDirectoryName(AppData);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(AppData, viewModelJson, Encoding.UTF8);
        }

        private void RestoreViewModelState()
        {
            if (!File.Exists(AppData))
                return;

            var viewModelJson = File.ReadAllText(AppData);
            JsonConvert.PopulateObject(viewModelJson, this);
        }

        IPAddress IPAdressFromHostname(string hostname)
        {
            return Dns.GetHostEntry(hostname).AddressList.First(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
