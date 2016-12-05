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
using Professional_Gift_Card_System;

namespace Professional_Gift_Card_System.Models
{

    /*
     * 
     * Actually, the only roles allowed on this system are:
     * SystemAdministrator
     * ClientAdministrator
     * Merchant
     * Clerk
     * CardHolder
     * 
     * 
     * 
    This provider works with the following schema for the tables of role data.
    
    CREATE TABLE Roles
    (
      Rolename NVARCHAR (50) NOT NULL,
      ApplicationName NVARCHAR (50) NOT NULL,
        CONSTRAINT PKRoles PRIMARY KEY (Rolename, ApplicationName)
    )

    GO
     
    CREATE TABLE UsersInRoles
    (
      Username NVARCHAR (50) NOT NULL,
      Rolename NVARCHAR (50) NOT NULL
      ApplicationName NVARCHAR (50) NOT NULL,
        CONSTRAINT PKUsersInRoles PRIMARY KEY (Username, Rolename, ApplicationName)
    )
    GO

      
      <configuration>
      <connectionStrings>
        <add name="OdbcServices" connectionString="DSN=RolesDSN;" />
      </connectionStrings>

      <system.web>
        <authentication mode="Forms" />
          <forms loginUrl="loginvb.aspx"
            name=".ASPXFORMSAUTH" />
        </authentication>

        <authorization>
          <deny users="?" />
        </authorization>

        <roleManager defaultProvider="GiftRoleProvider" 
          enabled="true"
          cacheRolesInCookie="true"
          cookieName=".ASPROLES"
          cookieTimeout="30"
          cookiePath="/"
          cookieRequireSSL="false"
          cookieSlidingExpiration="true"
          cookieProtection="All" >
          <providers>
            <clear />
            <add
              name="GiftRoleProvider"
              type="Professional_Gift_Card_System.Models.RoleProvider"
              connectionStringName="OdbcServices" 
              applicationName="SampleApplication" 
              writeExceptionsToEventLog="false" />
          </providers>
        </roleManager>

      </system.web>
    </configuration>

     * 
     * In order to write to the event log, we have to have the following registry key
     * HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Eventlog\Application\GiftRoleProvider
     */


    public sealed class GiftRoleProvider : RoleProvider
    {

        //
        // Global connection string, generic exception message, event log info.
        //

        private string eventSource = "GiftRoleProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";


        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //

