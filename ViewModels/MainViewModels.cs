using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BastonInteligente.ViewModels
{
    public class MainViewModels: INotifyPropertyChanged
    {
        // Variables para el puerto serial y el reloj
        private SerialPort _serialPort;
        private System.Timers.Timer _timer;

        // Propiedades que la Vista (XAML) va a observar
        private string _distanciaLabel = "Iniciando...";
        private Color _colorEstado = Colors.Gray;

        public string DistanciaLabel
        {
            get => _distanciaLabel;
            set { _distanciaLabel = value; OnPropertyChanged(); }
        }

        public Color ColorEstado
        {
            get => _colorEstado;
            set { _colorEstado = value; OnPropertyChanged(); }
        }

        public MainViewModels()
        {
            ConfigurarSerial();
            ConfigurarTimer();
        }

        private void ConfigurarSerial()
        {
            try
            {
                // CAMBIA "COM3" por el puerto real de tu placa
                _serialPort = new SerialPort("COM3", 115200);
                _serialPort.ReadTimeout = 500;
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                DistanciaLabel = "Error de conexión";
                System.Diagnostics.Debug.WriteLine($"Error Serial: {ex.Message}");
            }
        }

        private void ConfigurarTimer()
        {
            // El timer pedirá datos cada 100 milisegundos (10 veces por segundo)
            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    string dato = _serialPort.ReadLine().Trim();

                    if (int.TryParse(dato, out int distancia))
                    {
                        // Actualizamos la interfaz (usamos MainThread porque el Timer corre en otro hilo)
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            DistanciaLabel = $"{distancia} cm";
                            ActualizarColor(distancia);
                        });
                    }
                }
                catch { /* Error de lectura temporal */ }
            }
        }

        private void ActualizarColor(int distancia)
        {
            if (distancia < 30)
                ColorEstado = Colors.Red;
            else if (distancia < 60)
                ColorEstado = Colors.Yellow;
            else
                ColorEstado = Colors.Green;
        }

        // Lógica necesaria para que el Data Binding funcione
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
