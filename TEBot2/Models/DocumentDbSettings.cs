using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TEBot2.Models
{
    public class DocumentDbSettings
    {
        public string DatabaseName { get; set; }

        public string CollectionName { get; set; }


        public Uri DatabaseUri { get; set; }

        public string DatabaseKey { get; set; }

        private DocumentClient client { get; set; }

        public void Connect()
        {
            DatabaseName = "Profiles";
            CollectionName = "Profile";
            DatabaseUri = new Uri("https://auevangelistdb.documents.azure.com:443/");
            DatabaseKey = "H7pGL3zB8se0zCoUAfpxZ7tdFWZRieMqMxKHq0FnncxU3Jvp8pSDkRJU3fSGxX3ghkPaxDOSnKs2HgKMCg4ytw ==";

            //Connect
            client = new DocumentClient(DatabaseUri, DatabaseKey);
        }
        public List<Profile> GetProfiles()
        {
            List<Profile> profiles = new List<Profile>();

            //create Query on Profiles DB,
            //var profileQuery = client.CreateDocumentQuery<Profile>(
              // UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName)).Where(d=>d.tags.Contains("a");

            //add each profile to List of profiles.
            //foreach (Profile profile in profileQuery)
            //{
            //    profiles.Add(profile);
            //}

            return profiles;

        }

        public List<Profile> SearchTopic(string topic)
        {
            List<Profile> profiles = new List<Profile>();
            topic = UppercaseFirst(topic);
            //create Query on Profiles DB, GET ALL atm
            var profileQuery = client.CreateDocumentQuery<Profile>(
                UriFactory.CreateDocumentCollectionUri(
                    DatabaseName, CollectionName));

            foreach (Profile profile in profileQuery)
            {
                List<string> tagsLower = new List<string>();
                List<string> statesLower = new List<string>();

                foreach (string tag in profile.tags)
                {
                    tagsLower.Add(tag.ToLower());
                }
                foreach(string state in profile.states)
                {
                    statesLower.Add(state.ToLower());
                }
                
                profile.states = statesLower;
                profile.tags = tagsLower;
                profiles.Add(profile);
            }
            return profiles;
        }

        public string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

    }
}
