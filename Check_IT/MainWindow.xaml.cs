using System.Windows;

namespace Check_IT
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider? _serviceProvider;

        public MainWindow(IServiceProvider? serviceProvider = null) : this(false, serviceProvider)
        {
        }

        protected MainWindow(bool skipInitialize, IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
            if (!skipInitialize)
                InitializeComponent();
        }

        protected void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Resolve via DI to allow test injection
            var menu = ResolveWindow(typeof(MenuWindow));
            ShowResolvedWindow(menu);
        }

        protected void OpenRegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var reg = ResolveWindow(typeof(RegisterWindow));
            ShowResolvedWindow(reg);
        }

        protected void OpenLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var login = ResolveWindow(typeof(LoginWindow));
            ShowResolvedWindow(login);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "Check_IT - tender analysis tool", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Overridable resolution to allow tests to supply fake windows
        protected virtual object ResolveWindow(Type t)
        {
            // Try DI first
            if (_serviceProvider != null)
            {
                var svc = _serviceProvider.GetService(t);
                if (svc != null) return svc;
            }

            // Fallback to Activator
            return Activator.CreateInstance(t)!;
        }

        // Overridable showing – tests can intercept instead of actually showing UI
        protected virtual void ShowResolvedWindow(object window)
        {
            if (window is Window w)
            {
                w.Owner = this;
                w.ShowDialog();
            }
        }
    }
}