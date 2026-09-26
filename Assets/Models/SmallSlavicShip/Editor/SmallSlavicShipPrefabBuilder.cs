#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SmallSlavicShipPrefabBuilder
{
    [MenuItem("The Pirate/Create Small Slavic Ship Prefab")]
    public static void CreatePrefab()
    {
        const string modelPath = "Assets/ThePirate/SmallSlavicShip/SmallSlavicShip.obj";
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (!model) { Debug.LogError("Model not found at " + modelPath); return; }
        var root = new GameObject("SmallSlavicShip");
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
        visual.name = "Visual";
        Add(root.transform,"Mounts/LeftBroadside/Mount_L_01",new Vector3(-1.75f,.75f,-.9f),Vector3.left);
        Add(root.transform,"Mounts/LeftBroadside/Mount_L_02",new Vector3(-1.75f,.75f,1.1f),Vector3.left);
        Add(root.transform,"Mounts/RightBroadside/Mount_R_01",new Vector3(1.75f,.75f,-.9f),Vector3.right);
        Add(root.transform,"Mounts/RightBroadside/Mount_R_02",new Vector3(1.75f,.75f,1.1f),Vector3.right);
        Add(root.transform,"Mounts/Special/MortarMount",new Vector3(0,.7f,-.1f),Vector3.forward);
        Add(root.transform,"VFX/BowWake",new Vector3(0,0,5.3f),Vector3.forward);
        Add(root.transform,"VFX/SternWake",new Vector3(0,0,-5f),Vector3.back);
        foreach (var p in new[]{new Vector3(-1.4f,0,-3),new Vector3(1.4f,0,-3),new Vector3(-1.4f,0,3),new Vector3(1.4f,0,3)}) Add(root.transform,"Buoyancy/Point",p,Vector3.up);
        var collider=root.AddComponent<BoxCollider>(); collider.center=new Vector3(0,0,0); collider.size=new Vector3(4,1.8f,11);
        var rb=root.AddComponent<Rigidbody>(); rb.mass=4500; rb.interpolation=RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        System.IO.Directory.CreateDirectory("Assets/ThePirate/SmallSlavicShip/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(root,"Assets/ThePirate/SmallSlavicShip/Prefabs/SmallSlavicShip.prefab");
        Object.DestroyImmediate(root); AssetDatabase.Refresh();
        Debug.Log("Created SmallSlavicShip prefab.");
    }
    static Transform Add(Transform root,string path,Vector3 pos,Vector3 forward)
    {
        Transform parent=root; var parts=path.Split('/');
        for(int i=0;i<parts.Length;i++) { var t=parent.Find(parts[i]); if(!t){var go=new GameObject(parts[i]);t=go.transform;t.SetParent(parent,false);} parent=t; }
        parent.localPosition=pos; parent.localRotation=Quaternion.LookRotation(forward,Vector3.up); return parent;
    }
}
#endif
