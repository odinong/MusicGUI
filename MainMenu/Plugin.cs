using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BepInEx;
using System.Reflection;

namespace MusicPlayer
{
    [BepInPlugin("com.xenaz.MusicPlayer", "MusicPlayer", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private List<string> audioFiles = new List<string>();
        private Dictionary<string, List<string>> playlists = new Dictionary<string, List<string>>();
        private AudioSource audioSource;
        private AudioReverbFilter reverbFilter;
        static string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string folderPath = Path.Combine(assemblyDirectory, "Music GUI");
        public string playlistsPath = Path.Combine(folderPath, "playlists.txt");


        private float volume = 1.0f;
        private float speed = 1.0f;
        private float pitch = 1.0f;
        private float reverbLevel = 0.0f;
        private bool showGUI = true;
        private bool loopAudio = false;
        private bool shuffle = false;

        private int filesPerPage = 6;
        private int currentPage = 0;
        private int playlistPage = 0;


        private bool darkMode = false;
        private Color col;
        private Color col2;
        private Color col3;
        private Color coll4;
        private Color coll5;
        private Color col6;

        private string currentTab = "Music";
        private string newPlaylistName = "";
        private string selectedPlaylist = "";
        private List<string> currentPlaylist = new List<string>();
        private int currentPlaylistIndex = 0;

        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
            ScanFolder();
            LoadPlaylists();
            reverbLevel = -10000f;

            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.clip = null;
        }

        void ScanFolder()
        {
            if (Directory.Exists(folderPath))
            {
            string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    if (file.EndsWith(".wav") || file.EndsWith(".mp3") || file.EndsWith(".ogg"))
                    {
                        audioFiles.Add(file);
                    }
                }
            }
        }

