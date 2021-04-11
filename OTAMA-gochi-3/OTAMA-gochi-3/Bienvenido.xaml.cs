using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OTAMA_gochi_3
{
    /// <summary>
    /// Lógica de interacción para Bienvenido.xaml
    /// </summary>
    public partial class Bienvenido : Window
    {
        MainWindow padre;

        public Bienvenido(MainWindow padre)
        {
            InitializeComponent();
            this.padre = padre;
        }

        private void enviarNombre(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCuidador.Text))
            {
                padre.setNombre("Unknown");
            }
            else {
                padre.setNombre(txtCuidador.Text);
            }
            
            this.Close();
        }
    }
}
