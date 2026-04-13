using BastonInteligente.ViewModels;

namespace BastonInteligente.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            // Esto une el diseño con el código del Bluetooth
            BindingContext = new BastonInteligente.ViewModels.MainViewModels();
        }
    }
}