using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastonInteligente.Services
{
    
    public class SerialService
    {
        private SerialPort _puerto;
        public SerialService(string nombrePuerto)
        {
            _puerto = new SerialPort(nombrePuerto, 115200);
        }
        public string LeerDistancia()
        {
            try
            {
                if (!_puerto.IsOpen) _puerto.Open();
                return _puerto.ReadLine(); // Lee lo que manda el ESP32
            }
            catch { return "Error"; }
        }
    }
}
