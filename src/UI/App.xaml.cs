using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BetterAccounting.UI.Models;

namespace BetterAccounting.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            ThemeManager.Initialize();
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorReporter.Show("Unexpected application error", e.Exception);
            e.Handled = true;
        }

        private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ErrorLog.Write("Unhandled fatal exception", ex);
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception != null)
                ErrorLog.Write("Unobserved task exception", e.Exception);
            e.SetObserved();
        }
    }
}
