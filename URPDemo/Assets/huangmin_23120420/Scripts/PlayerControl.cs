using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private Button btn;
    private Image _btnImg;
    private Text[] _texts;
    [SerializeField] private PlaneControl _planeControl;
    private int state = 0; //状态为0时可以接收点击
    private Camera _camera;
    void Start()
    {
        _btnImg = btn.GetComponent<Image>();
        _texts = btn.GetComponentsInChildren<Text>();
        btn.onClick.AddListener(OnBtnClick);
        _camera=Camera.main;
    }

    private void OnBtnClick()
    {
        this.ToCircle();
    }

    private float CurrentTime;

    private void Update()
    {
        if (CurrentTime > 0)
        {
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                this.ToPlane();
            }
        }

        if (Input.GetMouseButtonDown(0) && state == 0)
        {

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            //如果碰撞到了物体，hit里面就包含该物体的相关信息；
            if (Physics.Raycast(ray,out RaycastHit  hit))
            {
                CurrentTime = 0;
                if (this._planeControl.Progress == 0)
                {
                   
                    this.ToCircle(false);
                }
                else if (Math.Abs(this._planeControl.Progress - 1) < 0.001f)
                {
                    ToPlane();
                }
            }
           
        }
    }

    private void ToCircle(bool needBtnAnim=true)
    {
        this.btn.interactable = false;
        state = 1;
     
        if (needBtnAnim)
        {
            DOTween.To(() => btn.transform.localScale, scale => { btn.transform.localScale = scale; },
                new Vector3(0.8f, 0.8f, 0.8f), 0.2f);

            DOTween.To(() => _btnImg.color.a, al=> SetBtnAlpha(al), 0f, 0.5f);
        }
        else
        {
            SetBtnAlpha(0);
        }
      
        
        var planeTw = DOTween.To(() => this._planeControl.Progress, f => { this._planeControl.Progress = f; }, 1f, 2)
            .SetEase(Ease.Linear);

        planeTw.onComplete = () =>
        {
            CurrentTime = 3;
            state = 0;
        };
    }

    private void SetBtnAlpha(float al)
    {
        var col = _btnImg.color;
        col.a = al;
        _btnImg.color = col;
        for (var i = 0; i < _texts.Length; i++)
        {
            var colText = _texts[i].color;
            colText.a = al;
            _texts[i].color = colText;
        }
    }

    private void ToPlane()
    {
        state = 1;
        var tw = DOTween.To(() => this._planeControl.Progress, f => { this._planeControl.Progress = f; }, 0, 2)
            .SetEase(Ease.Linear);
        tw.onComplete = () =>
        {
            this.ShowBtn();
            state = 0;
        };
    }

    private void ShowBtn()
    {
        this.btn.transform.localScale=Vector3.one;
        var tw = DOTween.To(() => _btnImg.color.a, al =>
        {
            var col = _btnImg.color;
            col.a = al;
            _btnImg.color = col;
            for (var i = 0; i < _texts.Length; i++)
            {
                var colText = _texts[i].color;
                colText.a = al;
                _texts[i].color = colText;
            }
        }, 1f, 0.2f);
        tw.onComplete = () => { this.btn.interactable = true; };
    }
}