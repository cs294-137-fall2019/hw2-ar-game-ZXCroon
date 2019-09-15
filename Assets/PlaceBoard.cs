using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceBoard : MonoBehaviour
{

    public GameObject gameBoard;
    public GameObject chessBlackHalf;
    public GameObject chessWhiteHalf;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private bool boardPlaced = false;
    private Vector3 boardPosition;

    private GameObject blackOriginObject, whiteOriginObject, xlineObject, zlineObject;
    private GameManager gameManager;
    // private int who = 1;
    private int chessPlaced = 4;

    public float debounceTime = 0.15f;
    public float remainingDebounceTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        blackOriginObject = GameObject.Find("Chess Black");
        whiteOriginObject = GameObject.Find("Chess White");
        xlineObject = GameObject.Find("X Line");
        zlineObject = GameObject.Find("Z Line");
        blackOriginObject.SetActive(false);
        whiteOriginObject.SetActive(false);
        xlineObject.SetActive(false);
        zlineObject.SetActive(false);

        Text text = GameObject.Find("Win Lose").GetComponent<Text>();
        text.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0)
        {
            return;
        }
        if (remainingDebounceTime > 0)
        {
            remainingDebounceTime -= Time.deltaTime;
            return;
        }
        remainingDebounceTime = debounceTime;
        Vector2 touchPosition = Input.GetTouch(0).position;
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
 
        if (boardPlaced)
        {
            if (chessPlaced == 64)
            {
                return;
            }
            if (!raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
            {
                return;
            }
            var hitPose = hits[0].pose;
            float hitX = hitPose.position.x, hitZ = hitPose.position.z;
            int r = (int)(Mathf.Round((hitX - boardPosition.x + 0.3f - 0.0375f) / 0.075f));
            int c = (int)(Mathf.Round((hitZ - boardPosition.z + 0.3f - 0.0375f) / 0.075f));
            if (gameManager.Put(r, c, 1) != -1)
            {
                int bestr = 0, bestc = 0, bestCnt = -1;
                for (int i = 0; i < 8; ++i)
                {
                    for (int j = 0; j < 8; ++j)
                    {
                        int x = gameManager.Put(i, j, -1, true);
                        if (x > bestCnt || (x == bestCnt && System.Math.Abs(i - r) + System.Math.Abs(j - c) < System.Math.Abs(bestr - r) + System.Math.Abs(bestc - c)))
                        {
                            bestr = i;
                            bestc = j;
                            bestCnt = x;
                        }
                    }
                }
                gameManager.Put(bestr, bestc, -1);
                chessPlaced += 2;
            }
        }
        else
        {
            if (!raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                return;
            }
            var hitPose = hits[0].pose;
            gameBoard.SetActive(true);
            gameBoard.transform.position = hitPose.position;
            boardPosition = hitPose.position;
            boardPlaced = true;
            DrawGrids();
            GameObject.Find("Plane Visualization Object").SetActive(false);
            /*
            foreach (ARPlane plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
            */

            gameManager = new GameManager();
            gameManager.Init(blackOriginObject, whiteOriginObject, boardPosition);
        }
    }

    private void DrawGrids()
    {
        for (int i = 1; i < 8; ++i)
        {
            GameObject xline = GameObject.Instantiate(xlineObject);
            xline.transform.position = new Vector3(boardPosition.x, boardPosition.y + 0.005f, boardPosition.z - 0.3f + 0.075f * i);
            xline.SetActive(true);
        }
        for (int i = 1; i < 8; ++i)
        {
            GameObject zline = GameObject.Instantiate(zlineObject);
            zline.transform.position = new Vector3(boardPosition.x - 0.3f + 0.075f * i, boardPosition.y + 0.005f, boardPosition.z);
            zline.SetActive(true);
        }
    }
}

class GameManager
{
    private int[][] state = new int[8][];
    private GameObject blackOriginObject, whiteOriginObject;
    private Vector3 boardPosition;
    private GameObject[][] blackObjectForEachGrid = new GameObject[8][];
    private GameObject[][] whiteObjectForEachGrid = new GameObject[8][];
    private int blackCnt = 0, whiteCnt = 0;

