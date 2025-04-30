using UnityEngine;

    public class MoneLibrary : MonoBehaviour
    {
        private static AndroidJavaClass unityClass; // to hold unity player class
        private static AndroidJavaObject unityActivity; // to hold current unity activity from the unity player
        private static AndroidJavaObject _pluginInstance; // android plugin created

        public static void InitializePlugin(string pluginName)
        {
            // Retrieve the UnityPlayer class.
            unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            // Retrieve the UnityPlayerActivity object 
            unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            // Retrieve the "Bridge" from our native plugin.
            _pluginInstance = new AndroidJavaObject(pluginName);

            if (_pluginInstance == null)
            {
                Debug.Log($"Plugin instance Error");
            }
            else
            {
                _pluginInstance.Call("receiveUnityActivity", unityActivity);
            }
        }

        public static void SendUsbData(sbyte a)
        {
            if (_pluginInstance != null)
            {
                _pluginInstance.Call("sendUsbData", a);
                Debug.Log($"Data send to USB: " + a);
            }
        }
    }
