using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Utils.Handles;

namespace TFSAssist.Control.DataBase.Settings
{
    [Serializable]
	public class SettingValue<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;

            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private T _value;

	    [XmlAttribute]
	    public virtual T Value
	    {
	        get => _value;
            set
	        {
	            if (_value != null && _value.Equals(value))
	                return;
	            _value = value;
	            OnPropertyChanged();
            }
	    }

	    public override string ToString()
		{
			return Value.ToString();
		}
	}

    [Serializable, XmlRoot("BootRun")]
    public class SettingBootRun : SettingValue<bool>
    {
        [XmlAttribute]
        public override bool Value
        {
            get => RegeditControl.EnabledBootRun(TFSControl.ApplicationName);
            set
            {
                RegeditControl.SetBootStartup(TFSControl.ApplicationName, TFSControl.ApplicationPath, value);
                base.Value = value;
            }
        }
    }

    //[Serializable]
    //public class SettingValue<T> : INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected virtual void OnPropertyChanged (string propertyName)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    protected bool SetField<T> (ref T field, T value, string propertyName)
    //    {
    //        if (EqualityComparer<T>.Default.Equals(field, value))
    //            return false;
    //        field = value;
    //        OnPropertyChanged(propertyName);
    //        return true;
    //    }


    //    private T _value;

    //    [XmlAttribute]
    //    public T Value
    //    {
    //        get { return _value; }
    //        set
    //        {
    //            if (_value != null && _value.Equals(value))
    //                return;
    //            //_value = value;
    //            SetField(ref _value, value, "Value");
    //        }
    //    }

    //    public void OutChanged (T value)
    //    {
    //        _value = value;
    //    }

    //    public override string ToString ()
    //    {
    //        return Value.ToString();
    //    }
    //}
}
