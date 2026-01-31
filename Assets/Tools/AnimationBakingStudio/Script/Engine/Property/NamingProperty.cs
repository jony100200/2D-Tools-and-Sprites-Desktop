using System;

namespace ABS
{
    [Serializable]
    public class NamingProperty : PropertyBase
    {
        public string fileNamePrefix = "";
        public bool useModelPrefixForSprite = true;
    }
}
