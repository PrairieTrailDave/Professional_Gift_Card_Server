// ************************************************************
//
// Copyright (c) 2016 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using Professional_Gift_Card_System;

    public class UserMembershipProvider: MembershipProvider
    {
        //
        // Global connection string, generated password length, generic exception message, event log info.
        //

        private int newPasswordLength = 8;
        private string eventSource = "UserMembershipProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string connectionString;

        //
        // Used when determining encryption key values.
        //

        private MachineKeySection machineKey;

        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //

        private bool pWriteExceptionsToEventLog;

        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }

        //
        // System.Web.Security.MembershipProvider properties.
        //


        private string pApplicationName;
        private bool pEnablePasswordReset;
        private bool pEnablePasswordRetrieval;
        private bool pRequiresQuestionAndAnswer;
        private bool pRequiresUniqueEmail;
        private int pMaxInvalidPasswordAttempts;
        private int pPasswordAttemptWindow;
        private MembershipPasswordFormat pPasswordFormat;





        //
        // System.Configuration.Provider.ProviderBase.Initialize Method
        //

        public override void Initialize(string name, NameValueCollection config)
        {
            //
            // Initialize values from web.config.
            //

            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "UserMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "User Membership provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            pApplicationName = GetConfigValue(config["applicationName"],
                                            System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            pMinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            pEnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            // we hard code that the passwords are encrypted. 
            // The other options are not available
            pPasswordFormat = MembershipPasswordFormat.Encrypted;

            //
            // Initialize OdbcConnection.
            //

            ConnectionStringSettings ConnectionStringSettings =
              ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;


            // Get encryption and decryption key information from the configuration.
            Configuration cfg =
              WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            //if (machineKey.ValidationKey.Contains("AutoGenerate"))
            //    if (PasswordFormat != MembershipPasswordFormat.Clear)
            //        throw new ProviderException("Hashed or Encrypted passwords " +
            //                                    "are not supported with auto-generated keys.");
        }


        //
        // A helper function to retrieve config values from the configuration file.
        //

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }




        //
        // System.Web.Security.MembershipProvider properties.
        //


        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return pEnablePasswordReset; }
        }


        public override bool EnablePasswordRetrieval
        {
            get { return pEnablePasswordRetrieval; }
        }


        public override bool RequiresQuestionAndAnswer
        {
            get { return pRequiresQuestionAndAnswer; }
        }


        public override bool RequiresUniqueEmail
        {
            get { return pRequiresUniqueEmail; }
        }


        public override int MaxInvalidPasswordAttempts
        {
            get { return pMaxInvalidPasswordAttempts; }
        }


        public override int PasswordAttemptWindow
        {
            get { return pPasswordAttemptWindow; }
        }


        public override MembershipPasswordFormat PasswordFormat
        {
            get { return pPasswordFormat; }
        }

        private int pMinRequiredNonAlphanumericCharacters;

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return pMinRequiredNonAlphanumericCharacters; }
        }

        private int pMinRequiredPasswordLength;

        public override int MinRequiredPasswordLength
        {
            get { return pMinRequiredPasswordLength; }
        }

        private string pPasswordStrengthRegularExpression;

        public override string PasswordStrengthRegularExpression
        {
            get { return pPasswordStrengthRegularExpression; }
        }

 









        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }



            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }


                User DBUser = new User();
                DBUser.PKID = (Guid)providerUserKey;
                DBUser.ApplicationName = pApplicationName;
                DBUser.UserName = username;
                DBUser.Password = Professional_Gift_Card_System.Models.GiftEncryption.EncryptPassword(password);
                DBUser.Email = email.ToLower();
                DBUser.PasswordQuestion = passwordQuestion;
                DBUser.PasswordAnswer = Professional_Gift_Card_System.Models.GiftEncryption.EncryptPassword(passwordAnswer);
                DBUser.IsApproved = isApproved;
                DBUser.Comment = "";
                DBUser.CreationDate = createDate;
                DBUser.LastPasswordChangedDate = createDate;
                DBUser.LastActivityDate = createDate;
                DBUser.IsLockedOut = false;
                DBUser.LastLockedOutDate = createDate;
                DBUser.FailedPasswordAttemptCount = 0;
                DBUser.FailedPasswordAnswerAttemptWindowStart = createDate;
                DBUser.FailedPasswordAnswerAttemptCount = 0;
                DBUser.FailedPasswordAttemptWindowStart = createDate;

                try
                {
                    Professional_Gift_Card_System.Models.IUserDAO UserRepository = new Professional_Gift_Card_System.Models.UserDAO();
                    if (!UserRepository.CreateUser(DBUser))
                        status = MembershipCreateStatus.UserRejected;
                    else
                    {
                        status = MembershipCreateStatus.Success;
                    }
                }
                catch (Exception e)
                {
                    if (WriteExceptionsToEventLog)
                    {
                        WriteToEventLog(e, "CreateUser");
                    }

                    status = MembershipCreateStatus.ProviderError;
                }

                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }


            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

 
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser u = null;

            try
            {
                Professional_Gift_Card_System.Models.IUserDAO UserRepository = new Professional_Gift_Card_System.Models.UserDAO();
                User Ch = UserRepository.GetUser(username);
                if (Ch != null)
                {
                    u = GetUserFromDatabaseUser(Ch);

                    if (userIsOnline)
                    {
                        UserRepository.UpdateUserOnline(username);
                    }
                }

            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUser(String, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            return u;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            string username = "";
            try
            {
                Professional_Gift_Card_System.Models.IUserDAO UserRepository = new Professional_Gift_Card_System.Models.UserDAO();
                User DBUser = UserRepository.GetUserByEmail(email);
                if (DBUser != null)
                    username = (string)DBUser.UserName;
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUserNameByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            if (username == null)
                username = "";

            return username;
        }


        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;
            try
            {
                Professional_Gift_Card_System.Models.IUserDAO UserRepository = new Professional_Gift_Card_System.Models.UserDAO();
                isValid = UserRepository.ValidateUser(username, password, PasswordAttemptWindow, MaxInvalidPasswordAttempts);

            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ValidateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            return isValid;
        }

        // we can't define new methods
        // why not? Because this module does not get compiled into the
        // same space as the rest of the system. Instead, this is compiled
        // by the ASP framework. Thus, our code does not know about
        // and new methods that we want to add nor any desired changes to the method signature

        //public bool ValidateUserByEmail(string eMailAddress, string password)
        //{
        //    bool isValid = false;
        //    try
        //    {
        //        Professional_Gift_Card_System.Models.IUserRepository UserRepository = new Professional_Gift_Card_System.Models.UserRepository();
        //        isValid = UserRepository.ValidateUserByEmail(eMailAddress, password, PasswordAttemptWindow, MaxInvalidPasswordAttempts);
        //
        //    }
        //    catch (Exception e)
        //    {
        //        if (WriteExceptionsToEventLog)
        //        {
        //            WriteToEventLog(e, "ValidateUserByEmail");
        //
        //            throw new ProviderException(exceptionMessage);
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }
        //    return isValid;
        //}



        //
        // GetUserFromReader
        //    A helper function that takes the current row from the DataReader
        // and hydrates a MembershipUser from the values. Called by the 
        // MembershipUser.GetUser implementation.
        //

        private MembershipUser GetUserFromDatabaseUser(User DbUser)
        {
            DateTime creationDate = new DateTime();
            if (DbUser.CreationDate != null)
                creationDate = (DateTime)DbUser.CreationDate;

            DateTime lastLoginDate = new DateTime();
            if (DbUser.LastLoginDate != null)
                lastLoginDate = (DateTime)DbUser.LastLoginDate;
            DateTime lastActivityDate = new DateTime();
            if (DbUser.LastActivityDate != null)
                lastActivityDate = (DateTime)DbUser.LastActivityDate;
            DateTime lastPasswordChangedDate = new DateTime();
            if (DbUser.LastPasswordChangedDate != null)
                lastPasswordChangedDate = (DateTime)DbUser.LastPasswordChangedDate;
            DateTime lastLockedOutDate = new DateTime();
            if (DbUser.LastLockedOutDate != null)
                lastLockedOutDate = (DateTime)DbUser.LastLockedOutDate;

            MembershipUser u = new MembershipUser(this.Name,
                                                  DbUser.UserName,
                                                  DbUser.PKID,
                                                  DbUser.Email,
                                                  DbUser.PasswordQuestion,
                                                  DbUser.Comment,
                                                  (bool)DbUser.IsApproved,
                                                  (bool)DbUser.IsLockedOut,
                                                  creationDate,
                                                  lastLoginDate,
                                                  lastActivityDate,
                                                  lastPasswordChangedDate,
                                                  lastLockedOutDate);

            return u;
        }


        //
        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        //

        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }


    }
