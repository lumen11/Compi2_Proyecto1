using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Irony.Ast;
using Irony.Parsing;
using Proyecto1.ast;
using Proyecto1.Interfaces;
using Proyecto1.Reportes;
using Proyecto1.Valores;

namespace Proyecto1.analizador
{
    class Sintactico
    {

        private string salida = ">> ";
        private ParseTreeNode raiz = null;
        private List<Errores> listaErrores { get; set; }

        private static Sintactico _objSintactico = new Sintactico();

        public static Sintactico ObjSintactico
        {
            get
            {
                return _objSintactico;
            }
        }

        public Sintactico()
        {
            listaErrores = new List<Errores>();
        }

        public void addError(string descripcion, int linea, int columna)
        {
            Errores e = new Errores(descripcion, linea, columna);
            this.listaErrores.Add(e);
        }

        //Metodos get y set de los atributos
        public ParseTreeNode getRaiz()
        {
            return raiz;
        }

        public String getSalida()
        {
            return salida;
        }

        public void setSalida(string texto)
        {
            salida = salida + texto;
        }

        public void limpiarSalida()
        {
            salida = ">> ";
        }

        public bool hayErrores()
        {
            return listaErrores.Count > 0 ? true: false;
        }

        //Metodo para analizar la entrada
        public void analizar(String cadena)
        {
            Gramatica gramatica = new Gramatica();
            LanguageData lenguaje = new LanguageData(gramatica);
            Parser parser = new Parser(lenguaje);
            ParseTree arbol = parser.Parse(cadena);            
            raiz = arbol.Root;            
            
            //Revisar si hubo problemas en la gramatica
            if(arbol == null)
            {
                MessageBox.Show("Hay errores con la entrada");
                return;
            }

            //TABLA DE ERRORES
            Errores error;
            for (int i = 0; i < arbol.ParserMessages.Count(); i++)
            {
                error = new Errores("Sintactico", arbol.ParserMessages.ElementAt(i).Location.Line, arbol.ParserMessages.ElementAt(i).Location.Column);
                listaErrores.Add(error);
            }

            if(!hayErrores())
            {                               
                AST ast = new AST(inicio_bloques(raiz.ChildNodes.ElementAt(0)));
                Ambito ambito = new Ambito(null);
                foreach(Instruccion ins in ast.Instrucciones)
                {
                    ins.ejecutar(ambito, ast);
                }
               
            }                                                
        }

        public LinkedList<Instruccion> inicio_bloques(ParseTreeNode nodo)
        {
            //inicio_bloques.Rule = PROGRAM + ID + ";" + bloque_principal
            if (nodo.ChildNodes.Count == 3)
            {
                LinkedList<Instruccion> lista = bloque_principal(nodo.ChildNodes.ElementAt(2));                
                return lista;
            }
            //inicio_bloques.Rule = PROGRAM + ID + ";" + bloques + bloque_principal;
            else
            {
                LinkedList<Instruccion> lista = bloque_principal(nodo.ChildNodes.ElementAt(2));
                return lista;
            }
        }

        public LinkedList<Instruccion> bloque_principal(ParseTreeNode nodo)
        {
            //bloque_principal.Rule = BEGIN + instrucciones + END + ".";
            LinkedList<Instruccion> lista = bloque(nodo.ChildNodes.ElementAt(1));            
            return lista;

        }

        public LinkedList<Instruccion> bloque(ParseTreeNode nodo)
        {
            //instrucciones -> instrucciones instruccion
            if (nodo.ChildNodes.Count == 2)
            {
                LinkedList<Instruccion> lista = bloque(nodo.ChildNodes.ElementAt(0));
                lista.AddLast(instruccion(nodo.ChildNodes.ElementAt(1)));
                return lista;
            }
            //instrucciones -> instruccion
            else
            {
                LinkedList<Instruccion> lista = new LinkedList<Instruccion>();
                lista.AddLast(instruccion(nodo.ChildNodes.ElementAt(0)));
                return lista;
            }            
        }        

