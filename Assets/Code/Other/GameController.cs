using Project.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    static GameController m_Controller;
    public bool m_Host;
    Transform m_SpawnerZombie;
    public List<Transform> m_PlayerTransforms;
    List<Transform> m_ZombieSpawnTransform;
    public List<string> m_PlayerIDS;
    private NetworkIdentity m_SocketComponent;

    public static GameController GetGameController()
    {
        return m_Controller;
    }
    void Awake()
    {
        if (m_Controller == null)
            m_Controller = this;
        else
            Destroy(m_Controller.gameObject);
    }
    public void AddOrRemovePlayer(Transform Player,bool Add)
    {
        if(Add)
            m_PlayerTransforms.Add(Player);
        else
            m_PlayerTransforms.Remove(Player);
    }
    public void AddOrRemovePlayerID(string Id, bool Add)
    {
        if (Add)
            m_PlayerIDS.Add(Id);
        else
            m_PlayerIDS.Remove(Id);
    }
    public List<Transform> GetPlayers()
    {
        return m_PlayerTransforms;
    }
    public List<string> GetPlayerIDS()
    {
        return m_PlayerIDS;
    }
    public void SetHost()
    {
        m_Host = true;
        Debug.Log("host true");
    }
    public bool GetHost()
    {
        return m_Host;
    }
    public void SetCloseSpawner(Transform Spawn)
    {
        m_SpawnerZombie = Spawn;
    }
    public Transform GetSpawn()
    {
        return m_SpawnerZombie;
    }
    public void SetAllZombieSpawnTransform(List<Transform> List)
    {
        m_ZombieSpawnTransform = List;
    }
    public List<Transform> GetAllZombieSpawner()
    {
        return m_ZombieSpawnTransform;
    }
    public void SetSocket(NetworkIdentity Socket)
    {
        m_SocketComponent = Socket;
    }
    public NetworkIdentity GetSocket()
    {
        return m_SocketComponent;
    }
}
