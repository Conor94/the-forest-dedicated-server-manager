using System;
using System.Collections.Generic;

namespace Extensions
{
    public static class ExceptionExtension
    {
        public static List<Exception> GetExceptions(this Exception e)
        {
            List<Exception> exceptions = new List<Exception>();
            while (e != null)
            {
                exceptions.Add(e);
                e = e.InnerException;
            }
            return exceptions;
        }
    }
}
