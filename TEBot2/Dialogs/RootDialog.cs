using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using TEBot2.Models;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;

namespace TEBot2.Dialogs
{
    [LuisModel("b38bb9d7-e591-49ea-87c8-241af8a7dad9", "f95f96895c014832b41d5dd9eec17fb8")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("FindSpeaker")]
        public async Task FindSpeaker(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            string topic = "";
            string location = "";
            if (result.Entities.Count > 0)
            {
                var ent = new EntityRecommendation();
                bool hasTopic = result.TryFindEntity("topic", out ent);
                topic = (hasTopic ? ent.Entity : "");
                bool hasLocation = result.TryFindEntity("location", out ent);
                location = (hasLocation ? ent.Entity : "");
                

                    await context.PostAsync("Searching with *topic* = **" + topic + "** and *location* = **" + location + "**");
                if (!hasLocation)
                {
                    await context.PostAsync("What state do you want a speaker in?");
                    var adaptive = new HeroCard();
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction p1Button = new CardAction("button") { Value = $"Find Speaker {topic} New South Wales", Title = "New South Wales", Type = ActionTypes.PostBack };
                    CardAction p2Button = new CardAction("button") { Value = $"Find Speaker {topic} Victoria", Title = "Victoria", Type = ActionTypes.PostBack };
                    CardAction p3Button = new CardAction("button") { Value = $"Find Speaker {topic} Queensland", Title = "Queensland", Type = ActionTypes.PostBack };
                    cardButtons.Add(p1Button);
                    cardButtons.Add(p2Button);
                    cardButtons.Add(p3Button);
                    adaptive.Buttons = cardButtons;
                    var attach = adaptive.ToAttachment();
                    reply.Attachments.Add(attach);

                    await context.PostAsync(reply);
                    context.Wait(this.MessageReceived);
                }
                else if (hasTopic && hasLocation)
                {
                    if (location.Equals("nsw") || location.Equals("sydney"))
                        location = "new south wales";
                    else if (location.Equals("qld") || location.Equals("brisbane"))
                        location = "queensland";
                    else if (location.Equals("vic") || location.Equals("melbourne"))
                        location = "victoria";

                    if (!(location.Equals("new south wales") || location.Equals("queensland") || location.Equals("victoria")))
                        hasLocation = false;

                    if (hasLocation)
                    {
                        var db = new DocumentDbSettings();
                        db.Connect();
                        List<Profile> profiles = db.SearchTopic(topic);

                        Random rnd = new Random();
                        IEnumerable<Profile> res = profiles.Where(p => p.tags.Any(m => m.Contains(topic))).Where(c => c.states.Any(m => m.Contains(location)));
                        var filteredProfiles = res.ToList<Profile>().OrderBy(p => rnd.Next());

                        if (filteredProfiles.Count() > 0)
                        {
                            reply.Attachments.Clear();
                            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            foreach (Profile p in filteredProfiles)
                            {
                                //Site and Email buttons on each card.
                                List<CardAction> buttons = new List<CardAction>();
                                CardAction siteButton = new CardAction("button") { Value = $"{p.website}", Title = "Website", Type = ActionTypes.OpenUrl };
                                CardAction emailButton = new CardAction("button") { Value = $"mailto:{p.email}", Title = "Email", Type = ActionTypes.OpenUrl };
                                buttons.Add(siteButton);
                                buttons.Add(emailButton);

                                var hero = new HeroCard();
                                hero.Subtitle = p.slogan;
                                hero.Title = p.name;

                                var bioText = p.bio;
                                //var bioTrunc = TruncateText(bioText, 220);
                                //bioTrunc += "...";

                                hero.Text = bioText;
                                var image = new CardImage();
                                image.Url = "http://tebot2.azurewebsites.net" + p.picture;
                                hero.Images.Add(image);
                                hero.Buttons = buttons;
                                var heroAttach = hero.ToAttachment();
                                reply.Attachments.Add(heroAttach);
                            }

                            await context.PostAsync(reply);
                            context.Wait(this.MessageReceived);
                        }
                    }
                    else
                    {
                        await context.PostAsync("Please search for speakers in New South Wales, Queensland or Victoria");
                    }
                }
                else
                {
                    await context.PostAsync("Topic unable to be captured.");
                } 

            }
            else
            {
                await context.PostAsync("Topic could not be found. Please try another topic.");
            }
        }

        public string TruncateText(string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            string message = $"Welcome to the TE Bot. Specify a topic you'd like to find a speaker for.\n\nTry a query like 'Find speaker [topic] [location]'.\n\nLocation must be an AU state (NSW/VIC/QLD)";
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }
    }
}