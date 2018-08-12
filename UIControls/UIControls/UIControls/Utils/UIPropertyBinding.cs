using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace UIControls.Utils
{
    [Serializable]
    public class UIPropertyValue<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private T _value;

        [XmlAttribute]
        public virtual T Value
        {
            get { return _value; }
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

    public class UICustomCommands
    {
        /// <summary>
        /// Биндим все необходимые свойства с параметрами в Windows форме. Чтобы значения синхронизировались, если изменить свойство в INotifyPropertyChanged
        /// или изменить свойство DependencyProperty в объекте Window формы. 
        /// Это тоже самое как сделать два эвента с измененными свойствами на примере: MailPassword_OnPasswordChanged, MailPassword_PropertyChanged
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dp"></param>
        /// <param name="notify"></param>
        /// <param name="toolTipShowTimeout"></param>
        public static void DefaultBinding(DependencyObject target, DependencyProperty dp, INotifyPropertyChanged notify, int toolTipShowTimeout = 2000)
        {
            ToolTipService.SetInitialShowDelay(target, toolTipShowTimeout);
            Binding myBinding = new Binding
                                {
                                    Source = notify,
                                    // Value свойства класса SettingsValue<T>
                                    Path = new PropertyPath("Value"),
                                    Mode = BindingMode.TwoWay,
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                };
            BindingOperations.SetBinding(target, dp, myBinding);
        }

        /// <summary>
        /// Клонирует уже существующие объекты представленные в xaml, очень важная хрень!!!!!!!!! Облегчает все в разы. Потому что в ебанном wpf нельзя просто так клонировать объекты.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T XamlClone<T>(T source)
        {
            string savedObject = System.Windows.Markup.XamlWriter.Save(source);

            // Load the XamlObject
            StringReader stringReader = new StringReader(savedObject);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            T target = (T)System.Windows.Markup.XamlReader.Load(xmlReader);

            return target;
        }
    }
}
