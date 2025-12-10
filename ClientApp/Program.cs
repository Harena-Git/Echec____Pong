using ClientApp.Forms;

namespace ClientApp;

/// <summary>
/// Point d'entrée de l'application client (Windows Forms)
/// </summary>
class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        System.Windows.Forms.Application.Run(new ConnectionForm());
    }
}