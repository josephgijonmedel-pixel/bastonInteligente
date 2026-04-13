using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastonInteligente.Models
{
    public class Alerta
    {
        public string Distancia { get; set; } = "0";
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
