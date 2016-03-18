using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class JsonExtension
{ 
    public static T Get<T>(this JToken jToken, string key, T defaultValue = default(T))
    {
        JToken ret = jToken[key];
        if (ret == null) return defaultValue;
        if (ret is JObject) return JsonConvert.DeserializeObject<T>(ret.ToString());
        return jToken.Value<T>(key);
    }

    //same as hasOwnMember
    public static bool Contains(this JToken jToken, string key)
    {
        var ret = jToken[key];
        if (ret == null) return false;
        return true;
    }

    //same as Contains
    public static bool hasOwnMember(this JToken jToken, string key)
    {
        var ret = jToken[key];
        if (ret == null) return false;
        return true;
    }
}

