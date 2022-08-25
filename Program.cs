using System;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Management;

internal class Program
{
    public static string[] priorities;

    static void Main()
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

        Console.Title = "WinPriority";
		Console.WriteLine(@"

 __          ___       _____      _            _ _         
 \ \        / (_)     |  __ \    (_)          (_) |        
  \ \  /\  / / _ _ __ | |__) | __ _  ___  _ __ _| |_ _   _ 
   \ \/  \/ / | | '_ \|  ___/ '__| |/ _ \| '__| | __| | | |
    \  /\  /  | | | | | |   | |  | | (_) | |  | | |_| |_| |
     \/  \/   |_|_| |_|_|   |_|  |_|\___/|_|  |_|\__|\__, |
                                                      __/ |
                                                     |___/ 

");

        if (!(new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator))
        {
            Logger.LogError("Failed to run WinPriority. Run the program with Administrator privileges.");
            Console.ReadLine();
            return;
        }

        Logger.LogInfo("Welcome to WinPriority. This utility program will help you to boost your daily performances mostly on games & audio feedback programs by manipulating Win32 Priority.");

        if (!System.IO.File.Exists("priorities.list"))
        {
            System.IO.File.WriteAllText("priorities.list", "");
        }

        priorities = File.ReadAllLines("priorities.list");

        foreach (string priority in priorities)
        {
            Logger.LogWarning("Checking for process '" + priority.ToLower() + "'.");
            bool found = false;

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (process.ProcessName.ToLower().Equals(priority.ToLower()))
                    {
                        found = true;

                        try
                        {
                            process.PriorityClass = ProcessPriorityClass.RealTime;

                            try
                            {
                                foreach (ProcessThread processThread in process.Threads)
                                {
                                    try
                                    {
                                        processThread.PriorityLevel = ThreadPriorityLevel.Highest;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            catch
                            {

                            }

                            Logger.LogInfo("Succesfully modified the priority of '" + priority.ToLower() + "'.");
                        }
                        catch
                        {
                            Logger.LogError("Failed to modify the priority of '" + priority.ToLower() + "'.");
                        }
                    }
                }
                catch
                {

                }
            }

            if (!found)
            {
                Logger.LogError("Could not find the process '" + priority.ToLower() + "'.");
            }
        }

        Thread thread = new Thread(ProcessMonitor);
        thread.Priority = ThreadPriority.Highest;
        thread.Start();

        if (priorities.Length == 0)
        {
            Logger.LogInfo("Now listening for opening processes.");
        }
        else
        {
            Logger.LogInfo("Finished manipulating priorities of processes in the file. Now listening for opening processes.");
        }

        while (true)
        {
            Console.ReadLine();
        }
    }

    public static void ProcessMonitor()
    {
        ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
        startWatch.EventArrived += StartWatch_EventArrived;
        startWatch.Start();
    }

    private static void StartWatch_EventArrived(object sender, EventArrivedEventArgs e)
    {
        string processName = e.NewEvent.Properties["ProcessName"].Value.ToString().ToLower();

        if (processName.EndsWith(".exe"))
        {
            processName = processName.Substring(0, processName.Length - 4);
        }

        foreach (string priority in priorities)
        {
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (process.ProcessName.ToLower().Equals(priority.ToLower()) && process.ProcessName.ToLower().Equals(processName.ToLower()))
                    {
                        Logger.LogWarning("Detected a new opened process that needs priority set: '" + priority.ToLower() + "'.");

                        try
                        {
                            process.PriorityClass = ProcessPriorityClass.RealTime;

                            try
                            {
                                foreach (ProcessThread processThread in process.Threads)
                                {
                                    try
                                    {
                                        processThread.PriorityLevel = ThreadPriorityLevel.Highest;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            catch
                            {

                            }

                            Logger.LogInfo("Succesfully modified the priority of '" + priority.ToLower() + "'.");
                        }
                        catch
                        {
                            Logger.LogError("Failed to modify the priority of '" + priority.ToLower() + "'.");
                        }
                    }
                }
                catch
                {

                }
            }
        }
    }
}