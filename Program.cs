using System;
using DSharpPlus;
using Newtonsoft.Json;

namespace SynoDiscordBot
{
    class Program
    {
        public class WordData
        {
            public string word { get; set; }
            public string key { get; set; }
            public string pos { get; set; }
            public List<string> synonyms { get; set; }
        }

        public class Synonym
        {
            public string word { get; set; }
            public List<string> synonyms { get; set; }
        }

        private static Dictionary<string, Synonym> synonymDict;
        static async Task Main(string[] args)
        {
            string jsonFilePath = "./synonyms.json";

            // Read the json file
            string jsonData = File.ReadAllText(jsonFilePath);

            // Deserialize the json into a list of objects
            List<WordData> wordDataList = JsonConvert.DeserializeObject<List<WordData>>(jsonData);

            // Create a dictionary of synonyms
            synonymDict = new Dictionary<string, Synonym>();

            if (wordDataList == null)
            {
                Console.WriteLine("wordDataList is null");
                return;
            }

            foreach (WordData wordData in wordDataList)
            {
                // Check if the word is already in the synonym dictionary
                if (!synonymDict.ContainsKey(wordData.word))
                {
                    Synonym synonym = new Synonym();
                    synonym.word = wordData.word;
                    synonym.synonyms = new List<string>();
                    synonymDict[wordData.word] = synonym;
                }

                // Add synonyms
                synonymDict[wordData.word].synonyms.AddRange(wordData.synonyms);
            }

            string tokenFromFile = File.ReadAllText("./token.txt");

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = tokenFromFile,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("!s"))
                {
                    await e.Message.RespondAsync(ConvertToSynonyms(e.Message.Content.Substring(2)));
                }
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);


        }

        static string GetSynonym(string word, Random random)
        {
            // Check if the word is in the dictionary
            if (synonymDict.ContainsKey(word))
            {
                try
                {
                    return synonymDict[word].synonyms[random.Next(synonymDict[word].synonyms.Count)];
                }
                catch (System.Exception)
                {

                    return word;
                }


            }
            else
            {
                // Return an error message
                return word;
            }
        }

        static string ConvertToSynonyms(string input)
        {
            // Split the input into words by space or end of line
            string[] words = input.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            Random random = new Random();

            // Convert each word to a synonym
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = GetSynonym(words[i].ToLower(), random);
            }

            // Join the words back together
            return string.Join(" ", words);
        }
    }
}