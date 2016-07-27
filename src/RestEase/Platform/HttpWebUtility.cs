using System;
using System.Collections.Generic;
using System.Text;
using RestEase.Implementation;

namespace RestEase.Platform
{
    public static class HttpWebUtility
    {
        public static string UrlEncode(string input)
        {
#if NET40
            return System.Web.HttpUtility.UrlEncode(input);
#else
            return System.Net.WebUtility.UrlEncode(input);
#endif
        }
    }
}
