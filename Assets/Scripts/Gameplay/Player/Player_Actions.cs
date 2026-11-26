using UnityEngine;
using static Utils;
using static GameConstants;

// Partial class for Player Actions
public partial class Player : MonoBehaviour
{
    PlayerInputActions inputActions;
    public Vector2 moveInput { get; private set; }
    public float rotateCameraInput { get; private set; }

    // Walking limits: steeper ground than this angle is slid down, and a fall this far below the last safe spot returns to it
    const float MaxClimbAngle = 45, SlideSpeed = 6, MaxFallDepth = 20;
    // Rays that find the ground under the feet, and ahead of them from high above
    const float FeetRayStart = 0.5f, FeetRayLength = 1, LookAhead = 0.2f, ProbeHeight = 100;
    const string GroundLayer = "Ground";
    int groundMask;
    bool isSliding;
    Vector3 safePosition;

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

        Vector3 horizontalDirection = (SM.Camera.forward * moveInput.y + SM.Camera.right * moveInput.x).normalized;
        bool onGround = Physics.Raycast(transform.position + Vector3.up * FeetRayStart, Vector3.down, out RaycastHit under, FeetRayLength, groundMask);
        isSliding = onGround && IsTooSteep(under.normal);
        if (onGround && !isSliding && isGrounded) safePosition = transform.position;

        Vector3 velocity = isSliding ? Downhill(under.normal) * SlideSpeed : KeepOnWalkableGround(horizontalDirection * currentSpeed);
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (horizontalDirection.x != 0 || horizontalDirection.z != 0) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(horizontalDirection.x, 0f, horizontalDirection.z)), Time.deltaTime * PlayerRotationSpeed);
    }

    static bool IsTooSteep(Vector3 groundNormal) => Vector3.Angle(groundNormal, Vector3.up) > MaxClimbAngle;

    static Vector3 Downhill(Vector3 groundNormal) => new Vector3(groundNormal.x, 0, groundNormal.z).normalized;

    Vector3 KeepOnWalkableGround(Vector3 velocity)
    {
        if (velocity == Vector3.zero) return velocity;
        Vector3 above = transform.position + velocity.normalized * (capsuleCollider.radius + LookAhead) + Vector3.up * ProbeHeight;
        if (!Physics.Raycast(above, Vector3.down, ProbeHeight * 2, ~0, QueryTriggerInteraction.Ignore)) return Vector3.zero;
        if (!Physics.Raycast(above, Vector3.down, out RaycastHit ahead, ProbeHeight * 2, groundMask) || !IsTooSteep(ahead.normal) || ahead.point.y <= transform.position.y) return velocity;
        Vector3 downhill = Downhill(ahead.normal);
        return velocity - downhill * Mathf.Min(0, Vector3.Dot(velocity, downhill));
    }

    void ReturnIfFallenOut()
    {
        if (transform.position.y > safePosition.y - MaxFallDepth) return;
        transform.position = rb.position = safePosition;
        rb.linearVelocity = Vector3.zero;
    }

    // Drains energy while sprinting, recharges when not; disables sprinting until fully recharged
    void Sprint()
    {
        float previousEnergy = currentEnergy;
        currentEnergy = Mathf.Clamp(isSprinting ? currentEnergy - Time.deltaTime : currentEnergy + Time.deltaTime, 0, maxEnergy);
        canSprint = currentEnergy >= maxEnergy || (canSprint && currentEnergy > 0);
        if (previousEnergy != currentEnergy)
        {
            SM.Hud.SetEnergy(currentEnergy / maxEnergy, canSprint);
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
        SM.PlaySound(audioSource, SoundType.PlayerSpin);
        this.Wait(spinTime, p => p.isSpinning = false);
    }

    // Shoots Shuriken towards nearest enemy in range. Has cooldown
    void Shoot()
    {
        float previousThrowCD = throwCD;
        throwCD = Mathf.Min(throwCD + Time.deltaTime, throwReady);
        if (throwCD != previousThrowCD) SM.Hud.SetThrow(throwCD / throwReady);
        if (!inputActions.Player.Shoot.WasPressedThisFrame() || isSpinning || isThrowing || throwCD < throwReady) return;
        throwCD = 0;
        SM.Hud.SetThrow(0);
        isThrowing = true;
        clipAnimator.Play(throwClip, ThrowSpeed);
        SM.PlaySound(audioSource, SoundType.ShurikenShoot);
        FindAndShootNearestEnemy();
        this.Wait(0.6f, p => p.isThrowing = false);
    }

    void Jump(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        if (!IsAlive || !isGrounded) return;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, PlayerJumpStrength, rb.linearVelocity.z);
        SM.PlaySound(audioSource, SoundType.PlayerJump);
    }

    void ToggleHud(UnityEngine.InputSystem.InputAction.CallbackContext _) => SM.ToggleHud();
}