using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OTAMA_gochi_3
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer t1;
        double decremento_hambre_inicial = 5.0;
        double decremento_cansancio_inicial = 5.0;
        double decremento_diversion_inicial = 5.0;
        double incremento_hambre = 5.0;
        double incremento_cansancio = 5.0;
        double incremento_diversion = 5.0;
        double decremento_hambre_aumento = 1.0;
        double decremento_cansancio_aumento = 1.0;
        double decremento_diversion_aumento = 1.0;
        TimeSpan tiempo_inicial;
        TimeSpan tiempo_derrota;
        string nombre_cuidador;
        ObservableCollection<Cuidador> items = new ObservableCollection<Cuidador>();
        List<Mapa> escenarios = new List<Mapa>();
        int n_mapa = 0;
        bool logro_yamato_hecho = false;
        bool logro_ivankov_hecho = false;
        bool logro_robin_hecho = false;
        bool logro_chopper_hecho = false;
        bool careta_zoro_up = false;
        bool careta_robin_up = false;
        bool espada_torao_up = false;
        bool ramita_up = false;

        public MainWindow()
        {
            InitializeComponent();

            Bienvenido pantallaInicio = new Bienvenido(this);
            pantallaInicio.ShowDialog();

            manual_historia();

            LVRanking.ItemsSource = items;//linkear la collection a la list view
            crearParticipantes();//añadir jugadores ficticios
            crearListaMapas();//dar valores a los distintos mapas
            generarTooltipsEscenarios();
            generarTooltipsObjetos();
            generarTooltipsPremios();
        }

        private void reloj(object sender, EventArgs e)
        {
            this.pbHambre.Value -= decremento_hambre_inicial;
            this.pbSueño.Value -= decremento_cansancio_inicial;
            this.pbDiversion.Value -= decremento_diversion_inicial;

            //animaciones estado critico
            if (pbHambre.Value <= 25 & pbHambre.Value > 0)//k no se ejecuten todas a la vez
            {
                pbHambre.Foreground = new SolidColorBrush(Color.FromRgb(255,0,0));
                Storyboard sbHambre = (Storyboard)this.Resources["Hambre"];
                sbHambre.Begin();
            }
            if (pbSueño.Value <= 25 & pbSueño.Value > 0)
            {
                pbSueño.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                Storyboard sbBostezar = (Storyboard)this.Resources["Bostezo"];
                sbBostezar.Begin();
            }
            if (pbDiversion.Value <= 25 & pbDiversion.Value > 0)
            {
                pbDiversion.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                Happy.Visibility = Visibility.Hidden;
                Sad.Visibility = Visibility.Visible;

                DoubleAnimation lagrimaOjoAni = new DoubleAnimation();
                LagrimaOjo.Visibility = Visibility.Visible;
                lagrimaOjoAni.From = 0;
                lagrimaOjoAni.To = 2;
                lagrimaOjoAni.Duration = new Duration(TimeSpan.FromSeconds(1));

                DoubleAnimation lagrimaCaraAni = new DoubleAnimation();
                LagrimaCara.Visibility = Visibility.Visible;
                lagrimaCaraAni.From = 0;
                lagrimaCaraAni.To = 15;
                lagrimaCaraAni.Duration = new Duration(TimeSpan.FromSeconds(3));
                lagrimaCaraAni.Completed += new EventHandler(finLlorar);

                LagrimaOjo.BeginAnimation(Ellipse.HeightProperty, lagrimaOjoAni);
                LagrimaCara.BeginAnimation(Rectangle.HeightProperty, lagrimaCaraAni);
            }

            //terminar el juego
            if (pbHambre.Value <= 0)
            {
                t1.Stop();
                finishGame("Otama se está muriendo de hambre");
            }
            else if (pbSueño.Value <= 0)
            {
                t1.Stop();
                finishGame("Otama cayo muerta de sueño");
            }
            else if (pbDiversion.Value <= 0)
            {
                t1.Stop();
                finishGame("Otama está sad");
            }
        }

        private void finishGame(string defeat_info)
        {
            tiempo_derrota = DateTime.Now.TimeOfDay;
            TimeSpan tiempo_final = tiempo_derrota - tiempo_inicial;
            double tiempo_final_redondeado = Math.Round(tiempo_final.TotalSeconds);
            lblInfo.Content = defeat_info + ". Has aguantado " + (tiempo_final_redondeado) + "s cuidando a Otama";
            
            lblBienvenido.Visibility = Visibility.Hidden;
            lblInfo.Visibility = Visibility.Visible;
            btnComer.IsEnabled = false;
            btnDormir.IsEnabled = false;
            btnJugar.IsEnabled = false;
            btnReiniciar.Visibility = Visibility.Visible;

            //añadir al ranking la puntuación          
            items.Add(new Cuidador() { Nombre = nombre_cuidador, Puntos = tiempo_final_redondeado });
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(LVRanking.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Puntos", ListSortDirection.Descending));

            comprobarLogrosAsync(tiempo_final_redondeado);
        }

        public void setNombre(string nombre)
        {
            nombre_cuidador = nombre;
            lblBienvenido.Content = "Bienvenido " + nombre;
        }

        private void cambiarFondo(object sender, MouseButtonEventArgs e)
        {
            imgFondo.Source = ((Image)sender).Source;

            switch (((Image)sender).Name)
            {
                case "imgCapital":
                    pbHambre.Value = escenarios.ElementAt(0).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(0).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(0).pbSueño;
                    n_mapa = 0;
                    break;
                case "imgMugiwaras":
                    pbHambre.Value = escenarios.ElementAt(1).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(1).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(1).pbSueño;
                    n_mapa = 1;
                    break;
                case "imgUdon":
                    pbHambre.Value = escenarios.ElementAt(2).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(2).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(2).pbSueño;
                    n_mapa = 2;
                    break;
                case "imgPlayaKuri":
                    pbHambre.Value = escenarios.ElementAt(3).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(3).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(3).pbSueño;
                    n_mapa = 3;
                    break;
                case "imgSamurai":
                    pbHambre.Value = escenarios.ElementAt(4).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(4).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(4).pbSueño;
                    n_mapa = 4;
                    break;
            }
        }

        private void acercaDe(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("Tamagochi creado por:\n\n       Sergio Silvestre Pavón\n\n"
                , "Acerca De ...", MessageBoxButton.OK);
        }

        private void btnComer_Click(object sender, RoutedEventArgs e)
        {
            this.pbHambre.Value += incremento_hambre;
            decremento_hambre_inicial += decremento_hambre_aumento;

            if (pbHambre.Value > 25) {
                pbHambre.Foreground = new SolidColorBrush(Color.FromRgb(247, 129, 9));
            }

            //Animación Comer
            Storyboard sbComer = (Storyboard)this.Resources["Comer"];
            sbComer.Begin();
        }

        private void btnDormir_Click(object sender, RoutedEventArgs e)
        {
            this.pbSueño.Value += incremento_cansancio;
            decremento_cansancio_inicial += decremento_cansancio_aumento;

            if (pbSueño.Value > 25)
            {
                pbSueño.Foreground = new SolidColorBrush(Color.FromRgb(6, 176, 37));
            }

            //Animacion Dormirse
            bloquearBotonesBarras();

            DoubleAnimation cerrarParpadoDcho = new DoubleAnimation();
            ParpadoDcho.Visibility = Visibility.Visible;
            PestañaDcha.Visibility = Visibility.Hidden;
            cerrarParpadoDcho.From = 0;
            cerrarParpadoDcho.To = 15;
            cerrarParpadoDcho.Duration = new Duration(TimeSpan.FromSeconds(2));
            cerrarParpadoDcho.AutoReverse = true;
            cerrarParpadoDcho.Completed += new EventHandler(finCerrarOjos);

            DoubleAnimation cerrarParpadoIzqdo = new DoubleAnimation();
            ParpadoIzqdo.Visibility = Visibility.Visible;
            PestañaIzqda.Visibility = Visibility.Hidden;
            cerrarParpadoIzqdo.From = 0;
            cerrarParpadoIzqdo.To = 15;
            cerrarParpadoIzqdo.Duration = new Duration(TimeSpan.FromSeconds(2));
            cerrarParpadoIzqdo.AutoReverse = true;

            ParpadoDcho.BeginAnimation(Ellipse.HeightProperty, cerrarParpadoDcho);
            ParpadoIzqdo.BeginAnimation(Ellipse.HeightProperty, cerrarParpadoIzqdo);
        }

        private void btnJugar_Click(object sender, RoutedEventArgs e)
        {
            this.pbDiversion.Value += incremento_diversion;
            decremento_diversion_inicial += decremento_diversion_aumento;

            if (pbDiversion.Value > 25)
            {
                pbDiversion.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }

            //Animación Jugar
            Storyboard sbSaltar = (Storyboard)this.Resources["Saltar"];
            sbSaltar.Begin();
        }

        private void finCerrarOjos(object sender, EventArgs e)
        {
            desbloquearBotonesBarras();
            PestañaDcha.Visibility = Visibility.Visible;
            PestañaIzqda.Visibility = Visibility.Visible;
        }

        private void finLlorar(object sender, EventArgs e)
        {
            LagrimaCara.Visibility = Visibility.Hidden;
            LagrimaOjo.Visibility = Visibility.Hidden;
            Happy.Visibility = Visibility.Visible;
            Sad.Visibility = Visibility.Hidden;
        }

        private void manual_historia()
        {
            MessageBoxResult resultado = MessageBox.Show("" +
                "************************************HISTORIA************************************\n" +
                "¡¡¡Bienvenido Cuidador!!!\nHas llegado a la tierra " +
                "de Wano con los Mugiwaras y se te ha encargado la importante misión de cuidar a Otama.\n" +
                "Antes de comenzar, seleccione bien el escenario y las herramientas pues de ello dependerá " +
                "todo.\nBuena suerte, la necesitarás.\n\n\n" +
                "************************************MANUAL************************************\n" +
                "Para seleccionar un escenario pinche sobre él, para poner un objeto arrástrelo a Otama y " +
                "para quitarlo arrástrelo a su panel. Al pararte sobre ellos podrás ver sus características. " +
                "Las combinaciones son múltiples.\nSi alguna de las barras llega a 0 será GAME OVER. Para evitarlo " +
                "pulse los botones correspondientes a las barras.\nCuando pierda tendrá la oportunidad de volver a " +
                "jugar. Para ello pulse el botón de Reiniciar, que le habilitará la selección de escenario y objetos " +
                "nuevamente.", 
                "Historia/Manual", MessageBoxButton.OK);
        }

        private void comenzarPartida(object sender, RoutedEventArgs e)
        {
            btnComer.IsEnabled = true;
            btnDormir.IsEnabled = true;
            btnJugar.IsEnabled = true;
            btnEmpezar.Visibility = Visibility.Hidden;
            WPColeccionables.IsEnabled = false;
            WPPremios.IsEnabled = false;
            tiempo_inicial = DateTime.Now.TimeOfDay;
            crearDispatcher();
        }

        private void crearDispatcher() {
            t1 = new DispatcherTimer();
            t1.Interval = TimeSpan.FromSeconds(3.0);
            t1.Tick += new EventHandler(reloj);
            t1.Start();
        }

        private void crearParticipantes() 
        {
            items.Add(new Cuidador() { Nombre = "Robin", Puntos = 45 });
            items.Add(new Cuidador() { Nombre = "Yamato", Puntos = 20 });
            items.Add(new Cuidador() { Nombre = "Chopper", Puntos = 60 });
            items.Add(new Cuidador() { Nombre = "Ivankov", Puntos = 30 });
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(LVRanking.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Puntos", ListSortDirection.Descending));
        }

        private void crearListaMapas()
        {
            escenarios.Add(new Mapa() { name = "capital", pbHambre = 70, pbSueño = 70, pbDiversion = 70 });
            escenarios.Add(new Mapa() { name = "mugiwaras", pbHambre = 80, pbSueño = 60, pbDiversion = 100 });
            escenarios.Add(new Mapa() { name = "udon", pbHambre = 30, pbSueño = 30, pbDiversion = 30 });
            escenarios.Add(new Mapa() { name = "playaKuri", pbHambre = 40, pbSueño = 50, pbDiversion = 90 });
            escenarios.Add(new Mapa() { name = "samurai", pbHambre = 60, pbSueño = 80, pbDiversion = 50 });
        }

        private void pillarObjeto(object sender, MouseButtonEventArgs e)
        {
            DataObject dobj = new DataObject((Image)sender);
            DragDrop.DoDragDrop((Image)sender, dobj, DragDropEffects.Move);
        }

        private void colocarColeccionable(object sender, DragEventArgs e)
        {
            Image aux = (Image)e.Data.GetData(typeof(Image));
            switch (aux.Name)
            {
                case "imgSombreroLuffy":
                    sombreroLuffy.Visibility = Visibility.Visible;
                    incremento_diversion += 5.0;
                    decremento_hambre_inicial += 5.0;
                    break;
                case "imgLogoAce":
                    escudoAce.Visibility = Visibility.Visible;
                    incremento_hambre += 2.0;
                    incremento_cansancio += 2.0;
                    incremento_diversion += 2.0;
                    break;
                case "imgCaretaZoro":
                    if (careta_robin_up)
                    {
                        caretaRobin.Visibility = Visibility.Hidden;
                        careta_robin_up = false;
                        decremento_hambre_aumento -= 0.5;
                        decremento_cansancio_aumento -= 0.5;
                        decremento_diversion_aumento -= 0.5;
                        decremento_diversion_inicial += 3;
                        decremento_cansancio_inicial += 3;
                        decremento_hambre_inicial += 3;
                    }
                    caretaZoro.Visibility = Visibility.Visible;
                    careta_zoro_up = true;
                    decremento_cansancio_inicial += 5.0;
                    incremento_hambre += 5.0;
                    break;
                case "imgEspadaTorao":
                    if (ramita_up)
                    {
                        ramita.Visibility = Visibility.Hidden;
                        ramita_up = false;
                        incremento_hambre -= 15;
                        incremento_cansancio -= 15;
                        incremento_diversion -= 15;
                    }
                    espadaTorao.Visibility = Visibility.Visible;
                    espada_torao_up = true;
                    incremento_diversion -= 2.0;
                    incremento_cansancio += 2.0;  
                    break;
                case "logroYamato":
                    yamato.Visibility = Visibility.Visible;
                    decremento_hambre_aumento -= 0.5;
                    decremento_cansancio_aumento -= 0.5;
                    decremento_diversion_aumento -= 0.5;
                    break;
                case "logroIvankov":
                    ivankov.Visibility = Visibility.Visible;
                    decremento_diversion_inicial += 2;
                    decremento_cansancio_inicial -= 4;
                    decremento_hambre_inicial -= 4;
                    break;
                case "logroRobin":
                    if (careta_zoro_up)
                    {
                        caretaZoro.Visibility = Visibility.Hidden;
                        careta_zoro_up = false;
                        decremento_cansancio_inicial -= 5.0;
                        incremento_hambre -= 5.0;
                    }
                    caretaRobin.Visibility = Visibility.Visible;
                    careta_robin_up = true;
                    decremento_hambre_aumento += 0.5;
                    decremento_cansancio_aumento += 0.5;
                    decremento_diversion_aumento += 0.5;
                    decremento_diversion_inicial -= 3;
                    decremento_cansancio_inicial -= 3;
                    decremento_hambre_inicial -= 3;
                    break;
                case "logroChopper":
                    if (espada_torao_up)
                    {
                        espadaTorao.Visibility = Visibility.Hidden;
                        espada_torao_up = false;
                        incremento_diversion += 2.0;
                        incremento_cansancio -= 2.0;
                    }
                    ramita.Visibility = Visibility.Visible;
                    ramita_up = true;
                    incremento_hambre += 15;
                    incremento_cansancio += 15;
                    incremento_diversion += 15;
                    break;
            }
        }

        private void quitarColeccionable(object sender, DragEventArgs e)
        {
            Image aux = (Image)e.Data.GetData(typeof(Image));
            switch (aux.Name)
            {
                case "sombreroLuffy":
                    sombreroLuffy.Visibility = Visibility.Hidden;
                    incremento_diversion -= 5.0;
                    decremento_hambre_inicial -= 5.0;
                    break;
                case "escudoAce":
                    escudoAce.Visibility = Visibility.Hidden;
                    incremento_hambre -= 2.0;
                    incremento_cansancio -= 2.0;
                    incremento_diversion -= 2.0;
                    break;
                case "caretaZoro":
                    caretaZoro.Visibility = Visibility.Hidden;
                    decremento_cansancio_inicial -= 5.0;
                    incremento_hambre -= 5.0;
                    careta_zoro_up = false;
                    break;
                case "espadaTorao":
                    espadaTorao.Visibility = Visibility.Hidden;
                    incremento_diversion += 2.0;
                    incremento_cansancio -= 2.0;
                    espada_torao_up = false;
                    break;
                case "yamato":
                    yamato.Visibility = Visibility.Hidden;
                    decremento_hambre_aumento += 0.5;
                    decremento_cansancio_aumento += 0.5;
                    decremento_diversion_aumento += 0.5;
                    break;
                case "ivankov":
                    ivankov.Visibility = Visibility.Hidden;
                    decremento_diversion_inicial -= 2;
                    decremento_cansancio_inicial += 4;
                    decremento_hambre_inicial += 4;
                    break;
                case "caretaRobin":
                    caretaRobin.Visibility = Visibility.Hidden;
                    careta_robin_up = false;
                    decremento_hambre_aumento -= 0.5;
                    decremento_cansancio_aumento -= 0.5;
                    decremento_diversion_aumento -= 0.5;
                    decremento_diversion_inicial += 3;
                    decremento_cansancio_inicial += 3;
                    decremento_hambre_inicial += 3;
                    break;
                case "ramita":
                    ramita.Visibility = Visibility.Hidden;
                    ramita_up = false;
                    incremento_hambre -= 15;
                    incremento_cansancio -= 15;
                    incremento_diversion -= 15;
                    break;
            }
        }

        private void reiniciarPartida(object sender, RoutedEventArgs e)
        {
            lblBienvenido.Visibility = Visibility.Visible;
            lblInfo.Visibility = Visibility.Hidden;
            btnReiniciar.Visibility = Visibility.Hidden;
            btnEmpezar.Visibility = Visibility.Visible;
            WPColeccionables.IsEnabled = true;
            WPPremios.IsEnabled = true;
            reiniciarMapa();
            reiniciarValores();
        }

        private void reiniciarMapa() 
        {
            switch (n_mapa)
            {
                case 0:
                    pbHambre.Value = escenarios.ElementAt(0).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(0).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(0).pbSueño;
                    break;
                case 1:
                    pbHambre.Value = escenarios.ElementAt(1).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(1).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(1).pbSueño;
                    break;
                case 2:
                    pbHambre.Value = escenarios.ElementAt(2).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(2).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(2).pbSueño;
                    break;
                case 3:
                    pbHambre.Value = escenarios.ElementAt(3).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(3).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(3).pbSueño;
                    break;
                case 4:
                    pbHambre.Value = escenarios.ElementAt(4).pbHambre;
                    pbDiversion.Value = escenarios.ElementAt(4).pbDiversion;
                    pbSueño.Value = escenarios.ElementAt(4).pbSueño;
                    break;
            }
        }

        private void reiniciarValores()
        {
            //poner los valores como al inicio
            decremento_hambre_inicial = 5.0;
            decremento_cansancio_inicial = 5.0;
            decremento_diversion_inicial = 5.0;
            incremento_hambre = 5.0;
            incremento_cansancio = 5.0;
            incremento_diversion = 5.0;
            decremento_hambre_aumento = 2.0;
            decremento_cansancio_aumento = 2.0;
            decremento_diversion_aumento = 2.0;

            //como los valores son los iniciales, corresponderia a no tener ningun objeto
            sombreroLuffy.Visibility = Visibility.Hidden;
            escudoAce.Visibility = Visibility.Hidden;
            caretaZoro.Visibility = Visibility.Hidden;
            espadaTorao.Visibility = Visibility.Hidden;
            yamato.Visibility = Visibility.Hidden;
            ivankov.Visibility = Visibility.Hidden;
            caretaRobin.Visibility = Visibility.Hidden;
            ramita.Visibility = Visibility.Hidden;
            careta_zoro_up = false;
            espada_torao_up = false;      
            careta_robin_up = false;     
            ramita_up = false;

            //color progress bar
            pbDiversion.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            pbSueño.Foreground = new SolidColorBrush(Color.FromRgb(6, 176, 37));
            pbHambre.Foreground = new SolidColorBrush(Color.FromRgb(247, 129, 9));
        }

        private async void comprobarLogrosAsync(double puntos)
        {
            if (puntos > 20 & !logro_yamato_hecho) 
            {
                logroYamato.Visibility = Visibility.Visible;
                lblLogroYamato.Content = "Mazas y Catapultas";
                lblLogroYamato.FontSize = 14;
                lblLogroYamato.ToolTip = "Logro conseguido al superar a Yamato en la clasificación.";
                Storyboard sbYamato = (Storyboard)this.Resources["logroYamatoNotificacion"];
                sbYamato.Begin();
                logro_yamato_hecho = true;
                await Task.Delay(2000);
            }
            if (puntos > 30 & !logro_ivankov_hecho)
            {
                logroIvankov.Visibility = Visibility.Visible;
                lblLogroIvankov.Content = "Welcome to OKAMA-gochi!";
                lblLogroIvankov.FontSize = 14;
                lblLogroIvankov.ToolTip = "Logro conseguido al superar a Ivankov en la clasificación.";
                Storyboard sbIvankov = (Storyboard)this.Resources["logroIvankovNotificacion"];
                sbIvankov.Begin();
                logro_ivankov_hecho = true;
                await Task.Delay(2000);
            }
            if (puntos > 45 & !logro_robin_hecho)
            {
                logroRobin.Visibility = Visibility.Visible;
                lblLogroRobin.Content = "Gigantesco Mano";
                lblLogroRobin.FontSize = 14;
                lblLogroRobin.ToolTip = "Logro conseguido al superar a Robin en la clasificación.";
                Storyboard sbRobin = (Storyboard)this.Resources["logroRobinNotificacion"];
                sbRobin.Begin();
                logro_robin_hecho = true;
                await Task.Delay(2000);
            }
            if (puntos > 60 & !logro_chopper_hecho)
            {
                logroChopper.Visibility = Visibility.Visible;
                lblLogroChopper.Content = "Tanuki Power";
                lblLogroChopper.FontSize = 14;
                lblLogroChopper.ToolTip = "Logro conseguido al superar a Chopper en la clasificación.";
                Storyboard sbChopper = (Storyboard)this.Resources["logroChopperNotificacion"];
                sbChopper.Begin();
                logro_chopper_hecho = true;
                await Task.Delay(2000);
            }
        }

        private void bloquearBotonesBarras() 
        {
            btnComer.IsEnabled = false;
            btnDormir.IsEnabled = false;
            btnJugar.IsEnabled = false;
        }

        private void desbloquearBotonesBarras()
        {
            btnComer.IsEnabled = true;
            btnDormir.IsEnabled = true;
            btnJugar.IsEnabled = true;
        }

        private void generarTooltipsEscenarios() {
            imgCapital.ToolTip = "Capital\nHambre: 70\nSueño: 70\nDiversión: 70";
            imgMugiwaras.ToolTip = "Mugiwaras\nHambre: 80\nSueño: 60\nDiversión: 100";
            imgUdon.ToolTip = "Udon\nHambre: 30\nSueño: 30\nDiversión: 30";
            imgPlayaKuri.ToolTip = "Playa de Kuri\nHambre: 40\nSueño: 50\nDiversión: 90";
            imgSamurai.ToolTip = "Samurai\nHambre: 60\nSueño: 80\nDiversión: 50";
        }

        private void generarTooltipsObjetos() {
            imgSombreroLuffy.ToolTip = "Sombrero Luffy\nEfectos:\n" +
                "Incremento diversión: +5\nDecremento inicial hambre: +5";
            imgLogoAce.ToolTip = "Escudo Ace\nEfectos:\n" +
                "Incremento hambre: +2\nIncremento cansancio: +2\nIncremento diversión: +2";
            imgEspadaTorao.ToolTip = "Espada Trafalgar\nEfectos:\n" +
                "Incremento cansancio: +2\nIncremento diversión: -2";
            imgCaretaZoro.ToolTip = "Careta Zoro\nEfectos:\n" +
                "Incremento hambre: +5\nDecremento inicial cansancio: +5";
        }

        private void generarTooltipsPremios()
        {
            logroChopper.ToolTip = "Rama de Sakura\nConseguido al completar el logro 'Tanuki Power'" +
                "\nEfectos:\nIncremento hambre: +15\nIncremento cansancio: +15\nIncremento diversión: +15";
            logroYamato.ToolTip = "Cuidadora Yamato\nConseguido al completar el logro 'Mazas y Catapultas'" +
                "\nEfectos:\nAumento decremento hambre: -0.5\nAumento decremento cansancio: -0.5\n" +
                "Aumento decremento diversión: -0.5";
            logroRobin.ToolTip = "Careta Robin\nConseguido al completar el logro 'Gigantesco Mano'" +
                "\nEfectos:\nAumento decremento hambre: +0.5\nAumento decremento cansancio: +0.5\n" +
                "Aumento decremento diversión: +0.5\nDecremento inicial hambre: -3\n" +
                "Decremento incial cansancio: -3\nDecremento inicial diversión: -3";
            logroIvankov.ToolTip = "Cuidadore Emporio Ivankov\nConseguido al completar el logro 'Welcome to OKAMA-gochi'" +
                "\nEfectos:\nDecremento inicial hambre: -4\nDecremento incial cansancio: -4\n" +
                "Decremento inicial diversión: +2";
        }

        private void verEstadísticas(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("" +
                "*****************************DECREMENTO INICIAL******************************" +
                "\nDescripción: decremento inicial de las barras." +
                "\nHambre: " + decremento_hambre_inicial +
                "\nSueño: " + decremento_cansancio_inicial +
                "\nDiversión: " + decremento_diversion_inicial +
                "\n\n**********************************INCREMENTO**********************************" +
                "\nDescripción: incremento de las barras." +
                "\nHambre: " + incremento_hambre +
                "\nSueño: " + incremento_cansancio +
                "\nDiversión: " + incremento_diversion +
                "\n\n***************************AUMENTO DECREMENTO****************************" +
                "\nDescripción: cantidad que se va sumando al dcremento inicial al pulsar el botón de la barra correspondiente." +
                "\nHambre: " + decremento_hambre_aumento +
                "\nSueño: " + decremento_cansancio_aumento +
                "\nDiversión: " + decremento_diversion_aumento,
                "Estadísticas", MessageBoxButton.OK);
        }
    }

    public class Cuidador
    {
        public string Nombre { get; set; }
        public double Puntos { get; set; }
    }

    public class Mapa
    {
        public string name { get; set; }
        public double pbHambre { get; set; }
        public double pbSueño { get; set;  }
        public double pbDiversion { get; set; }
    }
}
