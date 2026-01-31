using System;

namespace ABS
{
    [Serializable]
    public class TrimingProperty : PropertyBase
    {
        public bool on = true;
        public int margin = 2;
        public bool unifySize = false;

        public bool ShouldUnifySize()
        {
            return on && unifySize;
        }
    }
}
