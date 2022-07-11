using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using _Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("UI Props")]
        [SerializeField] private HorizontalLayoutGroup letterGroup;
        [SerializeField] private TMP_InputField guessField;
        [SerializeField] private TextMeshProUGUI logContent;
        [SerializeField] private TextMeshProUGUI roundsContent;
        [SerializeField] private TextMeshProUGUI placeHolder;
        

        private List<TextMeshProUGUI> _letters;
        private List<Image> _letterBGs;
        private List<RectTransform> _letterBGTransforms;

        private Action<string, bool> _onGuess;
        private string _currentWordToGuess;
        private string _latestGuess;

        // create a color with hex value of 98c379
        [Header("Colors")]
        [SerializeField] private Color correctColor;
        [SerializeField] private Color wrongColor;
        [SerializeField] private Color inWordColor;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color defaultLogColor;
        [SerializeField] private Color guessColor;
        
        private static readonly int MFontColor = Shader.PropertyToID("m_fontColor");

        private Queue<string> _wordQueue;
        [SerializeField] private int gameRounds = 10;

        private Action<string> onCorrectGuess;
        private bool _animComplete;

        private void OnEnable()
        {
            onCorrectGuess += HandleCorrectGuess;
        }
        
        private void OnDisable()
        {
            onCorrectGuess += HandleCorrectGuess;
        }
        
        private void Awake()
        {
            InitialSetup();
        }

        private void InitialSetup()
        {
            _wordQueue = new Queue<string>();
            _letters = new List<TextMeshProUGUI>(letterGroup.GetComponentsInChildren<TextMeshProUGUI>());
            _letterBGs = new List<Image>(letterGroup.GetComponentsInChildren<Image>());
            
            _letterBGTransforms = new List<RectTransform>();
            for (int i = 0; i < letterGroup.transform.childCount; i++)
            {
                _letterBGTransforms.Add(letterGroup.transform.GetChild(i).GetComponent<RectTransform>());
            }
            
            guessField.onSubmit.AddListener(OnGuessSubmit);
        }
        
        private void Start()
        {
            FillWordQueue(gameRounds);
            SetNewWord(_wordQueue.Peek());
            roundsContent.text = _wordQueue.Count.ToString();
        }
        
        private void FillWordQueue(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                string newWord;
                do
                { 
                    newWord = GameManager.Instance.WordManager.GetRandomWord(); 
                } while (_wordQueue.Contains(newWord)); // no duplicate words to guess
                
                _wordQueue.Enqueue(newWord);
            }
        }

        private void OnGuessSubmit(string inputLine)
        {
            _latestGuess = inputLine;
            
            if (!inputLine.Length.Equals(_currentWordToGuess.Length))
            {
                const string tooLong = "your guess was too long!";
                const string tooShort = "your guess was too short!";
                switch (inputLine.Length < _currentWordToGuess.Length)
                {
                    case true:
                        UpdateLog(tooShort, wrongColor);
                        break;
                    default:
                        UpdateLog(tooLong, wrongColor);
                        break;
                }

                var rect = guessField.GetComponent<RectTransform>();
                var rectImage = guessField.GetComponent<Image>();
                var originalColor = rectImage.color;
                float fullDuration = 0.5f;
                
                rectImage.DOColor(wrongColor, fullDuration * 0.7f).OnComplete(() =>
                {
                    rectImage.DOColor(originalColor, fullDuration - fullDuration * 0.7f);
                });
                rect.transform.DOShakeScale(fullDuration, 3f);
                GameManager.Instance.SoundManager.PlayFX("negative");
                
                return;
            }
            
            string result = GameManager.Instance.WordManager.CheckGuess(inputLine, _currentWordToGuess);
            int correctGuesses = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == GameManager.Instance.WordManager.correctPosition)
                {
                    _letters[i].text = _currentWordToGuess[i].ToString();
                    _letterBGs[i].color = correctColor;
                    correctGuesses++;
                }
                else if (result[i] == GameManager.Instance.WordManager.inWord)
                {
                    _letters[i].text = string.Empty;
                    _letterBGs[i].color = inWordColor;
                }
                else
                {
                    _letters[i].text = string.Empty;
                    _letterBGs[i].color = wrongColor;
                }
            }

            if (correctGuesses.Equals(_currentWordToGuess.Length))
            {
                onCorrectGuess?.Invoke(inputLine);
            }
            else
            {
                var optionList = new List<string>
                {
                    "hmmmm...", 
                    "keep going!",
                    "getting closer!",
                    "keep trying!",
                    "you got this!",
                    "that's definitely a word right?",
                    "holup...",
                    "is that in the dictionary?",
                    "this is kinda like scrabble"
                };
                
                string newMsg = optionList[Random.Range(0, optionList.Count)];
                while (newMsg == logContent.text)
                {
                    newMsg = optionList[Random.Range(0, optionList.Count)];
                }
                UpdateLog(newMsg, guessColor);
            }
        }
        
        private void HandleCorrectGuess(string guess)
        {
            if (_wordQueue.Peek().Equals(guess))
            {
                RemoveLastWord();
            }

            string correct = $"Congrats! the word was {guess.ToUpper()}";
            UpdateLog(correct, correctColor);

            StartCoroutine(WaitToPLayAgain(2f));
        }

        private IEnumerator WaitToPLayAgain(float duration)
        {
            guessField.text = string.Empty;
            yield return new WaitForSeconds(duration);
            SetNewWord(_wordQueue.Peek());
        }
        
        private void SetNewWord(string newWord)
        {
            EmptyLetters();
            ResetTileColours();
            StartCoroutine(LetterAnimation(5, 0.3f, _letters, _letterBGTransforms));
            StartCoroutine(WaitForAnim(newWord));
        }

        private IEnumerator WaitForAnim(string word)
        {
            roundsContent.text = _wordQueue.Count.ToString();
            yield return new WaitUntil(() => _animComplete);
            _currentWordToGuess = word;
            print($"New word is: {_currentWordToGuess}");
            print($"Remaining words: {_wordQueue.Count.ToString()}");
            _animComplete = false;
        }

        
        // Absolute monolith, at this point I don't even know
        private IEnumerator LetterAnimation(float duration, float period, List<TextMeshProUGUI> letterList, List<RectTransform> bgList)
        {
            var rectImage = guessField.GetComponent<Image>();
            var orig = rectImage.color;
            guessField.enabled = false;
            
            rectImage.DOColor(defaultColor, 1f).SetEase(Ease.InBounce).OnComplete(() =>
            {
                placeHolder.text = string.Empty;
            });
            
            
            float time = 0;
            var waitForPeriod = new WaitForSeconds(period);
            var sb = new StringBuilder();

            var totalTimes = duration / period;
                
            var rot = new Vector3(0, 0, 15);
            var opRot = new Vector3(0, 0, -15);
            
            var sequence = DOTween.Sequence();
            
            foreach (var rect in _letterBGTransforms)
            {
                Tween rotate = rect.DORotate(rot, duration / totalTimes / 3);
                rotate.OnStepComplete(() => GameManager.Instance.SoundManager.PlayFX("shake"));
                Tween opRotate = rect.DORotate(opRot, duration / totalTimes / 3);
                sequence.Append(rotate).Append(opRotate);
            }
            
            sequence.OnKill(() => {
                foreach (var rect in _letterBGTransforms)
                {
                    rect.rotation = Quaternion.identity;
                }
                _animComplete = true;
            });

            sequence.SetLoops(-1, LoopType.Yoyo);
            
            string scrambling = $"scrambling letters";
            
            while (time < duration)
            {
                for (int index = 0; index < letterList.Count; index++)
                {
                    var letter = letterList[index];
                    char newLetter = (char)Random.Range('a', 'z');
                    letter.text = newLetter.ToString().ToUpper();
                }

                sb.Append(".");
                if (sb.ToString() == ".....") sb.Clear();

                UpdateLog(string.Concat(scrambling, sb.ToString()), inWordColor);
                time += period;
                yield return waitForPeriod;
            }
            // reset here
            
            sequence.Kill();
            UpdateLog("New word is ready!", correctColor);
            rectImage.DOColor(orig, 0.5f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                placeHolder.text = "ENTER GUESS";
                guessField.enabled = true;
                GameManager.Instance.SoundManager.PlayFX("positive");
            });
           
            EmptyLetters();
        }

        private void UpdateLog(string msg, Color newColor)
        {
            logContent.color = newColor;
            logContent.text = msg;
        }
        
        private void UpdateLog(string msg) => logContent.text = msg;

        private void ResetTileColours()
        {
            foreach (var image in _letterBGs)
            {
                image.color = defaultColor;
            }
        }

        private void RemoveLastWord()
        {
            _wordQueue.Dequeue();
        }

        private void RestartGame()
        {
            _wordQueue.Clear();
            FillWordQueue(gameRounds);
        }

        private void EmptyLetters()
        {
            foreach (var letter in _letters)
            {
                letter.text = "";
            }
        }
        
    }
    
    
}
