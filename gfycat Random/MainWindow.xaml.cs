using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace gfycat_Random
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string format = "mp4";
        string[] adj;
        string[] ani;
        int fails;
        int threads = 10;

        CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();

            GetStrings();
        }

        async void GetStrings()
        {
            var error = false;
            if (!File.Exists("adjectives.txt"))
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://assets.gfycat.com/adjectives");
                    if (response.IsSuccessStatusCode)
                        File.WriteAllText("adjectives.txt", await response.Content.ReadAsStringAsync());
                    else
                    {
                        MessageBox.Show("adjectives status: " + response.StatusCode, "Cannot get file", MessageBoxButton.OK, MessageBoxImage.Error);
                        error = true;
                    }
                }

            if (!File.Exists("animals.txt"))
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://assets.gfycat.com/animals");
                    if (response.IsSuccessStatusCode)
                        File.WriteAllText("animals.txt", await response.Content.ReadAsStringAsync());
                    else
                    {
                        MessageBox.Show("animals status: " + response.StatusCode, "Cannot get file", MessageBoxButton.OK, MessageBoxImage.Error);
                        error = true;
                    }
                }

            if (error) return;
            LoadStrings();
            bt_random.IsEnabled = true;
        }

        void LoadStrings()
        {
            adj = File.ReadAllLines("adjectives.txt");
            ani = File.ReadAllLines("animals.txt");
        }

        void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = new TimeSpan(0, 0, 0);
        }

        void bt_random_Click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();

            if (mi_stopmedia.IsChecked)
                mediaElement.Close();

            for (var i = 0; i < threads; i++)
                DoStuff(cts.Token);
        }

        async Task<string> GetjsonStream(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        static DateTime UtsToDate(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        async void DoStuff(CancellationToken ct)
        {
            bt_random.IsEnabled = false;
            mi_settings.IsEnabled = false;

            var r = new Random();

            var word1 = adj[r.Next(0, adj.Length - 1)];
            var word2 = adj[r.Next(0, adj.Length - 1)];
            var word3 = ani[r.Next(0, ani.Length - 1)];

            var wordcomb = word1 + word2 + word3;
            wordcomb = "NervousInsistentGroundbeetle";

            try
            {
                var json = JObject.Parse(await GetjsonStream("https://gfycat.com/cajax/get/" + wordcomb));

                ct.ThrowIfCancellationRequested();


                if (json.Property("error") == null)
                {
                    cts.Cancel();

                    var url = json.Property("gfyItem").Value[format + "Url"].ToString();

                    mediaElement.Source = new Uri(url.Replace("https://", "http://"));
                    mediaElement.Play();

                    bt_random.IsEnabled = true;
                    bt_openlink.IsEnabled = true;
                    bt_copylink.IsEnabled = true;
                    mi_settings.IsEnabled = true;
                    mi_save.IsEnabled = true;
                    mi_stop.IsEnabled = false;

                    l_link.Content = "https://gfycat.com/" + json.Property("gfyItem").Value["gfyName"];
                    li_title.Content = "Title: " + json.Property("gfyItem").Value["title"];
                    li_views.Content = "Views: " + json.Property("gfyItem").Value["views"];
                    li_uploader.Content = "Uploader: " + json.Property("gfyItem").Value["userName"];
                    li_date.Content = UtsToDate(json.Property("gfyItem").Value["createDate"].ToObject<double>());
                    fails = 0;
                }
                else
                {
                    fails++;
                    l_link.Content = $"Searching ({fails})";
                    mi_save.IsEnabled = false;
                    mi_stop.IsEnabled = true;
                    bt_openlink.IsEnabled = false;
                    bt_copylink.IsEnabled = false;
                    DoStuff(cts.Token);
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        void bt_copylink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(l_link.Content.ToString());
        }

        void mi_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void mi_save_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
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

        void mi_mp4_Checked(object sender, RoutedEventArgs e)
        {
            format = "mp4";
            if (mi_webm != null || mi_gif != null)
            {
                mi_webm.IsChecked = false;
                mi_gif.IsChecked = false;
            }

        }

        void mi_webm_Checked(object sender, RoutedEventArgs e)
        {
            format = "webm";
            mi_mp4.IsChecked = false;
            mi_gif.IsChecked = false;
        }

        void mi_gif_Checked(object sender, RoutedEventArgs e)
        {
            format = "gif";
            mi_mp4.IsChecked = false;
            mi_webm.IsChecked = false;
        }

        void mi_about_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }

        void mi_stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cts.Cancel();
                bt_random.IsEnabled = true;
                mi_settings.IsEnabled = true;
                l_link.Content = "Search stopped";
                fails = 0;
            }
            catch
            {
                l_link.Content = "There is nothing to stop.";
            }
        }

        void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if (bt_random.IsEnabled)
                        bt_random_Click(null, null);
                    break;
                case Key.Escape:
                    if (mi_stop.IsEnabled)
                        mi_stop_Click(null, null);
                    break;
            }
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && mi_save.IsEnabled)
                mi_save_Click(null, null);
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && bt_copylink.IsEnabled)
                bt_copylink_Click(null, null);
            else if (e.Key == Key.O && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && bt_openlink.IsEnabled)
                bt_openlink_Click(null, null);
        }

        void mi_threads_Click(object sender, RoutedEventArgs e)
        {
            threads = mi_threads.IsChecked ? 10 : 1;
        }

        void bt_openlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(l_link.Content.ToString());
        }
    }
}
