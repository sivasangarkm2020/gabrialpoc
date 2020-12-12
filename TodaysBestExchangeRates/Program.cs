using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace TodaysBestExchangeRates
{
    static class Program
    {
        /*
    DEMO CODE ONLY: In general, this approach calls for re-thinking 
    your architecture!
    There are 4 possible ways this can run:
    1) User starts application from existing cmd window, and runs in GUI mode
    2) User double clicks to start application, and runs in GUI mode
    3) User starts applicaiton from existing cmd window, and runs in command mode
    4) User double clicks to start application, and runs in command mode.

    To run in console mode, start a cmd shell and enter:
        c:\path\to\Debug\dir\WindowsApplication.exe console
        To run in gui mode,  EITHER just double click the exe, OR start it from the cmd prompt with:
        c:\path\to\Debug\dir\WindowsApplication.exe (or pass the "gui" argument).
        To start in command mode from a double click, change the default below to "console".
    In practice, I'm not even sure how the console vs gui mode distinction would be made from a
    double click...
        string mode = args.Length > 0 ? args[0] : "console"; //default to console
    */

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [STAThread]
        static void Main(string[] args)
        {
            string PrintMode = ConfigurationManager.AppSettings["PrintMode"].ToString();

            try
            {
                if (PrintMode == "MessageBox")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    frmMessageBox frmAlert = new frmMessageBox();
                    frmAlert.Visible = false;
                    Application.Run(frmAlert);
                }
                else if (PrintMode == "Console")
                {
                    //Get a pointer to the forground window.
                    //If starts the application from an existing console
                    //shell, that shell will be the uppermost window. So need to get it
                    //and attach to it
                    int processid;
                    IntPtr foreGroundWindow = GetForegroundWindow();
                    GetWindowThreadProcessId(foreGroundWindow, out processid);
                    Process process = Process.GetProcessById(processid);
                    //Running the application from command window
                    if (process.ProcessName == "cmd")    //Is the uppermost window a cmd process?
                    {
                        //Attaching a console
                        AttachConsole(process.Id);
                        LoadData();
                    }
                    else //Running the application by double clicking the exe file
                    {
                        //As we are in console mode no console to attach. So creating a new console.
                        AllocConsole();
                        LoadData();
                    }
                    FreeConsole();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while loading today best rates");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Loding best rate data
        /// </summary>
        static void LoadData()
        {
            DBHelper dBHelper = new DBHelper();
            List<BestExchangeRate> bestExchangeRates = new List<BestExchangeRate>();
            bestExchangeRates = dBHelper.GetBestExchangeRates();
            if (bestExchangeRates.Count != 0)
            {
                Console.WriteLine("----------------------------------------------------------------");
                Console.WriteLine("{0,-25} {1,-10} {2}", "Data Source", "Currency", "Best Rate");
                Console.WriteLine("----------------------------------------------------------------");
                foreach (BestExchangeRate bestExchangeRate in bestExchangeRates)
                {
                    Console.WriteLine("{0,-25} {1,-10} {2}", bestExchangeRate.ResourceName, bestExchangeRate.Symbol, bestExchangeRate.Rate);
                }
                Console.WriteLine("----------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("Error while loding data ...");
            }
            Console.WriteLine("press any key to continue ...");
            Console.ReadLine();
        }
    }
}
