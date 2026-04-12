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
    private void OnConectarClicked(object sender, EventArgs e)
    {
        // Aquí puedes poner un mensaje o llamar a la conexión
        DisplayAlert("Conexión", "Intentando conectar al bastón...", "OK");
    }


}