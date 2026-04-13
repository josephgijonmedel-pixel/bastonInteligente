using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BastonInteligente.ViewModels
{
    public partial class MainViewModels : INotifyPropertyChanged
    {
        private BluetoothClient _btClient;
        private Stream _btStream;
        private CancellationTokenSource _cts;

        // Variables para el contador
        private int _totalAlertas = 0;
        private bool _objetoDetectadoPreviamente = false;

        private string _distanciaLabel = "0"; // Aquí mostraremos el número de veces
        public string DistanciaLabel
        {
            get => _distanciaLabel;
            set { _distanciaLabel = value; OnPropertyChanged(); }
        }

        private Color _colorEstado = Colors.Gray;
        public Color ColorEstado
        {
            get => _colorEstado;
            set { _colorEstado = value; OnPropertyChanged(); }
        }

        public MainViewModels()
        {
            _ = ConectarBastonAsync();
        }

        public async Task ConectarBastonAsync()
        {
            try
            {
                // Limpiamos cualquier intento previo
                if (_btClient != null) { _btClient.Close(); _btClient = null; }

                _btClient = new BluetoothClient();
                DistanciaLabel = "0";
                ColorEstado = Colors.Orange; // Intentando conectar...

                // Esperamos un segundo para que el hardware del cel reaccione
                await Task.Delay(1000);

                var devices = _btClient.PairedDevices;
                var device = devices.FirstOrDefault(d =>
                    d.DeviceName.Contains("Baston", StringComparison.OrdinalIgnoreCase));

                if (device != null)
                {
                    // Intentamos conectar en un hilo separado para no congelar la pantalla
                    await Task.Run(() =>
                    {
                        // Usamos el Standard Serial Port Service ID
                        _btClient.Connect(device.DeviceAddress, BluetoothService.SerialPort);
                    });

                    if (_btClient.Connected)
                    {
                        _btStream = _btClient.GetStream();

                        MainThread.BeginInvokeOnMainThread(() => {
                            ColorEstado = Colors.Green; // ¡POR FIN VERDE!
                        });

                        _cts = new CancellationTokenSource();
                        _ = LeerDatosAsync(_cts.Token);
                    }
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        DistanciaLabel = "NO VINCULADO";
                        ColorEstado = Colors.Gray;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR REAL: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() => {
                    ColorEstado = Colors.Red;
                    // Mostramos los primeros 15 caracteres del error para saber qué pasa
                    DistanciaLabel = "ERR";
                });
            }
        }

        private async Task LeerDatosAsync(CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            string datosAcumulados = ""; // Para guardar fragmentos de texto

            while (!token.IsCancellationRequested && _btClient.Connected)
            {
                try
                {
                    if (_btStream.CanRead)
                    {
                        int bytesRead = await _btStream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (bytesRead > 0)
                        {
                            // 1. Convertimos lo recibido a texto y lo sumamos a lo anterior
                            datosAcumulados += Encoding.ASCII.GetString(buffer, 0, bytesRead);

                            // 2. Si el texto contiene un salto de línea, significa que llegó un número completo
                            if (datosAcumulados.Contains("\n"))
                            {
                                // Separamos por líneas y tomamos la última línea completa
                                string[] lineas = datosAcumulados.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                foreach (var linea in lineas)
                                {
                                    // Limpiamos la línea para dejar solo dígitos
                                    string soloNumeros = new string(linea.Where(char.IsDigit).ToArray());

                                    if (int.TryParse(soloNumeros, out int cms))
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            // LÓGICA DEL CONTADOR
                                            if (cms <= 100 && !_objetoDetectadoPreviamente)
                                            {
                                                _totalAlertas++;
                                                DistanciaLabel = _totalAlertas.ToString();
                                                _objetoDetectadoPreviamente = true;
                                                ColorEstado = Colors.Red;
                                            }
                                            else if (cms > 100)
                                            {
                                                _objetoDetectadoPreviamente = false;
                                                ColorEstado = Colors.Green;
                                            }
                                        });
                                    }
                                }
                                // Limpiamos el acumulador para la siguiente lectura
                                datosAcumulados = "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error de conexión: {ex.Message}");
                    MainThread.BeginInvokeOnMainThread(() => {
                        ColorEstado = Colors.Red; // Aquí es donde se pone rojo
                        DistanciaLabel = "ERR";
                    });
                }
                await Task.Delay(50);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}