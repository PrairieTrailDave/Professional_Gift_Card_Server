// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Professional_Gift_Card_System;

namespace Professional_Gift_Card_System.Models
{
    public interface IUserDAO
    {
        bool ChangePasswordQuestionAndAnswer(string UserName, string newPwdQuestion, string newPwdAnswer);
        bool CreateUser(User ToAdd);
        bool DeleteUser(string UserName);
        String GetUniqueUserName(String Proposed);
        User GetUser(Guid ID);
        User GetUser(string UserName);
        User GetUserByEmail(String email);
        int GetUserCount();
        Guid GetUserID(String UserName);
        int GetUserOnlineCount(DateTime Window);
        string GetUserPassword(String UserName, out String PasswordAnswer, out bool IsLockedOut);
        List<User> GetUserPage(int pageNumber, int pageSize);
        List<User> ListUsers();
        List<User> ListUsersLike(String UserName, int pageNumber, int pageSize);
        List<User> ListUsersLikeEmail(String UserEmail, int pageNumber, int pageSize);
        void UpdateUserOnline(string UserName);
        void UpdateUserOnline(Guid ID);
        bool UnlockUser(String UserName);
        bool UpdateEmail(String UserName, String email, String comment, bool isapproved);
        bool UpdatePassword(String UserName, String NewPassword);
        bool UpdatePasswordAnswerFailureCount(String UserName, int PasswordAttemptWindow, int MaxInvalidPasswordAttempts);
        bool ValidateUser(String UserName, String Password, int PasswordAttemptWindow, int MaxInvalidPasswordAttempts);
        bool ValidateUserByEmail(String emailAddress, String Password, int PasswordAttemptWindow, int MaxInvalidPasswordAttempts, out String userName);
    }


    public interface IUserInRolesDAO
    {
        void AddUserToRole(String username, String RoleName, String ApplicationName);
        bool IsUserInRole(String username, String rolename, String ApplicationName);
        void DeleteUserFromRole(String username);
        bool VerifySuperUserExists(String ApplicationName);
    }

    public interface IRolesDAO
    {
        bool RoleExists(String rolename, String ApplicationName);
        void InsureSuperUserRoleExists(String ApplicationName);
    }










    public class UserDAO : IUserDAO
    {
        #region IUserRepository Members


        bool IUserDAO.ChangePasswordQuestionAndAnswer(string UserName,
          string newPwdQuestion, string newPwdAnswer)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                               where c.UserName == UserName
                               select c).FirstOrDefault();
                if (DBUser == null) return false;

                DBUser.LastPasswordChangedDate = DateTime.Now;
                DBUser.PasswordQuestion = newPwdQuestion;
                DBUser.PasswordAnswer = newPwdAnswer;

