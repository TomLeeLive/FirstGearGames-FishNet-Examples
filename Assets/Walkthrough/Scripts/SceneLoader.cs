using FishNet;
using FishNet.Managing.Logging;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{

    public const string SCENE_NAME = "NewScene";

    [Server(Logging = LoggingType.Off)]
    private void OnTriggerEnter(Collider other)
    {
        NetworkObject nob = other.GetComponent<NetworkObject>();
        if (nob != null)
            LoadScene(nob);
    }

    /// <summary>
    /// Load 
    /// </summary>
    /// <param name="nob"></param>
    private void LoadScene(NetworkObject nob)
    {
        //If there is no owner then exit method. We only want to load scenes for players.
        if (!nob.Owner.IsActive)
            return;

        //SceneLoadData sld = new SceneLoadData(SCENE_NAME);
        //SceneLookupData lookup = new SceneLookupData(_stackedSceneHandle, SCENE_NAME);
        //SceneLoadData sld = new SceneLoadData(lookup);
        //sld.Options.DisallowStacking = false;

        //SceneLookupData lookup;
        //Debug.Log("Loading by handle? " + (_stackedSceneHandle != 0));
        //if (_stackedSceneHandle != 0)
        //    lookup = new SceneLookupData(_stackedSceneHandle);
        //else
        //    lookup = new SceneLookupData(SCENE_NAME);
        //SceneLoadData sld = new SceneLoadData(lookup);

        SceneLookupData lookup = new SceneLookupData(SCENE_NAME);
        SceneLoadData sld = new SceneLoadData(lookup);
        sld.Options.AllowStacking = true;

        sld.MovedNetworkObjects = new NetworkObject[] { nob };
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadConnectionScenes(nob.Owner, sld);
    }


    public bool SceneStack = false;
    /// <summary>
    /// Handle of the stacked scene.
    /// </summary>
    private int _stackedSceneHandle = 0;

    private void Start()
    {
        InstanceFinder.SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
    }
    private void OnDestroy()
    {
        if (InstanceFinder.SceneManager != null)
            InstanceFinder.SceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
    }

    private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs obj)
    {
        /* Server handles scene loading and syncing
         * so do not bother setting up scene stacking if it was a client
         * that completed the scene load. */
        if (!obj.QueueData.AsServer)
            return;
        if (!SceneStack)
            return;
        //Stacked scene id is already set, not interested in creating a new stacked scene.
        if (_stackedSceneHandle != 0)
            return;

        //Set the first loaded scene as the handle.
        if (obj.LoadedScenes.Length > 0)
            _stackedSceneHandle = obj.LoadedScenes[0].handle;
    }


}
