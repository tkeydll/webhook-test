﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GitHubEventHandler
{
    public static class EventDataFormatter
    {
        public static string FormatEventData(string jsonData)
        {
            var jsonObject = JObject.Parse(jsonData);

            // Remove repository and sender and metadata fields
            jsonObject.Remove("repository");
            jsonObject.Remove("sender");
            jsonObject.Remove("_rid");
            jsonObject.Remove("_self");
            jsonObject.Remove("_etag");
            jsonObject.Remove("_attachments");
            jsonObject.Remove("_ts");

            // Format issue, comment, and pull_request fields
            FormatField(jsonObject, "issue");
            FormatField(jsonObject, "comment");
            FormatField(jsonObject, "pull_request");

            return jsonObject.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private static void FormatField(JObject jsonObject, string fieldName)
        {
            if (jsonObject[fieldName] != null)
            {
                var fieldObject = (JObject)jsonObject[fieldName];

                // Keep only specific properties
                var propertiesToKeep = new HashSet<string> { "url", "id", "title", "user", "labels", "state", "locked", "assignees", "milestone", "created_at", "updated_at", "closed_at", "body", "changes", "pull_request" };
                var propertiesToRemove = fieldObject.Properties().Where(p => !propertiesToKeep.Contains(p.Name)).ToList();
                foreach (var prop in propertiesToRemove)
                {
                    prop.Remove();
                }

                // Format user and assignees fields
                FormatUserField(fieldObject, "user");
                FormatUserField(fieldObject, "assignees");

                // Format labels field
                if (fieldObject["labels"] != null)
                {
                    var labelsArray = (JArray)fieldObject["labels"];
                    foreach (var label in labelsArray.Children<JObject>())
                    {
                        var labelPropertiesToKeep = new HashSet<string> { "description" };
                        var labelPropertiesToRemove = label.Properties().Where(p => !labelPropertiesToKeep.Contains(p.Name)).ToList();
                        foreach (var prop in labelPropertiesToRemove)
                        {
                            prop.Remove();
                        }
                    }
                }
            }
        }

        private static void FormatUserField(JObject fieldObject, string fieldName)
        {
            if (fieldObject[fieldName] != null)
            {
                if (fieldObject[fieldName].Type == JTokenType.Array)
                {
                    var userArray = (JArray)fieldObject[fieldName];
                    foreach (var user in userArray.Children<JObject>())
                    {
                        var userPropertiesToKeep = new HashSet<string> { "login" };
                        var userPropertiesToRemove = user.Properties().Where(p => !userPropertiesToKeep.Contains(p.Name)).ToList();
                        foreach (var prop in userPropertiesToRemove)
                        {
                            prop.Remove();
                        }
                    }
                }
                else if (fieldObject[fieldName].Type == JTokenType.Object)
                {
                    var userObject = (JObject)fieldObject[fieldName];
                    var userPropertiesToKeep = new HashSet<string> { "login" };
                    var userPropertiesToRemove = userObject.Properties().Where(p => !userPropertiesToKeep.Contains(p.Name)).ToList();
                    foreach (var prop in userPropertiesToRemove)
                    {
                        prop.Remove();
                    }
                }
            }
        }
    }
}
