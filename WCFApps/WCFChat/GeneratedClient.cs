using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WCFChat.Client.ServiceReference1;

namespace WCFChat.Client
{
    public class GeneratedUser : ISerializable
    {
        public GeneratedUser(User user)
        {
            MyUser = user;
        }
        public User MyUser { get; }

        /// <summary>
        /// Специальный вариант конструктора. 
        /// SerializationInfo - объект в который помещаем все пары имя-значение представляющие состояние объекта.
        /// SerializationInfo - мешок со свойствами (property bag)
        /// </summary>
        /// <param name="propertyBag"></param>
        /// <param name="context"></param>
        GeneratedUser(SerializationInfo propertyBag, StreamingContext context)
        {
            MyUser = new User();
            MyUser.Name = propertyBag.GetString("qazwsxedc");
            MyUser.Password = propertyBag.GetString("qazxswedc");
            MyUser.GUID = propertyBag.GetString("zaqxswcde");
            MyUser.Time = propertyBag.GetDateTime("zaqwsxcde");
        }

        /// <summary>
        /// Метод ISerializable.GetObjectData() вызывается Formatter-ом
        /// </summary>
        /// <param name="propertyBag"></param>
        /// <param name="context"></param>
        void ISerializable.GetObjectData(SerializationInfo propertyBag, StreamingContext context)
        {
            propertyBag.AddValue("qazwsxedc", MyUser.Name);
            propertyBag.AddValue("qazxswedc", MyUser.Password);
            propertyBag.AddValue("zaqxswcde", MyUser.GUID);
            propertyBag.AddValue("zaqwsxcde", MyUser.Time);
        }
    }
}
