using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyGarry
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Must run before any embedded dependency is touched. The real body lives in
            // Run() so that JIT-compiling it cannot need a dependency before the hook is up.
            EmbeddedAssemblies.Install();
            Run();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Collect the API keys, resume and job description before the interview window
            // opens. Exiting the setup screen without starting cancels the whole run.
            using (var setup = new Setup())
            {
                if (setup.ShowDialog() != DialogResult.OK)
                    return;
            }

            // Hard gate: no keys, no app. The setup screen already enforces this, so
            // reaching here means the save silently failed.
            if (!ApiKeyStore.HasSavedKeys())
            {
                MessageBox.Show(
                    "EasyGarry needs your own OpenAI and Deepgram API keys to run.",
                    "API keys required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Application.Run(new Interview());
        }
    }
}
