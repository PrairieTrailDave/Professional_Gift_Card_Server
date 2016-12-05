using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Professional_Gift_Card_System.Services
{

    /// <summary>
    /// Return Address stack stored in session variable
    /// </summary>
    public static class SessionExtensions
    {
        // this insures that the top address on the return stack is the given value
        public static void InsureReturnAddress(this HttpSessionStateBase session, String rAddress)
        {
            Stack<ReturnAddress> TempStack = new Stack<ReturnAddress>();
            ReturnAddress tReturn = new ReturnAddress(rAddress);
            TempStack.Push(tReturn);
            session["ReturnStack"] = TempStack;
        }

        public static void InsureReturnAddress(this HttpSessionStateBase session, ReturnAddress rAddress)
        {
            Stack<ReturnAddress> TempStack = new Stack<ReturnAddress>();
            TempStack.Push(rAddress);
            session["ReturnStack"] = TempStack;
        }

        // this pushes the return address on the stack but only once

        public static void PushReturnAddress(this HttpSessionStateBase session, String rAddress)
        {
            session.PushReturnAddress(new ReturnAddress(rAddress));
        }
        public static void PushReturnAddress(this HttpSessionStateBase session, ReturnAddress rAddress)
        {
            if (session["ReturnStack"] == null)
                session["ReturnStack"] = new Stack<ReturnAddress>();
            Stack<ReturnAddress> TempStack = (Stack<ReturnAddress>)session["ReturnStack"];
            if (TempStack.Count > 0)
            {
                if (TempStack.Peek().Controller == rAddress.Controller)
                {
                    if (TempStack.Peek().Module != rAddress.Module)
                        TempStack.Push(rAddress);
                }
                else
                    TempStack.Push(rAddress);
            }
            else
                TempStack.Push(rAddress);
        }

        // this pops the return address from the stack
        // but does not remove the top value from the stack
        // thus, this routine can be called multiple times
        // webforms callable version
        public static ReturnAddress PopReturnAddress(this System.Web.SessionState.HttpSessionState session)
        {
            if (session["ReturnStack"] == null)
            {
                session["ReturnStack"] = new Stack<String>();
                return new ReturnAddress("Index", "Home");
            }
            Stack<ReturnAddress> TempStack = (Stack<ReturnAddress>)session["ReturnStack"];
            if (TempStack.Count < 2)
                if (TempStack.Count > 0)
                    return TempStack.Peek();
                else
                    return new ReturnAddress("Index", "Home");
            return TempStack.Pop();
        }
        // razor compatible version
        public static ReturnAddress PopReturnAddress(this HttpSessionStateBase session)
        {
            if (session["ReturnStack"] == null)
            {
                session["ReturnStack"] = new Stack<ReturnAddress>();
                return new ReturnAddress("Index", "Home");
            }
            Stack<ReturnAddress> TempStack = (Stack<ReturnAddress>)session["ReturnStack"];
            if (TempStack.Count < 2)
                if (TempStack.Count > 0)
                    return TempStack.Peek();
                else
                    return new ReturnAddress("Index", "Home");
            return TempStack.Pop();
        }

//        public static T GetDataFromSession<T>(this HttpSessionStateBase session, string key)
//        {
//            return (T)session[key];
//        }
//
//        public static void SetDataInSession(this HttpSessionStateBase session, string key, object value)
//        {
//            session[key] = value;
//        }
    }
    public class ReturnAddress
    {
        public string Module { get; set; }
        public string Controller { get; set; }

        public ReturnAddress(string nModule)
        {
            Module = nModule;
        }
        public ReturnAddress(string nModule, string nController)
        {
            Module = nModule;
            Controller = nController;
        }
    }

}