using Project.Utility.Attributes;
using UnityEngine;

namespace Project.Gameplay
{
    public class WhoActivatedMe : MonoBehaviour
    {
        [SerializeField]
        [GreyOut]
        private string m_WhoActivatedMe;

        public void SetActivator(string ID)
        {
            m_WhoActivatedMe = ID;
        }

        public string GetActivator()
        {
            return m_WhoActivatedMe;
        }
    }
}