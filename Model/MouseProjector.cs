using SimWinInput;

namespace Model
{
    /// <summary>
    /// Class that projects a mouse click to another position.
    /// </summary>
    public class MouseProjector
    {
        private int _windowX;

        private int _windowY;

        public MouseProjector(int windowX, int windowY)
        {
            _windowX = windowX;
            _windowY = windowY;
        }


        public void HandleMouseClick(int posX, int posY)
        {
            // TODO: Calculate position in new window and perform mouse click
            NativeMethods.SendClickWithoutMoving(_windowX + posX, _windowY + posY);
            //SimMouse.Click(MouseButtons.Left, _windowX + posX, _windowY + posY);
        }
    }
}
