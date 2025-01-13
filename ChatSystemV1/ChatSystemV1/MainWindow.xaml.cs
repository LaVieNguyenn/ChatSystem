using System.Collections.Concurrent;
using System.Net;
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

namespace ChatSystemV1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener _server;
        private readonly ConcurrentDictionary<TcpClient, string> _clients = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartServer();

        }

        private void StartServer()
        {
            _server = new TcpListener(IPAddress.Any, 5001);
            _server.Start();
            PORT.Content = "5001";
            MessageListBox.Items.Add("Server started and listening on port 5001...");
            Task.Run(AcceptClientsAsync);
        }

        private async Task AcceptClientsAsync()
        {
            while (true)
            {
                // Cập nhật thông báo "Server is waiting for clients..."
                Dispatcher.Invoke(() =>
                {
                    MessageListBox.Items.Add("Server is waiting for clients...");
                });

                try
                {
                    var client = await _server.AcceptTcpClientAsync(); // Chờ kết nối từ client
                    _clients.TryAdd(client, client.Client.RemoteEndPoint.ToString());

                    // Cập nhật khi có client kết nối
                    Dispatcher.Invoke(() =>
                    {
                        MessageListBox.Items.Add($"New Client connected: {client.Client.RemoteEndPoint}");
                    });

                    // Xử lý client
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageListBox.Items.Add($"Error accepting client: {ex.Message}");
                    });
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var clientEndPoint = client.Client.RemoteEndPoint.ToString(); // Lấy thông tin IP:Port của client
            MessageListBox.Dispatcher.Invoke(() =>
            {
                MessageListBox.Items.Add($"New client connected: {clientEndPoint}");
            });

            var stream = client.GetStream();
            var buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Ngắt kết nối
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageListBox.Dispatcher.Invoke(() =>
                    {
                        MessageListBox.Items.Add($"Client {client.Client.RemoteEndPoint}: {message}");
                    });

                    // Phản hồi tới client
                    var response = Encoding.UTF8.GetBytes($"Server: Đã nhận '{message}'");
                    await stream.WriteAsync(response, 0, response.Length);
                }
            }
            catch (Exception ex)
            {
                MessageListBox.Dispatcher.Invoke(() =>
                {
                    MessageListBox.Items.Add($"Error with client {client.Client.RemoteEndPoint}: {ex.Message}");
                });
            }
            finally
            {
                _clients.TryRemove(client, out _);
                client.Close();
                MessageListBox.Dispatcher.Invoke(() =>
                {
                    MessageListBox.Items.Add($"Client {client.Client.RemoteEndPoint} disconnected");
                });
            }
        }
    }
}