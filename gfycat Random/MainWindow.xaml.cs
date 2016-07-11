using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace gfycat_Random
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string format = "mp4";

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = new TimeSpan(0, 0, 0);
        }

        private void bt_random_Click(object sender, RoutedEventArgs e)
        {
            DoStuff();
        }

        private void DoStuff()
        {
            mediaElement.Source = new Uri(GetRandomLink());
            mediaElement.Play();
            l_link.Content = mediaElement.Source.AbsoluteUri;
        }

        private string GetRandomLink()
        {
            var adj = File.ReadAllLines(@"adjectives.txt");
            var ani = File.ReadAllLines(@"animals.txt");
            var r = new Random();

            var word1 = UppercaseFirst(adj[r.Next(0, adj.Count() - 1)]);
            var word2 = UppercaseFirst(adj[r.Next(0, adj.Count() - 1)]);
            var word3 = UppercaseFirst(ani[r.Next(0, ani.Count() - 1)]);

            var link = string.Format("http://giant.gfycat.com/{0}.{1} ", word1 + word2 + word3, format);

            return link;
        }

        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private void bt_copylink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(mediaElement.Source.AbsoluteUri);
        }

        private void mi_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            DoStuff();

            mi_save.IsEnabled = false;
        }

        private void mi_save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = mediaElement.Source.LocalPath.Substring(1);
            dlg.DefaultExt = "." + format;
            dlg.Filter = "Video|*.mp4;*.webm;*.gif";

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                new WebClient().DownloadFile(mediaElement.Source.AbsoluteUri, filename);
            }
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            mi_save.IsEnabled = true;
        }

        private void mi_mp4_Checked(object sender, RoutedEventArgs e)
        {
            format = "mp4";
            if (mi_webm != null || mi_gif != null)
            {
                mi_webm.IsChecked = false;
                mi_gif.IsChecked = false;
            }

        }

        private void mi_webm_Checked(object sender, RoutedEventArgs e)
        {
            format = "webm";
            mi_mp4.IsChecked = false;
            mi_gif.IsChecked = false;
        }

        private void mi_gif_Checked(object sender, RoutedEventArgs e)
        {
            format = "gif";
            mi_mp4.IsChecked = false;
            mi_webm.IsChecked = false;
        }

        private void mi_about_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }
    }
}
