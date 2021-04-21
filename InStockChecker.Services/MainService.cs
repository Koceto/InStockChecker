using InStockChecker.Utility;
using Logger;
using SendGrid.Helpers.Mail;
using SMTP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace InStockChecker.Services
{
    public class MainService
    {
        private DateTime lastRun = DateTime.Now;
        private readonly NotifyIcon trayIcon = new NotifyIcon();
        private readonly ToolStripItem status = new ToolStripLabel();
        private readonly Timer mainTimer = new Timer();
        private readonly Timer statusTimer = new Timer();
        private readonly Dictionary<string, string> availabilityHistory = new Dictionary<string, string>();
        private readonly string emailRecipient = ConfigurationManager.AppSettings.Get(AppConfig.SendGridSendToEmail);
        private readonly string itemNrToCheck = ConfigurationManager.AppSettings.Get(AppConfig.ItemNrToCheck);

        public MainService()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += Exit;

            CreateTrayIcon();
            AddContextualMenu();
        }

        public void Run(int interval = 20 * 60 * 1000)
        {
            this.mainTimer.Interval = interval;
            this.mainTimer.Tick += this.Run;

            this.statusTimer.Interval = 1000;
            this.statusTimer.Tick += this.UpdateStatus;

            LogService logService = new LogService();
            StreamWriter logStream = logService.GetStream();
            logStream.AutoFlush = true;

            Console.SetOut(logStream);

            this.mainTimer.Start();
            this.statusTimer.Start();
            Application.Run();
        }

        private void Run(object myObject, EventArgs eventArgs)
        {
            this.lastRun = DateTime.Now;

            if (this.availabilityHistory.Count > 0)
            {
                Console.WriteLine(string.Join(Environment.NewLine, availabilityHistory.Select(h => h.Value)));
            }

            Console.WriteLine($"Running availability check at \'{DateTime.Now}\'");
            CheckJerryCaseAvailability(this.OnSuccess, availabilityHistory);
        }

        private void SendNotification(string title, string body, ToolTipIcon toolTipIcon = ToolTipIcon.Info, int timeout = 2000)
        {
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipText = body;
            trayIcon.BalloonTipIcon = toolTipIcon;
            trayIcon.ShowBalloonTip(timeout);
        }

        private void CheckJerryCaseAvailability(Action callback, Dictionary<string, string> availabilityHistory)
        {
            AvailabilityCheckerService availabilityChecker = new AvailabilityCheckerService();
            bool isAvailable = false;

            try
            {
                isAvailable = availabilityChecker.CheckItemAvailability(itemNrToCheck);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR during execution! \'{e.Message}\'");
            }

            Console.BackgroundColor = ConsoleColor.DarkGreen;
            if (!isAvailable)
            {
                Console.BackgroundColor = ConsoleColor.Red;
            }
            Console.WriteLine($"Product {this.GetAnswer(isAvailable)} available!");

            Console.BackgroundColor = ConsoleColor.Black;

            if (isAvailable)
            {
                if (!availabilityHistory.ContainsKey(itemNrToCheck))
                {
                    availabilityHistory[itemNrToCheck] = "";
                }
                availabilityHistory[itemNrToCheck] = $"JerryRigEveryting case was last available at: \'{DateTime.Now:dd-MM-yyyy hh:mm:ss}\'";

                callback();
            }
        }

        private string GetAnswer(bool isAvailable)
        {
            return isAvailable ? "..OMG, OMG, OMG ISSS, I CAN'T BELIEVE IT IT ACTUALLY IS" : "is not";
        }

        private void OnSuccess()
        {
            SendGridService sendGrid = new SendGridService();
            string subject = "Jerry Case is available!";
            string body = "Oneplus NORD JerryRigEverything case is available!";

            EmailAddress recipient = new EmailAddress(emailRecipient);

            Console.WriteLine("Sending email notification!");
            SendGrid.Response response = sendGrid.SendEmail(subject, body, body, recipient).GetAwaiter().GetResult();

            Console.WriteLine("Sending windows notification!");
            this.SendNotification(subject, body);
        }

        private void CreateTrayIcon()
        {
            trayIcon.Text = nameof(InStockChecker);
            trayIcon.Icon = new Icon("./icons/icon.ico");
            trayIcon.Visible = true;
        }

        private void AddContextualMenu()
        {
            this.trayIcon.ContextMenuStrip = new ContextMenuStrip();
            this.trayIcon.ContextMenuStrip.Items.Add(this.status);
            this.trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            this.trayIcon.ContextMenuStrip.Items.Add("Exit", new Bitmap("./icons/exit.ico"), Exit);
        }

        private void Exit(object sender, EventArgs eventArgs)
        {
            trayIcon.Dispose();
            Application.Exit();
        }

        private void UpdateStatus(object myObject, EventArgs eventArgs)
        {
            TimeSpan remaining = this.lastRun.AddMilliseconds(this.mainTimer.Interval) - DateTime.Now;
            this.status.Text = $"Next check in: {remaining.ToString(@"hh\:mm\:ss")}";
        }
    }
}