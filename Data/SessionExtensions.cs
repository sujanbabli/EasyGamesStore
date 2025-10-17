using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyGamesStore.Data
{
    // This is a helper class to store and retrieve complex objects in the session.
    // Normally, session can only store strings or byte arrays, so this allows us
    // to save things like lists, carts, or custom objects as JSON.
    public static class SessionExtensions
    {
        // Save an object to the session as JSON string.
        // "this ISession session" means this is an extension method and we can call
        // it like session.SetObjectAsJson("Cart", cartList)
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            // Convert the object to a JSON string and save it in session under the given key
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // Retrieve an object from the session.
        // Returns the object type we specify (T). If nothing is found, returns default value (null for reference types).
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            // Get the JSON string from the session
            var value = session.GetString(key);

            // If the value is null, return default. Otherwise, convert the JSON string back into an object of type T
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
