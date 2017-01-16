using System;
using UnityEngine;

namespace World
{
    static class Logger
    {
        static string log = null;

        //public static void Log(string msg)
        //{
        //    if (log == null)
        //    {
        //        log = msg;
        //        return;
        //    }
        //    log += "\n" + msg;
        //}

        public static void Flush()
        {
            if(log == null)
            {
                return;
            }
            Debug.Log(log);
            log = null;
        }
    }
}
