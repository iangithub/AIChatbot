using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.IO;
using System.Web;
using Bot_Application4.Models;
using Newtonsoft.Json;

namespace Bot_Application4.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        const string luisappid = "";
        const string luiskey = "";
        private string luisurl = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/"
            + luisappid +
            "?subscription-key=" + luiskey + "&verbose=true";

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            //get user chat text
            var activity = await result as Activity;

            var answer = string.Empty;

            //Send to LUIS
            WebRequest request = WebRequest.Create(luisurl + "&q=" + HttpUtility.UrlEncode(activity.Text));
            HttpWebResponse luisres = (HttpWebResponse)request.GetResponse();
            Stream datastream = luisres.GetResponseStream();
            StreamReader reader = new StreamReader(datastream);
            string resjson = reader.ReadToEnd();

            //Recevice LUIS Result
            LuisFeedBackResult luisresdata = JsonConvert.DeserializeObject<LuisFeedBackResult>(resjson);

            switch (luisresdata.topScoringIntent.intent)
            {
                case "匯率":
                    var entity = luisresdata.entities[0];

                    if (entity.type == "幣別")
                    {
                        var currency = (string)entity.resolution.values[0];
                        switch (currency)
                        {
                            case "日元":
                                answer = @"日圓匯率參考如下：即期買入0.26360，賣出0.26730 / 現鈔買入 0.26060，賣出 0.26780，以上匯率僅供參考, 請以實際承作匯率為主。 ";
                                break;
                            case "美元":
                                answer = @"美金匯率參考如下：即期買入30.05800，賣出30.16500 / 現鈔買入 29.90800，賣出 30.31500，以上匯率僅供參考, 請以實際承作匯率為主。 ";
                                break;
                        }
                    }
                    break;
                case "客訴":
                    answer = @"智慧小銀，實在感到很抱歉，造成您的困擾，我已將您反應的情況列入檢討事項資料庫。";
                    break;
                case "打招呼":
                    answer = @"我是人工智慧小銀，您好，很高興為您服務！(^0^)，目前我可以提供匯率資訊服務以及記錄客訴問題喔!";
                    break;
                case "感謝":
                    answer = @"不客氣喔，要繼續支持智慧小銀喔( ^.＜ )";
                    break;
                case "None":
                    answer = "我是智慧小銀，您好，實在不好意思，我無法理解您的意思，若有任何意見或建議，歡迎致電客服專線0800123456789。";
                    break;
            }

            reader.Close();
            datastream.Close();
            luisres.Close();

            // return our reply to the user
            await context.PostAsync(answer);

            context.Wait(MessageReceivedAsync);
        }
       
    }
}