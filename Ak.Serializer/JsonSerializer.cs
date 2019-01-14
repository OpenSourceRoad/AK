
using System;
using Newtonsoft.Json;

namespace Ak.Serializer
{
    public enum TypeNameHandling
    {
        None = Newtonsoft.Json.TypeNameHandling.None,
        Objects = Newtonsoft.Json.TypeNameHandling.Objects,
        Arrays = Newtonsoft.Json.TypeNameHandling.Arrays,
        All = Arrays | Objects,
        Auto = Newtonsoft.Json.TypeNameHandling.Auto,
    }
    public class JsonSerializer 
    {
        public static string Serialize(object value, TypeNameHandling typeNameHandling)
        {
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = (Newtonsoft.Json.TypeNameHandling) typeNameHandling };
            var serialized = JsonConvert.SerializeObject(value, value.GetType(), jsonSettings);

            return serialized;
        }

        public static string Serialize(object value)
        {
            var jsonSettings = new JsonSerializerSettings
                { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
            };
            var serialized = JsonConvert.SerializeObject(value, value.GetType(), jsonSettings);

            return serialized;
        }

        public static string Serialize<T>(object value)
        {
            var jsonSettings = new JsonSerializerSettings
                { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
            };
            var serialized = JsonConvert.SerializeObject(value, typeof(T), jsonSettings);

            return serialized;
        }

        public static string Serialize<T>(object value, TypeNameHandling typeNameHandling)
        {
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = (Newtonsoft.Json.TypeNameHandling) typeNameHandling };
            var serialized = JsonConvert.SerializeObject(value, typeof(T), jsonSettings);

            return serialized;
        }

        public  static T Deserialize<T>(string value)
        {
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto };
            var deserialized = JsonConvert.DeserializeObject<T>(value, jsonSettings);
            return deserialized;
        }

        public static object Deserialize(string value, Type type)
        {
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto };
            var deserialized = JsonConvert.DeserializeObject(value, type,  jsonSettings);
            return  deserialized;
        }
        public static object Deserialize(string value)
        {
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto };
            var deserialized = JsonConvert.DeserializeObject(value, jsonSettings);
            return deserialized;
        }

    }
}
