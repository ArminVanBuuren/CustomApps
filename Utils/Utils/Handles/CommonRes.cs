using System.Reflection;

namespace Utils.Handles
{
    /// <summary>
    /// Summary description for CommonRes.
    /// </summary>
    public static class CommonRes
    {
        private static System.Resources.ResourceManager resourceMan;

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (ReferenceEquals(resourceMan, null))
                {
                    var temp = new System.Resources.ResourceManager("FORIS.ServiceProvisioning.Common.Managing.CommonRes", typeof(CommonRes).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture { get; set; }

        /// <summary>
        ///   Looks up a localized string similar to Set additional service name.
        /// </summary>
        public static string AdditionalServices => ResourceManager.GetString("AdditionalServices", Culture);

        public static string Format(string resID, params object[] args)
        {
            return ResourceHelper.Format("Utils.Handles.CommonRes", Assembly.GetExecutingAssembly(), resID, args);
        }
    }
}
