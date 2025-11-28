using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Core
{
    public static class SaveSystem
    {
        static BinaryFormatter formater = new BinaryFormatter(new SerializationSurrogateSelector(), default);

        /// <summary>
        /// adds surrogate for type and base types,list is sorted so surrogates for child types will be prefered over their parent
        /// </summary>
        public static void AddSerializationSurrogate(System.Type type, ISerializationSurrogate surrogate) => SerializationSurrogateSelector.AddSerializationSurrogate(type, surrogate);

        static public byte[] Serialize(object data)
        {
            if (data == null) return null;
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                formater.Serialize(ms, data);
                bytes = ms.ToArray();
            }
            return bytes;
        }
        static public object Deserialize(byte[] bytes)
        {
            if (bytes == null) return null;
            object data = null;
            using (MemoryStream ms = new MemoryStream(bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                if (ms.Length > 0)
                    data = formater.Deserialize(ms);
            }
            return data;
        }
        static public void SaveDataToPlayerPrefs(string key, object data, bool callPlayerPrefsSave = true)
        {
            var bytes = Serialize(data);
            PlayerPrefs.SetString(key, Convert.ToBase64String(bytes));
            if (callPlayerPrefsSave) PlayerPrefs.Save();

        }
        static public object LoadDataFromPlayerPrefs(string key)
        {
            byte[] bytes = Convert.FromBase64String(PlayerPrefs.GetString(key));
            return Deserialize(bytes);
        }
        static public void SaveDataToFile(string filePath, object data)
        {
            var bytes = Serialize(data);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(filePath, bytes);
        }
        static public object LoadDataFromFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            return Deserialize(bytes);
        }
        /// <summary>
        /// its fast but works only with value types
        /// </summary>
        static public T[] LoadFromFileViaMemoryMarshal<T>(string filePath) where T : struct
        {
            var bytes = File.ReadAllBytes(filePath);
            return MemoryMarshal.Cast<byte, T>(bytes).ToArray();
        }
        /// <summary>
        /// its fast but works only with value types
        /// </summary>
        static public void SaveToFileViaMemoryMarshal<T>(string filePath, T[] data) where T : struct
        {
            int size = data.Length * Marshal.SizeOf<T>();
            byte[] bytes = MemoryMarshal.Cast<T, byte>(data).ToArray();
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(filePath, bytes);
        }

        static public void CallPlayerPrefsSave() => PlayerPrefs.Save();
    }
}
