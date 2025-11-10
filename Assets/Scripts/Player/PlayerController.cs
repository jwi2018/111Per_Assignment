using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    [Header("★ 플레이어")]
    [SerializeField] Player _player;

    [Header("★ 이동")]
    [SerializeField] Vector2 _moveDirection { get; set; }

    [Header("★ 컴포넌트")]
    [SerializeField] Rigidbody2D _rigidbody2D;

    private PlayerSkillsManager _playerSkillsManager;

    #region Unity 생명주기

    void Awake()
    {
        _playerSkillsManager = GetComponent<PlayerSkillsManager>();
        _player.SetPlayerState(PlayerState.Attack);
    }

    void FixedUpdate()
    {
        Move(_moveDirection);
    }

    #endregion

    #region Input Action 함수

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (_player.State == PlayerState.Skill1Active
        || _player.State == PlayerState.Skill2Active
        || _player.State == PlayerState.Skill3Active
        || _player.State == PlayerState.Skill5Active)
        {
            return;
        }

        _moveDirection = context.ReadValue<Vector2>();

        if (context.performed)
        {
            _player.SetPlayerState(PlayerState.Move);
            FlipSprite(_moveDirection.x);
        }
        else if (context.canceled)
        {
            _player.SetPlayerState(PlayerState.Attack);
            Vector3 scale = transform.localScale;
            if (scale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }
    }

    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (context.performed && _playerSkillsManager != null)
        {
            if (_playerSkillsManager.TryActivateSkill(0))
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;
                FlipSprite(1);
            }
        }
    }

    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.performed && _playerSkillsManager != null)
        {
            if (_player.State == PlayerState.Skill1Active) return;

            if (_playerSkillsManager.TryActivateSkill(1))
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;
                FlipSprite(1);
            }
        }
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed && _playerSkillsManager != null)
        {
            if (_player.State == PlayerState.Skill1Active) return;

            if (_playerSkillsManager.TryActivateSkill(2))
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;
                FlipSprite(1);
            }
        }
    }

    public void OnSkill4(InputAction.CallbackContext context)
    {
        if (context.performed && _playerSkillsManager != null)
        {
            if (_player.State == PlayerState.Skill1Active) return;

            _playerSkillsManager.TryActivateSkill(3);
        }
    }

    public void OnSkill5(InputAction.CallbackContext context)
    {
        if (context.performed && _playerSkillsManager != null)
        {
            if (_player.State == PlayerState.Skill1Active) return;

            if (_playerSkillsManager.TryActivateSkill(4))
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;
                FlipSprite(1);
            }
        }
    }

    #endregion

    #region Private 함수 (이동, 스프라이트 뒤집기)

    private void Move(Vector2 currentMoveDirection)
    {
        _rigidbody2D.linearVelocity = currentMoveDirection * _player.MoveSpeed;
    }

    private void FlipSprite(float moveX)
    {
        if (moveX == 0) return;

        Vector3 currentScale = transform.localScale;
        if (moveX > 0 && currentScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (moveX < 0 && currentScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    #endregion
}