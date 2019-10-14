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
        private Options opts;
        CancellationTokenSource cancellationSource = new CancellationTokenSource();


        public Main(Options opts)
        {
            InitializeComponent();
            this.opts = opts;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            exitCode = 1;
            cancellationSource.Cancel();
            Close();
        }

        async Task doUpload()
        {
            using (var content = new MultipartFormDataContent())
            {
                List<FileStream> streams = new List<FileStream>();
                try
                {
                    foreach (string fPath in opts.InputFiles)
                    {
                        FileStream stream = new FileStream(fPath, FileMode.Open, FileAccess.Read);
                        streams.Add(stream);
                        content.Add(new StreamContent(stream), fPath);
                    }
                    var progressContent = new ProgressableStreamContent(
                         content,
                         4096,
                         (sent, total) =>
                         {
                             throw new Exception("FooBar");
                             //double percent = progressBar.Maximum * sent / total;
                             //progressBar.Value = (int)percent;
                         });

                    using (var client = new HttpClient())
                    {
                        //client.Timeout = TimeSpan.FromMinutes(100); // 
                        using (var response = await client.PostAsync(opts.URL, progressContent, cancellationSource.Token).ConfigureAwait(false))
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
                            }
                        Close();
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

        private void Main_Load(object sender, EventArgs e)
        {
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !cancellationSource.IsCancellationRequested;
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            doUpload();
        }
    }
}
