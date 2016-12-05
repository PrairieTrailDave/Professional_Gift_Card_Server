using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Professional_Gift_Card_System.Models;


namespace Professional_Gift_Card_System.Services
{
    public class Log
    {
        // this first version will keep it simple. 
        // We will not worry about multiple threads trying to write
        // 
        const string Dir = "~\\Logs";
        const string BadDataLoggingFileName = "..\\Logs\\BadData";
        const string LoginLoggingFileName = "..\\Logs\\Login";
        const string ErrorLoggingFileName = "..\\Logs\\Error";

        private static object LogLock = new object();


        public String LogDirectory
        {
            get
            {
                if (HttpContext.Current != null)
                    if (HttpContext.Current.Server != null)
                        return HttpContext.Current.Server.MapPath(Dir);
                return Dir;
            }
        }

        private static String MapPath (String Dir)
        {
            if (HttpContext.Current != null)
                if (HttpContext.Current.Server != null)
                    return HttpContext.Current.Server.MapPath(Dir);
            return Dir;
        }

        public static List<LogHistoryModel> GetLogFiles ()
        {
            List<LogHistoryModel> Results = new List<LogHistoryModel>();
            string[] files = Directory.GetFiles(MapPath(Dir), "*.log");
            for (int i = 0; i < files.Length; i++)
            {
                LogHistoryModel ToAdd = new LogHistoryModel();
                ToAdd.id = i;
                ToAdd.FileName = Path.GetFileName(files[i]);
                Results.Add(ToAdd);
            }
            return Results;
        }

        public static String GetLogFile (String FileName)
        {
            StreamReader SR = new StreamReader(MapPath(Dir + "\\" + FileName));
            StringBuilder Contents = new StringBuilder();
            Contents.Append("<p>");
            while (SR.Peek() > -1)
            {
                Contents.Append (SR.ReadLine());
                Contents.Append("<br />");
            }
            SR.Close();
            Contents.Append("</p>");
            return Contents.ToString();
        }

        public static bool DeleteLogFile (string FileName)
        {
            File.Delete(MapPath(Dir + "\\" + FileName));
            return true;
        }

        public static void BadData(String IPAddress, String FunctionName, System.Collections.Specialized.NameValueCollection QueryString)
        {

            String DataElements = FunctionName;

            String CurrentFileName = BadDataLoggingFileName + String.Format("{0:0000}{1:00}{2:00}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) + ".log";
            lock (LogLock)
            {
                StreamWriter LogStream = new StreamWriter(MapPath(CurrentFileName), true);
                LogStream.WriteLine(
                    "{0} {1} {2} {3}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    IPAddress, FunctionName);
                if (QueryString != null)
                {
                    foreach (string qkey in QueryString.Keys)
                    {
                        LogStream.WriteLine(
                            "{0} {1} {2} {3}-{4}",
                            DateTime.Now.ToShortDateString(),
                            DateTime.Now.ToShortTimeString(),
                            IPAddress,
                            qkey,
                            QueryString[qkey]
                            );
                    }
                }
                LogStream.Close();
            }

        }

        public static void LoginAttempts(String IPAddress, String Username)
        {
            String CurrentFileName = LoginLoggingFileName + String.Format("{0:0000}{1:00}{2:00}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) + ".log";
            lock (LogLock)
            {
                StreamWriter LogStream = new StreamWriter(MapPath(CurrentFileName), true);
                LogStream.WriteLine(
                    "{0} {1} {2} {3}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    IPAddress,
                    Username);
                LogStream.Close();
            }

        }
        public static void LoginAttempts(String IPAddress, String Username, bool PassFail)
        {
            String CurrentFileName = LoginLoggingFileName + String.Format("{0:0000}{1:00}{2:00}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) + ".log";
            lock (LogLock)
            {
                StreamWriter LogStream = new StreamWriter(MapPath(CurrentFileName), true);
                String Result = " - failed";
                if (PassFail) Result = " - passed";
                LogStream.WriteLine(
                    "{0} {1} {2} {3} {4}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    IPAddress,
                    Username,
                    Result);
                LogStream.Close();
            }
        }
        public static void LoginAttempts(String IPAddress, String MerchantID, String ClerkID, bool PassFail)
        {
            String CurrentFileName = LoginLoggingFileName + String.Format("{0:0000}{1:00}{2:00}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) + ".log";
            lock (LogLock)
            {
                StreamWriter LogStream = new StreamWriter(MapPath(CurrentFileName), true);
                String Result = " - failed";
                if (PassFail) Result = " - passed";
                LogStream.WriteLine(
                    "{0} {1} {2} {3} {4}{5}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    IPAddress,
                   MerchantID, ClerkID, Result);
                LogStream.Close();
            }
        }
        /*        public static void LoginAttempts(String IPAddress, String Username, bool PassFail)
                {
                    using (CashForDebitEntities Entity = new CashForDebitEntities())
                    {
                        LogInHistory nHistory = new LogInHistory();
                        nHistory.IPAddr = IPAddress;
                        nHistory.LocalTime = DateTime.Now;
                        nHistory.WhenHappened = DateTime.Now;
                        if (PassFail)
                            nHistory.TransType = "LOGN";
                        else
                            nHistory.TransType = "ANOT";
                        nHistory.WhoDid = Username;
                        nHistory.WebCellOrDialup = "W";
                        Entity.LogInHistories.Add(nHistory);
                        Entity.SaveChanges();
                    }
                }
        */
        public static void OtherError(String ErrorMessage)
        {
            String CurrentFileName = ErrorLoggingFileName + String.Format("{0:0000}{1:00}{2:00}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) + ".log";
            lock (LogLock)
            {
                StreamWriter LogStream = new StreamWriter(MapPath(CurrentFileName), true);
                LogStream.WriteLine(
                    "{0} {1} {2}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    ErrorMessage);
                LogStream.Close();
            }
        }
        public static void OtherError (String ErrorMessage, System.Collections.Specialized.NameValueCollection QueryString)
        {
            String CurrentFileName = ErrorLoggingFileName + String.Format("{0:0000}{1:00}{2:00}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) + ".log";
            lock (LogLock)
            {
                StreamWriter LogStream = new StreamWriter(MapPath(CurrentFileName), true);
                LogStream.WriteLine(
                    "{0} {1} {2}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    ErrorMessage);
                if (QueryString != null)
                {
                    foreach (string qkey in QueryString.Keys)
                    {
                        LogStream.WriteLine(
                            "{0} {1} {2} {3}-{4}",
                            DateTime.Now.ToShortDateString(),
                            DateTime.Now.ToShortTimeString(),
                            qkey,
                            QueryString[qkey]
                            );
                    }
                }
                LogStream.Close();
            }
        }
    }
}