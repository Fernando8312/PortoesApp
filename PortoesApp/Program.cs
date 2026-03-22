using System;
using System.Windows.Forms;

namespace PortoesApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Form1 limpo, sem precisar de resx acoplado!
        }
    }
}
