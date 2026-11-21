using System.Net;
using System.Net.Sockets;

namespace ChatClientGUI
{
    public partial class Form1 : Form
    {
        Socket sock;
        StreamWriter writer;
        readonly object sendLock = new();
        string myName = "";
        // Chỉ luồng nhận đụng vào dictionary này -> không cần khoá.
        // Giá trị null = người dùng từ chối nhận file đó
        readonly Dictionary<string, FileStream> incoming = new();

        public Form1()
        {
            InitializeComponent();
            txtIP.Text = "127.0.0.1";
            btnSend.Enabled = btnSendFile.Enabled = false;
        }

        // ---------- tiện ích ----------
        void RunUI(Action a)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { Invoke(a); } catch (ObjectDisposedException) { } catch (InvalidOperationException) { }
        }
        void Append(string s) => RunUI(() => txtChat.AppendText(s + Environment.NewLine));

        void Send(string line)
        {
            lock (sendLock) { writer.WriteLine(line); writer.Flush(); }
        }

        // ---------- kết nối ----------
        private void btnConnect_Click(object sender, EventArgs e)
        {
            myName = txtName.Text.Trim();
            if (myName == "") { MessageBox.Show("Nhap ten"); return; }
            try
            {
                sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(new IPEndPoint(IPAddress.Parse(txtIP.Text.Trim()), 1308));
                var ns = new NetworkStream(sock);
                writer = new StreamWriter(ns);
                var reader = new StreamReader(ns);

                Send("LOGIN|" + myName);
                btnConnect.Enabled = false;
                new Thread(() => ReceiveLoop(reader)) { IsBackground = true }.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong ket noi duoc: " + ex.Message);
                sock?.Close();
            }
        }

        // ---------- luồng nhận ----------
        void ReceiveLoop(StreamReader reader)
        {
            try
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try { HandleLine(line); }
                    catch (Exception ex) { Append("[Loi xu ly] " + ex.Message); }
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }

            foreach (var fs in incoming.Values) fs?.Dispose();
            incoming.Clear();
            RunUI(() =>
            {
                lstUsers.Items.Clear();
                btnConnect.Enabled = true;
                btnSend.Enabled = btnSendFile.Enabled = false;
            });
            Append("--- Mat ket noi ---");
        }

        void HandleLine(string line)
        {
            string[] p = line.Split('|');
            switch (p[0])
            {
                case "OK":
                    Append("Da ket noi voi server.");
                    RunUI(() => btnSend.Enabled = btnSendFile.Enabled = true);
                    break;
                case "ERROR":
                    Append("[Loi] " + string.Join("|", p.Skip(1)));
                    break;
                case "LIST":
                    UpdateUserList(p.Length > 1 ? p[1] : "");
                    break;
                case "MSG" when p.Length >= 3:
                    Append($"{p[1]}: {string.Join("|", p.Skip(2))}");
                    break;
                case "FILE_BEGIN" when p.Length >= 4:
                    OnFileBegin(p[1], p[2], p[3]);
                    break;
                case "FILE_DATA" when p.Length >= 3:
                    OnFileData(p[1], p[2]);
                    break;
                case "FILE_END" when p.Length >= 2:
                    OnFileEnd(p[1]);
                    break;
            }
        }

        // ---------- câu c: cập nhật danh sách online ----------
        void UpdateUserList(string csv)
        {
            RunUI(() =>
            {
                string old = lstUsers.SelectedItem as string;
                lstUsers.Items.Clear();
                lstUsers.Items.Add("SERVER");
                foreach (string n in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    if (n != myName) lstUsers.Items.Add(n);
                int i = old == null ? 0 : lstUsers.Items.IndexOf(old);
                lstUsers.SelectedIndex = i >= 0 ? i : 0;
            });
        }

        // ---------- câu b, d: gửi chat ----------
        private void btnSend_Click(object sender, EventArgs e)
        {
            string target = lstUsers.SelectedItem as string;
            string text = txtMsg.Text.Trim();
            if (target == null || text == "") return;
            try
            {
                Send($"MSG|{target}|{text}");
                Append($"Toi -> {target}: {text}");
                txtMsg.Clear();
            }
            catch (Exception ex) { Append("[Gui that bai] " + ex.Message); }
        }

        // ---------- câu e: gửi file ----------
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            string target = lstUsers.SelectedItem as string;
            if (target == null || target == "SERVER")
            { MessageBox.Show("Chon 1 client trong danh sach de gui file"); return; }

            using var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string path = ofd.FileName;
            btnSendFile.Enabled = false;
            Task.Run(() => SendFile(target, path));   // luồng riêng, UI không đơ
        }

        void SendFile(string target, string path)
        {
            try
            {
                var fi = new FileInfo(path);
                Append($"Dang gui '{fi.Name}' ({fi.Length} bytes) toi {target}...");
                Send($"FILE_BEGIN|{target}|{fi.Name}|{fi.Length}");

                byte[] buf = new byte[32 * 1024];
                using var fs = File.OpenRead(path);
                int n;
                while ((n = fs.Read(buf, 0, buf.Length)) > 0)
                    Send($"FILE_DATA|{target}|{Convert.ToBase64String(buf, 0, n)}");

                Send($"FILE_END|{target}");
                Append("Gui xong.");
            }
            catch (Exception ex) { Append("[Gui file that bai] " + ex.Message); }
            finally { RunUI(() => btnSendFile.Enabled = true); }
        }

        // ---------- câu e: nhận file ----------
        void OnFileBegin(string from, string fname, string size)
        {
            string savePath = null;
            RunUI(() =>
            {
                var ask = MessageBox.Show($"{from} gui file '{fname}' ({size} bytes). Nhan khong?",
                                          "File", MessageBoxButtons.YesNo);
                if (ask != DialogResult.Yes) return;
                // GetFileName: không tin tên file từ mạng (chặn ..\..\ )
                using var dlg = new SaveFileDialog { FileName = Path.GetFileName(fname) };
                if (dlg.ShowDialog() == DialogResult.OK) savePath = dlg.FileName;
            });

            if (incoming.TryGetValue(from, out var old)) old?.Dispose();
            incoming[from] = savePath == null ? null : new FileStream(savePath, FileMode.Create);
        }

        void OnFileData(string from, string b64)
        {
            if (incoming.TryGetValue(from, out var fs) && fs != null)
            {
                byte[] data = Convert.FromBase64String(b64);
                fs.Write(data, 0, data.Length);
            }
        }

        void OnFileEnd(string from)
        {
            if (incoming.TryGetValue(from, out var fs))
            {
                fs?.Dispose();
                incoming.Remove(from);
                if (fs != null) Append($"Da nhan xong file tu {from}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sock?.Close();
        }
    }
}