using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Managers
{
    public class WordManager : MonoBehaviour
    {
        public List<string> WordList { get; private set; }
        
        private const int CharLimit = 5;

        public char correctPosition = 'X';
        public char inWord = 'O';
        public char notInWord = '_';

        #region Unity Events
        private void Awake()
        {
            WordList = ReturnWordList("WordList", CharLimit);
        }
        
        #endregion

        #region Custom Methods
        private static List<string> ReturnWordList(string fileName, int characterLimit)
        {
            return Resources.Load<TextAsset>(fileName).text
                .Split("\n")
                .Where(word => word.Length <= characterLimit)
                .ToList();
        }
        
        public string GetRandomWord()
        {
            return WordList[Random.Range(0, WordList.Count)];
        }

        private bool IsRealWord(string guess, List<string> list)
        {
            return list.Contains(guess);
        }

        public string CheckGuess(string userGuess, string wordToGuess)
        {
            var guessMarks = new Dictionary<char, bool>();

            // no duplicate keys in dict
            foreach (char letter in userGuess.Where(letter => !guessMarks.ContainsKey(letter))) 
            {
                guessMarks.Add(letter, false);
            }
            // so, if its marked i.e true then we append '_' instead of 'O'
            void MarkGuess(char key) => guessMarks[key] = true;
            
            var sb = new StringBuilder();
            
            for (int i = 0; i < wordToGuess.Length; i++)
            {
                if (wordToGuess[i].Equals(userGuess[i]))
                {
                    sb.Append(correctPosition);
                    MarkGuess(userGuess[i]);
                }
                
                else if (wordToGuess.Contains(userGuess[i]))
                {
                    switch (guessMarks[userGuess[i]])
                    {
                        default: 
                            sb.Append(inWord);
                            MarkGuess(userGuess[i]); 
                            break;
                        case true: 
                            sb.Append(notInWord); 
                            break;
                    }
                }
                
                else
                {
                    sb.Append(notInWord);
                }
                
            }
            
            return sb.ToString();
        }
        
        #endregion
        

  
        
    }
}
