using System.Collections;

namespace VD.TelegramBot.Db.Extensions
{
    public static class EntityExtensions
    {
        public static bool IsGenericList(this object obj)
        {
            var type = obj.GetType();
            return type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(IList<>) || type.GetGenericTypeDefinition() == typeof(List<>));
        }
        public static string GenericEntityToString(this object genericType)
        {
            string message = string.Empty;
            var type = genericType.GetType();
            var properties = type.GetProperties();
            if (genericType.IsGenericList())
            {
                var list = (IList)genericType;
                for (int i = 0; i < list.Count; i++)
                {
                    var elementProperties = list[i]?.GetType().GetProperties();
                    if (elementProperties == null)
                        break;
                    for (int j = 0; j < elementProperties.Length; j++)
                    {
                        message += $"{elementProperties[j].Name} : {elementProperties[j].GetValue(list[i])} \n";
                    }
                    if (i + 1 <= list.Count)
                    {
                        message += "\n";
                    }
                }
            }
            else
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    message += $"{properties[i].Name} : {properties[i].GetValue(genericType)} \n";
                }
            }
            return message;
        }
    }
}
