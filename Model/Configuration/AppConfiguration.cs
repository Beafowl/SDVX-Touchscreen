using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Configuration
{
    public abstract class AppConfiguration
    {
        /// <summary>
        /// X Position of the upper left corner (Desktop in landsape mode).
        /// </summary>
        public abstract int WindowXPos { get; }

        /// <summary>
        /// Y Position of the upper left corner (Desktop in landsape mode).
        /// </summary>
        public abstract int WindowYPos { get; }

        /// <summary>
        /// Width of the capture (Desktop in landsape mode).
        /// </summary>
        public abstract int WindowCaptureWidth { get; }

        /// <summary>
        /// Height of the capture (Desktop in landsape mode).
        /// </summary>
        public abstract int WindowCaptureHeight { get; }

        /// <summary>
        /// X position of the upper left corner of then panel icon (Desktop in landsape mode).
        /// </summary>
        public abstract int PanelIconXPos { get; }
        /// <summary>
        /// Y position of the upper left corner of then panel icon (Desktop in landsape mode).
        /// </summary>
        public abstract int PanelIconYPos { get; }

        public abstract bool IsInLandscapeMode { get; }

        public bool IsInPortraitMode { get => !IsInLandscapeMode; }
    }
}
