using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Configuration
{
    public class ConfigurationFactory
    {
        /// <summary>
        /// Returns an instance of the <see cref="AppConfiguration"/> class depending on the
        /// environment.
        /// </summary>
        /// <param name="isInLandscapeMode">Whether Windows is in landscape mode.</param>
        /// <param name="res">The resolution of the display where the game runs.</param>
        /// <returns></returns>
        public static AppConfiguration GetConfiguration(bool isInLandscapeMode, Resolution res)
        {
            if (!isInLandscapeMode)
                throw new Exception("Portrait mode is currently not supported. Please set Windows to landscape mode and the game to portrait.");
            if (res == Resolution._1440P)
                return new ConfigLandscape1440P();
            return new ConfigLandscape1080P();
        }
    }
}
