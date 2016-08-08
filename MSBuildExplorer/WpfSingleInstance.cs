//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="WpfSingleInstance.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    public static class WpfSingleInstance
    {
        internal static DispatcherTimer AutoExitAplicationIfStartupDeadlock;

        internal enum SingleInstanceMode
        {
            /// <summary>
            /// Do nothing.
            /// </summary>
            NotInited = 0,

            /// <summary>
            /// Every user can have own single instance.
            /// </summary>
            ForEveryUser,
        }

        public static void RemoveApplicationsStartupDeadlockForStartupCrushedWindows()
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
            {
                AutoExitAplicationIfStartupDeadlock = new DispatcherTimer(
                        TimeSpan.FromSeconds(6),
                        DispatcherPriority.ApplicationIdle,
                        (o, args) =>
                        {
                            if (Application.Current.Windows.Cast<Window>().Count(window => !double.IsNaN(window.Left)) == 0)
                            {
                                // For that exit no interceptions.
                                Environment.Exit(0);
                            }
                        },
                            Application.Current.Dispatcher);
            }),
                                                                                      DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Processing single instance in <see cref="SingleInstanceMode"/> <see cref="SingleInstanceMode.ForEveryUser"/> mode.
        /// </summary>
        internal static void Make()
        {
            Make(SingleInstanceMode.ForEveryUser);
        }

        internal static void Make(SingleInstanceMode singleInstanceModes)
        {
            var appName = Application.Current.GetType().Assembly.ManifestModule.ScopeName;

            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var keyUserName = windowsIdentity != null ? windowsIdentity.User.ToString() : string.Empty;

            // Be careful! Max 260 chars!
            var eventWaitHandleName = string.Format("{0}{1}", appName, singleInstanceModes == SingleInstanceMode.ForEveryUser ? keyUserName : string.Empty);

            try
            {
                using (var eventWaitHandle = EventWaitHandle.OpenExisting(eventWaitHandleName))
                {
                    // It informs first instance about other startup attempting.
                    eventWaitHandle.Set();
                }

                Environment.Exit(0);
            }
            catch
            {
                // It's first instance.

                // Register EventWaitHandle.
                using (var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventWaitHandleName))
                {
                    ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, OtherInstanceAttemptedToStart, null, Timeout.Infinite, false);
                }

                RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
            }
        }

        private static void OtherInstanceAttemptedToStart(object state, bool timedOut)
        {
            RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                                                      {
                                                                          try
                                                                          {
                                                                              Application.Current.MainWindow.Activate();
                                                                              Application.Current.MainWindow.Topmost = true;
                                                                              Application.Current.MainWindow.Topmost = false;

                                                                              Application.Current.MainWindow.Focus();
                                                                          }
                                                                          catch (Exception ex)
                                                                          {
                                                                              MessageBox.Show(ex.ToString());
                                                                          }
                                                                      }));
        }
    }
}
