// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.13

// handling build variants - one of these must be defined
//#define STEAMBUILD
#define INDIEPASSBUILD // for IndiePass store, no Steam
//#define GOOGLEPLAY
//#define DRMFREE // DRM-free, no Steam or IndiePass
// default is Steam build

// set up platform checks and includes
#if DRMFREE
#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
this will intentionally cause an error - DRMFREE set but not on acceptable build mode
#endif

#elif INDIEPASSBUILD
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
this will intentionally cause an error - INDIEPASSBUILD set but not on acceptable build mode
#endif

#elif GOOGLEPLAY
#if !UNITY_ANDROID
this will intentionally cause an error - GOOGLEPLAY set but not on Android build mode
#endif

#elif STEAMBUILD
#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
this will intentionally cause an error - STEAMBUILD set but not on acceptable build mode
#endif
using System.Collections;
using Steamworks;

#else
this will intentionally cause an error - no build mode set

#endif

// no more #define after here
using UnityEngine;

//
// The SteamManager provides a base implementation of Steamworks.NET on which you can build upon.
// It handles the basics of starting up and shutting down the SteamAPI for use.
//
[DisallowMultipleComponent]
public class CSSteamManager : MonoBehaviour {
#if DRMFREE
    public const string BUILDMODE = "DRM-free";
#elif INDIEPASSBUILD
    public const string BUILDMODE = "Indie Pass";
    private static readonly HttpClient client = new HttpClient();
#elif GOOGLEPLAY
    public const string BUILDMODE = "Google Play";
#elif STEAMBUILD
    public const string BUILDMODE = "Steam";
#else
    public const string BUILDMODE = "ERROR NOT SET";
#endif
    // CSSteamWorks
    public static string userName { get; private set; } = "";
    public static string userID { get; private set; } = "";
    public static bool userStatsReceived { get; private set; } = false;
    protected bool m_bInitialized = false;
    public static bool Initialized
    {
        get
        {
#if STEAMBUILD
            return Instance.m_bInitialized;
#else
            return false;
#endif
        }
    }
    protected static CSSteamManager s_instance;
    protected static CSSteamManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                return new GameObject("CSSteamManager").AddComponent<CSSteamManager>();
            }
            else
            {
                return s_instance;
            }
        }
    }

    protected virtual void Awake()
    {
        // Only one instance of SteamManager at a time!
        if (s_instance != null)
        {
            if (s_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        s_instance = this;

        // We want our SteamManager Instance to persist across scenes.
        DontDestroyOnLoad(gameObject);

#if STEAMBUILD
        SteamAwake();
#elif INDIEPASSBUILD
        ValidateToken();
#endif
    }

#if INDIEPASSBUILD
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            //[DllImport("drm_dll_kek")]
            [DllImport("drm_dll")]
            private static extern bool validate_token();
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            [DllImport("libdrm_dll.dylib")]
            private static extern bool validate_token();
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            [DllImport("libdrm_dll.so")]
            private static extern bool validate_token();
#endif

    public static bool ValidateToken()
    {
#if INDIEPASSBUILD
        string validationEndpointUrl = "https://api.indiepass.gg/api/auth/stripe/subscription";
        string validationSuccessStatus = "success";

        try
        {
            // NEW CODE FOR IndiePass (12/25)

            // Parse --token from command line
            string[] args = System.Environment.GetCommandLineArgs();
            string token = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--token="))
                {
                    token = args[i].Substring(8);
                    break;
                }
                if (args[i] == "--token" && i + 1 < args.Length)
                {
                    token = args[i + 1];
                    break;
                }
            }
            if (string.IsNullOrEmpty(token)) throw new Exception();

            // Validate subscription with IndiePass API
            var request = new HttpRequestMessage(HttpMethod.Get, validationEndpointUrl);
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = client.SendAsync(request).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode) throw new Exception();

            string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!content.Contains($"\"status\":\"{validationSuccessStatus}\"")) throw new Exception();

            /* IndiePass old version (pre-12/25)

            if (!validate_token())
            {
                //#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#if UNITY_EDITOR
                // editor QOL
                Debug.LogWarning("Validation failed in editor");
                //UnityEditor.EditorApplication.isPlaying = false;
#else
            // for game build
            Debug.LogError("IndiePass: Validation failed");
            Application.Quit();
#endif
                return false;
            }
            */
        }
        catch //(DllNotFoundException e)
        {
#if UNITY_EDITOR
            // editor QOL
            Debug.LogWarning("Indiepass (editor): DLL not found or validation failed");
            //UnityEditor.EditorApplication.isPlaying = false;
#else
            // for game build
            Debug.LogError("IndiePass: Validation failed");
            Application.Quit();
#endif
        }
#else
        Debug.LogError("IndiePass: Not Indiepass build, you shouldn't be calling ValidateToken()");
#endif
        return true;
    }
#elif STEAMBUILD
    protected static bool s_EverInitialized = false;
    private CGameID m_GameID;
	private AppId_t m_AppID;
    // Void Scout settings
    public const int STEAMAPPID = 3226540;



	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    //protected Callback<UserStatsStored_t> m_UserStatsStored;
    //protected Callback<UserAchievementStored_t> m_UserAchievementStored;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
		Debug.LogWarning(pchDebugText);
	}

