using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Core
{
    internal class SerializationSurrogateSelector : SurrogateSelector
    {
        static List<(System.Type type, ISerializationSurrogate surrogate)> surrogateSelection = new();

        /// <summary>
        /// adds surrogate for type and base types,list is sorted so surrogates for child types will be prefered over their parent
        /// </summary>
        public static void AddSerializationSurrogate(System.Type type, ISerializationSurrogate surrogate)
        {
            if (surrogateSelection.Any(item => item.type.Equals(type))) return;
            for (int i = 0; i < surrogateSelection.Count; i++)
                if (type.IsSubclassOf(surrogateSelection[i].type))
                {
                    surrogateSelection.Insert(i, (type, surrogate));
                    return;
                }
            surrogateSelection.Add((type, surrogate));
        }

        public SerializationSurrogateSelector() : base()
        {
            this.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new SerializationSurrogateForVector2());
            this.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new SerializationSurrogateForVector3());
            this.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), new SerializationSurrogateForQuaternion());
            this.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), new SerializationSurrogateForColor());
            this.AddSurrogate(typeof(Object), new StreamingContext(StreamingContextStates.All), new IgnoreUnityObjectSurrogate());
        }
        public override ISerializationSurrogate GetSurrogate(System.Type type, StreamingContext context, out ISurrogateSelector selector)
        {
#if UNITY_EDITOR
            if (type.FullName.Contains("System.DelegateSerializationHolder"))
            {
                Debug.LogError("it is not recomended to serialize lambda expressions, if you do so, you cant edit classes that subscribe to delegates or serialization will crash");
            }
#endif
            ISerializationSurrogate surrogate = base.GetSurrogate(type, context, out selector);
            if (surrogate != null)
                return surrogate;

            if (type.Equals(typeof(Object)) || type.IsSubclassOf(typeof(Object)))
                return base.GetSurrogate(typeof(Object), context, out selector);

            foreach (var s in surrogateSelection)
                if (type.Equals(s.type) || type.IsSubclassOf(s.type))
                    return s.surrogate;
            return null;
        }
    }
}
