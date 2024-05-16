using CreapsFPS.Enums;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace CreapsFPS.BaseJoystick
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
    {
        #region Variables

        public float Horizontal
        {
            get { return _input.x; }
        }

        public float Vertical
        {
            get { return _input.y; }
        }

        public Vector2 Direction
        {
            get { return new Vector2(Horizontal, Vertical); }
        }

        public float HandleRange
        {
            get { return _handleRange; }
            set { _handleRange = Mathf.Abs(value); }
        }

        public float DeadZone
        {
            get { return _deadZone; }
            set { _deadZone = Mathf.Abs(value); }
        }

        public AxisOptions AxisOptions
        {
            get { return AxisOptions; }
            set { _axisOptions = value; }
        }

        [FormerlySerializedAs("handleRange")] [Header("Joystick")] [SerializeField] private float _handleRange = 1;
        [FormerlySerializedAs("deadZone")] [SerializeField] private float _deadZone = 0;
        [FormerlySerializedAs("normalizeInput")] [SerializeField] private bool _normalizeInput = false;
        [FormerlySerializedAs("axisOptions")] [SerializeField] private AxisOptions _axisOptions = AxisOptions.Both;
        [FormerlySerializedAs("verticalTopLimit")] [Range(0, 1f)] public float VerticalTopLimit = 1f;
        [FormerlySerializedAs("verticalBotLimit")] [Range(-1f, 0)] public float VerticalBotLimit = -1f;
        [FormerlySerializedAs("horizontalTopLimit")] [Range(0, 1f)] public float HorizontalTopLimit = 1f;
        [FormerlySerializedAs("horizontalBotLimit")] [Range(-1f, 0)] public float HorizontalBotLimit = -1f;

        [FormerlySerializedAs("gravityEffect")] [Header("Effect Active")] [SerializeField, Range(0, 1f)]
        private float _gravityEffect = 0;

        [FormerlySerializedAs("mirrorEffect")] [SerializeField] private bool _mirrorEffect = false;
        [FormerlySerializedAs("imagesEffect")] [SerializeField] private bool _imagesEffect = false;

        [FormerlySerializedAs("effect")] [Header("Rect Transforms")] [SerializeField]
        private RectTransform _effect;

        [FormerlySerializedAs("background")] [SerializeField] protected RectTransform _background = null;
        [FormerlySerializedAs("handle")] [SerializeField] private RectTransform _handle = null;

        [FormerlySerializedAs("backgroundImage")] [Header("Image Effect")] [SerializeField]
        protected Image _backgroundImage;

        [FormerlySerializedAs("handleImage")] [SerializeField] private Image _handleImage;
        [FormerlySerializedAs("backgroundNormalColor")] [SerializeField] private Color _backgroundNormalColor = Color.white;
        [FormerlySerializedAs("handleNormalColor")] [SerializeField] private Color _handleNormalColor = Color.white;
        [FormerlySerializedAs("backgroundHighlightColor")] [SerializeField] private Color _backgroundHighlightColor = Color.white;
        [FormerlySerializedAs("handleHighlightColor")] [SerializeField] private Color _handleHighlightColor = Color.white;

        private RectTransform _baseRect = null;

        private Canvas _canvas;
        private Camera _cam;

        private Tween _gravityTween;
        private Vector2 _input = Vector2.zero;
        private Vector2 _startPosition;

        #endregion

        #region Initialize

        protected virtual void Start()
        {
            HandleRange = _handleRange;
            DeadZone = _deadZone;
            _baseRect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
                Debug.LogError("The Joystick is not placed inside a canvas");

            Vector2 center = new Vector2(0.5f, 0.5f);
            _background.pivot = center;
            _handle.anchorMin = center;
            _handle.anchorMax = center;
            _handle.pivot = center;
            _handle.anchoredPosition = Vector2.zero;
            _startPosition = _background.transform.localPosition;

            _cam = null;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _cam = _canvas.worldCamera;
        }


        #endregion

        #region Mouse Click Functions

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _gravityTween.Kill();
            if (_effect)
            {
                _effect.gameObject.SetActive(true);
            }

            if (_imagesEffect)
            {
                _backgroundImage.color = _backgroundHighlightColor;
                _handleImage.color = _handleHighlightColor;
            }

            OnDrag(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            Vector2 position = RectTransformUtility.WorldToScreenPoint(_cam, _background.position);
            Vector2 radius = _background.sizeDelta / 2;
            _input = (eventData.position - position) / (radius * _canvas.scaleFactor);

            FormatInput();


            HandleInput(_input.magnitude, _input.normalized, radius, _cam);
            _handle.anchoredPosition = _input * radius * _handleRange;

            if (_normalizeInput)
            {
                _input = _input.normalized;
            }

            if (_effect)
            {
                var rotateY = Mathf.Atan2(_input.x, _input.y) * Mathf.Rad2Deg;

                if (_mirrorEffect) rotateY += 180;
                _effect.localEulerAngles = Vector3.back * rotateY;
            }

        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;

            if (_gravityEffect > 0)
            {
                _gravityTween = _handle.transform.DOLocalMove(Vector2.zero, _gravityEffect, false).OnComplete(() => { });
            }
            else
            {
                _handle.anchoredPosition = Vector2.zero;
            }

            if (_imagesEffect)
            {
                _backgroundImage.color = _backgroundNormalColor;
                _handleImage.color = _handleNormalColor;
            }

            if (_effect)
            {
                _effect.gameObject.SetActive(false);
            }

            _background.transform.localPosition = _startPosition;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {

        }

        #endregion

        #region Auxiliary Functions

        protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {

            if (magnitude > _deadZone)
            {
                if (magnitude > 1)
                    _input = normalised;
            }
            else
                _input = Vector2.zero;

            if (_input.y >= VerticalTopLimit)
            {
                _input.y = VerticalTopLimit;
            }
            else if (_input.y <= VerticalBotLimit)
            {
                _input.y = VerticalBotLimit;
            }

            if (_input.x >= HorizontalTopLimit)
            {
                _input.x = HorizontalTopLimit;
            }
            else if (_input.x <= HorizontalBotLimit)
            {
                _input.x = HorizontalBotLimit;
            }
        }

        private void FormatInput()
        {
            if (_axisOptions == AxisOptions.Horizontal)
                _input = new Vector2(_input.x, 0f);
            else if (_axisOptions == AxisOptions.Vertical)
                _input = new Vector2(0f, _input.y);
        }

        private float SnapFloat(float value, AxisOptions snapAxis)
        {
            if (value == 0)
                return value;

            if (_axisOptions == AxisOptions.Both)
            {
                float angle = Vector2.Angle(_input, Vector2.up);
                if (snapAxis == AxisOptions.Horizontal)
                {
                    if (angle < 22.5f || angle > 157.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }
                else if (snapAxis == AxisOptions.Vertical)
                {
                    if (angle > 67.5f && angle < 112.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }

                return value;
            }
            else
            {
                if (value > 0)
                    return 1;
                if (value < 0)
                    return -1;
            }

            return 0;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out localPoint))
            {
                Vector2 pivotOffset = _baseRect.pivot * _baseRect.sizeDelta;
                return localPoint - (_background.anchorMax * _baseRect.sizeDelta) + pivotOffset;
            }

            return Vector2.zero;
        }

        #endregion
    }
}