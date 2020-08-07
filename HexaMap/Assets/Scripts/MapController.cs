using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Hexa
{
    public class MapController : MonoBehaviour
    {
        static public MapController instance;

        #region Fields

        public Transform mapObjects;

        public Material startmaterial;
        public Material greenmaterial;
        public Material redmaterial;

        public Hexa findhexa;
        #endregion

        #region MonoBehaviour CallBack

        public void Awake()
        {
            if (instance == null)
                instance = this;
        }

        public void Start()
        {
            SetHexaMap(); //###
            findhexa = new Hexa(2, 0);
        }

        private void OnDestroy()
        {
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                
                if (Physics.Raycast(ray, out hitInfo))
                {
                    HexaTile hithexa = hitInfo.collider.GetComponent<HexaTile>();

                    if (hithexa != null) 
                    {
                        findhexa = hithexa.hexa;
                        hithexa.element = 1;
                        hithexa.GetComponent<MeshRenderer>().material = greenmaterial;
                    }
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    HexaTile hithexa = hitInfo.collider.GetComponent<HexaTile>();

                    if (hithexa != null)
                    {
                        hithexa.element = -1;
                        hithexa.GetComponent<MeshRenderer>().material = redmaterial;
                    }
                }
            }
            if (Input.GetMouseButtonDown(2))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    HexaTile hithexa = hitInfo.collider.GetComponent<HexaTile>();

                    if (hithexa != null)
                    {
                        hithexa.element = 0;
                        hithexa.GetComponent<MeshRenderer>().material = startmaterial;
                    }
                }
            }
        }

        #endregion

        #region HexaMap

        public HexaTile tilePrefab;

        public HexaTile[] tiles;

        private int radians = 4;

        /// <summary>
        /// 헥사 타일 맵을 셋팅한다.
        /// </summary>
        void SetHexaMap()
        {
            tiles = new HexaTile[radians * radians * radians*radians];

            int n = 0;
            int start = -2;

            for (int z = 3,j = 4; j < 8 ; ++j,--z)
            {

                for (int x = start, i = 0; i < j; ++i,++x)
                {
                    CreateTile(x, z, ++n);
                }

                if (j % 2 == 1)
                {
                    --start;
                }
            }

            start = -2;

            for (int z = -1, j = 6; j > 3; --j, --z)
            {

                for (int x = start, i = 0; i < j; ++i, ++x)
                {
                    CreateTile(x, z, ++n);
                }

                if (j % 2 == 1)
                {
                    ++start;
                }
            }
        }

        public void FindThat()
        {
            List<Hexa> pathlist = FindPath(new Hexa(0, 0), findhexa);

            HexaTile tile;

            foreach (Hexa hexa in pathlist)
            {
                tile = FindHexa(hexa.POS);
                tile.element = 1;
                tile.GetComponent<MeshRenderer>().material = greenmaterial;
            }
        }

        /// <summary>
        /// 타일을 생성한다.
        /// </summary>
        /// <param name="x">좌표x</param>
        /// <param name="z">좌표z</param>
        /// <param name="i">인덱스</param>
        void CreateTile(int x, int z, int i)
        {
            Vector3 position;
            position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
            position.y = 0f;
            position.z = z * (HexMetrics.outerRadius * 1.5f);

            HexaTile tile = tiles[i] = Instantiate<HexaTile>(tilePrefab);
            tile.transform.SetParent(transform, false);
            tile.transform.localPosition = position;
            
            tile.index = i;
            tile.hexa = Hexa.FromOffsetHexa(x, z);

            tile.gameObject.SetActive(true);

            //Debug.Log(tile.hexa.ToString());
        }

        /// <summary>
        /// 길을 찾는다
        /// </summary>
        /// <param name="start">시작지점</param>
        /// <param name="end">종료지점</param>
        /// <returns></returns>
        List<Hexa> FindPath(Hexa start, Hexa end) 
        {
            //검사 대상들 >(용도변경)> 완성된 길
            List<Hexa> openlist = new List<Hexa>();

            //검사된 대상들 
            List<Hexa> closelist = new List<Hexa>();

            //현재 검사하는 대상
            Hexa curhexa = start;

            //되돌아오는 길을 찾는 변수
            float mindir = 100;
            float dir = 0;
            int minindex = 0;

            //검사할 첫번째 위치들을 담는다.
            AddPathForList(curhexa, end, openlist, closelist);

            while (true)
            {
                //예외처리 // 종료조건
                if (openlist.Count <= 0) 
                    break;

                //현재 검사하는 대상을 완료로 설정.
                closelist.Add(curhexa);

                //새로운 대상을 가져온다. (휴리스틱을 기본으로 한다.)
                curhexa = FindLowWeightWithList(openlist,start, end);

                //Debug.DrawLine(FindHexa(closelist[closelist.Count - 1].POS).transform.position + (Vector3.up * 0.4f),
                //    FindHexa(check.POS).transform.position + (Vector3.up * 0.4f), Color.green, 10f);

                //도착시 끝
                if (curhexa.POS == end.POS) 
                {
                    closelist.Add(curhexa);
                    break;
                }

                //새로운 위치에 대한 검사목록을 추가한다.
                AddPathForList(curhexa, end, openlist, closelist);
            }

            //기본적 A* 검사는 이것으로 종료.
            //return closelist;

            //모든 검사된 위치를 담는 closelist을 통해
            //종료 지점에서 시작 지점으로 가는 길을 찾는다.

            //openlist 용도 변경
            //검사 대상들 >(용도변경)> 완성된 길
            openlist.Clear();
            minindex = closelist.Count - 1; //처음 위치

            //검사 이동 방향 :: 종료 지점 >> 시작 지점 
            for (int i = closelist.Count - 1; i >= 0; --i) 
            {
                i = minindex;               //가까운 지점을 현재 위치로 설정
                openlist.Add(closelist[i]); //현재 위치를 길로 지점
                mindir = 100;               //최소 거리를 리셋, 크게 잡는게 포인트

                //안간 검사지를 탐색
                for (int j = i - 1; j >= 0; --j) 
                {
                    dir = Hexa.Distance(closelist[i].POS, closelist[j].POS);

                    //가장 가까운 거리의 지점을 찾아 담는다.
                    if (mindir > dir)
                    {
                        minindex = j;
                        mindir = dir;
                    }
                    //동일하면 시작지점과 가까운 것을 우선한다.
                    else if (mindir == dir) 
                    {
                        dir += Hexa.Distance(start.POS, closelist[j].POS);

                        if (mindir > dir) 
                        {
                            minindex = j;
                            mindir = dir;
                        }
                    }
                }
            }

            //디버깅용 표시
            //지나간 것을 담는 용도로 수정
            curhexa = null;
            foreach (Hexa xa in openlist)
            {
                if (curhexa != null)
                {
                    Debug.DrawLine(FindHexa(curhexa.POS).transform.position + (Vector3.up * 0.4f), 
                        FindHexa(xa.POS).transform.position + (Vector3.up * 0.4f), Color.green, 1f);
                }
                curhexa = xa;
            }

            //검사가 완료된 것을 되돌려준다.
            return openlist;
        }

        /// <summary>
        /// 리스트 안의 가중치 계산
        /// </summary>
        /// <param name="openlist"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Hexa FindLowWeightWithList(List<Hexa> openlist, Hexa start ,Hexa end)
        {
            int lowindex = 0;
            float lowWeight = 10000;

            float weight = 0;
            Hexa result = null;

            for (int i = 0; i < openlist.Count; ++i)
            {
                weight = Hexa.Distance(openlist[i].POS, end.POS);
                weight += Hexa.Distance(openlist[i].POS, start.POS);

                if (lowWeight > weight)
                {
                    lowWeight = weight;
                    lowindex = i;
                }
            }

            result = openlist[lowindex];
            openlist.RemoveAt(lowindex);

            //Debug.Log("Low - weight[" + lowWeight + "] opne(" + lowindex + ")" + result.POS.ToString());

            return result;
        }

        /// <summary>
        /// 리스트에 6방향을 추가해준다. (이미 지나간 길은 제외)
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="end"></param>
        /// <param name="openlist"></param>
        /// <param name="closelist"></param>
        void AddPathForList(Hexa cur,Hexa end, List<Hexa> openlist, List<Hexa> closelist) 
        {
            Vector3 pos = new Vector3(cur.X, cur.Y, cur.Z);
            HexaTile check;

            List<int> randomlist = new List<int>();
            int random = 0;
            int value = 0;

            for (int i = 5; i >= 0 ; --i) 
            {
                randomlist.Add(i);
            }

            Debug.Log("Random Start!");

            for (int i = 0; i < 6; ++i)
            {
                random = (Random.Range(0, randomlist.Count));
                value = randomlist[random];
                Debug.Log("["+ value + "]");

                randomlist.RemoveAt(random);

                switch (value)
                {
                    case 0:
                        //Vector(-1,0,1)
                        check = FindHexa(pos + Vector3.left + Vector3.forward);
                        break;

                    case 1:
                        //Vector(1,0,-1)
                        check = FindHexa(pos + Vector3.right + Vector3.back);
                        break;

                    case 2:
                        //Vector(1,0,0)
                        check = FindHexa(pos + Vector3.right);
                        break;

                    case 3:
                        //Vector(-1,0,0)
                        check = FindHexa(pos + Vector3.left);
                        break;

                    case 4:
                        //Vector(0,0,-1)
                        check = FindHexa(pos + Vector3.back);
                        break;

                    case 5:
                        //Vector(0,0,1)
                        check = FindHexa(pos + Vector3.forward);
                        break;

                    default:
                        //Vector(0,0,1)
                        check = FindHexa(pos + Vector3.forward);
                        break;
                }

                //맵에 없거나 벽일 경우는 제외
                if (check != null && check.element != -1)
                {
                    //이미 검사했다면 제외
                    if (!closelist.Exists(item => item.POS == check.hexa.POS))
                    {
                        if (!openlist.Exists(item => item.POS == check.hexa.POS))
                        {
                            openlist.Add(check.hexa);
                        }
                    }
                }
            }


        }

        /// <summary>
        /// 좌표를 통해 진짜 헥사 타일을 찾는다.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        HexaTile FindHexa(Vector3 pos) 
        {
            foreach (HexaTile hexa in tiles) 
            {
                if (hexa == null) continue;

                if (hexa.hexa.X == pos.x && hexa.hexa.Z == pos.z)
                    return hexa;
            }

            return null;
        }

        #endregion


    }//end class
}//end namespace
