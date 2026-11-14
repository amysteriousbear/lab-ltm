using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CalcServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, 1308));
            listener.Listen(10);
            Console.WriteLine("Server dang lang nghe tren cong 1308...");

            while (true)
            {
                Socket worker = listener.Accept();
                Console.WriteLine($"Client ket noi: {worker.RemoteEndPoint}");
                Thread t = new Thread(() => HandleClient(worker));
                t.IsBackground = true;   // tat server thi cac thread tu dong dung
                t.Start();
            }
        }

        static void HandleClient(Socket worker)
        {
            try
            {
                using var stream = new NetworkStream(worker);
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream);

                string request;
                // ReadLine tra ve null khi client dong ket noi
                while ((request = reader.ReadLine()) != null)
                {
                    Console.WriteLine($"[{worker.RemoteEndPoint}] {request}");
                    writer.WriteLine(Process(request));
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi client: " + ex.Message);
            }
            finally
            {
                worker.Close();
                Console.WriteLine("Client da ngat.");
            }
        }

        static string Process(string request)
        {
            string[] p = request.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (p.Length == 0) return "ERROR: yeu cau rong";

            switch (p[0].ToUpper())
            {
                case "CALC": return Calc(p);
                case "CHECK": return Check(p);
                default: return "ERROR: lenh khong hop le (CALC / CHECK)";
            }
        }

        static string Calc(string[] p)
        {
            if (p.Length != 4) return "ERROR: cu phap CALC a op b";

            if (!double.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double a) ||
                !double.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double b))
                return "ERROR: toan hang khong hop le";

            switch (p[2])
            {
                case "+": return (a + b).ToString(CultureInfo.InvariantCulture);
                case "-": return (a - b).ToString(CultureInfo.InvariantCulture);
                case "*": return (a * b).ToString(CultureInfo.InvariantCulture);
                case "/":
                    if (b == 0) return "ERROR: chia cho 0";
                    return (a / b).ToString(CultureInfo.InvariantCulture);
                default: return "ERROR: phep toan khong ho tro";
            }
        }

        static string Check(string[] p)
        {
            if (p.Length != 2) return "ERROR: cu phap CHECK n";
            if (!long.TryParse(p[1], out long n)) return "ERROR: khong phai so nguyen";

            string parity = (n % 2 == 0) ? "chan" : "le";
            string prime = IsPrime(n) ? "la so nguyen to" : "khong phai so nguyen to";
            return $"{n} la so {parity}, {prime}";
        }

        static bool IsPrime(long n)
        {
            if (n < 2) return false;
            if (n % 2 == 0) return n == 2;
            for (long i = 3; i * i <= n; i += 2)
                if (n % i == 0) return false;
            return true;
        }
    }
}