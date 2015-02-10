using System;

namespace InJoy.AssetBundles
{
    // this part of the class is dedicated to allow to get version of the component
    // and there is a static property
    public static partial class AssetBundles
    {
        #region Interface

        /// <summary>
        /// Gets the actual version of the component.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public static Version version
        {
            get { return _version ?? (_version = new Version(0, 9, 8)); }
        }

        #endregion
        #region Implementation

        private static Version _version = null;

        #endregion
    }
}
