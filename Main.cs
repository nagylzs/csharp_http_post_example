using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test
{

    public partial class Main : Form
    {

        public int exitCode = 1;
        private readonly Options opts;
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();


        public Main(Options opts)
        {
            InitializeComponent();
            this.opts = opts;
        }


        async Task DoUpload()
        {
            using (var content = new MultipartFormDataContent())
            {
                List<FileStream> streams = new List<FileStream>();
                try
                {
                    int idx = 0;
                    foreach (string fPath in opts.InputFiles)
                    {
                        FileStream stream = new FileStream(fPath, FileMode.Open, FileAccess.Read);
                        streams.Add(stream);
                        content.Add(new StreamContent(stream), fPath);
                        idx += 1;
                    }
                    var progressContent = new ProgressableStreamContent(
                         content,
                         4096,
                         (sent, total) =>
                         {
                             Invoke((Action)(() =>
                             {
                                 double percent = progressBar.Maximum * sent / total;
                                 progressBar.Value = (int)percent;
                             }));
                         });
                    using (var client = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11;

                        //client.Timeout = TimeSpan.FromMinutes(100); // 
                        try
                        {
                            using (var response = await client.PostAsync(opts.URL, progressContent, cancellationSource.Token))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    exitCode = 0;
                                }
                                else
                                {
                                    MessageBox.Show(
                                        response.Content.ToString(),
                                        "Error " + response.StatusCode,
                                        MessageBoxButtons.OK, MessageBoxIcon.Error
                                   );
                                    exitCode = 2;
                                }
                            }
                        }
                        catch (HttpRequestException requestException)
                        {
                            if (requestException.InnerException is System.Net.WebException webException)
                            {
                                MessageBox.Show(
                                    webException.Message,
                                    "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error
                               );
                            }
                            else
                            {
                                MessageBox.Show(
                                    requestException.Message,
                                    "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error
                               );
                            }
                            exitCode = 3;
                        }
                    };
                }
                finally
                {
                    foreach (FileStream stream in streams)
                    {
                        stream.Close();
                    }
                }
            }

        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !cancellationSource.IsCancellationRequested;
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            DoUpload().ContinueWith((result) =>
            {
                cancellationSource.Cancel();
                Invoke((Action)(() =>
                {
                    Application.Exit();
                }));
            });
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            exitCode = 1;
            cancellationSource.Cancel();
            Application.Exit();
        }
    }
}
