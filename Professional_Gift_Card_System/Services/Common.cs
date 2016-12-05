using System;

namespace Professional_Gift_Card_System.Services
{
    public class Common
    {
        public static String StandardExceptionHandler (Exception Ex, String Context, System.Collections.Specialized.NameValueCollection RequestParameters)
        {
            String Response = "A system error occured in " + Context + ". Please let the help desk know." + Ex.Message;
            String LogMessage = "Exception:" + Ex.Message;
            if (Ex.TargetSite != null)
                LogMessage = LogMessage +" At:" + Ex.TargetSite.Name;

            while (Ex.InnerException != null)
            {
                Ex = Ex.InnerException;
                Response = Response + Environment.NewLine + Ex.Message;
                LogMessage = LogMessage + Environment.NewLine + Ex.Message;
            }
            LogMessage = LogMessage + Environment.NewLine + Ex.StackTrace + Environment.NewLine;

            // now log all that for support

            Log.OtherError(LogMessage, RequestParameters);
            return Response;
        }

        public static string StandardExceptionErrorMessage(Exception Ex)
        {
            String Response = "A system error occured. Please let the help desk know." + Ex.Message;
            String LogMessage = "Exception:" + Ex.Message;
            if (Ex.TargetSite != null)
                LogMessage = LogMessage + " At:" + Ex.TargetSite.Name;

            while (Ex.InnerException != null)
            {
                Ex = Ex.InnerException;
                Response = Response + Environment.NewLine + Ex.Message;
                LogMessage = LogMessage + Environment.NewLine + Ex.Message;
            }
            LogMessage = LogMessage + Environment.NewLine + Ex.StackTrace + Environment.NewLine;

            // now log all that for support

            Log.OtherError(LogMessage);
            return Response;
        }

    }
}