        public Instruccion instruccion(ParseTreeNode nodo)
        {
            ParseTreeNode nodoInstruccion = nodo.ChildNodes.ElementAt(0);
            //string ins = nodoInstruccion.ToString().Split(' ')[0].ToLower();
            string ins = nodoInstruccion.ToString();
            int linea = nodoInstruccion.ChildNodes[0].Token.Location.Line;
            int columna = nodoInstruccion.ChildNodes[0].Token.Location.Column;
            
            switch (ins)
            {
                //instruccion.Rule = print + ";"
                case "print":                                        
                    if (nodoInstruccion.ChildNodes.ElementAt(0).ToString().Split(' ')[0].ToLower() == "writeln")
                    {                        
                        return new Print(expresiones(nodoInstruccion.ChildNodes.ElementAt(1)), linea, columna, true);
                    }
                    else
                    {                        
                        return new Print(expresiones(nodoInstruccion.ChildNodes.ElementAt(1)), linea, columna, false);
                    }
            }            
            return new Print(expresiones(nodo.ChildNodes.ElementAt(1)), 0, 0, true); //solo para que corra
        }

        public LinkedList<Expresion> expresiones(ParseTreeNode nodo)
        {
            //lista_exprs.Rule = lista_exprs + COMA + expresion_imprimible                      
            if (nodo.ChildNodes.Count == 2)
            {
                LinkedList<Expresion> lista = expresiones(nodo.ChildNodes.ElementAt(0));
                lista.AddLast(expresion_imp(nodo.ChildNodes.ElementAt(1)));
                return lista;
            }
            //lista_exprs.Rule = expresion_imprimible;
            else
            {
                LinkedList<Expresion> lista = new LinkedList<Expresion>();
                lista.AddLast(expresion_imp(nodo.ChildNodes.ElementAt(0)));
                return lista;
            }
        }

        public Expresion expresion_imp(ParseTreeNode nodo)
        {
            //expresion_imprimible.Rule = expresion;
            if (nodo.ChildNodes.Count == 1)
            {
                return expresion(nodo.ChildNodes.ElementAt(0));
            }
            //expresion_imprimible.Rule = expresion + DOSPTS + ENTERO + DOSPTS + ENTERO              
            else
            {
                return expresion(nodo.ChildNodes.ElementAt(0));
            }
        }

        public Expresion expresion(ParseTreeNode nodo)
        {
            //expresion.Rule = dato
            //expresion.Rule = PARIZQ + expresion + PARDER
            if (nodo.ChildNodes.Count == 1)
            {
                //MessageBox.Show(nodo.ChildNodes.ElementAt(0).ToString());
                if(nodo.ChildNodes.ElementAt(0).ToString() == "expresion")
                {
                    return expresion(nodo.ChildNodes.ElementAt(0));
                }
                else
                {
                    return dato(nodo.ChildNodes.ElementAt(0));
                }                
            }

            else if (nodo.ChildNodes.Count == 2)
            {
                return new Operacion(expresion(nodo.ChildNodes.ElementAt(1)), Operacion.getOperador(nodo.ChildNodes.ElementAt(0).Token.Text));

            }
            //operaciones
            //expresion.Rule = expresion + MAS + expresion
            else if (nodo.ChildNodes.Count == 3)
            {
                return new Operacion(expresion(nodo.ChildNodes.ElementAt(0)), expresion(nodo.ChildNodes.ElementAt(2)), Operacion.getOperador(nodo.ChildNodes.ElementAt(1).Token.Text));
            }
            else
            {
                return dato(nodo.ChildNodes.ElementAt(0));
            }            
        }

