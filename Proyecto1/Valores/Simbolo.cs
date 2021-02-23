using System;
using System.Collections.Generic;
using System.Text;

namespace Proyecto1.Valores
{
    class Simbolo
    {
        public enum Tipos
        {
            STRING,
            INT,
            DOUBLE,
            REAL,
            BOOL,
            OBJETO,
            FUNCTION,
            PROCEDURE
        }

        public string identificador { get; set; }
        public object valor { get; set; }
        public Tipos tipo { get; set; }
        public int linea { get; set; }
        public int columna { get; set; }

    }
}
