using Model.Enums;
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
        public override int WindowXPos { get; } = 1416;

        /// <summary>
        /// Y Position of the upper left corner (Desktop in landsape mode).
        /// </summary>
        public override int WindowYPos { get; } = 92;

        /// <summary>
        /// Width of the capture (Desktop in landsape mode).
        /// </summary>
        public override int WindowCaptureWidth { get; } = 504;

        /// <summary>
        /// Height of the capture (Desktop in landsape mode).
        /// </summary>
        public override int WindowCaptureHeight { get; } = 896;

        /// <summary>
        /// X position of the upper left corner of then panel icon (Desktop in landsape mode).
        /// </summary>
        public override int PanelIconXPos { get; } = 1856;
        /// <summary>
        /// Y position of the upper left corner of then panel icon (Desktop in landsape mode).
        /// </summary>
        public override int PanelIconYPos { get; } = 87;

        public override bool IsInLandscapeMode { get; } = true;

        public override Resolution Resolution { get; } = Resolution._1080P;
    }
}
