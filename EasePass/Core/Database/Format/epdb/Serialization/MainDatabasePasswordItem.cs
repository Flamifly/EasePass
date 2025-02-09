using EasePass.Helper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EasePass.Core.Database.Format.epdb.Serialization
{
    public class MainDatabasePasswordItem
    {
        #region Password Properties
        /// <summary>
        /// The Displayname of the <see cref="MainDatabasePasswordItem"/>
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// The Name of the User
        /// </summary>
        public char[] Username { get; set; }
        /// <summary>
        /// The E-Mail Address of the <see cref="Username"/>
        /// </summary>
        public char[] Email { get; set; }
        /// <summary>
        /// The Notes about the <see cref="MainDatabasePasswordItem"/>
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// The Website, were the <see cref="Username"/> and the <see cref="Password"/> are used for
        /// </summary>
        public char[] Website { get; set; }
        /// <summary>
        /// The Password
        /// </summary>
        public char[] Password { get; set; }
        #endregion

        #region SecondFactor Properties
        /// <summary>
        /// The Amount of Digits of the TOTP Token
        /// </summary>
        public int Digits { get; set; }
        /// <summary>
        /// The Interval until a new Token will be generated
        /// </summary>
        public int Interval { get; set; }
        /// <summary>
        /// The Secret of the TOTP Token
        /// </summary>
        public char[] Secret { get; set; }
        /// <summary>
        /// The Hash Algorithm for the TOTP Tokn
        /// </summary>
        public HashMode Algorithm { get; set; }
        #endregion

        #region Other Properties
        public List<string> Clicks { get; }
        #endregion

        /// Deserialize the given <paramref name="json"/> to the <see cref="ObservableCollection{PasswordManagerItem}"/>
        /// </summary>
        /// <param name="json">The JSON String, which should be deserialized to a <see cref="ObservableCollection{PasswordManagerItem}"/> object</param>
        /// <returns>Returns an Instance of <see cref="ObservableCollection{PasswordManagerItem}"/> if the Deserialization was successfull, otherwise <see cref="default"/> will be returned</returns>
        public static ObservableCollection<MainDatabasePasswordItem> DeserializeItems(string json)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<MainDatabasePasswordItem>>(json);
            }
            catch { }
            return default;
        }
        /// <summary>
        /// Serialize the given <paramref name="items"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="json">The JSON String, which should be serialized to a <see cref="string"/></param>
        /// <returns>Returns an Instance of <see cref="string"/> if the Serialization was successfull, otherwise <see cref="string.Empty"/> will be returned</returns>
        public static string SerializeItems(ObservableCollection<MainDatabasePasswordItem> items)
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
