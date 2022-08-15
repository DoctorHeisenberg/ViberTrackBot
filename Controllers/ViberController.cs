using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackBot4.Entities;
using TrackBot4.Services;
using Viber.Bot.NetCore.Infrastructure;
using Viber.Bot.NetCore.Models;
using Viber.Bot.NetCore.RestApi;

namespace TrackBot4.Controllers
{
    [Route("viber")]
    [ApiController]
    public class ViberController : ControllerBase
    {
        private readonly IViberBotApi _viberBotApi;
        private DatabaseServices _databaseServices;

        public ViberController(IViberBotApi viberBotApi, IConfiguration config)
        {
            _viberBotApi = viberBotApi;
            _databaseServices = new DatabaseServices(config);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] object data)
        {
            var callback = JsonConvert.DeserializeObject<Callback>(data.ToString());

            switch (callback.Event)
            {
                case ViberEventType.Message:
                    {
                        string responseMessage;

                        if (callback.Message.textMessage.StartsWith("reply to me top 10"))
                        {
                            var resultDictionary = _databaseServices.GetTop10(long.Parse(callback.Message.textMessage.Split("+").Last()));

                            responseMessage = "Walk date ******* walk time min.";

                            foreach (var row in resultDictionary)
                            {
                                responseMessage += $"\n{row.Key} ***** {row.Value}";
                            }

                            await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(new ViberMessage.KeyboardMessage
                            {
                                Receiver = callback.Sender.Id,
                                Sender = new ViberUser.User()
                                {
                                    Name = "TrackBot"
                                },
                                Text = responseMessage,
                                Keyboard = new ViberKeyboard
                                {
                                    DefaultHeight = false,
                                    Buttons = new List<ViberKeyboardButton>
                                    {
                                        new ViberKeyboardButton
                                        {
                                            ActionType = "reply",
                                            ActionBody = "reply to me back",
                                            Text = "Back",
                                            TextSize = "regular"
                                        }
                                    }
                                }
                            });

                            break;
                        }
                        if (callback.Message.textMessage == "reply to me back")
                        {
                            responseMessage = "Enter your IMEI";

                            await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(new ViberMessage.TextMessage
                            {
                                Receiver = callback.Sender.Id,
                                Sender = new ViberUser.User()
                                {
                                    Name = "TrackBot"
                                },
                                Text = responseMessage
                            });

                            break;
                        }

                        int countOfWalkings = 0;

                        if (!long.TryParse(callback.Message.textMessage, out long imei))
                        {
                            responseMessage = "Wrong format";
                        }
                        else
                        {
                            countOfWalkings = _databaseServices.GetNumberOfWalkings(imei);

                            if (countOfWalkings == 0)
                            {
                                responseMessage = "Wrong IMEI";
                            }
                            else
                            {
                                int timeOfWalkings = _databaseServices.GetTimeOfWalkings(imei);
                                float allDistance = _databaseServices.GetTotalDistance(imei);
                                responseMessage = $"Total walkings: {countOfWalkings}\nTotal walking time: " +
                                    $"{timeOfWalkings} min.\nTotal distance: {allDistance} km.";
                            }
                        }

                        await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(new ViberMessage.KeyboardMessage
                        {
                            Receiver = callback.Sender.Id,
                            Sender = new ViberUser.User()
                            {
                                Name = "TrackBot"
                            },
                            Text = responseMessage,
                            Keyboard = countOfWalkings == 0 ? null : new ViberKeyboard
                            {
                                DefaultHeight = false,
                                Buttons = new List<ViberKeyboardButton>
                                {
                                    new ViberKeyboardButton
                                    {
                                        ActionType = "reply",
                                        ActionBody = $"reply to me top 10+{imei}",
                                        Text = "Top 10 walkings:",
                                        TextSize = "regular"
                                    }
                                }
                            }
                        });

                        break;
                    }
                case ViberEventType.ConversationStarted:
                    {
                        var result = new ViberMessage.TextMessage
                        {
                            Sender = new ViberUser.User()
                            {
                                Name = "TrackBot"
                            },
                            Text = "Enter your IMEI"
                        };
                        return Ok(result);
                    }
            }

            return Ok();
        }
    }
}
