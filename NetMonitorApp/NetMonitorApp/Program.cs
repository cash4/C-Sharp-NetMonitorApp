using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

namespace NetMonitorApp
{
    class VPNAndModemMonitor
    {
        private System.Diagnostics.Process process1;
        private System.Diagnostics.Process process3;
        private System.Diagnostics.Process process4;
        private System.Diagnostics.Process process5;
        private System.Diagnostics.Process player;
        private bool playerStarted;
        private System.ServiceProcess.ServiceController ipsecdController;
        private System.ServiceProcess.ServiceController ikedController;

        System.Net.IPAddress vpnEndPoint;
        System.Net.NetworkInformation.Ping vpnEndPointPinger;
        System.Net.NetworkInformation.PingReply lastPingReply;
        int failCount;
        bool modemActive;
        int modemRestartFailCount;
        int modemFailCount;
        System.Timers.Timer myTimer;
        int tickCounter;

        System.IO.FileStream serviceLog;

        public VPNAndModemMonitor()
        {
            //VPN End Point IP is currently set to General NOC
            vpnEndPoint = System.Net.IPAddress.Parse("192.168.20.1");
            vpnEndPointPinger = new System.Net.NetworkInformation.Ping();
            failCount = 0;
            modemActive = false;
            modemFailCount = 0;
            playerStarted = false;
            tickCounter = 0;
            modemRestartFailCount = 0;

            AppendToLog("Initialized the NetMonitor service\n");

            this.process1 = new System.Diagnostics.Process();
            this.process3 = new System.Diagnostics.Process();
            this.process4 = new System.Diagnostics.Process();
            this.player = new System.Diagnostics.Process();

            this.myTimer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.myTimer)).BeginInit();
            // 
            // process1
            // 
            this.process1.EnableRaisingEvents = true;
            this.process1.StartInfo.Arguments = "-r 209.118.225.23 -a";
            this.process1.StartInfo.Domain = "";
            this.process1.StartInfo.FileName = "c:\\Program Files\\shrewsoft\\vpn client\\ipsecc.exe";
            this.process1.StartInfo.LoadUserProfile = false;
            this.process1.StartInfo.Password = null;
            this.process1.StartInfo.StandardErrorEncoding = null;
            this.process1.StartInfo.StandardOutputEncoding = null;
            this.process1.StartInfo.CreateNoWindow = true;
            this.process1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            this.process1.StartInfo.UserName = "";
            this.process1.StartInfo.UseShellExecute = false;
            this.process1.StartInfo.WorkingDirectory = "c:\\MriTools\\NetMonitor";

            

            //this.player.EnableRaisingEvents = true;
            //this.player.StartInfo.Arguments = "/fullscreen c:\\MriContent\\Verifone_5.mp4";
            //this.player.StartInfo.Domain = "";
            //this.player.StartInfo.FileName = "c:\\Program Files\\Windows Media Player\\wmplayer.exe";
            //this.player.StartInfo.LoadUserProfile = false;
            //this.player.StartInfo.Password = null;
            //this.player.StartInfo.StandardErrorEncoding = null;
            //this.player.StartInfo.StandardOutputEncoding = null;
            //this.player.StartInfo.UserName = "";
            //this.player.StartInfo.UseShellExecute = false;
            //this.player.StartInfo.WorkingDirectory = "c:\\Program Files\\Windows Media Player";

            // 
            // process3
            // 
            this.process3.StartInfo.Domain = "";
            this.process3.StartInfo.FileName = "c:\\MriTools\\NetMonitor\\HSPARestart.bat";
            this.process3.StartInfo.LoadUserProfile = false;
            this.process3.StartInfo.Password = null;
            this.process3.StartInfo.StandardErrorEncoding = null;
            this.process3.StartInfo.StandardOutputEncoding = null;
            this.process3.StartInfo.UserName = "";
            this.process3.StartInfo.UseShellExecute = false;
            //// 
            //// process4
            //// 
            //this.process4.StartInfo.Domain = "";
            //this.process4.StartInfo.FileName = "c:\\Program Files\\Sprint\\Sprint SmartView\\SprintSV.exe";
            //this.process4.StartInfo.LoadUserProfile = false;
            //this.process4.StartInfo.Password = null;
            //this.process4.StartInfo.RedirectStandardError = true;
            //this.process4.StartInfo.RedirectStandardOutput = true;
            //this.process4.StartInfo.StandardErrorEncoding = null;
            //this.process4.StartInfo.StandardOutputEncoding = null;
            //this.process4.StartInfo.UserName = "";
            //this.process4.StartInfo.UseShellExecute = false;

