using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//Data container de objetos
namespace Project.Scriptable
{
    [CreateAssetMenu(fileName ="Server_Objects",menuName="Scriptable Objects/Server Objects",order=3)]
    public class ServerObjects : ScriptableObject
    {
        public List<ServerObjectsData> Objects;

        public ServerObjectsData GetObjectByName(string Name)
        {
            //Pilla el primer object que tenga el mismo nombre que le pasamos
            return Objects.SingleOrDefault(x => x.Name == Name);
        }
    }
    [Serializable]
    public class ServerObjectsData
    {
        public string Name = "new Object";
        public GameObject Prefab;
    }
}

