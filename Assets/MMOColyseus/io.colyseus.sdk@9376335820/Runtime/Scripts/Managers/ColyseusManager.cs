using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;
using UnityEngine.PlayerLoop;

// ReSharper disable InconsistentNaming

namespace Colyseus
{
    /// <summary>
    /// Base manager class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ColyseusManager<T> : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Colyseus settings object.
        /// </summary>
        [SerializeField]
        protected ColyseusSettings _colyseusSettings;
        [SerializeField]
        protected ColyseusSettings _colyseusTestSettings;

        private ColyseusRequest _requests;

        public bool isAppRelease = true;

        // Getters
        //==========================
        /// <summary>
        /// The singleton instance of the Colyseus Manager.
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Returns the Colyseus server address as defined
        /// in the <see cref="ColyseusSettings"/> object
        /// </summary>
        public string ColyseusServerAddress
        {
            get { return (isAppRelease)?_colyseusSettings.colyseusServerAddress:_colyseusTestSettings.colyseusServerAddress; }
        }



        /// <summary>
        /// Returns the Colyseus server port as defined
        /// in the <see cref="ColyseusSettings"/> object
        /// </summary>
        public string ColyseusServerPort
        {
            get { return (isAppRelease)?_colyseusSettings.colyseusServerPort:_colyseusTestSettings.colyseusServerPort; }
        }

        /// <summary>
        /// Returned if the desired protocol security as defined
        /// in the <see cref="ColyseusSettings"/> object
        /// </summary>
        public bool ColyseusUseSecure
        {
            get { return (isAppRelease)?_colyseusSettings.useSecureProtocol:_colyseusTestSettings.useSecureProtocol; }
        }
        //==========================

        /// <summary>
        /// The Client that is created when connecting to the Colyseus server.
        /// </summary>
        protected ColyseusClient client;

        protected ColyseusClient ChatLoungeclient;

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback when the manager object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            // Copy request headers
            List<ColyseusSettings.RequestHeader> requestHeaders;
            if(isAppRelease)
	            requestHeaders = new List<ColyseusSettings.RequestHeader>(_colyseusSettings.GetRequestHeaders());
            else
	            requestHeaders = new List<ColyseusSettings.RequestHeader>(_colyseusTestSettings.GetRequestHeaders());
            InitializeInstance();
        }

        /// <summary>
        /// Initializes the Colyseus manager singleton.
        /// </summary>
        private void InitializeInstance()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = GetComponent<T>();

            // Initialize the requests object with settings
            if(isAppRelease)
	            _requests = new ColyseusRequest(_colyseusSettings);
            else
	            _requests = new ColyseusRequest(_colyseusTestSettings);
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        protected virtual void Start()
        {
        }

        /// <summary>
        /// Frame-rate independent message for physics calculations.
        /// </summary>
        protected virtual void FixedUpdate()
        {
        }

        /// <summary>
        /// Override the current <see cref="ColyseusSettings"/>
        /// </summary>
        /// <param name="newSettings">The new settings to use</param>
        public virtual void OverrideSettings(ColyseusSettings newSettings)
        {
	        if (isAppRelease)
	        {
		        _colyseusSettings = newSettings;
		        _requests = new ColyseusRequest(_colyseusSettings);
	        }
	        else
	        {
		        _colyseusTestSettings = newSettings;
		        _requests = new ColyseusRequest(_colyseusTestSettings);
	        }
        }

        /// <summary>
        /// Get a copy of the manager's settings configuration
        /// </summary>
        /// <returns></returns>
        public virtual ColyseusSettings CloneSettings()
        {
	        if(isAppRelease)
	            return ColyseusSettings.Clone(_colyseusSettings);
	        else
		        return ColyseusSettings.Clone(_colyseusTestSettings);
        }

        /// <summary>
        /// Creates a new <see cref="ColyseusClient"/> object, with the given endpoint, and returns it
        /// </summary>
        /// <param name="endpoint">URL to the Colyseus server</param>
        /// <returns></returns>
        public ColyseusClient CreateClient(string endpoint)
        {
            client = new ColyseusClient(endpoint);
            return client;
        }

        public ColyseusClient CreateChatLoungeClient(string endpoint)
        {
	        ChatLoungeclient = new ColyseusClient(endpoint);
	        return ChatLoungeclient;
        }

        /// <summary>
        /// /// Create a new <see cref="ColyseusClient"/> along with any other client initialization you may need to perform
        /// /// </summary>
        public virtual void InitializeClient()
        {
	        if (isAppRelease)
	        {
		        CreateClient(_colyseusSettings.WebSocketEndpoint);
		        CreateChatLoungeClient(_colyseusSettings.WebSocketEndpoint);
	        }
	        else
	        {
		        CreateClient(_colyseusTestSettings.WebSocketEndpoint);
		        CreateChatLoungeClient(_colyseusTestSettings.WebSocketEndpoint);
	        }
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback that gets called just before app exit.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
        }
    }
}