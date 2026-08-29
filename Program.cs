using System.Diagnostics;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Forms;

namespace TraceLock.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowFatalError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex) ShowFatalError(ex);
        };

        try
        {
            AppPaths.Initialize();
            Database.Initialize();

            while (true)
            {
                using var login = new LoginForm();
                if (login.ShowDialog() != DialogResult.OK || login.AuthenticatedUser is null) break;
                using var main = new MainForm(login.AuthenticatedUser);
                Application.Run(main);
                break;
            }
        }
        catch (Exception ex)
        {
            ShowFatalError(ex);
        }
    }

    private static void ShowFatalError(Exception exception)
    {
        try { Debug.WriteLine(exception); } catch { }
        MessageBox.Show(
            $"TraceLock encountered an unexpected problem.\n\n{exception.Message}\n\n" +
            "If this happens again, close TraceLock and start it again.",
            "TraceLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
