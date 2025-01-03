﻿using chat_site_istemci.Entities;
using chat_site_istemci.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading;

namespace chat_site_istemci.Services
{
    public class SocketService
    {
        private static Socket _socket;
        private static Thread _listenerThread;
        private static bool _isConnected = false;
        private Chats _chats;
        private IHubContext<ChatHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;

        public SocketService(IServiceProvider serviceProvider, Chats chats, IHubContext<ChatHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _chats = chats;
            _hubContext = hubContext;
        }
        public void ConnectToServer(string userId)
        {
            try
            {
                if (_isConnected)
                {
                    Console.WriteLine("Already connected to the server.");
                    return;
                }

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect("127.0.0.1", 5000);

                byte[] messageBytes = Encoding.UTF8.GetBytes(userId);
                _socket.Send(messageBytes);

                _isConnected = true;

                _listenerThread = new Thread(ListenToServer);
                _listenerThread.Start();
                Console.WriteLine("Connected to server and started listening for messages.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        public void SendMessageToServer(Message message)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message, new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    });


                    byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);

                    _socket.Send(messageBytes);

                    Console.WriteLine("Message sent to server.");
                }
                else
                {
                    Console.WriteLine("Socket is not connected. Cannot send message.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to server: {ex.Message}");
            }
        }

        private void ListenToServer()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (_socket.Connected)
                {
                    bytesRead = _socket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Message message = JsonConvert.DeserializeObject<Message>(receivedMessage);
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            message.Sender = dbContext.Users.SingleOrDefault(u => u.UserId == Guid.Parse(message.SenderId));
                        }
                        _hubContext.Clients.All.SendAsync("ReceiveMessage", message,message.SentAt.ToString(),message.ReceiverId);


                        string chat;
                        if (message.Type == "Private")
                        {
                            chat = message.SenderId.ToString();
                        }
                        else
                        {
                            chat = message.GroupId.ToString();
                        }
                        var existingChat = _chats.chats.FirstOrDefault(c => c.ChatKey == chat);
                        if (existingChat != null)
                        {
                            existingChat.Messages.Add(message);
                        }
                        else
                        {
                            var newChat = new Chat
                            {
                                ChatKey = chat,
                            };
                            newChat.Messages.Add(message);
                            _chats.chats.Add(newChat);
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving from server: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _isConnected = false;
                    Console.WriteLine("Disconnected from server.");
                }
                else
                {
                    Console.WriteLine("No connection to disconnect.");
                }

                if (_listenerThread != null && _listenerThread.IsAlive)
                {
                    _listenerThread.Abort();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from server: {ex.Message}");
            }
        }
    }
}
