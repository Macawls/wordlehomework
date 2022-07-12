using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.Managers
{
    [DefaultExecutionOrder(-100)]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClip> fxClips;
        [SerializeField] private List<AudioClip> musicClips;
        
        private AudioSource _fxSource;
        private AudioSource _musicSource;
        
        private Dictionary<string, AudioClip> _fxVault;
        private Dictionary<string, AudioClip> _musicVault;


        private event Action<string> OnPlayFX;
        
        public void PlayFX(string clipName) => OnPlayFX?.Invoke(clipName);

        public void PlayMusic()
        {
            _musicSource.Play();
        }

        // I prefer to use 0 and 1 rather than -1 and 1
        public void AdjustPan(float panVal, bool isFX)
        {
            Mathf.Clamp(panVal, 0, 1);
            // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
            static float Remap(float val, float in1, float in2, float out1, float out2)
            {
                return out1 + (val - in1) * (out2 - out1) / (in2 - in1);
            }

            switch (isFX)
            {
                case true:
                    _fxSource.panStereo = Remap(panVal, 0, 1, -1, 1);
                    break;
                default:
                    _musicSource.panStereo = Remap(panVal, 0, 1, -1, 1);
                    break;
            }
        }

        private void OnEnable()
        {
            OnPlayFX += PlayFxSound;
        }

        private void OnDisable()
        {
            OnPlayFX -= PlayFxSound;
        }


        private void Awake()
        {
            _fxSource = gameObject.transform.GetChild(0).GetComponent<AudioSource>();
            _musicSource = gameObject.transform.GetChild(1).GetComponent<AudioSource>();
        }

        private void Start()
        {
            _fxVault = InitializeDictionary(fxClips);
            _musicVault = InitializeDictionary(musicClips);
        }

        private void StopTrack(bool b) => _musicSource.Stop();

        private void ResumeTrack() => _musicSource.Play();

        private static Dictionary<string, AudioClip> InitializeDictionary(IEnumerable<AudioClip> audioClips)
        {
            return audioClips.ToDictionary(audioClip => audioClip.name);
        }
        
        private void PlayFxSound(string clipName)
        {
            if (!ClipExists(_fxVault, clipName)) return;
            
            _fxSource.PlayOneShot(_fxVault[clipName]);
        }

        private void PlayMusic(string clipName, bool shouldLoop)
        {
            if (!ClipExists(_musicVault, clipName)) return;
            
            _musicSource.Stop();
            _musicSource.clip = _musicVault[clipName];
            _musicSource.loop = shouldLoop;
            _musicSource.Play();
        }

        private bool ClipExists(Dictionary<string, AudioClip> dictionary, string clipName)
        {
            if (dictionary.ContainsKey(clipName)) return true;
            
            print($"{clipName} does not exist in {nameof(dictionary)}");
            return false;
        }
        
    }
}
