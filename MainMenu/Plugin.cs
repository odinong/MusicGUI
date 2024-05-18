using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BepInEx;

namespace MusicPlayer
{
    [BepInPlugin("com.xenaz.MusicPlayer", "MusicPlayer", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private List<string> audioFiles = new List<string>();
        private Dictionary<string, List<string>> playlists = new Dictionary<string, List<string>>();
        private AudioSource audioSource;
        private AudioReverbFilter reverbFilter;
        private string folderPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\BepInEx\\plugins\\Music GUI";
        private string playlistsPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\BepInEx\\plugins\\Music GUI\\playlists.txt";

        private float volume = 1.0f;
        private float speed = 1.0f;
        private float pitch = 1.0f;
        private float reverbLevel = 0.0f;
        private float bassLevel = 1.0f;
        private bool showGUI = true;
        private bool loopAudio = false;
        private bool shuffle = false;

        private int filesPerPage = 6;
        private int currentPage = 0;

        private GUILayoutOption[] buttonLayoutOptions = { GUILayout.Height(30) };
        private GUILayoutOption[] sliderLayoutOptions = { GUILayout.Width(300) };
        private GUILayoutOption[] paginationLayoutOptions = { GUILayout.ExpandWidth(true) };
        private GUIStyle toggleStyle;
        private GUIStyle labelStyle;
        private GUIStyle boxStyle;
        private GUIStyle buttonStyle;
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

        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
            ScanFolder();
            LoadPlaylists();
            reverbLevel = -10000f;
        }

        void ScanFolder()
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                if (file.EndsWith(".wav") || file.EndsWith(".mp3") || file.EndsWith(".ogg"))
                {
                    audioFiles.Add(file);
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
            // GUI Style Setup
            if (darkMode == false)
            {
                col = Color.white;
                col2 = Color.white;
                col3 = Color.white;
            }
            if (darkMode)
            {
                coll4 = Color.black;
                coll5 = Color.black;
                col6 = Color.black;
            }
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.white;
            boxStyle.normal.background = MakeTex(2, 2, col);
            boxStyle.fontSize = 12;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
            if (darkMode == false)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.normal.textColor = Color.black;
                buttonStyle.normal.background = MakeTex(2, 2, col2);
                buttonStyle.hover.background = MakeTex(2, 2, col3);
                buttonStyle.fontSize = 12;
                buttonStyle.padding = new RectOffset(10, 10, 5, 5);
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = Color.black;
                labelStyle.fontSize = 12;
            }
            if (darkMode == true)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.normal.textColor = Color.white;
                buttonStyle.normal.background = MakeTex(2, 2, coll4);
                buttonStyle.hover.background = MakeTex(2, 2, coll5);
                buttonStyle.fontSize = 12;
                buttonStyle.padding = new RectOffset(10, 10, 5, 5);
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = Color.white;
                labelStyle.fontSize = 12;
            }
            Color col4 = new Color(0.2f, 0.2f, 0.2f, 1f);
            Color col5 = new Color(0.2f, 0.2f, 0.6f, 1f);
            Color white2 = Color.white;
            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.normal.textColor = white2;
            toggleStyle.onNormal.textColor = white2;
            toggleStyle.hover.textColor = white2;
            toggleStyle.onHover.textColor = white2;
            toggleStyle.focused.textColor = white2;
            toggleStyle.onFocused.textColor = white2;
            toggleStyle.active.textColor = white2;
            toggleStyle.onActive.textColor = white2;
            toggleStyle.normal.background = MakeTex(2, 2, col4);
            toggleStyle.onNormal.background = MakeTex(2, 2, col5);
            toggleStyle.hover.background = MakeTex(2, 2, col4);
            toggleStyle.onHover.background = MakeTex(2, 2, col5);
            toggleStyle.focused.background = MakeTex(2, 2, col4);
            toggleStyle.onFocused.background = MakeTex(2, 2, col5);
            toggleStyle.active.background = MakeTex(2, 2, col4);
            toggleStyle.onActive.background = MakeTex(2, 2, col5);
            toggleStyle.border = new RectOffset(1, 1, 1, 1);
            toggleStyle.margin = new RectOffset(4, 4, 4, 4);
            toggleStyle.padding = new RectOffset(4, 4, 4, 4);
            toggleStyle.fontSize = 12;
            toggleStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("Audio Player By Odin", GUILayout.Height(30));
            if (GUILayout.Button(showGUI ? "Hide GUI" : "Show GUI", buttonStyle, GUILayout.Height(30)))
            {
                showGUI = !showGUI;
            }
            if (GUILayout.Button("Rescan Folder", buttonStyle))
            {
                audioFiles.Clear();
                ScanFolder();
            }
            if (GUILayout.Button("Dark Mode: " + YesNo(darkMode), buttonStyle))
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
            if (GUILayout.Button("Music", buttonStyle))
            {
                currentTab = "Music";
            }
            if (GUILayout.Button("Playlists", buttonStyle))
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
            volume = GUILayout.HorizontalSlider(volume, 0.0f, 1.0f, sliderLayoutOptions);
            audioSource.volume = volume;

