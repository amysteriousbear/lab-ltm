using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CalcClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1308);
            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            try { socket.Connect(ep); }
            catch (SocketException ex)
            {
                Console.WriteLine("Khong ket noi duoc server: " + ex.Message);
                return;
            }

            using var stream = new NetworkStream(socket);
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream);

            Console.WriteLine("Lenh: CALC | CHECK | exit");
            while (true)
            {
                Console.Write("> ");
                string request = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(request)) continue;
                if (request.Trim().ToLower() == "exit") break;

                writer.WriteLine(request);
                writer.Flush();

                string response = reader.ReadLine();
                if (response == null) { Console.WriteLine("Server dong ket noi."); break; }
                Console.WriteLine($"RESULT: {response}");
            }
        }
    }
}