#if UNITY_2019_3_OR_NEWER
	// In case of disabled Domain Reload, reset static members before entering Play Mode.
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitOnPlayMode()
	{
		s_EverInitialized = false;
		s_instance = null;
	}
#endif

    private void SteamAwake()
    {
        if (s_EverInitialized)
        {
            // This is almost always an error.
            // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
            // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
            // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        m_AppID = new AppId_t(STEAMAPPID);

        try
        {
            // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
            // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.
            // Note that this will run which ever version you have installed in steam. Which may not be the precise executable
            // we were currently running.

            // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
            // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
            // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            if (SteamAPI.RestartAppIfNecessary(m_AppID))
            {
                Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");

                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        { // We catch this exception here, as it will be the first occurrence of it.
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

            Application.Quit();
            return;
        }

        // Initializes the Steamworks API.
        // If this returns false then this indicates one of the following conditions:
        // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
        // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
        // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
        // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
        // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
        // Valve's documentation for this is located here:
        // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
        m_bInitialized = SteamAPI.Init();
        if (!m_bInitialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

            return;
        }

        s_EverInitialized = true;
        m_GameID = new CGameID(m_AppID);

        // CSSteamWorks
        userName = SteamFriends.GetPersonaName();
        userID = SteamUser.GetSteamID().ToString();
        Debug.Log(userName + " - " + userID);

        if (SteamUserStats.RequestCurrentStats())
        {
            Debug.Log("RequestCurrentStats returned true");
        }
        else
        {
            Debug.Log("RequestCurrentStats returned false");

        }
    }

    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
		Debug.Log("<color=yellow>callback received</color> " + pCallback);

        if (!m_bInitialized)
            return;

		// we may get callbacks for other games' stats arriving, ignore them
		if (pCallback.m_nGameID == (ulong)m_GameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("<color=yellow>OnUserStatsReceived</color>Received stats and achievements from Steam\n");

				userStatsReceived = true;
			}
			else
			{
				Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
			}
		}
		else
			Debug.Log("Stats for wrong game received " + pCallback.m_nGameID.ToString());
    }

    // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
    protected virtual void OnEnable() {
		if (s_instance == null) {
			s_instance = this;
		}

		if (!m_bInitialized) {
			return;
		}

		if (m_SteamAPIWarningMessageHook == null) {
			// Set up our callback to receive warning messages from Steam.
			// You must launch with "-debug_steamapi" in the launch args to receive warnings.
			m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}

		if (m_UserStatsReceived == null)
			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        //m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        //m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
    }

    // OnApplicationQuit gets called too early to shutdown the SteamAPI.
    // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
    // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
    protected virtual void OnDestroy() {
		if (s_instance != this) {
			return;
		}

		s_instance = null;

		if (!m_bInitialized) {
			return;
		}

		SteamAPI.Shutdown();
	}

    protected virtual void Update() {
		if (!m_bInitialized) {
			//Debug.Log("Can't run callbacks!");
			return;
		}

		// Run Steam client callbacks
		SteamAPI.RunCallbacks();
	}

    public static Texture2D GetAvatar(CSteamID id)
    {
        // See docs for when this is valid. Also for other sizes.
        int avatar_id = SteamFriends.GetMediumFriendAvatar(id);
        Texture2D avatar = null;
        if (!SteamUtils.GetImageSize(avatar_id, out uint width, out uint height)
                && width > 0
                && height > 0)
        {
            var image = new byte[width * height * 4];
            SteamUtils.GetImageRGBA(avatar_id, image, (int)(width * height * 4));
            avatar = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
            avatar.LoadRawTextureData(image);
            // Unity expects texture data to start from "bottom", so avatar
            // will be upside down. You could change the pixels here to fix it.

            avatar.Apply();
        }
        return avatar;
    }

    public static Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D ret = null;
        uint ImageWidth;
        uint ImageHeight;
        bool bIsValid = SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight);

        if (bIsValid)
        {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];

            bIsValid = SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4));
            if (bIsValid)
            {
                ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                ret.LoadRawTextureData(Image);
                ret.Apply();
            }
        }

        return ret;
    }
#endif // STEAMBUILD

    public static bool IsLoggedIn()
    {
#if STEAMBUILD
        return (Initialized && userName.Length > 0 && userID.Length > 0);
#else
        return false;
#endif
    }

    public static bool SteamEnabled()
	{
#if STEAMBUILD
        return true;
#else
        return false;
#endif
	}

    public static bool IsOnSteamDeck()
    {
#if STEAMBUILD
        if (Initialized)
            return SteamUtils.IsSteamRunningOnSteamDeck();
#endif
        return false;
    }

    public static Texture2D GetUserIcon()
	{
#if STEAMBUILD
        return GetSteamImageAsTexture2D(SteamFriends.GetSmallFriendAvatar(SteamUser.GetSteamID()));
#else
        Debug.LogError("GetUserIcon called in non-Steam build");
        return null;
#endif
	}
}
