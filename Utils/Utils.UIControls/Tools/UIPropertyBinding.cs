using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;

namespace Utils.UIControls.Tools
{
    [Serializable]
    public class UIPropertyValue<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
            return Value != null ? Value.ToString() : base.ToString();
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
            var myBinding = new Binding
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
            var savedObject = System.Windows.Markup.XamlWriter.Save(source);

            // Load the XamlObject
            var stringReader = new StringReader(savedObject);
            var xmlReader = System.Xml.XmlReader.Create(stringReader);
            T target = (T)System.Windows.Markup.XamlReader.Load(xmlReader);

            return target;
        }
    }
}
