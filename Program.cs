using System.Data;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml;
using System.IO;
using System.Transactions;
using System.Net.Mail;

StringBuilder _stringBuilder = new StringBuilder();
Dictionary<string, DateTime> lastSendEmailTime = new Dictionary<string, DateTime>();
Dictionary<string, DateTime> showLogTime = new Dictionary<string, DateTime>();
int delayForSendEmailMinute = 360;
Console.WriteLine("Service Start");


string settingFileName = "Setting.txt";
string _url = "https://api.coinmarketcap.com/data-api/v3/cryptocurrency/detail/chart";
while (true)
{
    StartThread();
    System.Threading.Thread.Sleep(60000);
}
void StartThread()
{
    try
    {
        //var settings = File.ReadAllLines(settingFileName);
        var settings = "1027 29270 2343 2252".Split(' ');
        //Console.WriteLine("File Readed : " + settings.Length);
        for (int i = 0; i < settings.Length; i += 4)
        {
            var firstCoin = Convert.ToInt32(settings[i + 0]);
            var secondCoin = Convert.ToInt32(settings[i + 1]);

            var highRange = Convert.ToDecimal(settings[i + 2]);
            var lowRange = Convert.ToDecimal(settings[i + 3]);

            if (!lastSendEmailTime.Keys.Contains(firstCoin.ToString() + secondCoin.ToString()))
            {
                lastSendEmailTime.Add(firstCoin.ToString() + secondCoin.ToString(), DateTime.MinValue);
                showLogTime.Add(firstCoin.ToString() + secondCoin.ToString(), DateTime.MinValue);
            }

            dynamic json = GetProducts(_url, firstCoin);
            if (json == null) return;
            var data = json["data"];//.data;
            var points = data["points"];
            if (points == null) return;
            var lastKey = "";
            foreach (var point in points)
            {
                lastKey = point.Key;
            }
            var v = points[lastKey]["v"] as JsonArray;
            var firstPrice = Convert.ToDecimal(v[0].ToString());


            json = GetProducts(_url, secondCoin);
            if (json == null) return;
            data = json["data"];//.data;
            points = data["points"];
            if (points == null) return;
            lastKey = "";
            foreach (var point in points)
            {
                lastKey = point.Key;
            }
            v = points[lastKey]["v"] as JsonArray;
            var secondPrice = Convert.ToDecimal(v[0].ToString());



            if (showLogTime[firstCoin.ToString() + secondCoin.ToString()].AddMinutes(30) < DateTime.Now)
            {
                Console.WriteLine("First Coin : " + (Currencies)firstCoin + " " + firstPrice + "\n" +
                                    "Second Coin : " + (Currencies)secondCoin + " " + secondPrice +  "\n" +
                                    "Low Range : " + lowRange + "\n" +
                                    "High Range : " + highRange + "\n" +
                                    "Current Range : " + (firstPrice / secondPrice));
                showLogTime[firstCoin.ToString() + secondCoin.ToString()] = DateTime.Now;
            }

            if (lastSendEmailTime[firstCoin.ToString() + secondCoin.ToString()].AddMinutes(delayForSendEmailMinute) > DateTime.Now)
                continue;


            lastSendEmailTime[firstCoin.ToString() + secondCoin.ToString()] = DateTime.Now;

            if (firstPrice / secondPrice > highRange) SendMail("High Range", "First Coin : " + (Currencies)firstCoin + " " + firstPrice + "</br>" +
                                                            "Second Coin : " + (Currencies)secondCoin + " " + secondPrice + "</br>" +
                                                            "High Range : " + highRange + "</br>" +
                                                            "Current Range : " + (firstPrice / secondPrice));
            if (firstPrice / secondPrice < lowRange) SendMail("Low Range", "First Coin : " + (Currencies)firstCoin + " " + firstPrice + "</br>" +
                                                                "Second Coin : " + (Currencies)secondCoin + " " + secondPrice + "</br>" +
                                                                "Low Range : " + lowRange + "</br>" +
                                                                "Current Range : " + (firstPrice / secondPrice));
        }
    }
    catch (Exception ex)
    {
    }
}
object GetProducts(string url, int coinId)
{
    try
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url + "?id=" + coinId + "&range=1D");
        //request.Headers.Add("User-Agent", "PostmanRuntime/7.36.3");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:132.0) Gecko/20100101 Firefox/132.0");

        var response = client.Send(request);
        response.EnsureSuccessStatusCode();
        var result = response.Content.ReadAsStream();
        StreamReader reader = new StreamReader(result);
        var divarResponse = reader.ReadToEnd();
        return JsonNode.Parse(divarResponse);
    }
    catch { return null; }

}
void SendMail(string mailSubject, string mailText)
{
    string recipient = "reza.yy@gmail.com";


    Console.WriteLine("Email Send : " + mailSubject);
    using (MailMessage mail = new MailMessage())
    {
        mail.From = new MailAddress(recipient);
        mail.To.Add(recipient);
        mail.Subject = mailSubject;
        mail.Body = "<h1>" + mailText + "</h1>";
        mail.IsBodyHtml = true;
        //mail.Attachments.Add(new Attachment("C:\\file.zip"));

        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
        {
            smtp.Credentials = new NetworkCredential(recipient, "ydrbzpzvzewgrxxe");
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
    }
}
public enum Currencies
{
    Bitcoin = 1,
    Ethereum = 1027,
    AeroDrome = 29270,
}