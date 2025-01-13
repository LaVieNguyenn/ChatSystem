using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync("localhost", 5001);
                ChatMessages.Items.Add("Connecting to server on PORT 5001");
                _stream = _tcpClient.GetStream();
                ChatMessages.Items.Add("Connected to server");
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_stream == null) return;

            var message = MessageInput.Text;
            var data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
            ChatMessages.Items.Add($"You:  {message}");
            MessageInput.Clear();
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];
            try
            {
                while (true) {

                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; 

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ChatMessages.Dispatcher.Invoke(() =>
                    {
                        ChatMessages.Items.Add(message);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _stream?.Close();
            _tcpClient?.Close();
        }
    }
}