    public void Init(GameObject blackOriginObject, GameObject whiteOriginObject, Vector3 boardPosition)
    {
        for (int i = 0; i < 8; ++i)
        {
            state[i] = new int[8];
            blackObjectForEachGrid[i] = new GameObject[8];
            whiteObjectForEachGrid[i] = new GameObject[8];
            for (int j = 0; j < 8; ++j)
            {
                state[i][j] = 0;
            }
        }
        this.blackOriginObject = blackOriginObject;
        this.whiteOriginObject = whiteOriginObject;
        this.boardPosition = boardPosition;

        Put(3, 3, 1);
        Put(4, 4, 1);
        Put(3, 4, -1);
        Put(4, 3, -1);
    }

    private void _Flip(int r, int c)
    {
        float x = blackObjectForEachGrid[r][c].transform.position.x, z = blackObjectForEachGrid[r][c].transform.position.z;
        if (state[r][c] == 1)
        {
            blackObjectForEachGrid[r][c].transform.position = new Vector3(x, boardPosition.y + 0.007f, z);
            whiteObjectForEachGrid[r][c].transform.position = new Vector3(x, boardPosition.y + 0.0111f, z);
        } else
        {
            blackObjectForEachGrid[r][c].transform.position = new Vector3(x, boardPosition.y + 0.0111f, z);
            whiteObjectForEachGrid[r][c].transform.position = new Vector3(x, boardPosition.y + 0.007f, z);
        }
    }

    private void _Put(int r, int c, int who)
    {
        // blackOriginObject.SetActive(true);
        // whiteOriginObject.SetActive(true);
        blackObjectForEachGrid[r][c] = GameObject.Instantiate(blackOriginObject);
        whiteObjectForEachGrid[r][c] = GameObject.Instantiate(whiteOriginObject);
        // blackOriginObject.SetActive(false);
        // whiteOriginObject.SetActive(false);
        blackObjectForEachGrid[r][c].SetActive(true);
        whiteObjectForEachGrid[r][c].SetActive(true);

        float putX = boardPosition.x - 0.3f + 0.0375f + r * 0.075f;
        float putZ = boardPosition.z - 0.3f + 0.0375f + c * 0.075f;
        float topY = boardPosition.y + 0.0111f, bottomY = boardPosition.y + 0.007f;

        if (who == 1)
        {
            blackObjectForEachGrid[r][c].transform.position = new Vector3(putX, topY, putZ);
            whiteObjectForEachGrid[r][c].transform.position = new Vector3(putX, bottomY, putZ);
        }
        if (who == -1)
        {
            blackObjectForEachGrid[r][c].transform.position = new Vector3(putX, bottomY, putZ);
            whiteObjectForEachGrid[r][c].transform.position = new Vector3(putX, topY, putZ);
        }
    }

    public int Put(int r, int c, int who, bool onlyTry = false)
    {
        if (r < 0 || r >= 8 || c < 0 || c >= 8 || state[r][c] != 0)
        {
            return -1;
        }
        int[] inci = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 };
        int[] incj = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 };

        int cnt = 0;
        for (int k = 0; k < 8; ++k)
        {
            int nowr = r + inci[k], nowc = c + incj[k];
            while (nowr >= 0 && nowr < 8 && nowc >= 0 && nowc < 8 && state[nowr][nowc] == -who)
            {
                nowr += inci[k];
                nowc += incj[k];
            }
            if (nowr >= 0 && nowr < 8 && nowc >= 0 && nowc < 8 && state[nowr][nowc] == who)
            {
                int nnowr = r + inci[k], nnowc = c + incj[k];
                while (nnowr != nowr || nnowc != nowc)
                {
                    ++cnt;
                    if (!onlyTry)
                    {
                        _Flip(nnowr, nnowc);
                        state[nnowr][nnowc] = -state[nnowr][nnowc];
                        if (who == 1)
                        {
                            ++blackCnt;
                            --whiteCnt;
                        } else
                        {
                            ++whiteCnt;
                            --blackCnt;
                        }
                    }
                    nnowr += inci[k];
                    nnowc += incj[k];
                }
            }
        }

        if (!onlyTry)
        {
            _Put(r, c, who);
            state[r][c] = who;
            if (who == 1) ++blackCnt; else ++whiteCnt;
            Text text = GameObject.Find("Canvas/Score").GetComponent<Text>();
            text.text = string.Format("Black: {0}\nWhite: {1}", blackCnt, whiteCnt);
            if (blackCnt + whiteCnt == 64)
            {
                text = GameObject.Find("Win Lose").GetComponent<Text>();
                text.text = (blackCnt == whiteCnt ? "Draw" : "You " + (blackCnt > whiteCnt ? "Win" : "Lose"));
            }
        }

        return cnt;
    }
}