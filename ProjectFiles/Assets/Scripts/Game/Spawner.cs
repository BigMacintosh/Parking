using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    
    public class Spawner : MonoBehaviour
    {
        public T spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Object
        {
            return Instantiate(prefab, position, rotation);
        }
    }

}