using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Enums
{
    public enum Resolution
    {
        _1080P,
        _1440P
    }

    public class ResolutionMethods
    {
        public static string GetResolutionString(Resolution resolution)
        {
            return resolution switch
            {
                Resolution._1080P => "1080P",
                Resolution._1440P => "1440P",
                _ => throw new ArgumentException("What"),
            };
        }
    }
}
