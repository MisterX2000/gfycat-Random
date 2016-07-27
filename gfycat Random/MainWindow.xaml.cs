using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace gfycat_Random
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string format = "mp4";
        private readonly string[] adj;
        private readonly string[] ani;
        private int fails;

        private CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();

            adj = File.ReadAllLines(@"adjectives.txt");
            ani = File.ReadAllLines(@"animals.txt");
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = new TimeSpan(0, 0, 0);
        }

        private void bt_random_Click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();

            if (mi_stopmedia.IsChecked)
                mediaElement.Close();

            if (mi_threads.IsChecked)
            {
                for (int i = 0; i < 5; i++)
                {
                    DoStuff(cts.Token);
                }
            }
            else
            {
                DoStuff(cts.Token);
            }
        }

        private async void DoStuff(CancellationToken cToken)
        {
            bt_random.IsEnabled = false;

            var r = new Random();

            var word1 = UppercaseFirst(adj[r.Next(0, adj.Count() - 1)]);
            var word2 = UppercaseFirst(adj[r.Next(0, adj.Count() - 1)]);
            var word3 = UppercaseFirst(ani[r.Next(0, ani.Count() - 1)]);

            var wordcomb = word1 + word2 + word3;
            //wordcomb = "NervousInsistentGroundbeetle";

            using (var httpClient = new HttpClient())
            {
                var jstring = await httpClient.GetStringAsync(string.Format("https://gfycat.com/cajax/get/" + wordcomb));

                if (cToken.IsCancellationRequested)
                    return;

                var json = JObject.Parse(jstring);

                if (json.Property("error") == null)
                {
                    cts.Cancel();

                    var url = json.Property("gfyItem").Value[format + "Url"].ToObject<string>();

                    mediaElement.Source = new Uri(url.Replace("https://", "http://"));
                    mediaElement.Play();

                    bt_random.IsEnabled = true;
                    l_link.Content = url;
                    mi_save.IsEnabled = true;
                    bt_copylink.IsEnabled = true;
                    fails = 0;
                }
                else
                {
                    fails++;
                    l_link.Content = $"Searching ({fails})";
                    mi_save.IsEnabled = false;
                    bt_copylink.IsEnabled = false;
                    DoStuff(cts.Token);
                }
            }
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

        private void mi_save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = mediaElement.Source.LocalPath.Substring(1),
                DefaultExt = "." + format,
                Filter = "Video|*.mp4;*.webm;*.gif"
            };

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                var filename = dlg.FileName;
                new WebClient().DownloadFile(mediaElement.Source.AbsoluteUri, filename);
            }
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

        private void mi_stop_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            bt_random.IsEnabled = true;
            l_link.Content = "Search stopped";
            fails = 0;
        }
    }
}
