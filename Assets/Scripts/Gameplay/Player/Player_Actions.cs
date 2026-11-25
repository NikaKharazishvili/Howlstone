using UnityEngine;
using static Utils;
using static GameConstants;

// Partial class for Player Actions
public partial class Player : MonoBehaviour
{
    PlayerInputActions inputActions;
    public Vector2 moveInput { get; private set; }
    public float rotateCameraInput { get; private set; }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Jump.performed += Jump;
        inputActions.Player.ToggleHud.performed += ToggleHud;
    }
    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Jump.performed -= Jump;
        inputActions.Player.ToggleHud.performed -= ToggleHud;
    }

    void Move()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        rotateCameraInput = inputActions.Player.RotateCamera.ReadValue<float>();

        Vector3 horizontalDirection = (GM.Camera.forward * moveInput.y + GM.Camera.right * moveInput.x).normalized;
        Vector3 velocity = horizontalDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (horizontalDirection.x != 0 || horizontalDirection.z != 0) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(horizontalDirection.x, 0f, horizontalDirection.z)), Time.deltaTime * PlayerRotationSpeed);
    }

    // Drains energy while sprinting, recharges when not; disables sprinting until fully recharged
    void Sprint()
    {
        float previousEnergy = currentEnergy;
        currentEnergy = Mathf.Clamp(isSprinting ? currentEnergy - Time.deltaTime : currentEnergy + Time.deltaTime, 0, maxEnergy);
        canSprint = currentEnergy >= maxEnergy || (canSprint && currentEnergy > 0);
        if (previousEnergy != currentEnergy)
        {
            energyFill.fillAmount = currentEnergy / maxEnergy;
            energyFill.color = canSprint ? Color.white : Color.grey;
        }
    }

    // Spins Player, damaging nearby enemies. Has cooldown
    void Spin()
    {
        spinCD = Mathf.Min(spinCD + Time.deltaTime, spinReady);
        if (!inputActions.Player.Spin.WasPressedThisFrame() || isThrowing || isSpinning || spinCD < spinReady) return;
        spinCD = 0;
        isSpinning = true;
        if (Chance(50)) clipAnimator.Play(spin1Clip); else clipAnimator.Play(spin2Clip, Spin2Speed);
        GM.PlaySound(audioSource, SoundType.PlayerSpin);
        this.Wait(spinTime, p => p.isSpinning = false);
    }

    // Shoots Shuriken towards nearest enemy in range. Has cooldown
    void Shoot()
    {
        throwCD = Mathf.Min(throwCD + Time.deltaTime, throwReady);
        if (!inputActions.Player.Shoot.WasPressedThisFrame() || isSpinning || isThrowing || throwCD < throwReady) return;
        throwCD = 0;
        isThrowing = true;
        clipAnimator.Play(throwClip, ThrowSpeed);
        GM.PlaySound(audioSource, SoundType.ShurikenShoot);
        FindAndShootNearestEnemy();
        this.Wait(0.6f, p => p.isThrowing = false);
    }

    void Jump(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        if (!IsAlive || !isGrounded) return;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, PlayerJumpStrength, rb.linearVelocity.z);
        GM.PlaySound(audioSource, SoundType.PlayerJump);
    }

    void ToggleHud(UnityEngine.InputSystem.InputAction.CallbackContext _) => GM.ToggleHud();
}