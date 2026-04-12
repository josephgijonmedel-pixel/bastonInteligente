namespace BastonInteligente;

public partial class MainPage : ContentPage
{

    // Constructor de la página
    public MainPage()
    {
        InitializeComponent();

        // Aquí es donde ocurre la magia de MVVM:
        // Le decimos a la Vista que su "cerebro" es el MainViewModel
        BindingContext = new ViewModels.MainViewModels();
    }

    
}