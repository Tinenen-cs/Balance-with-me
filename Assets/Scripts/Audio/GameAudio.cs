using System;
using UnityEngine;

namespace BalanceWithMe
{
    public class GameAudio : MonoBehaviour
    {
        [Serializable] public class Cue { public string name; public AudioClip clip; public float volume = 1; }
        public AudioSource music, effects;
        public AudioClip[] tracks;
        public Cue[] cues;
        public string LastCue { get; private set; }
        public bool Muted { get; private set; }
        public int TrackIndex { get; private set; }
        void Start()
        {
            SetMuted(PlayerPrefs.GetInt("BalanceWithMe.Muted",0) == 1, false);
            SelectTrack(0);
        }
        public void Play(string name)
        {
            var cue = Array.Find(cues, item => item.name == name);
            if (cue == null || cue.clip == null) return;
            LastCue = name;
            effects.PlayOneShot(cue.clip, cue.volume);
        }
        public void SelectTrack(int index)
        {
            if (tracks == null || tracks.Length == 0) return;
            TrackIndex = (index % tracks.Length + tracks.Length) % tracks.Length;
            music.clip = tracks[TrackIndex]; music.loop = true; music.Play();
        }
        public void NextTrack() => SelectTrack(TrackIndex + 1);
        public void ToggleMute() => SetMuted(!Muted);
        public void SetMuted(bool muted, bool persist = true)
        {
            Muted = muted; music.mute = effects.mute = muted;
            if (persist) { PlayerPrefs.SetInt("BalanceWithMe.Muted", muted ? 1 : 0); PlayerPrefs.Save(); }
        }
    }
}
