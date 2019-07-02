using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm.Handles
{
    [Serializable]
    public struct FormLocation
    {
        [NonSerialized]
        private Screen _screen;

        public static FormLocation Default { get; } = new FormLocation()
        {
            _screen = Screen.PrimaryScreen,
            Location = Point.Empty
        };

        /// <summary>Returns the location of the form relative to the top-left corner
        /// of the screen that contains the top-left corner of the form, or null if the
        /// top-left corner of the form is off-screen.</summary>
        public FormLocation(Form form)
        {
            foreach (var screen in Screen.AllScreens)
            {
                if (!screen.Bounds.Contains(form.Location))
                    continue;

                _screen = screen;
                Location = new Point(form.Location.X - screen.Bounds.Left, form.Location.Y - screen.Bounds.Top);
                return;
            }

            _screen = Screen.AllScreens.Any() ? Screen.AllScreens.FirstOrDefault() : Screen.PrimaryScreen;
            Location = form.Location;
        }

        public Screen Screen => _screen;
        public Point Location { get; private set; }

        public static bool operator ==(FormLocation first, FormLocation second)
        {
            return Equals(first.Screen, second.Screen) && first.Location == second.Location;
        }

        public static bool operator !=(FormLocation first, FormLocation second)
        {
            return !Equals(first.Screen, second.Screen) || first.Location != second.Location;
        }

        public bool Equals(FormLocation other)
        {
            return Equals(Screen, other.Screen) && Location.Equals(other.Location);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FormLocation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Screen != null ? Screen.GetHashCode() : 0) * 397) ^ Location.GetHashCode();
            }
        }
    }
}