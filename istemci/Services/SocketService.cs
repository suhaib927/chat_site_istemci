using chat_site_istemci.Entities;
using chat_site_istemci.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
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
        public SocketService(Chats chats, IHubContext<ChatHub> hubContext)
        {
            _chats = chats;
            _hubContext = hubContext;
        }
        // اتصال بالخادم
        public void ConnectToServer(string userId)
        {
            try
            {
                // تحقق من إذا كان هناك اتصال سابق قبل إنشاء اتصال جديد
                if (_isConnected)
                {
                    Console.WriteLine("Already connected to the server.");
                    return;
                }

                // إنشاء socket جديد
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect("127.0.0.1", 5000);  // الاتصال بالخادم (IP والـ Port)

                byte[] messageBytes = Encoding.UTF8.GetBytes(userId);
                _socket.Send(messageBytes);

                // تعيين حالة الاتصال إلى true
                _isConnected = true;

                // بدء الاستماع للرسائل من الخادم في thread منفصل
                _listenerThread = new Thread(ListenToServer);
                _listenerThread.Start();
                Console.WriteLine("Connected to server and started listening for messages.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        // إرسال رسالة إلى الخادم
        public void SendMessageToServer(Message message)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                    if (_socket != null && _socket.Connected)
                    {
                        // تحويل الكائن إلى JSON
                        string messageJson = JsonConvert.SerializeObject(message);

                        // تحويل الـ JSON إلى bytes لإرسالها عبر الـ Socket
                        byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);

                        // إرسال الرسالة (بما في ذلك كل التفاصيل) كوحدة واحدة
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

        // الاستماع للرسائل الواردة من الخادم
        private void ListenToServer()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (_socket.Connected)
                {
                    // قراءة الرسائل الواردة من الخادم
                    bytesRead = _socket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Message message = JsonConvert.DeserializeObject<Message>(receivedMessage);

                        _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Sender, message,message.SentAt.ToString());
                        var existingChat = _chats.chats.FirstOrDefault(c => c.ChatKey == message.SenderId.ToString());
                        if (existingChat != null)
                        {
                            existingChat.Messages.Add(message);
                        }
                        else
                        {
                            var newChat = new Chat
                            {
                                ChatKey = message.SenderId.ToString(),
                            };
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
                // إغلاق الاتصال إذا كان هناك أي خطأ أو تم قطع الاتصال
                Disconnect();
            }
        }

        // قطع الاتصال بالخادم
        public void Disconnect()
        {
            try
            {
                // تحقق من وجود الاتصال أولاً
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

                // إيقاف الـ thread الخاص بالاستماع
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
