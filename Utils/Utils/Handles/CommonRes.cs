using System.Reflection;

namespace Utils.Handles
{
    /// <summary>
    /// Summary description for CommonRes.
    /// </summary>
    public static class CommonRes
    {
        private static global::System.Resources.ResourceManager resourceMan;

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("FORIS.ServiceProvisioning.Common.Managing.CommonRes", typeof(CommonRes).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture { get; set; }

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
