using System;
using Newtonsoft.Json;

namespace Minx.ZRpcNet.Serialization
{
    /// <summary>
    /// To address issues with automatic Int64 deserialization -- see https://stackoverflow.com/a/9444519/1037948
    /// </summary>
    public class Int32Converter : JsonConverter
    {
        /// <summary>
        /// Only want to deserialize
        /// </summary>
        public override bool CanWrite { get { return false; } }

        /// <summary>
        /// Placeholder for inheritance -- not called because <see cref="CanWrite"/> returns false
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // since CanWrite returns false, we don't need to implement this
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                try
                {
                    return Convert.ToInt32(reader.Value);
                }
                catch
                {
                    return Convert.ToInt64(reader.Value);
                }
            }
            else
            {
                return serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int) ||
                    objectType == typeof(long) ||
                    // need this last one in case we "weren't given" the type
                    // and this will be accounted for by `ReadJson` checking tokentype
                    objectType == typeof(object);
        }
    }
}
