using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;

using Colyseus;
using Colyseus.Schema;

using GameDevWare.Serialization;

class Pos
{
    public float x;
    public float y;
}

public class GameClient : MonoBehaviour
{

    public GameObject playerPrefab;
    public GameObject allyPrefab;
    private GameObject localPlayer;
    private float TICK_TIME = .3f;
    private string ENDPOINT = "ws://localhost:2567";
    private string ROOM_NAME = "amongus";


    protected Client client;
    protected Room<WorldState> room;
    protected IndexedDictionary<MyPlayer, GameObject> players = new IndexedDictionary<MyPlayer, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
    }

    float lastUpdateTime = 0;
    // Update is called once per frame
    void Update()
    {
        if (room != null && lastUpdateTime > TICK_TIME)
        {
            room.Send("move", new Pos()
            {
                x = this.localPlayer.transform.position.x,
                y = this.localPlayer.transform.position.y
            });
            lastUpdateTime = 0;
        }

        lastUpdateTime += Time.deltaTime;
    }

    async void ConnectToServer()
    {
        Debug.Log("Connecting to " + ENDPOINT);
        client = ColyseusManager.Instance.CreateClient(ENDPOINT);

        await client.Auth.Login();

        // Update username
        client.Auth.Username = "user-" + UnityEngine.Random.Range(0f, 100000.0f);
        await client.Auth.Save();

        // Join room
        room = await client.JoinOrCreate<WorldState>(ROOM_NAME, new Dictionary<string, object>() { });
        room.State.players.OnAdd += OnPlayerAdd;
        room.State.players.OnRemove += OnPlayerRemove;
        room.OnError += (code, message) => Debug.LogError("ERROR, code =>" + code + ", message => " + message);
        room.OnStateChange += OnStateChangeHandler;
    }


    void OnPlayerAdd(MyPlayer player, string key)
    {
        Debug.Log("Player add! x => " + player.x + ", y => " + player.y);

        GameObject cube;

        // This is how we find local player
        if (key.Equals(room.SessionId))
        {
            cube = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            cube.transform.position = new Vector3(player.x, player.y, 0);

            this.localPlayer = cube;
            players.Add(player, this.localPlayer);
        }
        else
        {
            cube = Instantiate(allyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            cube.transform.position = new Vector3(player.x, player.y, 0);
            players.Add(player, cube);
        }


        // On player update...
        player.OnChange += (List<Colyseus.Schema.DataChange> changes) =>
        {

            // Dont want to move our own character..
            if (!key.Equals(room.SessionId))
            {
                // basic way
                // cube.transform.position = new Vector3(player.x, player.y);
                StartCoroutine(LerpPosition(cube, new Vector3(player.x, player.y, 0), TICK_TIME));
            }
        };
    }


    // Great lerp reference - https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/
    IEnumerator LerpPosition(GameObject cube, Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = cube.transform.position;

        while (time < duration)
        {
            cube.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cube.transform.position = targetPosition;
    }

    void OnPlayerRemove(MyPlayer player, string key)
    {
        GameObject cube;
        players.TryGetValue(player, out cube);
        Destroy(cube);

        players.Remove(player);
    }

    void OnStateChangeHandler(WorldState state, bool isFirstState)
    {
        Debug.Log("State has been updated!");
        Debug.Log(state);
    }
}
