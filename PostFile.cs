using System;
using CommandLine;
using System.Windows.Forms;

namespace test
{
    static class PostFile
    {


        static private void showUsage(string extra = null)
        {
            string msg = "Usage:\nPostFile <URL> <InputFilePath> [<InputFilePath>...]";
            if (extra != null)
            {
                msg += "\n\n" + extra;
            }
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);            
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            int result = 1;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if ((o.URL == null))
                    {
                        showUsage("URL argument is mandatory.");
                        return;
                    }
                    if ((o.URL.Trim().Length == 0))
                    {
                        showUsage("URL argument must not be empty.");
                        return;
                    }
                    Main main = new Main(o);
                    Application.Run(main);
                    result = main.exitCode;
                })
                .WithNotParsed<Options>(o =>
                {
                    showUsage();
                });
            return result;
        }
    }
}