        public Expresion dato(ParseTreeNode nodo)
        {
   
            ParseTreeNode tipoDato1 = nodo.ChildNodes.ElementAt(0);                       
            string ins = tipoDato1.ToString().Split(' ')[1].ToLower();
            object valor = tipoDato1.ToString().Split(' ')[0].ToLower();
            int linea = tipoDato1.Token.Location.Line;
            int columna = tipoDato1.Token.Location.Column;
            
           
            switch (ins)
            {
                //dato.Rule =  CADENA
                case "(cadena)":                    
                    return new Cadena(valor, linea, columna);

                case "(entero)":                    
                    return new Primitivo(Convert.ToInt32(valor), linea, columna);
                
                case "(decimal)":                    
                    return new Primitivo(Convert.ToDouble(valor), linea, columna);

                case "(booleano)":                    
                    return new Primitivo(Convert.ToBoolean(valor), linea, columna);

                default:
                    return new Cadena(tipoDato1, linea, columna);
            }            
            
        }
    
        
        public void generarGraficaAST(ParseTreeNode raiz)
        {
            String grafoDOT = ControlRep.getDOT(raiz);
            File.Create("C:\\compiladores2\\GraficaAST.dot").Dispose();
            TextWriter tw = new StreamWriter("C:\\compiladores2\\GraficaAST.dot");
            tw.WriteLine(grafoDOT);
            tw.Close();

            ProcessStartInfo startinfo = new ProcessStartInfo("C:\\Program Files (x86)\\Graphviz2.38\\bin\\dot.exe");
            Process Process;
            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            startinfo.CreateNoWindow = true;
            startinfo.Arguments = "-Tpng C:\\compiladores2\\GraficaAST.dot -o C:\\compiladores2\\graficaAST.png";
            Process = Process.Start(startinfo);
            Process.Close();          
        }

        public void generarTablaErrores()
        {
            int veri = 0;
            string folderName = @"C:\compiladores2";
            string pathString = System.IO.Path.Combine(folderName);
            System.IO.Directory.CreateDirectory(pathString);
            string fileName = "ListaErrores.html";
            pathString = System.IO.Path.Combine(pathString, fileName);
            string path = pathString;

            // Create a file to write to.
            using (StreamWriter ht = File.CreateText(path))
            {
                ht.WriteLine("<!DOCTYPE html>");
                ht.WriteLine("<html>");
                ht.WriteLine("<title>Lista de errores</title>");
                ht.WriteLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css\">");
                ht.WriteLine("</head>");

                ht.WriteLine("<body>");

                ht.WriteLine("<script src=\"https://code.jquery.com/jquery-3.4.1.slim.min.js\"></script>");
                ht.WriteLine("<script src=\"https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js\"></script>");
                ht.WriteLine("<script src=\"https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js\"></script>");                                         
                
                ht.WriteLine("<table class=\"table\">");
                ht.WriteLine("<thead class=\"thead - dark\">");                                                             
                ht.WriteLine("<tr>");
                ht.WriteLine("<th scope=\"col\">" + "No." + "</th>");
                ht.WriteLine("<th scope=\"col\">" + "Descripcion" + "</th>");
                ht.WriteLine("<th scope=\"col\">" + "Tipo de error" + "</th>");
                ht.WriteLine("<th scope=\"col\">" + "Linea" + "</th>");
                ht.WriteLine("<th scope=\"col\">" + "Columna" + "</th>");
                ht.WriteLine("</tr>");
                ht.WriteLine("</thead>");

                ht.WriteLine("<tbody>");
                int i = 0;
                foreach (Errores dato in listaErrores)
                {
                    i++;
                    veri++;
                    ht.WriteLine("<tr>");
                    ht.WriteLine("<td>&nbsp;" + i + "</td>"); //col1
                    ht.WriteLine("<td>&nbsp;" + dato.Descricpion + "</td>"); //col2
                    ht.WriteLine("<td>&nbsp;" + "Sintactico" + "</td>"); //col3
                    ht.WriteLine("<td>&nbsp;" + dato.Columna + "</td>"); //col4
                    ht.WriteLine("<td>&nbsp;" + dato.Fila + "</td>"); //col5                     

                    ht.WriteLine("</tr>");                   
                }

                ht.WriteLine("</tbody>");
                ht.WriteLine("</table>");                                   
                ht.WriteLine(" </body>");
                ht.WriteLine("</html>");

            }
            //System.Diagnostics.Process.Start(path);
            /*
            if (veri == 0)
            {
                MessageBox.Show("No existen errores lexicos");
            }
            else 
            { 
                System.Diagnostics.Process.Start(path); 
            }
            */

        }



        }
    }
