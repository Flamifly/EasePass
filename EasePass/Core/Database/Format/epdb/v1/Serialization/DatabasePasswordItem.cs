using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EasePass.Core.Database.Format.epdb.v1.Serialization
{
    public class DatabasePasswordItem
    {
        public string Password { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public string Secret { get; set; }
        public string Digits { get; set; } = "6";
        public string Interval { get; set; } = "30";
        public string Algorithm { get; set; } = "SHA1";
        public List<string> Clicks { get; } = new List<string>();
        public string DisplayName { get; set; }
        public string Website { get; set; }

        /// <summary>
        /// Deserialize the given <paramref name="json"/> to the <see cref="ObservableCollection{PasswordManagerItem}"/>
        /// </summary>
        /// <param name="json">The JSON String, which should be deserialized to a <see cref="ObservableCollection{PasswordManagerItem}"/> object</param>
        /// <returns>Returns an Instance of <see cref="ObservableCollection{PasswordManagerItem}"/> if the Deserialization was successfull, otherwise <see cref="default"/> will be returned</returns>
        public static ObservableCollection<DatabasePasswordItem> DeserializeItems(string json)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<DatabasePasswordItem>>(json);
            }
            catch { }
            return default;
        }
        /// <summary>
        /// Serialize the given <paramref name="items"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="json">The JSON String, which should be serialized to a <see cref="string"/></param>
        /// <returns>Returns an Instance of <see cref="string"/> if the Serialization was successfull, otherwise <see cref="string.Empty"/> will be returned</returns>
        public static string SerializeItems(ObservableCollection<DatabasePasswordItem> items)
        {
            try
            {
                return JsonConvert.SerializeObject(items, Formatting.Indented);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}