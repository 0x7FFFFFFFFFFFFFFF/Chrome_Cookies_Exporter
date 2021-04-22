using Dapper;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace cookies_exporter
{
    public partial class MainWindow : Window
    {
        private IEnumerable<Cookie> entries;
        private Task initial_read_cookie_file_task;
        string default_cookie_path;

        public MainWindow()
        {
            InitializeComponent();



            initial_read_cookie_file_task = Task.Factory.StartNew(() =>
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string local_appdata = "".GetAppFolderLocalPath();

                List<string> cookie_db_list = new List<string>(10);

                string tmp = $@"{local_appdata}\Google\Chrome\User Data\Default\Cookies";

                if (tmp.IsFileExists())
                {
                    cookie_db_list.Add(tmp);

                    Dispatcher.Invoke(() =>
                    {
                        MenuItem m = new MenuItem();
                        m.Header = tmp;
                        m.Tag = tmp;
                        m.Click += (o, e) =>
                        {
                            cookie_path_textbox.Text = (string) ((MenuItem)o).Tag;
                            default_cookie_path = cookie_path_textbox.Text;
                            InitCookieEntries(default_cookie_path);
                            output.Text = "";
                            generate_cookies_txt(null, null);
                        };
                        detected_items.Items.Add(m);
                    });
                }

                tmp = $@"{local_appdata}\Microsoft\Edge\User Data\Default\Cookies";
                if (tmp.IsFileExists())
                {
                    cookie_db_list.Add(tmp); 
                    Dispatcher.Invoke(() =>
                    {
                        MenuItem m = new MenuItem();
                        m.Header = tmp;
                        m.Tag = tmp;
                        m.Click += (o, e) =>
                        {
                            cookie_path_textbox.Text = (string)((MenuItem)o).Tag;
                            default_cookie_path = cookie_path_textbox.Text;
                            InitCookieEntries(default_cookie_path);
                            output.Text = "";
                            generate_cookies_txt(null, null);
                        };
                        detected_items.Items.Add(m);
                    });
                }

                tmp = $@"{local_appdata}\Microsoft\Edge Beta\User Data\Default\Cookies";
                if (tmp.IsFileExists())
                {
                    cookie_db_list.Add(tmp);
                    Dispatcher.Invoke(() =>
                    {
                        MenuItem m = new MenuItem();
                        m.Header = tmp;
                        m.Tag = tmp;
                        m.Click += (o, e) =>
                        {
                            cookie_path_textbox.Text = (string)((MenuItem)o).Tag;
                            default_cookie_path = cookie_path_textbox.Text;
                            InitCookieEntries(default_cookie_path);
                            output.Text = "";
                            generate_cookies_txt(null, null);
                        };
                        detected_items.Items.Add(m);
                    });
                }

                tmp = $@"{local_appdata}\BraveSoftware\Brave-Browser\User Data\Default\Cookies";
                if (tmp.IsFileExists())
                {
                    cookie_db_list.Add(tmp);
                    Dispatcher.Invoke(() =>
                    {
                        MenuItem m = new MenuItem();
                        m.Header = tmp;
                        m.Tag = tmp;
                        m.Click += (o, e) =>
                        {
                            cookie_path_textbox.Text = (string)((MenuItem)o).Tag;
                            default_cookie_path = cookie_path_textbox.Text;
                            InitCookieEntries(default_cookie_path);
                            output.Text = "";
                            generate_cookies_txt(null, null);
                        };
                        detected_items.Items.Add(m);
                    });
                }

                if (cookie_db_list.Count > 0)
                {
                    default_cookie_path = cookie_db_list[0];
                    cookie_path_textbox.Dispatcher.Invoke(() =>
                    {
                        cookie_path_textbox.Text = default_cookie_path;
                    });
                }
                else
                {
                    output.Dispatcher.Invoke(() => { output.Text = "Can't find the default cookies database of Chrome or Firefox. Please click the \"Choose...\" button and manually specify its path."; });
                    return;
                }

                InitCookieEntries(default_cookie_path);
            }, TaskCreationOptions.LongRunning);
        }

        private void InitCookieEntries(string default_cookie_path)
        {
            SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLite);
            SQLiteConnection conn = new SQLiteConnection($"Data Source={default_cookie_path};Version=3;");
            entries = conn.Query<Cookie>("select * from cookies");
            foreach (var cookie in entries) cookie.decrypted_value = decrypt_data(cookie.encrypted_value);
        }


        private void choose_cookie_path(object sender, RoutedEventArgs e)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var default_cookie_path = appdata.GoUpNLevels() + @"\Local\Google\Chrome\User Data\Default\";
            while (!default_cookie_path.IsDirectoryExists()) default_cookie_path = default_cookie_path.GoUpNLevels();

            var dlg = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                InitialDirectory = default_cookie_path
            };

            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value) cookie_path_textbox.Text = dlg.FileName;
        }

        private void copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(output.Text);
        }

        private void save_to(object sender, RoutedEventArgs e)
        {
            if (output.Text.IsNullOrEmpty()) return;
            var dlg = new SaveFileDialog
            {
                FileName = "cookies",
                DefaultExt = ".txt",
                Filter = "Text documents (*.txt)|*.txt|All types (*.*)|*.*"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                var filename = dlg.FileName;
                ("# Netscape HTTP Cookie File\n\n" + output.Text).WriteToFileInOneShot(filename, MyUtils.WriteToFileInOneShotConfig.Utf8NoBomUnixLineEnding);
            }
        }


        private void generate_cookies_txt(object sender, RoutedEventArgs e)
        {
            var host_filter = host_key.Text;
            if (always_true_2nd_column == null)
            {
                return;
            }

            bool always_true = (bool) always_true_2nd_column.IsChecked;


            if (sender!= null && sender.Equals(always_true_2nd_column) && Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control))
            {
                StringBuilder sb1 = new StringBuilder();
                foreach (string s in output.Text.ToWindowsLineEnding().Split("\r\n"))
                {
                    if (s.Trim() != s || s.Split("\t").Length != 7)
                    {
                        sb1.Append(s).Append("\r\n");
                    }
                }

                output.Text = sb1.ToString();
                return;
            }

            if (cookie_path_textbox.Text.IsNullOrEmpty() || host_filter.IsNullOrEmpty())
            {
                if (output != null) output.Text = "";

                return;
            }

            if (entries == null)
            {
                InitCookieEntries(cookie_path_textbox.Text.Trim());
            }


            var sb = new StringBuilder();

            if ((bool) use_regex.IsChecked)
            {
                var regex = new Regex(host_filter);
                foreach (var entry in entries)
                    if (regex.IsMatch(entry.host_key))
                        generate(sb, entry);
            }
            else
            {
                foreach (var entry in entries)
                    if (entry.host_key.Contains(host_filter))
                        generate(sb, entry);
            }

            output.Text = sb.ToString();

            void generate(StringBuilder stringBuilder, Cookie entry)
            {
                stringBuilder.Append(entry.host_key).Append("\t");

                if (always_true)
                {
                    stringBuilder.Append("TRUE").Append("\t");
                }
                else
                {
                    if (entry.host_key.StartsWith("."))
                    {
                        stringBuilder.Append("TRUE").Append("\t");
                    }
                    else
                    {
                        stringBuilder.Append("FALSE").Append("\t");
                    }
                }

                stringBuilder.Append(entry.path).Append("\t");
                if (entry.is_secure == 1)
                    stringBuilder.Append("TRUE").Append("\t");
                else
                    stringBuilder.Append("FALSE").Append("\t");

                var tmp_expire = entry.expires_utc / 1000000 - 11644473600;
                if (tmp_expire < 0)
                {
                    tmp_expire = 0;
                }

                stringBuilder.Append(tmp_expire).Append("\t");
                stringBuilder.Append(entry.name).Append("\t");
                stringBuilder.Append(entry.decrypted_value);

                stringBuilder.Append("\r\n");
            }
        }


        public string decrypt_data(byte[] data)
        {
            var encryptedData = data;
            var encKey = File.ReadAllText(Environment.GetEnvironmentVariable("APPDATA").GoUpNLevels() + @"/Local/Google/Chrome/User Data/Local State");
            encKey = JObject.Parse(encKey)["os_crypt"]["encrypted_key"].ToString();
            var decodedKey = ProtectedData.Unprotect(Convert.FromBase64String(encKey).Skip(5).ToArray(), null, DataProtectionScope.LocalMachine);

            const int MAC_BIT_SIZE = 128;
            const int NONCE_BIT_SIZE = 96;

            using (var cipherStream = new MemoryStream(encryptedData))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                var nonSecretPayload = cipherReader.ReadBytes(3);
                var nonce = cipherReader.ReadBytes(NONCE_BIT_SIZE / 8);
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(decodedKey), MAC_BIT_SIZE, nonce);
                cipher.Init(false, parameters);
                var cipherText = cipherReader.ReadBytes(encryptedData.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
                try
                {
                    var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                    cipher.DoFinal(plainText, len);
                }
                catch (InvalidCipherTextException)
                {
                }

                return Encoding.Default.GetString(plainText);
            }
        }

        private void Output_wrap_OnChecked(object sender, RoutedEventArgs e)
        {
            output.TextWrapping = TextWrapping.Wrap;
        }

        private void Output_wrap_OnUnchecked(object sender, RoutedEventArgs e)
        {
            output.TextWrapping = TextWrapping.NoWrap;
        }
    }


    public static class ContextMenuLeftClickBehavior
    {
        public static bool GetIsLeftClickEnabled(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsLeftClickEnabledProperty);
        }

        public static void SetIsLeftClickEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLeftClickEnabledProperty, value);
        }

        public static readonly DependencyProperty IsLeftClickEnabledProperty = DependencyProperty.RegisterAttached("IsLeftClickEnabled", typeof(bool), typeof(ContextMenuLeftClickBehavior), new UIPropertyMetadata(false, OnIsLeftClickEnabledChanged));

        private static void OnIsLeftClickEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = sender as UIElement;

            if (uiElement != null)
            {
                bool IsEnabled = e.NewValue is bool && (bool) e.NewValue;

                if (IsEnabled)
                {
                    if (uiElement is ButtonBase)
                        ((ButtonBase) uiElement).Click += OnMouseLeftButtonUp;
                    else
                        uiElement.MouseLeftButtonUp += OnMouseLeftButtonUp;
                }
                else
                {
                    if (uiElement is ButtonBase)
                        ((ButtonBase) uiElement).Click -= OnMouseLeftButtonUp;
                    else
                        uiElement.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                }
            }
        }

        private static void OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null)
            {
                // if we use binding in our context menu, then it's DataContext won't be set when we show the menu on left click
                // (it seems setting DataContext for ContextMenu is hardcoded in WPF when user right clicks on a control, although I'm not sure)
                // so we have to set up ContextMenu.DataContext manually here
                if (fe.ContextMenu.DataContext == null)
                {
                    fe.ContextMenu.SetBinding(FrameworkElement.DataContextProperty, new Binding {Source = fe.DataContext});
                }

                fe.ContextMenu.IsOpen = true;
            }
        }
    }
}