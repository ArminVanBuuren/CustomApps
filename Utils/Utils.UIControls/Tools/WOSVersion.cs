namespace Utils.UIControls.Tools
{
    public class WOSVersion
    {
        public WOSVersion()
        {
            System.OperatingSystem osInfo = System.Environment.OSVersion;
            switch (osInfo.Platform)
            {
                case System.PlatformID.Win32Windows:

                    switch (osInfo.Version.Minor)
                    {
                        case 0:
                            {
                                Version = WOSNames.Win95;
                                Name = "Windows 95";
                                break;
                            }
                        case 10:
                            {
                                if (osInfo.Version.Revision.ToString() == "2222A")
                                {
                                    Version = WOSNames.Win98SecondEdition;
                                    Name = "Windows 98 Second Edition";
                                }
                                else
                                {
                                    Version = WOSNames.Win98;
                                    Name = "Windows 98";
                                }
                                break;
                            }
                        case 90:
                            {
                                Version = WOSNames.WinMe;
                                Name = "Windows Me";
                                break;
                            }
                    }
                    break;

                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                // or Windows XP.
                case System.PlatformID.Win32NT:

                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            {
                                Version = WOSNames.WinNT351;
                                Name = "Windows NT 3.51";
                                break;
                            }
                        case 4:
                            {
                                Version = WOSNames.WinNT40;
                                Name = "Windows NT 4.0";
                                break;
                            }
                        case 5:
                            {
                                if (osInfo.Version.Minor == 0)
                                {
                                    Version = WOSNames.Win2000;
                                    Name = "Windows 2000";
                                }
                                else
                                {
                                    Version = WOSNames.WinXP;
                                    Name = "Windows XP";
                                }
                                break;
                            }
                        case 6:
                            {
                                Version = WOSNames.Win8;
                                Name = "Windows 8";
                                break;
                            }
                    }
                    break;
            }
        }

        public string Name { get; set; } = string.Empty;
        public WOSNames Version { get; set; } = WOSNames.None;
    }
}
