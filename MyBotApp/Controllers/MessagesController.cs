using System;
using System.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
//local
using MyBotApp.ObjController;

namespace MyBotApp
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        TimeZoneInfo nzTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
        DateTime utcNow = DateTime.UtcNow;

        bool currencyRequest = false;
        string inputfromcurrency = string.Empty;
        string inputtocurrency = string.Empty;
        decimal inputamountCurrency = 0;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {       

            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                //state service make accssible and grab data
                StateClient stateClient = activity.GetStateClient();

                Model.BotDataEntities DB = new Model.BotDataEntities();
                // Create a new UserLog object
                Model.TableData NewUserLog = new Model.TableData();

                // Set the properties on the UserLog object

                NewUserLog.Channel = activity.ChannelId;
                NewUserLog.UserID = activity.From.Id;
                NewUserLog.UserName = activity.From.Name;
                NewUserLog.created = TimeZoneInfo.ConvertTimeFromUtc(utcNow, nzTimeZoneInfo);
                NewUserLog.Message = activity.Text.Truncate(500);
                NewUserLog.BeSymbol = null;
                NewUserLog.BeName = null;
                NewUserLog.BeValue = null;


                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            

                // Get Stock information, show user.
                //string ReplyStr = await StockController.GetStock(activity.Text);
                //// return our reply to the user
                //Activity reply = activity.CreateReply(ReplyStr);

                //await connector.Conversations.ReplyToActivityAsync(reply);
                Activity reply;
               
                var userMessage = activity.Text;
                string ReplyStr = "Iam sorry I did not understand";
                if (userMessage.ToLower().Contains("hi")|| userMessage.ToLower().Contains("hello"))
                {  // calculate something for us to return
                    if (userData.GetProperty<bool>("SentGreeting"))
                    {
                        ReplyStr = "Hello again";
                    }
                    else
                    {
                        ReplyStr = "Hello I am B_Bot";
                        userData.SetProperty<bool>("SentGreeting", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                }

               
                // help is requested
                else if (userMessage.ToLower().Contains("help"))
                {
                    ReplyStr = "\"help\" - view help.\n \"clear\" - to remove data.\n \"set stock\" - to set yourstock.\n  \"MyStock\" - to view yourstock";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                
                }
                // clear data
                else if (userMessage.ToLower().Contains("clear"))
                {
                    ReplyStr = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                
                }
                //  set my stock
                else if (userMessage.ToLower().Contains("set stock"))
                 {
                     string myStock = userMessage.Substring(10);
                    if(myStock == null)
                    {

                    }
                    else
                    {
                        userData.SetProperty<string>("MyStock", myStock);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        ReplyStr = myStock;
                    }
               
                 }
               

                else if (userMessage.ToLower().Equals("my"))
                {
                    string myStock = userData.GetProperty<string>("MyStock");
                    if (myStock == null)
                    {
                        ReplyStr = "Set MyStock Please. To Set try \"set stock\"";
                    }
                    else
                    {
                        activity.Text = myStock;
                       // Get Stock information, show user.
                        ReplyStr = await StockController.GetStock(activity.Text);
                    }
                }
                else if (userMessage.ToLower().Equals("msa"))
                {
                    Activity replyToConversation = activity.CreateReply("MSA information");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://cdn2.iconfinder.com/data/icons/ios-7-style-metro-ui-icons/512/MetroUI_iCloud.png"));

                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://msa.ms",
                        Type = "openUrl",
                        Title = "MSA Website"
                    };
                    cardButtons.Add(plButton);

                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Visit MSA",
                        Subtitle = "The MSA Website is here",
                        Images = cardImages,
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                else if (userMessage.ToLower().Equals("contact"))
                {
                    // added fields

                    // return our reply to the user
                    Activity contactReply = activity.CreateReply($"ContactSena");
                    contactReply.Recipient = activity.From;
                    contactReply.Type = "message";
                    contactReply.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://senawebapp.azurewebsites.net/image/massivekittens2.png"));

                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://senawebapp.azurewebsites.net/",
                        Type = "openUrl",
                        Title = "More Info"
                    };
                    cardButtons.Add(plButton);

                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Sena Kim",
                        Subtitle = "Please if you'd like any advice or make any transaction please contact your broker Sena ",
                        Images = cardImages,
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    contactReply.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(contactReply);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                else if (userMessage.ToLower().Equals("list"))
                {
                    var ParamYesterday = TimeZoneInfo.ConvertTimeFromUtc(utcNow, nzTimeZoneInfo).AddDays(-1);
                    // Get the top 5 high scores since yesterday
                    var HighScores = (from TableData in DB.TableDatas
                                      where TableData.BeValue != null
                                      where TableData.BeValue > 0.0
                                      where TableData.created > ParamYesterday
                                      select TableData)
                                        .OrderByDescending(x => x.BeValue)
                                        .Take(5)
                                        .ToList();

                    // Create a response
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("5 infos from lists:\n\n");
                    // Loop through each high score
                    foreach (var Score in HighScores)
                    {
                        // Add the High Score to the response
                        sb.Append(String.Format("- {0} - {1} - {2})\n\n"
                            , Score.BeValue
                            , Score.BeSymbol
                            , Score.BeName));
                        //, Score.created.ToLocalTime().ToShortDateString()
                        //, Score.created.ToLocalTime().ToShortTimeString()));
                    }
                    // Create a reply message
                    Activity replyToConversation = activity.CreateReply();
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    // Set the text containg the High Scores as the response
                    replyToConversation.Text = sb.ToString();
                    // Create a ConnectorClient and use it to send the reply message

                    await connector.Conversations.SendToConversationAsync(replyToConversation);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                else if (userMessage.ToLower().Contains("Currency"))
                {
                    if (inputamountCurrency == 0)
                    {
                        ReplyStr = "Please enter set amount";
                    }
                    if (inputfromcurrency == null)
                    {
                        ReplyStr = "Please enter set from";
                    }
                    if (inputtocurrency == null)
                    {
                        ReplyStr = "Please enter set to";
                    }
                    else
                    {
                        ReplyStr = "Result : " + CurrencyConvert(inputamountCurrency, inputfromcurrency, inputtocurrency);
                    }

                    currencyRequest = true;
                }
                else if (userMessage.ToLower().Contains("set from"))
                {
                    inputfromcurrency = userMessage.Substring(7);

                    userData.SetProperty<string>("from", inputfromcurrency);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    ReplyStr = "you  set from to " + inputfromcurrency;

                }
                else if (userMessage.ToLower().Contains("set to"))
                {
                    inputtocurrency = userMessage.Substring(7);

                    userData.SetProperty<string>("to", inputtocurrency);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    ReplyStr = "you  set to to " + inputtocurrency;


                }
                else if (userMessage.ToLower().Contains("set amount"))
                {
                    string inputamountCurrency = userMessage.Substring(11);
                    userData.SetProperty<string>("amt", inputamountCurrency);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    ReplyStr = "you  set amount to " + inputamountCurrency;

                }

                else
                {
                    // Get Stock information, show user.
                    ReplyStr = await StockController.GetStock(activity.Text);
                    // return our reply to the user
                }

                // return our reply to the user
                reply = activity.CreateReply(ReplyStr);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
            
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        public static string CurrencyConvert(decimal amount, string fromCurrency, string toCurrency)
        {

            //Grab your values and build your Web Request to the API
            string apiURL = String.Format("https://www.google.com/finance/converter?a={0}&from={1}&to={2}&meta={3}", amount, fromCurrency, toCurrency, Guid.NewGuid().ToString());

            //Make your Web Request and grab the results
            var request = WebRequest.Create(apiURL);

            //Get the Response
            var streamReader = new StreamReader(request.GetResponse().GetResponseStream(), System.Text.Encoding.ASCII);

            //Grab your converted value (ie 2.45 USD)
            var result = Regex.Matches(streamReader.ReadToEnd(), "<span class=\"?bld\"?>([^<]+)</span>")[0].Groups[1].Value;

            //Get the Result
            return result;
        }

        private Activity HandleSystemMessage(Activity message)
        {
        
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}