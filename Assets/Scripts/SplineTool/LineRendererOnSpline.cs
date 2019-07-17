using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineRendererOnSpline : MonoBehaviour
{ 
    [SerializeField, Range(0, 10)] float _Width;
    private ISpline _Spline;
    [SerializeField] private LineRenderer _LineRenderer;
    [SerializeField, Range(0, 1024)] private int resolution = 1;
    [SerializeField] private Gradient _VertexColor = new Gradient();
    [SerializeField] public Mesh _BakedMesh;


    public void Reset() 
    {
        _LineRenderer = GetComponent<LineRenderer>();

    }

    ISpline GetSpline()
    {
        if (_Spline == null)
        {
            _Spline = GetComponent<ISpline>();
        }
        return _Spline;
    }

    void Update()
    {
        _LineRenderer.useWorldSpace = false;
        _LineRenderer.loop = GetSpline().SplineLoop();
        _LineRenderer.startWidth = _Width;
        _LineRenderer.positionCount = 0;
        _LineRenderer.colorGradient = _VertexColor;




        if (_Spline.GetPointCount() > 0)
        {
            var P = 0f;
            var start = GetSpline().GetNonUniformPoint(0f);
            Vector3 _FirstPoint = start;
            _FirstPoint = transform.InverseTransformPoint(_FirstPoint);
            _LineRenderer.positionCount++;
            _LineRenderer.SetPosition(_LineRenderer.positionCount - 1, _FirstPoint);
            var step = 1f / resolution;
            do
            {
                _LineRenderer.positionCount++;
               
                P += step;
                var here = GetSpline().GetNonUniformPoint(P);
                here = transform.InverseTransformPoint(here);
                _LineRenderer.SetPosition(_LineRenderer.positionCount - 1, here);

               start = here;
            } while (P + step <= 1);
        }

        if (_BakedMesh == null)
            _BakedMesh = new Mesh();
        _LineRenderer.BakeMesh(_BakedMesh, false);
    }
}
