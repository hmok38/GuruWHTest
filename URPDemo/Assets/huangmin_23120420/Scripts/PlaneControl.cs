using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此实现方案还可以用Shader实现,只要将计算坐标的算法移到Shader中去即可,
/// 我实在是没有时间了,如果要实际应用到项目中去,有时间我再写成shader,
/// 性能消耗会非常低
/// </summary>
public class PlaneControl : MonoBehaviour
{
    private float _progress;
    private List<Vector3> _xOrigVertices = new List<Vector3>();
    private List<Vector3> _zOrigVertices = new List<Vector3>();
    private float _radius;
    private Vector3 _center;
    public string RollAxis = "x";
    [SerializeField] private Collider xCollide;
    [SerializeField] private Collider zCollide;
    [SerializeField] private Collider planeCollider;
    private List<Vector3> _tempList;
    private Mesh _xMesh, _zMesh;
    private Transform _xPlane, _zPlane;

    public float Progress
    {
        get => _progress;
        set
        {
            var v = Mathf.Clamp(value, 0, 1);
            if (v == _progress) return;

            this._progress = v;
            EditMesh();
        }
    }

    void Start()
    {
        _xPlane = this.transform.Find("XPlane");
        _xMesh = _xPlane.GetComponent<MeshFilter>().mesh;
        _zPlane = this.transform.Find("ZPlane");
        _zMesh = _zPlane.GetComponent<MeshFilter>().mesh;
        _radius = 2f / Mathf.PI / 2;
        _xMesh.GetVertices(_xOrigVertices);
        _zMesh.GetVertices(_zOrigVertices);
        _tempList = new List<Vector3>(_xOrigVertices.Count);
        SetCollider(CheckBeX());
    }

    private void SetCollider(bool beX)
    {
        xCollide.gameObject.SetActive(_progress == 1 && beX);
        zCollide.gameObject.SetActive(_progress == 1 && !beX);
        planeCollider.gameObject.SetActive(_progress == 0);
    }

    private void GetCenter(int step, bool beX)
    {
        var centerV = step / 360f * 2 - 1;
        _center = beX ? new Vector3(centerV, _radius, 0) : new Vector3(0, _radius, centerV);
    }

    private bool CheckBeX()
    {
        if (!RollAxis.Equals("x") && !RollAxis.Equals("z"))
        {
            RollAxis = "x";
        }

        return RollAxis.Equals("x");
    }

    private void EditMesh()
    {
        Mesh curretnMesh = _zMesh;
        List<Vector3> origList = _zOrigVertices;
        int stepInde = Mathf.FloorToInt(_progress * 360); //0-360
        var beX = this.CheckBeX();
        if (beX)
        {
            curretnMesh = _xMesh;
            origList = _xOrigVertices;
        }

        _zPlane.gameObject.SetActive(!beX);
        _xPlane.gameObject.SetActive(beX);
        GetCenter(stepInde, beX);
        _tempList.Clear();
        for (int i = 0; i < origList.Count; i++)
        {
            var vert = origList[i];
            var index = GetVerticesIndex(vert, beX);
            var beEdit = GetVerticesPos(_progress, index, beX, out Vector3 newPos);
            if (beEdit)
            {
                if (beX)
                    newPos.z = vert.z;
                else

                    newPos.x = vert.x;

                _tempList.Add(newPos);
            }
            else
            {
                _tempList.Add(vert);
            }
        }

        curretnMesh.SetVertices(_tempList);
        SetCollider(beX);
    }

    private int GetVerticesIndex(Vector3 origPos, bool beX)
    {
        var length = 2f / 360;
        return Mathf.FloorToInt(((beX ? origPos.x : origPos.z) + 1) / length);
    }

    private bool GetVerticesPos(float progress, int index, bool beX, out Vector3 pos)
    {
        var pro = Mathf.Clamp(progress, 0, 1);

        int stepInde = Mathf.FloorToInt(progress * 360);
        ;

        if (index > stepInde)
        {
            pos = Vector3.zero;
            return false;
        }


        var anglePerStep = 360f / 360;

        var angle = (stepInde - index) * anglePerStep;
        //0度轴设为向下 
        var dirZero = Vector3.down * _radius;

        var matrix = GetRotationMatrix(beX, angle);
        var dir = matrix * dirZero;
        pos = new Vector3(dir.x, dir.y, dir.z) + _center;
        return true;
    }

    private Matrix4x4 GetRotationMatrix(bool beX, float angle)
    {
        var qua = Quaternion.AngleAxis(angle, beX ? Vector3.back : Vector3.right);
        return Matrix4x4.Rotate(qua);
    }
}