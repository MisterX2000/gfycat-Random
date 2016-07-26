using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Navigation;

namespace gfycat_Random
{
    /// <summary>
    /// Interaktionslogik für About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            l_version.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