        void LoadPlaylists()
        {
            if (File.Exists(playlistsPath))
            {
                string[] lines = File.ReadAllLines(playlistsPath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        string playlistName = parts[0];
                        List<string> playlistFiles = parts[1].Split(';').ToList();
                        playlists[playlistName] = playlistFiles;
                    }
                }
            }
        }

        void SavePlaylists()
        {
            List<string> lines = new List<string>();
            foreach (var playlist in playlists)
            {
                string line = playlist.Key + "|" + string.Join(";", playlist.Value);
                lines.Add(line);
            }
            File.WriteAllLines(playlistsPath, lines);
        }

        void OnGUI()
        {
            GUILayout.Label("Audio Player By Odin", GUILayout.Height(30));
            if (GUILayout.Button(showGUI ? "Hide GUI" : "Show GUI", GUILayout.Height(30)))
            {
                showGUI = !showGUI;
            }
            if (GUILayout.Button("Rescan Folder"))
            {
                audioFiles.Clear();
                ScanFolder();
            }
            if (GUILayout.Button("Dark Mode: " + YesNo(darkMode)))
            {
                darkMode = !darkMode;
            }

            if (!showGUI)
            {
                return;
            }
            GUILayout.Space(10);

            // Tab buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Music"))
            {
                currentTab = "Music";
            }
            if (GUILayout.Button("Playlists"))
            {
                currentTab = "Playlists";
            }
            GUILayout.EndHorizontal();

            // Current tab content
            if (currentTab == "Music")
            {
                DisplayMusicTab();
            }
            else if (currentTab == "Playlists")
            {
                DisplayPlaylistsTab();
            }

            GUILayout.Label("Volume");
            volume = GUILayout.HorizontalSlider(volume, 0.0f, 1.0f);
            audioSource.volume = volume;

            GUILayout.Label("Speed + Pitch");
            pitch = GUILayout.HorizontalSlider(pitch, 0.5f, 2.0f);
            audioSource.pitch = pitch;

            GUILayout.Space(10);

            GUILayout.Label("Reverb Level");
            reverbLevel = GUILayout.HorizontalSlider(reverbLevel, -10000.0f, 1000.0f);
            reverbFilter.reverbLevel = reverbLevel;

            GUILayout.Space(10);

            loopAudio = GUILayout.Toggle(loopAudio, "Loop Audio");

            GUILayout.Space(10);

            if (audioSource.isPlaying)
            {
                if (GUILayout.Button("Pause", GUILayout.Height(30)))
                {
                    audioSource.Pause();
                }
                if (GUILayout.Button("Stop", GUILayout.Height(30)))
                {
                    audioSource.Stop();
                }
                if (GUILayout.Button("Slowed + Reverb Audio", GUILayout.Height(30)))
                {
                    pitch = 0.8902f;
                    reverbLevel = 0f;
                }
                if (GUILayout.Button("Sped Up", GUILayout.Height(30)))
                {
                    pitch = 1.20f;
                    reverbLevel = -10000f;
                }
                if (GUILayout.Button("Normal", GUILayout.Height(30)))
                {
                    pitch = 1f;
                    reverbLevel = -10000f;
                }
            }
            else if (audioSource.time > 0)
            {
                if (GUILayout.Button("Resume", GUILayout.Height(30)))
                {
                    audioSource.Play();
                }
            }
        }

        void DisplayMusicTab()
        {
            int startIndex = currentPage * filesPerPage;
            int endIndex = Mathf.Min(startIndex + filesPerPage, audioFiles.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Path.GetFileName(audioFiles[i])))
                {
                    PlayAudio(audioFiles[i]);
                }

                if (!string.IsNullOrEmpty(selectedPlaylist))
                {
                    if (GUILayout.Button("Add to Playlist"))
                    {
                        AddToPlaylist(selectedPlaylist, audioFiles[i]);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (currentPage > 0 && GUILayout.Button("Previous Page"))
            {
                currentPage--;
            }
            if (endIndex < audioFiles.Count && GUILayout.Button("Next Page"))
            {
                currentPage++;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DisplayPlaylistsTab()
        {
            GUILayout.Label("Playlists");

            foreach (var playlist in playlists)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(playlist.Key))
                {
                    selectedPlaylist = playlist.Key;
                    playlistPage = 0;
                }
                if (GUILayout.Button("Delete"))
                {
                    playlists.Remove(playlist.Key);
                    SavePlaylists();
                    selectedPlaylist = "";
                }
                if (GUILayout.Button("Play"))
                {
                    PlayPlaylist(playlist.Key);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            GUILayout.Label("Create New Playlist");
            newPlaylistName = GUILayout.TextField(newPlaylistName, 25);
            if (GUILayout.Button("Create Playlist"))
            {
                if (!string.IsNullOrEmpty(newPlaylistName) && !playlists.ContainsKey(newPlaylistName))
                {
                    playlists[newPlaylistName] = new List<string>();
                    SavePlaylists();
                    newPlaylistName = "";
                }
            }

            GUILayout.Space(10);

            if (!string.IsNullOrEmpty(selectedPlaylist))
            {
                GUILayout.Label("Playlist: " + selectedPlaylist);

                var playlistFiles = playlists[selectedPlaylist];
                int startIndex = playlistPage * filesPerPage;
                int endIndex = Mathf.Min(startIndex + filesPerPage, playlistFiles.Count);

                for (int i = startIndex; i < endIndex; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Path.GetFileName(playlistFiles[i]));
                    if (GUILayout.Button("Remove"))
                    {
                        playlistFiles.RemoveAt(i);
                        SavePlaylists();
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (playlistPage > 0 && GUILayout.Button("Previous Page"))
                {
                    playlistPage--;
                }
                if (endIndex < playlistFiles.Count && GUILayout.Button("Next Page"))
                {
                    playlistPage++;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                if (GUILayout.Button("Shuffle"))
                {
                    shuffle = !shuffle;
                }
                GUILayout.Label("Shuffle: " + YesNo(shuffle));
            }
        }

        void AddToPlaylist(string playlistName, string filePath)
        {
            if (playlists.ContainsKey(playlistName))
            {
                playlists[playlistName].Add(filePath);
                SavePlaylists();
            }
        }

        void PlayAudio(string path)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            AudioClip clip = LoadAudioClip(path);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.loop = loopAudio;
                audioSource.Play();
            }
        }

        void PlayPlaylist(string playlistName)
        {
            if (playlists.ContainsKey(playlistName))
            {
                currentPlaylist = playlists[playlistName];
                currentPlaylistIndex = 0;

                if (shuffle)
                {
                    currentPlaylist = currentPlaylist.OrderBy(x => Random.value).ToList();
                }

                PlayNextInPlaylist();
            }
        }

        void PlayNextInPlaylist()
        {
            if (currentPlaylistIndex < currentPlaylist.Count)
            {
                string nextSong = currentPlaylist[currentPlaylistIndex];
                currentPlaylistIndex++;
                PlayAudio(nextSong);
                Invoke("PlayNextInPlaylist", audioSource.clip.length + 1);
            }
        }

        AudioClip LoadAudioClip(string path)
        {
            AudioType audioType = AudioType.UNKNOWN;
            if (path.EndsWith(".wav"))
            {
                audioType = AudioType.WAV;
            }
            else if (path.EndsWith(".mp3"))
            {
                audioType = AudioType.MPEG;
            }
            else if (path.EndsWith(".ogg"))
            {
                audioType = AudioType.OGGVORBIS;
            }

            using (WWW www = new WWW("file://" + path))
            {
                while (!www.isDone) { }

                if (www.error == null)
                {
                    return www.GetAudioClip(false, true, audioType);
                }
                else
                {
                    Debug.LogError("Failed to load audio file: " + www.error);
                    return null;
                }
            }
        }


        private static string YesNo(bool input)
        {
            return input ? "On" : "Off";
        }
    }
}