            //process5 is executing a ping to Google (8.8.8.8), to ensure connectivity to the world wide web
            process5 = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c ping 8.8.8.8",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            

            this.ipsecdController = new System.ServiceProcess.ServiceController();
            this.ipsecdController.ServiceName = "ipsecd";

            this.ikedController = new System.ServiceProcess.ServiceController();
            this.ikedController.ServiceName = "iked";

            // 
            // myTimer
            // 
            this.myTimer.Enabled = true;
            this.myTimer.Interval = 35000;
            this.myTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);

            ((System.ComponentModel.ISupportInitialize)(this.myTimer)).EndInit();

            StartModem();
            
            
        }

        private void AppendToLog(String toAdd)
        {
            serviceLog = System.IO.File.OpenWrite(@"c:\MriTools\NetMonitor\netMonitor.log");
            serviceLog.Position = serviceLog.Length;

            System.IO.StreamWriter logStream = new System.IO.StreamWriter(serviceLog);

            logStream.WriteLine(toAdd);
            System.Console.WriteLine(toAdd);

            logStream.Flush();
            logStream.Close();
            serviceLog.Close();
        }

        private void StartModem()
        {
            //bool quit = false;
            //// try and start the process.
            //Process[] processes = Process.GetProcesses();
            //foreach (Process process in processes)
            //{
            //    if (process.ProcessName.ToLower().Contains("sprintsv"))
            //    {
            //        quit = true;
            //        AppendToLog("Found SprintSV.exe, no need to start it");
            //    }
            //}

            //if (!quit)
            //{
            //    try
            //    {
            //        if (process4.Start())
            //        {
            //            AppendToLog("Success starting " + process4.Id);
            //        }
            //        //System.Diagnostics.Process.Start(@"c:\Program Files\Sprint\Sprint SmartView\SprintSV.exe");
            //    }
            //    catch (Exception e)
            //    {
            //        AppendToLog("Exception starting modem=" + e.Message);
            //    }
            //}
            Console.WriteLine("Restarting modem...");
            process3.Start();
            

        }

        private void RestartModem()
        {
            /*try
            {
                process3.Start();
            }
            catch (Exception e)
            {
                AppendToLog("Exception Killing modem=" + e.Message);
            }*/
            StartModem();
        }

        private void StartVpn()
        {
            try
            {
                bool found = false;
                // try and start the process.
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName.ToLower().Contains("ipsecc"))
                    {
                        found = true;
                        AppendToLog("Found ipsecc.exe, terminating!");
                        process1 = process;
                    }
                }

                if (found)
                {
                    process1.Kill();
                }

                process1.Dispose();
                process1 = new System.Diagnostics.Process();
                this.process1.EnableRaisingEvents = true;
                this.process1.StartInfo.Arguments = "-r 209.118.225.23 -a";
                this.process1.StartInfo.Domain = "";
                this.process1.StartInfo.FileName = "c:\\Program Files\\shrewsoft\\vpn client\\ipsecc.exe";
                this.process1.StartInfo.LoadUserProfile = false;
                this.process1.StartInfo.Password = null;
                this.process1.StartInfo.StandardErrorEncoding = null;
                this.process1.StartInfo.StandardOutputEncoding = null;
                this.process1.StartInfo.UserName = "";
                this.process1.StartInfo.UseShellExecute = false;
                this.process1.StartInfo.WorkingDirectory = "c:\\MriTools\\NetMonitor";

                string svcStatus = ipsecdController.Status.ToString();
                if (svcStatus == "Running")
                {
                    AppendToLog("ipsecd running");
                    ipsecdController.Stop();
                }
                else
                {
                    AppendToLog("ipsecd stopped");
                    ipsecdController.Stop();
                }

                svcStatus = ikedController.Status.ToString();
                if (svcStatus == "Running")
                {
                    AppendToLog("iked running");
                    ikedController.Stop();
                }
                else
                {
                    AppendToLog("iked stopped");
                    ikedController.Stop();
                }

                System.Threading.Thread.Sleep(4000);
            }
            catch (Exception e)
            {
                AppendToLog("Failed to Kill VPN, message=" + e.Message);
            }

            try
            {
                ikedController.Start();
                System.Threading.Thread.Sleep(4000);
                AppendToLog("iked started, status=" + ikedController.Status.ToString());
                ipsecdController.Start();
                System.Threading.Thread.Sleep(4000);
                AppendToLog("ipsecd started, status=" + ipsecdController.Status.ToString());
                if (process1.Start())
                {
                    AppendToLog("Success starting " + process1.ProcessName);
                }
                else
                {
                    AppendToLog("Failed to start " + process1.ProcessName);
                }
                System.Threading.Thread.Sleep(10000);
                //if (!playerStarted)
                //{
                //    if (!player.Start())
                //    {
                //        AppendToLog("Player failed to start!!!!!");
                //    }
                //    else
                //    {
                //        AppendToLog("Player started successfully");
                //        playerStarted = true;
                //    }
                //}
            }
            catch (Exception e)
            {
                AppendToLog("Failed to start VPN, message=" + e.Message);
            }
        }

        void process1_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            AppendToLog("Received error data starting vpn, message=" + e.Data);
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            int retries = 3;
            System.Net.NetworkInformation.PingReply pingReply;

            process5.Start();
            while (!process5.StandardOutput.EndOfStream)
            {
                string line = process5.StandardOutput.ReadToEnd();
                if (line.Contains("failed") || line.Contains("unreachable"))
                {
                    StartModem();
                }



            }

            //if (++tickCounter > 5)
            //{
            //    if (!playerStarted)
            //    {
            //        if (!player.Start())
            //        {
            //            AppendToLog("Player failed to start!!!!!");
            //        }
            //        else
            //        {
            //            AppendToLog("Player started successfully");
            //            playerStarted = true;
            //        }
            //    }

            //}

            if (!modemActive)
            {
                System.Net.IPAddress googlePublicDns;
                System.Net.NetworkInformation.Ping publicDnsPinger;
                googlePublicDns = System.Net.IPAddress.Parse("8.8.8.8");
                publicDnsPinger = new System.Net.NetworkInformation.Ping();

                //
                pingReply = publicDnsPinger.Send(googlePublicDns);
                while (retries-- > 0 && !modemActive)
                {
                    //AppendToLog("Modem Ping reply status=" + pingReply.Status.ToString());
                    switch (pingReply.Status)
                    {
                        case System.Net.NetworkInformation.IPStatus.Success:
                            lastPingReply = pingReply;
                            failCount = 0;
                            modemActive = true;
                            StartVpn();
                            modemRestartFailCount = 0;
                            
                            break;
                        case System.Net.NetworkInformation.IPStatus.TimedOut:
                        case System.Net.NetworkInformation.IPStatus.IcmpError:
                        default:
                            //try again...maybe
                            //May need to consider situations where a player reboot is required...though perhaps that can be handled
                            //by the PMS
                            break;

                    }
                    pingReply = publicDnsPinger.Send(googlePublicDns);
                }

                if (pingReply.Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    modemFailCount++;
                    if (modemRestartFailCount > 100)
                    {
                        AppendToLog("Attempting reboot to recover data network");
                        System.Diagnostics.Process.Start("ShutDown", "/r");
                    }
                    if (modemFailCount >= 10)
                    {
                        AppendToLog("Modem ping failed too many times, resetting");
                        modemFailCount = 0;
                        modemRestartFailCount++;
                        //force a re-check on the status of the modem...
                        modemActive = false;
                        RestartModem();
                    }
                }
            }
            else
            {
                pingReply = vpnEndPointPinger.Send(vpnEndPoint);
                bool pingSuccess = false;
                while (retries-- > 0 && !pingSuccess)
                {
                    //AppendToLog("VPN Ping reply status=" + pingReply.Status.ToString());
                    switch (pingReply.Status)
                    {
                        case System.Net.NetworkInformation.IPStatus.Success:
                            lastPingReply = pingReply;
                            failCount = 0;
                            pingSuccess = true;
                            
                            break;                           
                        case System.Net.NetworkInformation.IPStatus.TimedOut:
                        case System.Net.NetworkInformation.IPStatus.IcmpError:
                        default:
                            //try again...maybe
                            //May need to consider situations where a player reboot is required...though perhaps that can be handled
                            //by the PMS
                            break;

                    }
                    pingReply = vpnEndPointPinger.Send(vpnEndPoint);
                }

                if (pingReply.Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    failCount++;
                    if (failCount >= 20)
                    {
                        AppendToLog("VPN ping failed too many times, resetting");
                        failCount = 0;
                        //force a re-check on the status of the modem...
                        modemActive = false;
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            VPNAndModemMonitor vpnMonitor = new VPNAndModemMonitor();

            while (true) ;
        }
    }
}
