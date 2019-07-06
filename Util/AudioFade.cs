using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine {

    public static class AudioSourceExtensions {

        public static void FadeIn(this AudioSource audio, float duration) {
            audio.GetComponent<MonoBehaviour>().StartCoroutine(FadeInCore(audio, duration));
        }

        private static IEnumerator FadeInCore(AudioSource audio, float duration) {
            float startVolume = audio.volume;
            audio.volume = 0;

            while (audio.volume < startVolume) {
                audio.volume += startVolume * Time.deltaTime / duration;
                yield return new WaitForEndOfFrame();
            }
        }

        public static void FadeOut(this AudioSource audio, float duration) {
            audio.GetComponent<MonoBehaviour>().StartCoroutine(FadeOutCore(audio, duration));
        }

        private static IEnumerator FadeOutCore(AudioSource audio, float duration) {
            float startVolume = audio.volume;

            while (audio.volume > 0) {
                audio.volume -= startVolume * Time.deltaTime / duration;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}