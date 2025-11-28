using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace Core
{
    public class IgnoreUnityObjectSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
#if UNITY_EDITOR
            var o = obj as Object;
            var path = o ? AssetDatabase.GetAssetPath(o) : "";
            var guid = AssetDatabase.GUIDFromAssetPath(path);
            info.AddValue("id", o ? o.GetInstanceID() : -1);
            info.AddValue("guid", guid);
#endif
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
#if UNITY_EDITOR

            int id = (int)info.GetValue("id", typeof(int));
            var guid = (GUID)info.GetValue("guid", typeof(GUID));
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o.GetInstanceID() == id);
            return asset;
#else
        return null;
#endif


        }
    }
}