                GiftEntity.SaveChanges();
            }
            return true;
        }


        bool IUserDAO.CreateUser(User UserToAdd)
        {
            using (var GiftEntity = new GiftEntities())
            {
                UserToAdd.PKID = Guid.NewGuid();
                GiftEntity.Users.Add(UserToAdd);
                GiftEntity.SaveChanges();
                return true;
            }
        }


        bool IUserDAO.DeleteUser(string UserName)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                               where c.UserName == UserName
                               select c).FirstOrDefault();
                if (DBUser == null) return false;
                GiftEntity.Users.Remove(DBUser);
                GiftEntity.SaveChanges();
            }
            return true;
        }



        String IUserDAO.GetUniqueUserName(String Proposed)
        {
            String results = "";
            StringBuilder NewName = new StringBuilder();

            foreach (char c in Proposed)
            {
                if (c == ' ')
                    NewName.Append('_');
                else
                    NewName.Append(c);
            }

            results = NewName.ToString();
            using (var GiftEntity = new GiftEntities())
            {
                var DBUserName = (from c in GiftEntity.Users
                              orderby c.UserName descending
                               where c.UserName.StartsWith(results)
                               select c.UserName).FirstOrDefault();

                if (DBUserName == null) return results;

                int ToAppend = PullNumberFromName(DBUserName);
                results = results + Convert.ToString(ToAppend + 1);
            }

            return results;
        }


        // PullNumberFromName

        int PullNumberFromName(String Name)
        {
            int index;
            int result = 0;

            // first find the start of the added number

            if (Char.IsDigit(Name[0]))
                index = 0;
            else
            {
                index = Name.Length - 1;
                while (Char.IsDigit(Name[index]))
                    index--;
            }

            // then pull off that added number

            while (index < Name.Length)
            {
                if (Char.IsDigit(Name[index]))
                    result = result * 10 + (Name[index] - '0');
                index++;
            }
            return result;
        }





        User IUserDAO.GetUser(Guid ID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.PKID == ID
                                           select c).FirstOrDefault();
                return DBUser;
            }
        }
        User IUserDAO.GetUser(String UserName)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.UserName == UserName
                                           select c).FirstOrDefault();
                return DBUser;
            }
        }


        User IUserDAO.GetUserByEmail(String email)
        {
            User DBUser = null;
            using (var GiftEntity = new GiftEntities())
            {
                DBUser = (from c in GiftEntity.Users
                          where c.Email == email
                          select c).FirstOrDefault();
            }
            return DBUser;

        }

        int IUserDAO.GetUserCount()
        {
            int count;
            using (var GiftEntity = new GiftEntities())
            {
                count = (from c in GiftEntity.Users select c).Count();
            }
            return count;
        }


        Guid IUserDAO.GetUserID(String UserName)
        {
            if (UserName == null) return Guid.NewGuid();

            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                               where c.UserName == UserName
                               select c).FirstOrDefault();
                return DBUser.PKID;
            }
        }


        int IUserDAO.GetUserOnlineCount(DateTime Window)
        {
            int count;
            using (var GiftEntity = new GiftEntities())
            {
                count = (from c in GiftEntity.Users
                         where c.LastActivityDate > Window
                         select c).Count();
            }
            return count;
        }

        String IUserDAO.GetUserPassword(String UserName, out String PasswordAnswer, out bool IsLockedOut)
        {
            String ReturnedPassword = null;
            PasswordAnswer = "";
            IsLockedOut = true;

            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                               where c.UserName == UserName
                               select c).FirstOrDefault();
                if (DBUser == null) return ReturnedPassword;
                ReturnedPassword = DBUser.Password;
                PasswordAnswer = DBUser.PasswordAnswer;
                IsLockedOut = (bool)DBUser.IsLockedOut;
            }
            return ReturnedPassword;

        }

        List<User> IUserDAO.GetUserPage(int pageNumber, int pageSize)
        {
            List<User> UserList = new List<User>();
            using (var GiftEntity = new GiftEntities())
            {
                var UserPage = (from c in GiftEntity.Users
                             .OrderBy(o => o.UserName)
                            .Skip(pageSize * pageNumber).Take(pageSize).Select(o => o)
                                      select c);
                foreach (User ch in UserPage)
                    UserList.Add(ch);
            }
            return UserList;
        }

        List<User> IUserDAO.ListUsers()
        {
            using (var GiftEntity = new GiftEntities())
            {
                List<User> ToReturn = new List<User>();
                foreach (User ch in GiftEntity.Users)
                {
                    ToReturn.Add(ch);
                }
                return ToReturn;
            }
        }

        List<User> IUserDAO.ListUsersLike(String UserName, int pageNumber, int pageSize)
        {
            using (var GiftEntity = new GiftEntities())
            {
                List<User> ToReturn = new List<User>();
                foreach (User ch in (from c in GiftEntity.Users
                                           .OrderBy(o => o.UserName)
                                           .Skip(pageSize * pageNumber).Take(pageSize).Select(o => o)
                                           where c.UserName.StartsWith(UserName)
                                           select c))
                {
                    ToReturn.Add(ch);
                }
                return ToReturn;
            }
        }
        List<User> IUserDAO.ListUsersLikeEmail(String UserEmail, int pageNumber, int pageSize)
        {
            using (var GiftEntity = new GiftEntities())
            {
                List<User> ToReturn = new List<User>();
                foreach (User ch in (from c in GiftEntity.Users
                                           .OrderBy(o => o.UserName)
                                           .Skip(pageSize * pageNumber).Take(pageSize).Select(o => o)
                                           where c.Email.StartsWith(UserEmail)
                                           select c))
                {
                    ToReturn.Add(ch);
                }
                return ToReturn;
            }
        }

        void IUserDAO.UpdateUserOnline(string UserName)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.UserName == UserName
                                           select c).FirstOrDefault();
                if (DBUser == null) return;
                DBUser.LastActivityDate = DateTime.Now;
                GiftEntity.SaveChanges();
            }
        }
        void IUserDAO.UpdateUserOnline(Guid ID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.PKID == ID
                                           select c).FirstOrDefault();
                if (DBUser == null) return;
                DBUser.LastActivityDate = DateTime.Now;
                GiftEntity.SaveChanges();
            }
        }

        bool IUserDAO.UnlockUser(String UserName)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.UserName == UserName
                                           select c).FirstOrDefault();
                if (DBUser == null) return false;
                DBUser.LastLockedOutDate = DateTime.Now;
                DBUser.IsLockedOut = false;
                GiftEntity.SaveChanges();
            }
            return true;
        }

        bool IUserDAO.UpdateEmail(String UserName, String email, String comment, bool isapproved)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.UserName == UserName
                                           select c).FirstOrDefault();
                if (DBUser == null) return false;
                DBUser.Email = email;
                DBUser.Comment = comment;
                DBUser.IsApproved = isapproved;
                GiftEntity.SaveChanges();
            }
            return true;
        }
        bool IUserDAO.UpdatePassword(String UserName, String NewPassword)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.UserName == UserName
                                           select c).FirstOrDefault();
                if (DBUser == null) return false;
                DBUser.Password = NewPassword;
                DBUser.LastPasswordChangedDate = DateTime.Now;
                GiftEntity.SaveChanges();
            }
            return true;
        }

        bool IUserDAO.UpdatePasswordAnswerFailureCount(String UserName, int PasswordAttemptWindow, int MaxInvalidPasswordAttempts)
        {
            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.UserName == UserName
                                           select c).FirstOrDefault();
                if (DBUser == null) return false;
                // update the failure count for the password answer

                DateTime windowStart = new DateTime();
                DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                // First password answer failure or outside of PasswordAttemptWindow. 
                // Start a new password answer failure count from 1 and a new window starting now.

                if (DBUser.FailedPasswordAnswerAttemptCount == 0 || DateTime.Now > windowEnd)
                {
                    DBUser.FailedPasswordAnswerAttemptCount = 1;
                    DBUser.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                }
                else
                {
                    // Password answer attempts have exceeded the failure threshold. Lock out
                    // the user.

                    if (DBUser.FailedPasswordAnswerAttemptCount + 1 >= MaxInvalidPasswordAttempts)
                    {
                        DBUser.IsLockedOut = true;
                        DBUser.LastLockedOutDate = DateTime.Now;
                    }
                    else
                    // Password answer attempts have not exceeded the failure threshold. Update
                    // the failure counts. Leave the window the same.
                    {
                        DBUser.FailedPasswordAnswerAttemptCount = DBUser.FailedPasswordAnswerAttemptCount + 1;
                    }
                }
                GiftEntity.SaveChanges();
            }
            return true;
        }



        bool IUserDAO.ValidateUser(String UserName, String Password, int PasswordAttemptWindow, int MaxInvalidPasswordAttempts)
        {
            bool isValid = false;

            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                                           where c.UserName == UserName
                                           select c).FirstOrDefault();
                if (DBUser == null) return false;
                if (CheckPassword(Password, DBUser.Password))
                {
                    if ((bool)DBUser.IsApproved)
                    {
                        DBUser.LastLoginDate = DateTime.Now;
                        GiftEntity.SaveChanges();
                        isValid = true;
                    }
                    else
                        isValid = false;
                }
                else
                {
                    // update the failure count for the password

                    DateTime windowStart = new DateTime();
                    DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.

                    if (DBUser.FailedPasswordAttemptCount == 0 || DateTime.Now > windowEnd)
                    {
                        DBUser.FailedPasswordAttemptCount = 1;
                        DBUser.FailedPasswordAttemptWindowStart = DateTime.Now;
                    }
                    else
                    {
                        // Password attempts have exceeded the failure threshold. Lock out
                        // the user.

                        if (DBUser.FailedPasswordAttemptCount + 1 >= MaxInvalidPasswordAttempts)
                        {
                            DBUser.IsLockedOut = true;
                            DBUser.LastLockedOutDate = DateTime.Now;
                        }
                        else
                        // Password attempts have not exceeded the failure threshold. Update
                        // the failure counts. Leave the window the same.
                        {
                            DBUser.FailedPasswordAttemptCount = DBUser.FailedPasswordAttemptCount + 1;
                        }
                    }
                    GiftEntity.SaveChanges();
                    isValid = false;
                }

            }
            return isValid;
        }

        bool IUserDAO.ValidateUserByEmail(String emailAddress, String Password, int PasswordAttemptWindow, int MaxInvalidPasswordAttempts, out String userName)
        {
            bool isValid = false;
            userName = "";

            using (var GiftEntity = new GiftEntities())
            {
                User DBUser = (from c in GiftEntity.Users
                               where c.Email == emailAddress
                               select c).FirstOrDefault();
                if (DBUser == null) return false;
                if (CheckPassword(Password, DBUser.Password))
                {
                    if ((bool)DBUser.IsApproved)
                    {
                        DBUser.LastLoginDate = DateTime.Now;
                        GiftEntity.SaveChanges();
                        userName = DBUser.UserName;
                        isValid = true;
                    }
                    else
                        isValid = false;
                }
                else
                {
                    // update the failure count for the password

                    DateTime windowStart = new DateTime();
                    DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.

                    if (DBUser.FailedPasswordAttemptCount == 0 || DateTime.Now > windowEnd)
                    {
                        DBUser.FailedPasswordAttemptCount = 1;
                        DBUser.FailedPasswordAttemptWindowStart = DateTime.Now;
                    }
                    else
                    {
                        // Password attempts have exceeded the failure threshold. Lock out
                        // the user.

                        if (DBUser.FailedPasswordAttemptCount + 1 >= MaxInvalidPasswordAttempts)
                        {
                            DBUser.IsLockedOut = true;
                            DBUser.LastLockedOutDate = DateTime.Now;
                        }
                        else
                        // Password attempts have not exceeded the failure threshold. Update
                        // the failure counts. Leave the window the same.
                        {
                            DBUser.FailedPasswordAttemptCount = DBUser.FailedPasswordAttemptCount + 1;
                        }
                    }
                    GiftEntity.SaveChanges();
                    isValid = false;
                }

            }
            return isValid;
        }




        #endregion

        #region SupportMethods
        //
        // CheckPassword
        //   Compares password values based on the MembershipPasswordFormat.
        //

        public bool CheckPassword(string password, string dbpassword)
        {
            string pass2 = dbpassword;

            //pass2 = GiftEncryption.DecryptPassword(dbpassword);
            pass2 = GiftEncryption.EncryptPassword(password);
            //if (password == pass2)
            if (pass2 == dbpassword)
            {
                return true;
            }
//            Professional_Gift_Card_System.Services.Log.OtherError("Password failed: Entered=" + pass2 + " Database=" + dbpassword);
            return false;
        }

        //
        // EncodePassword
        //   Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
        //

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            encodedPassword = GiftEncryption.EncryptPassword(password);

            return encodedPassword;
        }


        //
        // UnEncodePassword
        //   Decrypts or leaves the password clear based on the PasswordFormat.
        //

        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            //password = GiftEncryption.DecryptPassword(password);
            return password;
        }
        #endregion SupportMethods
    }














    public class UserInRolesDAO : IUserInRolesDAO
    {
        public void AddUserToRole(String username, String RoleName, String ApplicationName)
        {
            if (IsUserInRole(username, RoleName, ApplicationName))
            {
                throw new Exception("User is already in role.");
            }

            using (var GiftEntity = new GiftEntities())
            {

                UsersInRole UR = new UsersInRole();
                UR.Username = username;
                UR.Rolename = RoleName;
                UR.ApplicationName = ApplicationName;
                GiftEntity.UsersInRoles.Add(UR);
                GiftEntity.SaveChanges();
            }
        }
        public bool IsUserInRole(string username, string rolename, String ApplicationName)
        {

            using (var GiftEntity = new GiftEntities())
            {
                var nRole = (from u in GiftEntity.UsersInRoles
                             where u.Rolename == rolename &&
                                   u.Username == username &&
                                   u.ApplicationName == ApplicationName
                             select u.Username).FirstOrDefault();
                if (nRole == null) return false;
            }  // if it is not null, then the username is in the role
            return true;
        }
        void IUserInRolesDAO.DeleteUserFromRole(String UserName)
        {

            using (var GiftEntity = new GiftEntities())
            {
                UsersInRole DBUserInRole = (from c in GiftEntity.UsersInRoles
                               where c.Username == UserName
                               select c).FirstOrDefault();
                if (DBUserInRole == null) return;
                GiftEntity.UsersInRoles.Remove(DBUserInRole);
                GiftEntity.SaveChanges();
            }
        }
        public bool VerifySuperUserExists(String ApplicationName)
        {
            using (var GiftEntity = new GiftEntities())
            {
                var nRole = (from u in GiftEntity.UsersInRoles
                             where u.Rolename == "SystemAdministrator" &&
                                   u.ApplicationName == ApplicationName
                             select u.Username).FirstOrDefault();
                if (nRole == null) return false;
            }  // if it is not null, then the superuser exists
            return true;
        }
    }

    public class RolesDAO : IRolesDAO
    {

        public bool RoleExists(String rolename, String ApplicationName)
        {

            using (var GiftEntity = new GiftEntities())
            {
                var nRole = (from r in GiftEntity.Roles
                             where r.Rolename == rolename &&
                             r.ApplicationName == ApplicationName
                             select r).FirstOrDefault();
                if (nRole == null) return false;
                return true;
            }
        }
        public void InsureSuperUserRoleExists (String ApplicationName)
        {
            using (var GiftEntity = new GiftEntities())
            {
                var nRole = (from r in GiftEntity.Roles
                             where r.Rolename == "SystemAdministrator" &&
                             r.ApplicationName == ApplicationName
                             select r).FirstOrDefault();
                if (nRole == null)
                {
                    Role nr = GiftEntity.Roles.Create();
                    nr.ApplicationName = ApplicationName;
                    nr.Rolename = "SystemAdministrator";
                    GiftEntity.Roles.Add(nr);
                    GiftEntity.SaveChanges();
                }
            }
        }
    }


}