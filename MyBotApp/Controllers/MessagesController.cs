﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using MyBotApp.ObjController;

namespace MyBotApp
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
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
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);


                // Get Stock information, show user.
                //string ReplyStr = await StockController.GetStock(activity.Text);
                //// return our reply to the user
                //Activity reply = activity.CreateReply(ReplyStr);

                //await connector.Conversations.ReplyToActivityAsync(reply);
                Activity reply;
                bool isWeatherRequest = true;
                var userMessage = activity.Text;
                string ReplyStr = "Hello I am B_Bot";
                if (userMessage.ToLower().Contains("hi")|| userMessage.ToLower().Contains("hello"))
                {  // calculate something for us to return
                    if (userData.GetProperty<bool>("SentGreeting"))
                    {
                        ReplyStr = "Hello again";
                    }
                    else
                    {
                        userData.SetProperty<bool>("SentGreeting", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                }

               
                // help is requested
                else if (userMessage.ToLower().Contains("help"))
                {
                    ReplyStr = "\"help\" - view help.\n \"clear\" - to remove data.\n \"set stock\" - to set yourstock.\n  \"MyStock\" - to view yourstock";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isWeatherRequest = false;
                }
                // clear data
                else if (userMessage.ToLower().Contains("clear"))
                {
                    ReplyStr = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isWeatherRequest = false;
                }
                //  set my stock
                else if (userMessage.Length > 8)
                {
                    if (userMessage.ToLower().Contains("set stock"))
                    {
                        string myStock = userMessage.Substring(10);
                        userData.SetProperty<string>("MyStock", myStock);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        ReplyStr = myStock;
                        isWeatherRequest = false;
                    }
                }

                else if (userMessage.ToLower().Equals("my"))
                {
                    string myStock = userData.GetProperty<string>("MyStock");
                    if (myStock == null)
                    {
                        ReplyStr = "Set MyStock Please. To Set try \"set stock\"";
                        isWeatherRequest = false;
                    }
                    else
                    {
                        activity.Text = myStock;
                       // Get Stock information, show user.
                        ReplyStr = await StockController.GetStock(activity.Text);
                    }
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