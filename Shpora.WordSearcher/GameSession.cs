using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shpora.WordSearcher.Types;

namespace Shpora.WordSearcher
{
    public class GameSession
    {
        public string Server { get; private set; }
        public string Auth_key { get; private set; }
        public Dictionary<string, char> Alphabet { get; }
        public int TimeToFinish { get; private set; }

        public GameSession()
        {
            RegistrationBot();
            Alphabet = MakeDictionaryAlphabet();
        }

        public void RegistrationBot()
        {
            Console.Write("Shpora.WordSearcher.exe ");
            Server = Console.ReadLine();
            Auth_key = Console.ReadLine();
        }

        public string InitializingGamingSession()
        {
            var content = "";
            try
            {
                var resp = NetworkActions.POST(Server + "/task/game/start", "", Auth_key);
                var sr = new StreamReader(resp.GetResponseStream());
                content = sr.ReadToEnd();
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    TimeToFinish = int.Parse(resp.Headers.Get("Expires"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                RegistrationBot();
            }
            return content;
        }

        public void EndGameSession()
        {
            try
            {
                var resp = NetworkActions.POST(Server + "/task/game/finish", "", Auth_key);
                var sr = new StreamReader(resp.GetResponseStream());
                var content = sr.ReadToEnd();
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        public GameStatistics GetStats()
        {
            GameStatistics gameStat = null;
            try
            {
                var resp = NetworkActions.GET(Server + "/task/game/stats", Auth_key);
                var sr = new StreamReader(resp.GetResponseStream());
                var content = sr.ReadToEnd();

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var parsed = JsonConvert.DeserializeObject<Dictionary<string, int>>(content);
                    gameStat = new GameStatistics(parsed["points"], parsed["moves"], parsed["words"]);
                    Console.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            return gameStat;
        }

        public string Move(TypeMove typeMove)
        {
            var partToroid = "";
            try
            {
                var resp = NetworkActions.POST(Server + "/task/move/" + typeMove.ToString(), "", Auth_key);
                var sr = new StreamReader(resp.GetResponseStream());
                var content = sr.ReadToEnd();
                if (resp.StatusCode == HttpStatusCode.OK)
                {                   
                    partToroid = content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            return partToroid;
        }

        public void DeliveryFoundWords(List<string> foundWords)
        {
            var postData = MakemakeDataForPost(foundWords);
            try
            {
                var resp = NetworkActions.POST(Server + "/task/words/", postData, Auth_key);
                var sr = new StreamReader(resp.GetResponseStream());
                var content = sr.ReadToEnd();

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        private string MakemakeDataForPost(List<string> foundWords)
        {
            var postData = "[";
            if (foundWords.Count > 0)
                postData += "\"" + foundWords[0] + "\"";
            for (int i = 0; i < foundWords.Count - 1; i++)
            {
                    postData += ", \"" + foundWords[i + 1] + "\"";
            }
            return postData + "]";
        }

        private Dictionary<string, char> MakeDictionaryAlphabet()
        {
            var dict = new Dictionary<string, char>();
            var alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();
            var text = Properties.Resources.AlphabetTx.Split(alphabet)
                .Select(word => word.Replace("\r\n", " "))
                .Select(word => word.Remove(0, 1).Remove(word.Length - 2, 1))
                .ToArray();
            text[0] = text[0].Remove(0, 1);
            for(int i =0; i< alphabet.Length;i++)
            {
                dict.Add(text[i], alphabet[i]);
            }
            return dict;
        }

    }
    
}
