using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using TMPro;
using UnityEngine;

enum LauncherStatus
{
    ready,
    update,
    failed,
    downloadingGame,
    downloadingUpdate,
}

public class LauncherUpdateMenu : MonoBehaviour
{
    public GameObject statusButton;
    public TMP_Text versionText;

    public string rootPath;
    public string versionFile;
    public string launcherZip;
    public string launcherExe;
    public string updaterExe;

    private LauncherStatus _status;
    internal LauncherStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            switch (_status)
            {
                case LauncherStatus.ready:
                    statusButton.GetComponentInChildren<TMP_Text>().text = "Ready";
                    break;
                case LauncherStatus.update:
                    statusButton.GetComponentInChildren<TMP_Text>().text = "Update Now";
                    break;
                case LauncherStatus.failed:
                    statusButton.GetComponentInChildren<TMP_Text>().text = "Update failed";
                    break;
                case LauncherStatus.downloadingGame:
                    statusButton.GetComponentInChildren<TMP_Text>().text = "Downloading Game";
                    break;
                case LauncherStatus.downloadingUpdate:
                    statusButton.GetComponentInChildren<TMP_Text>().text = "Downloading Update";
                    break;
                default:
                    break;
            }
        }
    }

    private void Awake()
    {
        rootPath = Directory.GetCurrentDirectory();
        versionFile = Path.Combine(rootPath, "LauncherVersion.txt");
        launcherZip = Path.Combine(rootPath, "Launcher_Client.zip");
        launcherExe = Path.Combine(rootPath, "Launcher_Client.exe");
        updaterExe = Path.Combine(rootPath,"Updater", "LauncherUpdater.exe");
    }

    //Start is called before the first frame update
    void Start()
    {
        CheckForUpdates();
        InvokeRepeating("CheckForUpdates", 300, 300);
    }

    private async void CheckForUpdates()
    {
        statusButton.GetComponentInChildren<TMP_Text>().text = "Checking";

        if (File.Exists(versionFile))
        {
            Version localVersion = new Version(File.ReadAllText(versionFile));
            versionText.text = localVersion.ToString();

            try
            {
                Version onlineVersion = new Version();

                using (WebClient webClient = new WebClient())
                {
                    onlineVersion = new Version(await webClient.DownloadStringTaskAsync("https://www.dropbox.com/s/xl9b2ksvko54ru2/LauncherVersion.txt?dl=1"));
                }

                //WebClient webClient = new WebClient();
                //Version onlineVersion = new Version (webClient.DownloadString("https://www.dropbox.com/s/xl9b2ksvko54ru2/LauncherVersion.txt?dl=1"));

                // Version update found on start up
                if(onlineVersion.IsDifferentThan(localVersion) && ClientData.Singleton.IsConnected == false)
                {
                    Status = LauncherStatus.update;
                    MenuManager.Singleton.launcherUpdateMenu.SetActive(true);
                }
                // Version update found while connected
                else if (onlineVersion.IsDifferentThan(localVersion) && ClientData.Singleton.IsConnected == true)
                {
                    Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendDisconnectMe);
                    message.AddString("DisconnectMe");
                    NetworkManager.Singleton.Client.Send(message);

                    Status = LauncherStatus.update;
                    MenuManager.Singleton.launcherUpdateMenu.SetActive(true);
                }
                // We are up to date on start up
                else if (!onlineVersion.IsDifferentThan(localVersion) && ClientData.Singleton.IsConnected == false)
                {
                    Status = LauncherStatus.ready;
                    MenuManager.Singleton.loginMenu.SetActive(true);
                    MenuManager.Singleton.launcherUpdateMenu.SetActive(false);
                }
                // We are up to date while connected
                else if (!onlineVersion.IsDifferentThan(localVersion) && ClientData.Singleton.IsConnected == true)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MenuManager.Singleton.alertMenu.SetActive(true);
                MenuManager.Singleton.alertMenuText.text = $"Error checking for updates: {ex}";
            }
        }
        // Files were not found, assuming fresh install
        else
        {
            // Launch updater
            Status = LauncherStatus.update;
            MenuManager.Singleton.launcherUpdateMenu.SetActive(true);
        }
    }

    public void StatusButton()
    {
        if (Status == LauncherStatus.update)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(updaterExe);
            Process.Start(startInfo);
            Application.Quit();
        }
        else if (Status == LauncherStatus.failed)
        {
            CheckForUpdates();
        }
    }
    public void ExitButton()
    {
        Application.Quit();
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string _verson)
        {
            string[] _versionString = _verson.Split(".");
            if(_versionString.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }
            major = short.Parse(_versionString[0]);
            minor = short.Parse(_versionString[1]);
            subMinor = short.Parse(_versionString[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {
            if(major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                if(minor != _otherVersion.minor)
                {
                    return true;
                }
                else
                {
                    if(subMinor != _otherVersion.subMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
