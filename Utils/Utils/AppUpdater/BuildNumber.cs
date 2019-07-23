using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Utils.AppUpdater
{
    [Serializable]
    public class BuildNumber : IComparable
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public int Revision { get; private set; }

        BuildNumber()
        {

        }

        BuildNumber(DateTime buildDateTime)
        {
            if (buildDateTime == null)
                throw new ArgumentNullException(nameof(buildDateTime));

            var startOfDay = DateTime.ParseExact(buildDateTime.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.CurrentCulture);
            var substrSec = buildDateTime.Subtract(startOfDay);
            var startOfYear = new DateTime(2000, 1, 1);
            var substrDay = buildDateTime.Subtract(startOfYear);
            Major = 1;
            Minor = 0;
            Build = (int) substrDay.TotalDays;
            Revision = (int) (substrSec.TotalSeconds / 2);
        }

        public static BuildNumber FromFile(string file)
        {
            if (!File.Exists(file))
                throw new ArgumentException($"File [{file}] not exist!");

            var fileVersion = FileVersionInfo.GetVersionInfo(file);
            if (fileVersion.FileVersion == null)
            {
                var lastChange = File.GetLastWriteTime(file);
                return new BuildNumber(lastChange);
            }
            else
            {
                if (!TryParse(fileVersion, out var getVers))
                {
                    TryParse("1.0.0.0", out getVers);
                }

                return getVers;
            }
        }

        public static bool TryParse(string input, out BuildNumber buildNumber)
        {
            try
            {
                buildNumber = Parse(input);
                return true;
            }
            catch
            {
                buildNumber = null;
                return false;
            }
        }

        public static bool TryParse(FileVersionInfo input, out BuildNumber buildNumber)
        {
            try
            {
                buildNumber = Parse(input);
                return true;
            }
            catch
            {
                buildNumber = null;
                return false;
            }
        }

        public static BuildNumber Parse(FileVersionInfo fileVersion)
        {
            return new BuildNumber
            {
                Major = fileVersion.FileMajorPart,
                Minor = fileVersion.FileMinorPart,
                Build = fileVersion.FileBuildPart,
                Revision = fileVersion.FilePrivatePart
            };
        }

        /// <summary>
        /// Parses a build number string into a BuildNumber class
        /// </summary>
        /// <param name="buildNumber">The build number string to parse</param>
        /// <returns>A new BuildNumber class set from the buildNumber string</returns>
        /// <exception cref="ArgumentException">Thrown if there are less than 2 or 
        /// more than 4 version parts to the build number</exception>
        /// <exception cref="FormatException">Thrown if string cannot be parsed 
        /// to a series of integers</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any version 
        /// integer is less than zero</exception>
        public static BuildNumber Parse(string buildNumber)
        {
            if (buildNumber == null)
                throw new ArgumentNullException(nameof(buildNumber));

            if (DateTime.TryParse(buildNumber, out var dateTime))
                return new BuildNumber(dateTime);

            var versions = buildNumber.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToList();

            if (versions.Count < 2)
                throw new ArgumentException("BuildNumber string was too short");

            if (versions.Count > 4)
                throw new ArgumentException("BuildNumber string was too long");

            return new BuildNumber
            {
                Major = ParseVersion(versions[0]),
                Minor = ParseVersion(versions[1]),
                Build = versions.Count > 2 ? ParseVersion(versions[2]) : -1,
                Revision = versions.Count > 3 ? ParseVersion(versions[3]) : -1
            };
        }

        private static int ParseVersion(string input)
        {
            if (!int.TryParse(input, out var version))
                throw new FormatException("buildNumber string was not in a correct format");

            if (version < 0)
                throw new ArgumentOutOfRangeException(nameof(input), "Versions must be greater than or equal to zero");

            return version;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            var buildNumber = obj as BuildNumber;
            if (buildNumber == null)
                return 1;

            if (ReferenceEquals(this, buildNumber))
                return 0;

            if (Major == buildNumber.Major)
            {
                if (Minor == buildNumber.Minor)
                {
                    if (Build == buildNumber.Build)
                        return Revision.CompareTo(buildNumber.Revision);
                    else
                        return Build.CompareTo(buildNumber.Build);
                }
                else
                {
                    return Minor.CompareTo(buildNumber.Minor);
                }
            }
            else
            {
                return Major.CompareTo(buildNumber.Major);
            }
        }

        public static bool operator >(BuildNumber first, BuildNumber second)
        {
            return (first.CompareTo(second) > 0);
        }

        public static bool operator <(BuildNumber first, BuildNumber second)
        {
            return (first.CompareTo(second) < 0);
        }

        public static bool operator ==(BuildNumber first, BuildNumber second)
        {
            return first != null && (first.CompareTo(second) == 0);
        }

        public static bool operator !=(BuildNumber first, BuildNumber second)
        {
            return first != null && (first.CompareTo(second) != 0);
        }

        public override bool Equals(object obj)
        {
            return (CompareTo(obj) == 0);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Major.GetHashCode();
                hash = hash * 23 + Minor.GetHashCode();
                hash = hash * 23 + Build.GetHashCode();
                hash = hash * 23 + Revision.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}{(Build < 0 ? "" : "." + Build)}{(Revision < 0 ? "" : "." + Revision)}";
        }
    }
}