        private bool pWriteExceptionsToEventLog = false;

        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }



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
                name = "GiftRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Gift Role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);


            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                pApplicationName = config["applicationName"];
            }


            if (config["writeExceptionsToEventLog"] != null)
            {
                if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
                {
                    pWriteExceptionsToEventLog = true;
                }
            }

            /*   We use the regular connection string
                  //
                  // Initialize OdbcConnection.
                  //

                  pConnectionStringSettings = ConfigurationManager.
                    ConnectionStrings[config["connectionStringName"]];

                  if (pConnectionStringSettings == null || pConnectionStringSettings.ConnectionString.Trim() == "")
                  {
                    throw new ProviderException("Connection string cannot be blank.");
                  }

                  connectionString = pConnectionStringSettings.ConnectionString;
             */
        }



        //
        // System.Web.Security.RoleProvider properties.
        //


        private string pApplicationName;


        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        //
        // System.Web.Security.RoleProvider methods.
        //

        //
        // RoleProvider.AddUsersToRoles
        //

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                if (username.Contains(","))
                {
                    throw new ArgumentException("User names cannot contain commas.");
                }

                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }


            try
            {
                Professional_Gift_Card_System.Models.IUserInRolesDAO UserInRoleDAO = new Professional_Gift_Card_System.Models.UserInRolesDAO();
                foreach (string username in usernames)
                {
                    foreach (string rolename in rolenames)
                    {
                        UserInRoleDAO.AddUserToRole(username, rolename, ApplicationName);
                    }
                }
            }
            catch (Exception e)
            {

                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "AddUsersToRoles");
                }
                else
                {
                    throw e;
                }
            }
        }


        //
        // RoleProvider.CreateRole
        //

        public override void CreateRole(string rolename)
        {
            if (rolename.Contains(","))
            {
                throw new ArgumentException("Role names cannot contain commas.");
            }

            if (RoleExists(rolename))
            {
                throw new ProviderException("Role name already exists.");
            }

            try
            {
                Role nRole = new Role();
                nRole.Rolename = rolename;
                nRole.ApplicationName = pApplicationName;

                using (var GiftEntity = new GiftEntities())
                {
                    GiftEntity.Roles.Add(nRole);
                    GiftEntity.SaveChanges();
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "CreateRole");
                }
                else
                {
                    throw e;
                }
            }
        }


        //
        // RoleProvider.DeleteRole
        //

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            if (!RoleExists(rolename))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            try
            {
                using (var GiftEntity = new GiftEntities())
                {
                    var nRole = (from r in GiftEntity.Roles
                                 where r.Rolename == rolename &&
                                       r.ApplicationName == pApplicationName
                                 select r).FirstOrDefault();
                    if (nRole == null) return false;
                    GiftEntity.Roles.Remove(nRole);
                    GiftEntity.SaveChanges();
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteRole");

                    return false;
                }
                else
                {
                    throw e;
                }
            }
            return true;
        }


        //
        // RoleProvider.GetAllRoles
        //

        public override string[] GetAllRoles()
        {
            try
            {
                using (var GiftEntity = new GiftEntities())
                {
                    var Roles = (from r in GiftEntity.Roles
                                 where r.ApplicationName == pApplicationName
                                 select r);

                    List<String> sRoles = new List<string>();
                    foreach (Role g in Roles)
                        sRoles.Add(g.Rolename);
                    return sRoles.ToArray();
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetAllRoles");
                }
                else
                {
                    throw e;
                }
            }
            return new string[0];
        }


        //
        // RoleProvider.GetRolesForUser
        //

        public override string[] GetRolesForUser(string username)
        {
            string[] sRoles = new string[1];

            try
            {
                using (var GiftEntity = new GiftEntities())
                {
                    var nRole = (from u in GiftEntity.UsersInRoles
                                 where u.Username == username &&
                                       u.ApplicationName == pApplicationName
                                 select u.Rolename);
                    if (nRole == null) return new string[0];
                    int cnt = 0;
                    foreach (string c in nRole)
                        cnt++;


                    sRoles = new string[cnt];
                    int i = 0;
                    foreach (string c in nRole)
                    {
                        sRoles[i] = c;
                        i++;
                    }
                }
                return sRoles;
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetRolesForUser");
                }
                else
                {
                    throw e;
                }
            }
            return new string[0];
        }


        //
        // RoleProvider.GetUsersInRole
        //

        public override string[] GetUsersInRole(string rolename)
        {
            string[] sRoles = new string[1];


            try
            {
                using (var GiftEntity = new GiftEntities())
                {
                    var nRole = (from u in GiftEntity.UsersInRoles
                                 where u.Rolename == rolename &&
                                       u.ApplicationName == pApplicationName
                                 select u.Username);
                    if (nRole == null) return new string[0];
                    int cnt = 0;
                    foreach (string c in nRole)
                        cnt++;


                    sRoles = new string[cnt];
                    int i = 0;
                    foreach (string c in nRole)
                    {
                        sRoles[i] = c;
                        i++;
                    }
                }
                return sRoles;
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUsersInRole");
                }
                else
                {
                    throw e;
                }
            }
            return new string[0];
        }


        //
        // RoleProvider.IsUserInRole
        //

        public override bool IsUserInRole(string username, string rolename)
        {
            bool userIsInRole = false;
            try
            {
                Professional_Gift_Card_System.Models.IUserInRolesDAO UserInRoleDAO = new Professional_Gift_Card_System.Models.UserInRolesDAO();
                if (UserInRoleDAO.IsUserInRole(username, rolename, pApplicationName))
                    return true;
                return false;
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "IsUserInRole");
                }
                else
                {
                    throw e;
                }
            }
            return userIsInRole;
        }


        //
        // RoleProvider.RemoveUsersFromRoles
        //

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }


            try
            {
                using (var GiftEntity = new GiftEntities())
                {
                    foreach (string username in usernames)
                    {
                        foreach (string rolename in rolenames)
                        {
                            var nRole = (from r in GiftEntity.UsersInRoles
                                         where r.Rolename == rolename &&
                                               r.Username == username &&
                                               r.ApplicationName == pApplicationName
                                         select r).FirstOrDefault();
                            if (nRole == null) continue;
                            GiftEntity.UsersInRoles.Remove(nRole);
                        }
                    }
                    GiftEntity.SaveChanges();
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "RemoveUsersFromRoles");
                }
                else
                {
                    throw e;
                }
            }
        }


        //
        // RoleProvider.RoleExists
        //

        public override bool RoleExists(string rolename)
        {
            bool exists = false;
            try
            {
                Professional_Gift_Card_System.Models.IRolesDAO RoleDAO = new Professional_Gift_Card_System.Models.RolesDAO();
                if (RoleDAO.RoleExists(rolename, pApplicationName))
                    return true;
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "RoleExists");
                }
                else
                {
                    throw e;
                }
            }
            return exists;
        }

        //
        // RoleProvider.FindUsersInRole
        //

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            string[] usernames;
            try
            {
                using (var GiftEntity = new GiftEntities())
                {
                    var nRole = (from r in GiftEntity.UsersInRoles
                                 where r.Rolename == rolename &&
                                 r.Username.StartsWith(usernameToMatch) &&
                                 r.ApplicationName == pApplicationName
                                 select r.Username);
                    if (nRole == null) return new string[0];
                    int cnt = 0;
                    foreach (string u in nRole)
                        cnt++;
                    usernames = new String[cnt];
                    int i = 0;
                    foreach (string u in nRole)
                    {
                        usernames[i] = u;
                        i++;
                    }
                }
                return usernames;
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersInRole");
                }
                else
                {
                    throw e;
                }
            }
            return new string[0];
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

            string message = exceptionMessage + "\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

    }

}