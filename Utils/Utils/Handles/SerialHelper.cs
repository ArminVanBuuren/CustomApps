namespace Utils.Handles
{
    public class SerialHelper
    {
        //public static bool СramDynamicProperty(SerializationInfo propertyBag, StreamingContext context, object @object, string cryptoKey)
        //{

        //}

        //public static bool PullDynamicProperty(SerializationInfo propertyBag, StreamingContext context, object @object, string cryptoKey)
        //{
        //    try
        //    {
        //        Type tp = @object.GetType();

        //        PropertyInfo[] props = tp.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        //        foreach (SerializationEntry result in propertyBag)
        //        {
        //            foreach (PropertyInfo prop in props)
        //            {
        //                if (result.Name.Equals(prop.Name))
        //                {
        //                    //SettingValue<string> settValue =  result.Value as SettingValue<string>;
        //                    string settValue = result.Value as string;
        //                    if (settValue == null)
        //                        continue;

        //                    object potentialContext = Activator.CreateInstance(tp);
        //                    prop.SetValue(@object, AES.DecryptStringAES(settValue, cryptoKey));
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    catch (CryptoException ex)
        //    {
        //        //расшифровка файла неудачна, возможно если ключ в реестре отличается от ключа зашифрованных значений
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}


    }
}
