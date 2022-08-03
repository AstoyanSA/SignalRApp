using System;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HubConnection connection;

        public MainWindow()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder()
                            .WithUrl("https://localhost:7172/chathub")
                            .WithAutomaticReconnect()
                            .Build();

            connection.Reconnecting += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Переподключение...";
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Reconnected += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Подключено к серверу";
                    messages.Items.Clear();
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Closed += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Отключено от сервера";
                    messages.Items.Add(newMessage);
                    openConnection.IsEnabled = true;
                    closeConnection.IsEnabled = false;
                    sendMessage.IsEnabled = false;
                });

                return Task.CompletedTask;
            };
        }

        private async void openConnection_Click(object sender, RoutedEventArgs e)
        {
            connection.On<string, string>("RecieveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";
                    messages.Items.Add(newMessage);
                });
            });

            try
            {
                await connection.StartAsync();
                messages.Items.Add("Подключено");
                openConnection.IsEnabled = false;
                sendMessage.IsEnabled = true;
                closeConnection.IsEnabled = true;
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }

        private async void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.InvokeAsync("SendMessage", "WPF client", messageInput.Text);
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }

        private async void closeConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.StopAsync();
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }
    }
}
