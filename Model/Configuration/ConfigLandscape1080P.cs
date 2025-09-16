using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Configuration
{
    public class ConfigLandscape1080P : AppConfiguration
    {
        internal ConfigLandscape1080P() { }

        /// <summary>
        /// X Position of the upper left corner (Desktop in landsape mode).
        /// </summary>
        public override int WindowXPos { get; } = 1888;

        /// <summary>
        /// Y Position of the upper left corner (Desktop in landsape mode).
        /// </summary>
        public override int WindowYPos { get; } = 123;

        /// <summary>
        /// Width of the capture (Desktop in landsape mode).
        /// </summary>
        public override int WindowCaptureWidth { get; } = 672;

        /// <summary>
        /// Height of the capture (Desktop in landsape mode).
        /// </summary>
        public override int WindowCaptureHeight { get; } = 1194;

        /// <summary>
        /// X position of the upper left corner of then panel icon (Desktop in landsape mode).
        /// </summary>
        public override int PanelIconXPos { get; } = 2473;
        /// <summary>
        /// Y position of the upper left corner of then panel icon (Desktop in landsape mode).
        /// </summary>
        public override int PanelIconYPos { get; } = 145;

        public override bool IsInLandscapeMode { get; } = true;
    }
}
