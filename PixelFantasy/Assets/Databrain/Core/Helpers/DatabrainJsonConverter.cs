/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Reflection;
using System;
using System.Linq;

using UnityEngine;

using Databrain.Attributes;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Databrain.Helpers
{
    public class DatabrainJsonConverter : JsonConverter<DataObject>
    {
        private readonly Type[] _ignoredAttributes = { typeof(DatabrainNonSerializeAttribute) };

        public override DataObject ReadJson(JsonReader reader, Type objectType, DataObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            DataObject result = existingValue ?? new DataObject();

            // Deserialize all fields that do not have the IgnoreAttribute
            var properties = typeof(DataObject).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                          .Where(p => !p.GetCustomAttributes(false).Any(a => _ignoredAttributes.Contains(a.GetType())));
            foreach (var property in properties)
            {
                JToken value = jsonObject[property.Name];
                if (value != null && value.Type != JTokenType.Null)
                {
                    property.SetValue(result, value.ToObject(property.PropertyType, serializer));
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, DataObject value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject();

            // Serialize all fields that do not have the IgnoreAttribute
            var properties = typeof(DataObject).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                          .Where(p => !p.GetCustomAttributes(false).Any(a => _ignoredAttributes.Contains(a.GetType())));
            foreach (var property in properties)
            {
                jsonObject.Add(property.Name, JToken.FromObject(property.GetValue(value)));
            }

            jsonObject.WriteTo(writer);
        }
    }
}