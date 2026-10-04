using System;
using System.Collections.Generic;
using System.Linq;
namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Class that represents custom field
    /// </summary>
    public class CustomField
    {
        /// <summary>
        /// Custom type, for example "post" or "theme"
        /// </summary>
        public string CustomType { get; set; }
        /// <summary>
        /// Object ID, for example post ID or theme name
        /// </summary>
        public string ObjectId { get; set; }
        /// <summary>
        /// The blog ID
        /// </summary>
        public Guid BlogId { get; set; }
        /// <summary>
        /// The key in the key/value pair
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The value. Can be simple string or string
        /// representation of object that client can parse
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// custom meta data like "hidden", "style" etc.
        /// </summary>
        public string Attribute { get; set; }
        /// <summary>
        /// Used to assign key to HTML controls when edit field in admin
        /// </summary>
        public string ControlId
        {
            get
            {
                return Utils.RemoveIllegalCharacters(Key);
            }
        }

        /// <summary>
        /// Collapses fields that share a key (case-insensitive) into one, so the
        /// editor can never store, and the site never has to choke on, duplicates.
        /// Duplicate "Role" fields are merged into one comma-separated list (the
        /// format Security.IsInRole already reads); for any other key the last wins.
        /// </summary>
        public static List<CustomField> Coalesce(IEnumerable<CustomField> fields)
        {
            var result = new List<CustomField>();
            foreach (var field in fields)
            {
                if (field == null || field.Key == null)
                    continue;

                var i = result.FindIndex(r => string.Equals(r.Key, field.Key, StringComparison.OrdinalIgnoreCase));
                if (i < 0)
                {
                    result.Add(field);
                }
                else if (string.Equals(field.Key, "role", StringComparison.OrdinalIgnoreCase))
                {
                    var current = result[i];
                    var roles = ((current.Value ?? "") + "," + (field.Value ?? ""))
                        .Split(',')
                        .Select(r => r.Trim())
                        .Where(r => r.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase);

                    result[i] = new CustomField
                    {
                        CustomType = current.CustomType,
                        ObjectId = current.ObjectId,
                        BlogId = current.BlogId,
                        Key = current.Key,
                        Attribute = current.Attribute,
                        Value = string.Join(",", roles)
                    };
                }
                else
                {
                    result[i] = field;
                }
            }
            return result;
        }

        /// <summary>
        /// Key/field dictionary (case-insensitive key) of the coalesced fields.
        /// </summary>
        public static Dictionary<string, CustomField> ToDictionary(IEnumerable<CustomField> fields)
        {
            return Coalesce(fields).ToDictionary(f => f.Key, f => f, StringComparer.OrdinalIgnoreCase);
        }
    }
}