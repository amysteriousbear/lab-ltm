using System.Net;
using System.Net.Sockets;

namespace ChatServerGUI
{
    public partial class Server : Form
    {
        class ClientInfo
        {
            public string Name = "";
            public Socket Sock;
            public StreamWriter Writer;
            // Nhiều luồng có thể cùng gửi tới 1 client -> phải khoá
            public void Send(string line)
            {
                lock (Writer) { Writer.WriteLine(line); Writer.Flush(); }
            }
        }

        Socket listener;
        readonly Dictionary<string, ClientInfo> clients = new();
        readonly object lockObj = new();

        public Server() { InitializeComponent(); }

        // ---------- tiện ích UI ----------
        void RunUI(Action a)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { Invoke(a); } catch (ObjectDisposedException) { } catch (InvalidOperationException) { }
        }
        void Log(string s) => RunUI(() => txtLog.AppendText(s + Environment.NewLine));

        // ---------- khởi động ----------
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, 1308));
                listener.Listen(10);
            }
            catch (SocketException ex) { MessageBox.Show(ex.Message); return; }

            btnStart.Enabled = false;
            Log("Server lang nghe cong 1308");
            new Thread(AcceptLoop) { IsBackground = true }.Start();
        }

        void AcceptLoop()
        {
            while (true)
            {
                Socket s;
                try { s = listener.Accept(); } catch { break; }
                new Thread(() => HandleClient(s)) { IsBackground = true }.Start();
            }
        }

        // ---------- mỗi client 1 luồng ----------
        void HandleClient(Socket sock)
        {
            ClientInfo me = null;
            try
            {
                var ns = new NetworkStream(sock);
                var reader = new StreamReader(ns);
                var writer = new StreamWriter(ns);

                // Dòng đầu tiên bắt buộc là LOGIN|tên
                string[] lp = (reader.ReadLine() ?? "").Split('|');
                if (lp[0] != "LOGIN" || lp.Length != 2) return;

                string name = lp[1].Trim();
                string err = null;
                if (name == "" || name.Contains(',') || name.ToUpper() == "SERVER")
                    err = "Ten khong hop le";

                var info = new ClientInfo { Name = name, Sock = sock, Writer = writer };
                lock (lockObj)
                {
                    if (err == null && clients.ContainsKey(name)) err = "Ten da duoc dung";
                    if (err == null) clients[name] = info;
                }
                if (err != null) { info.Send("ERROR|" + err); return; }

                me = info;
                me.Send("OK");
                Log($"{name} ket noi tu {sock.RemoteEndPoint}");
                BroadcastList();

                string line;
                while ((line = reader.ReadLine()) != null)
                    Route(me, line);
            }
            catch (IOException) { }                       // client tắt đột ngột
            catch (Exception ex) { Log("Loi: " + ex.Message); }
            finally
            {
                if (me != null)
                {
                    lock (lockObj) clients.Remove(me.Name);
                    Log($"{me.Name} da ngat ket noi");
                    BroadcastList();                      // đóng vòng phản hồi
                }
                sock.Close();
            }
        }

        // ---------- định tuyến ----------
        void Route(ClientInfo from, string line)
        {
            string[] p = line.Split('|');
            if (p.Length < 2) return;
            string cmd = p[0], to = p[1];
            string rest = string.Join("|", p.Skip(2));

            if (cmd == "MSG")
            {
                if (to == "SERVER") { Log($"[{from.Name}] {rest}"); return; }
                if (!Forward(to, $"MSG|{from.Name}|{rest}"))
                    from.Send($"ERROR|{to} khong online");
            }
            else if (cmd.StartsWith("FILE_"))
            {
                if (cmd == "FILE_BEGIN") Log($"{from.Name} -> {to}: gui file ({rest})");
                // Server ghi đè "đến" thành "từ": client không giả danh được
                if (!Forward(to, $"{cmd}|{from.Name}|{rest}") && cmd != "FILE_DATA")
                    from.Send($"ERROR|{to} khong online");
            }
        }

        bool Forward(string to, string line)
        {
            ClientInfo target;
            lock (lockObj) { if (!clients.TryGetValue(to, out target)) return false; }
            try { target.Send(line); return true; } catch { return false; }
        }

        // ---------- câu c: gửi danh sách online ----------
        void BroadcastList()
        {
            List<ClientInfo> snapshot; string names;
            lock (lockObj)
            {
                snapshot = clients.Values.ToList();
                names = string.Join(",", clients.Keys);
            }
            foreach (var c in snapshot)
                try { c.Send("LIST|" + names); } catch { }

            RunUI(() =>
            {
                lstClients.Items.Clear();
                foreach (var n in names.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    lstClients.Items.Add(n);
            });
        }

        // ---------- câu b: server chat với client ----------
        private void btnSend_Click(object sender, EventArgs e)
        {
            string text = txtMsg.Text.Trim();
            if (text == "") return;

            List<ClientInfo> targets;
            string label;
            lock (lockObj)
            {
                if (lstClients.SelectedItem is string name && clients.TryGetValue(name, out var c))
                { targets = new() { c }; label = name; }
                else
                { targets = clients.Values.ToList(); label = "TAT CA"; }
            }
            foreach (var t in targets)
                try { t.Send("MSG|SERVER|" + text); } catch { }

            Log($"Server -> {label}: {text}");
            txtMsg.Clear();
        }
    }
}