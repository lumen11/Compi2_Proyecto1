using System;
using System.Collections.Generic;
using System.Text;

namespace Proyecto1
{
    class Errores
    {

        private string descricpion;
        private int fila;
        private int columna;

        public string Descricpion
        {
            get { return descricpion; }
            set { descricpion = value; }
        }
        

        public int Fila
        {
            get { return fila; }
            set { fila = value; }
        }

        public int Columna
        {
            get { return columna; }
            set { columna = value; }
        }

        public Errores(string d, int f, int c)
        {
            descricpion = d;
            fila = f;
            columna = c;
        }
    }
}
