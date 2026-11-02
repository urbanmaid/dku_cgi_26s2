using System.Collections;
using System.Linq;
using CityCourier;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class CourierPlayModeTests
{
    CourierStage stage;
    CourierMotor motor;
    const float Step = 1f / 60;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("SampleScene");
        yield return null;
        stage = Object.FindFirstObjectByType<CourierStage>();
        motor = stage.Player;
        motor.AutomaticInput = false;
        stage.EnterStage();
        yield return null;
    }

    void Tick(CourierInput input)
    {
        Physics.SyncTransforms();
        motor.Simulate(input, Step);
    }

    IEnumerator Settle(Vector3 position)
    {
        motor.Teleport(position);
        for (int i = 0; i < 15; i++) { Tick(default); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.Grounded, Is.True, "Test start should be grounded.");
    }

    [UnityTest]
    public IEnumerator DeliveryRequiresUniqueLettersAndOpenedRoad()
    {
        Assert.That(stage.TryComplete(), Is.False);
        for (int i = 0; i < 5; i++) Assert.That(stage.Collect(i), Is.True);
        Assert.That(stage.Collect(0), Is.False);
        Assert.That(stage.LetterCount, Is.EqualTo(5));
        Assert.That(stage.TryComplete(), Is.False);
        stage.OpenRoad();
        Assert.That(stage.Road.activeSelf, Is.True);
        Assert.That(stage.RoadBarrier.activeSelf, Is.False);
        Assert.That(stage.TryComplete(), Is.True);
        yield return null;
    }

    [UnityTest]
    public IEnumerator BasicJumpClearsHurdleThatBlocksRunning()
    {
        yield return Settle(new Vector3(0, 0.1f, 9));
        for (int i = 0; i < 60; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.transform.position.z, Is.LessThan(11.4f));
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        for (int i = 0; i < 75; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.transform.position.z, Is.GreaterThan(14));
    }

    [UnityTest]
    public IEnumerator SlideFitsTunnelAndCannotStandInside()
    {
        yield return Settle(new Vector3(0, 0.1f, 52));
        for (int i = 0; i < 35; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.transform.position.z, Is.LessThan(54));
        Tick(new CourierInput { Move = Vector2.up, Slide = true });
        for (int i = 0; i < 19; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.CanStand(), Is.False);
        Assert.That(motor.IsSliding, Is.True);
        for (int i = 0; i < 45; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.transform.position.z, Is.GreaterThan(61));
        Assert.That(motor.IsSliding, Is.False);
    }

    [UnityTest]
    public IEnumerator SlideStartsWithBurstAndEasesToLowerSpeed()
    {
        yield return Settle(new Vector3(0, 0.1f, 115));
        Tick(new CourierInput { Move = Vector2.up, Slide = true, SlideHeld = true });
        float initialSpeed = new Vector2(motor.Velocity.x, motor.Velocity.z).magnitude;
        for (int i = 0; i < 35; i++)
        {
            Tick(new CourierInput { Move = Vector2.up, SlideHeld = true });
            yield return new WaitForFixedUpdate();
        }
        float easedSpeed = new Vector2(motor.Velocity.x, motor.Velocity.z).magnitude;
        Assert.That(initialSpeed, Is.GreaterThan(16f));
        Assert.That(easedSpeed, Is.LessThan(initialSpeed - 4f));
        Assert.That(easedSpeed, Is.GreaterThanOrEqualTo(motor.SlideEndSpeed - 0.1f));
    }

    [UnityTest]
    public IEnumerator SlideSpamWhileSlidingDoesNotResetBurstInfiniteBoost()
    {
        yield return Settle(new Vector3(0, 0.1f, 115));
        Tick(new CourierInput { Move = Vector2.up, Slide = true, SlideHeld = true });
        for (int i = 0; i < 30; i++)
        {
            Tick(new CourierInput { Move = Vector2.up, Slide = true, SlideHeld = true });
            yield return new WaitForFixedUpdate();
        }
        float speedAfterSpam = motor.HorizontalSpeed;
        Assert.That(speedAfterSpam, Is.LessThan(12f), "Spamming slide mid-slide must not reset burst to 17 m/s.");
    }

    [UnityTest]
    public IEnumerator JumpDuringSlideKeepsForwardMomentum()
    {
        yield return Settle(new Vector3(0, 0.1f, 115));
        Tick(new CourierInput { Move = Vector2.up, Slide = true, SlideHeld = true });
        for (int i = 0; i < 8; i++)
        {
            Tick(new CourierInput { Move = Vector2.up, SlideHeld = true });
            yield return new WaitForFixedUpdate();
        }
        float speedBeforeJump = new Vector2(motor.Velocity.x, motor.Velocity.z).magnitude;
        Tick(new CourierInput { Move = Vector2.up, Jump = true, SlideHeld = true });
        float speedAfterJump = new Vector2(motor.Velocity.x, motor.Velocity.z).magnitude;
        Assert.That(motor.IsSliding, Is.False);
        Assert.That(motor.Velocity.y, Is.GreaterThan(8f));
        Assert.That(speedAfterJump, Is.GreaterThanOrEqualTo(speedBeforeJump - 0.1f));
    }

    [UnityTest]
    public IEnumerator KeyboardTurnChangesHeadingGradually()
    {
        yield return Settle(new Vector3(0, 0.1f, 115));
        Tick(new CourierInput { Move = Vector2.right });
        Assert.That(Vector3.Dot(motor.Heading, Vector3.right), Is.GreaterThan(0f));
        Assert.That(Vector3.Dot(motor.Heading, Vector3.right), Is.LessThan(0.95f));
        for (int i = 0; i < 30; i++)
        {
            Tick(new CourierInput { Move = Vector2.right });
            yield return new WaitForFixedUpdate();
        }
        Assert.That(Vector3.Dot(motor.Heading, Vector3.right), Is.GreaterThan(0.99f));
    }

    [UnityTest]
    public IEnumerator CameraRecenterMovesSmoothlyBehindHeading()
    {
        CourierCamera view = motor.View;
        view.ApplyLook(new Vector2(90f, 0f), false, Vector3.forward);
        Assert.That(view.Yaw, Is.EqualTo(90f).Within(0.01f));
        view.ApplyLook(Vector2.zero, true, Vector3.forward);
        Assert.That(view.Yaw, Is.EqualTo(90f).Within(0.01f), "Recenter must not snap on input.");
        yield return null;
        Assert.That(view.Yaw, Is.LessThan(90f));
        Assert.That(view.Yaw, Is.GreaterThan(0f));
        yield return new WaitForSecondsRealtime(1f);
        Assert.That(Mathf.Abs(Mathf.DeltaAngle(view.Yaw, 0f)), Is.LessThan(1f));
    }

    [UnityTest]
    public IEnumerator DynamicFovExpandsDuringHighSpeedSlideAndRecovers()
    {
        yield return Settle(new Vector3(0, 0.1f, 115));
        CourierCamera view = motor.View;
        Assert.That(view.CurrentFov, Is.EqualTo(view.BaseFov).Within(0.5f));
        Tick(new CourierInput { Move = Vector2.up, Slide = true, SlideHeld = true });
        for (int i = 0; i < 20; i++)
        {
            Tick(new CourierInput { Move = Vector2.up, SlideHeld = true });
            yield return new WaitForFixedUpdate();
        }
        Assert.That(view.CurrentFov, Is.GreaterThan(view.BaseFov + 4.5f));
        for (int i = 0; i < 65; i++)
        {
            Tick(default);
            yield return new WaitForFixedUpdate();
        }
        Assert.That(view.CurrentFov, Is.LessThan(view.BaseFov + 1.5f));
    }

    [UnityTest]
    public IEnumerator CameraDoesNotTiltOnGroundRunningAndTiltsGentlyOnWallRun()
    {
        yield return Settle(new Vector3(0, 0.1f, 115));
        CourierCamera view = motor.View;
        for (int i = 0; i < 15; i++)
        {
            Tick(new CourierInput { Move = new Vector2(1f, 1f).normalized });
            yield return new WaitForFixedUpdate();
        }
        Assert.That(Mathf.Abs(view.CurrentRoll), Is.LessThan(0.1f), "Ground running must not rock/tilt camera.");

        yield return Settle(new Vector3(2, 0.1f, 22));
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        for (int i = 0; i < 18; i++)
        {
            Tick(new CourierInput { Move = Vector2.up });
            yield return new WaitForFixedUpdate();
        }
        Assert.That(motor.IsWallRunning, Is.True);
        Assert.That(Mathf.Abs(view.CurrentRoll), Is.GreaterThan(2.5f));
        Assert.That(Mathf.Abs(view.CurrentRoll), Is.LessThanOrEqualTo(view.WallRunTiltAngle + 0.5f));
    }

    [UnityTest]
    public IEnumerator LandingImpactEasesSmoothlyWithoutInstantSnap()
    {
        yield return Settle(new Vector3(0, 0.1f, 115));
        CourierCamera view = motor.View;
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        for (int i = 0; i < 5; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Tick(new CourierInput { Move = Vector2.up, Slide = true, SlideHeld = true });
        for (int i = 0; i < 20 && !motor.Grounded; i++)
        {
            Tick(new CourierInput { Move = Vector2.up, SlideHeld = true });
            yield return new WaitForFixedUpdate();
        }
        Assert.That(motor.Grounded, Is.True);
        Assert.That(motor.LastLandingImpact, Is.GreaterThan(10f));
        // On the exact landing frame, pitch offset must not snap to a huge angle
        Assert.That(view.LandingPitchOffset, Is.LessThan(1.5f));
        // Over the next few frames, it smoothly eases in
        for (int i = 0; i < 4; i++) { Tick(default); yield return new WaitForFixedUpdate(); }
        Assert.That(view.LandingPitchOffset, Is.GreaterThan(0.2f));
    }

    [UnityTest]
    public IEnumerator CyberpunkAudioPlaysBgmAndParkourActionSfx()
    {
        var audio = stage.GetComponent<CourierAudio>();
        if (audio == null) audio = stage.gameObject.AddComponent<CourierAudio>();
        audio.Stage = stage;
        audio.Motor = motor;
        audio.StartBgm();
        Assert.That(audio.IsBgmPlaying, Is.True);

        yield return Settle(new Vector3(0, 0.1f, 115));
        int beforeCount = audio.SfxPlayCount;
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        Assert.That(audio.SfxPlayCount, Is.GreaterThan(beforeCount));
        Assert.That(audio.LastPlayedSfx, Is.EqualTo("Jump"));

        for (int i = 0; i < 4; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Tick(new CourierInput { Move = Vector2.up, Slide = true });
        Assert.That(audio.LastPlayedSfx, Is.EqualTo("Dive"));
    }

    [UnityTest]
    public IEnumerator CyberpunkCityPropsAreBuiltWithZeroColliders()
    {
        var city = stage.GetComponent<CourierCityProps>();
        Assert.That(city, Is.Not.Null);
        Transform container = stage.transform.Find("Cyberpunk City Props");
        Assert.That(container, Is.Not.Null);
        Assert.That(container.GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(80));
        Assert.That(container.GetComponentsInChildren<Collider>().Length, Is.Zero);
        yield return null;
    }

    [UnityTest]
    public IEnumerator WallRunCrossesLongGap()
    {
        yield return Settle(new Vector3(2, 0.1f, 22));
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        bool ran = false;
        for (int i = 0; i < 145 && motor.transform.position.z < 46; i++)
        {
            Tick(new CourierInput { Move = Vector2.up });
            ran |= motor.IsWallRunning;
            yield return new WaitForFixedUpdate();
        }
        Assert.That(ran, Is.True);
        Assert.That(stage.DamageCount, Is.Zero);
        Assert.That(motor.transform.position.z, Is.GreaterThan(45));
        Assert.That(motor.transform.position.y, Is.GreaterThan(-0.05f));
    }

    [UnityTest]
    public IEnumerator WallJumpPushesAwayFromWall()
    {
        yield return Settle(new Vector3(2, 0.1f, 22));
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        for (int i = 0; i < 8; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.IsWallRunning, Is.True);
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        Assert.That(motor.Velocity.x, Is.LessThan(-5));
        Assert.That(motor.Velocity.y, Is.GreaterThan(8));
    }

    [UnityTest]
    public IEnumerator SlideJumpCrossesGapAfterTunnel()
    {
        yield return Settle(new Vector3(0, 0.1f, 70));
        Tick(new CourierInput { Move = Vector2.up, Slide = true });
        for (int i = 0; i < 60 && motor.transform.position.z < 78.6f; i++)
        { Tick(new CourierInput { Move = Vector2.up, SlideHeld = true }); yield return new WaitForFixedUpdate(); }
        Assert.That(motor.transform.position.z, Is.GreaterThanOrEqualTo(78.6f));
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        Assert.That(motor.Velocity.y, Is.GreaterThan(8), "Jump must fit between tunnel exit and edge.");
        for (int i = 0; i < 65; i++) { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Assert.That(stage.DamageCount, Is.Zero);
        Assert.That(motor.transform.position.z, Is.GreaterThan(84));
    }

    [UnityTest]
    public IEnumerator JumpDiveClearsOverheadHazard()
    {
        yield return Settle(new Vector3(0, 0.1f, 95));
        for (int i = 0; i < 35 && motor.transform.position.z < 97.6f; i++)
        { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        for (int i = 0; i < 40 && motor.transform.position.z < 102.7f; i++)
        { Tick(new CourierInput { Move = Vector2.up }); yield return new WaitForFixedUpdate(); }
        Tick(new CourierInput { Move = Vector2.up, Slide = true, SlideHeld = true });
        for (int i = 0; i < 60; i++)
        { Tick(new CourierInput { Move = Vector2.up, SlideHeld = true }); yield return new WaitForFixedUpdate(); }
        Assert.That(stage.DamageCount, Is.Zero);
        Assert.That(motor.transform.position.z, Is.GreaterThan(113.2f));
    }

    [UnityTest]
    public IEnumerator HazardRespawnsAndPreservesLetters()
    {
        stage.Collect(0);
        stage.SetCheckpoint(new Vector3(0, 0.1f, 90));
        motor.Teleport(new Vector3(0, 1.2f, 109));
        for (int i = 0; i < 5; i++) { Tick(default); yield return new WaitForFixedUpdate(); }
        Assert.That(stage.DamageCount, Is.EqualTo(1));
        Assert.That(motor.transform.position.z, Is.EqualTo(90).Within(0.1));
        Assert.That(stage.LetterCount, Is.EqualTo(1));
        Assert.That(motor.IsDiving || motor.IsSliding || motor.IsWallRunning, Is.False);
    }

    [UnityTest]
    public IEnumerator RoadRequiresNearbyInteraction()
    {
        Assert.That(stage.RoadOpen, Is.False);
        stage.Interact(motor);
        Assert.That(stage.RoadOpen, Is.False);
        motor.Teleport(new Vector3(-1.8f, 0.1f, 122));
        stage.Interact(motor);
        Assert.That(stage.RoadOpen, Is.True);
        Assert.That(Object.FindObjectsByType<CourierTrigger>(FindObjectsSortMode.None)
            .Count(x => x.Kind == CourierTrigger.TriggerKind.Letter), Is.EqualTo(5));
        yield return null;
    }

    IEnumerator ForwardTo(float z, bool slide = false)
    {
        for (int i = 0; i < 600 && motor.transform.position.z < z; i++)
        {
            Tick(new CourierInput { Move = Vector2.up, SlideHeld = slide });
            yield return new WaitForFixedUpdate();
        }
        Assert.That(motor.transform.position.z, Is.GreaterThanOrEqualTo(z), "Route stalled before " + z);
        Assert.That(stage.DamageCount, Is.Zero, "Route damaged before " + z);
    }

    [UnityTest]
    public IEnumerator EntireCourseCanBeCompletedWithoutTeleporting()
    {
        yield return ForwardTo(9.8f);
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        yield return ForwardTo(19);
        for (int i = 0; i < 40 && motor.transform.position.x < 1.95f; i++)
        { Tick(new CourierInput { Move = Vector2.right }); yield return new WaitForFixedUpdate(); }
        yield return ForwardTo(22.5f);
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        yield return ForwardTo(50);
        for (int i = 0; i < 40 && motor.transform.position.x > 0.1f; i++)
        { Tick(new CourierInput { Move = Vector2.left }); yield return new WaitForFixedUpdate(); }
        yield return ForwardTo(52);
        Tick(new CourierInput { Move = Vector2.up, Slide = true });
        yield return ForwardTo(63, true);
        yield return ForwardTo(70);
        Tick(new CourierInput { Move = Vector2.up, Slide = true });
        yield return ForwardTo(78.4f, true);
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        yield return ForwardTo(97.6f);
        Tick(new CourierInput { Move = Vector2.up, Jump = true });
        yield return ForwardTo(102.7f);
        Tick(new CourierInput { Move = Vector2.up, Slide = true });
        yield return ForwardTo(115, true);
        yield return ForwardTo(124);
        stage.Interact(motor);
        Assert.That(stage.RoadOpen, Is.True);
        yield return ForwardTo(150);
        Assert.That(stage.LetterCount, Is.EqualTo(5));
        Assert.That(stage.Completed, Is.True);
    }
}
