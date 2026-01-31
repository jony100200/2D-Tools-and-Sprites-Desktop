namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Supported game types for camera positioning and capture directions.
    /// </summary>
    public enum GameType
    {
        /// <summary>
        /// Side-scrolling games with right-facing camera.
        /// </summary>
        SideScroller,

        /// <summary>
        /// Top-down games with overhead camera.
        /// </summary>
        TopDown,

        /// <summary>
        /// Isometric games with angled camera.
        /// </summary>
        Iso
    }

    /// <summary>
    /// Defines the facing axis that determines the "right" direction for sprite capture.
    /// </summary>
    public enum FacingAxis
    {
        /// <summary>
        /// Positive X axis (right)
        /// </summary>
        PositiveX,

        /// <summary>
        /// Negative X axis (left)
        /// </summary>
        NegativeX,

        /// <summary>
        /// Positive Y axis (up)
        /// </summary>
        PositiveY,

        /// <summary>
        /// Negative Y axis (down)
        /// </summary>
        NegativeY
    }

    /// <summary>
    /// Direction mask for selecting which angles to render.
    /// </summary>
    [System.Flags]
    public enum DirectionMask
    {
        /// <summary>
        /// North direction (0°)
        /// </summary>
        North = 1 << 0,

        /// <summary>
        /// East direction (90°)
        /// </summary>
        East = 1 << 1,

        /// <summary>
        /// South direction (180°)
        /// </summary>
        South = 1 << 2,

        /// <summary>
        /// West direction (270°)
        /// </summary>
        West = 1 << 3,

        /// <summary>
        /// All directions
        /// </summary>
        All = North | East | South | West
    }

    /// <summary>
    /// Axis to rotate around for direction changes.
    /// </summary>
    public enum RotateAxis
    {
        /// <summary>
        /// Rotate around X axis
        /// </summary>
        X,

        /// <summary>
        /// Rotate around Y axis (default)
        /// </summary>
        Y,

        /// <summary>
        /// Rotate around Z axis
        /// </summary>
        Z
    }
}