            GUILayout.Label("Speed");
            speed = GUILayout.HorizontalSlider(speed, 0.5f, 2.0f, sliderLayoutOptions);
            audioSource.pitch = speed;

            GUILayout.Label("Pitch");
            pitch = GUILayout.HorizontalSlider(pitch, 0.5f, 2.0f, sliderLayoutOptions);
            audioSource.pitch = pitch;

            GUILayout.Space(10);

            GUILayout.Label("Reverb Level");
            reverbLevel = GUILayout.HorizontalSlider(reverbLevel, -10000f, 0f, sliderLayoutOptions);
            reverbFilter.reverbLevel = reverbLevel;

            GUILayout.Space(10);

            if (audioSource.isPlaying)
            {
                if (GUILayout.Button("Pause", buttonStyle, GUILayout.Height(30)))
                {
                    audioSource.Pause();
                }
                if (GUILayout.Button("Stop", buttonStyle, GUILayout.Height(30)))
                {
                    audioSource.Stop();
                }
                if (GUILayout.Button("Slowed + Reverb Audio", buttonStyle, GUILayout.Height(30)))
                {
                    pitch = 0.8902f;
                    reverbLevel = 0f;
                }
                if (GUILayout.Button("Sped Up", buttonStyle, GUILayout.Height(30)))
                {
                    pitch = 1.20f;
                    reverbLevel = -10000f;
                }
                if (GUILayout.Button("Normal", buttonStyle, GUILayout.Height(30)))
                {
                    pitch = 1f;
                    reverbLevel = -10000f;
                }
                if (GUILayout.Button("Sped Up Config", buttonStyle, GUILayout.Height(30)))
                {
                    pitch = 1.20f;
                    reverbLevel = -10000f;
                    volume = 0.809f;
                }
            }
            else if (audioSource.time > 0)
            {
                if (GUILayout.Button("Resume", buttonStyle, GUILayout.Height(30)))
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
                if (GUILayout.Button(Path.GetFileName(audioFiles[i]), buttonStyle, buttonLayoutOptions))
                {
                    PlayAudio(audioFiles[i]);
                }

                if (!string.IsNullOrEmpty(selectedPlaylist))
                {
                    if (GUILayout.Button("Add to Playlist", buttonStyle, buttonLayoutOptions))
                    {
                        AddToPlaylist(selectedPlaylist, audioFiles[i]);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal(paginationLayoutOptions);
            GUILayout.FlexibleSpace();
            if (currentPage > 0 && GUILayout.Button("Previous Page", buttonStyle, buttonLayoutOptions))
            {
                currentPage--;
            }
            if (endIndex < audioFiles.Count && GUILayout.Button("Next Page", buttonStyle, buttonLayoutOptions))
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
                if (GUILayout.Button(playlist.Key, buttonStyle, buttonLayoutOptions))
                {
                    selectedPlaylist = playlist.Key;
                }
                if (GUILayout.Button("Delete", buttonStyle, buttonLayoutOptions))
                {
                    playlists.Remove(playlist.Key);
                    SavePlaylists();
                    selectedPlaylist = "";
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            GUILayout.Label("Create New Playlist");
            newPlaylistName = GUILayout.TextField(newPlaylistName, 25);
            if (GUILayout.Button("Create Playlist", buttonStyle))
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
                for (int i = 0; i < playlistFiles.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Path.GetFileName(playlistFiles[i]), labelStyle, buttonLayoutOptions);
                    if (GUILayout.Button("Remove", buttonStyle, buttonLayoutOptions))
                    {
                        playlistFiles.RemoveAt(i);
                        SavePlaylists();
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Shuffle", buttonStyle))
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

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] array = new Color[width * height];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = col;
            }
            Texture2D texture2D = new Texture2D(width, height);
            texture2D.SetPixels(array);
            texture2D.Apply();
            return texture2D;
        }

        private static string YesNo(bool input)
        {
            return input ? "On" : "Off";
        }
    }
}
