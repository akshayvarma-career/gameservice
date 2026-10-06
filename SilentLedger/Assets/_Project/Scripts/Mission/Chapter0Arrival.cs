using System;
using System.Collections;
using SilentLedger.Player;
using SilentLedger.UI;
using SilentLedger.Weapons;
using UnityEngine;

namespace SilentLedger.Mission
{
    /// <summary>
    /// Chapter 0 · 0.1 Arrival. Tony lands at Site Hollow and Dev shows him around: Bishop's sniper
    /// range (aim down sights), Okafor's armory (loadout, reload, swap), Tamsin's ops room (objective
    /// marker) and Varga's briefing. Controls appear as they are taught.
    /// </summary>
    public class Chapter0Arrival : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] PlayerIntent intent;
        [SerializeField] WeaponHolder weapons;
        [SerializeField] TouchHud touchHud;
        [SerializeField] MissionHud hud;
        [SerializeField] Transform player;

        [Header("Squad")]
        [SerializeField] SquadMember tony;
        [SerializeField] SquadMember bishop;
        [SerializeField] SquadMember okafor;
        [SerializeField] SquadMember tamsin;
        [SerializeField] SquadMember varga;

        [Header("Places")]
        [SerializeField] MissionInteractable loadoutBench;
        [SerializeField] Target[] rangeTargets;
        [SerializeField] Transform killHouseEntrance;

        const int RangeHitsNeeded = 3;
        static readonly Color DevColor = new Color(0.95f, 0.95f, 0.95f);

        int rangeHits;
        bool hintedAim;

        IEnumerator Start()
        {
            foreach (TouchHud.Control control in Enum.GetValues(typeof(TouchHud.Control)))
                touchHud.SetControlVisible(control, false);

            yield return hud.ShowBanner("CHAPTER 0 · FIRST LIGHT", "Site Hollow · 06:12", 3f);
            yield return Arrival();
            yield return SniperRange();
            yield return Armory();
            yield return OpsRoom();
            yield return Briefing();

            hud.SetObjective("Go to the kill house");
            hud.SetWaypoint(killHouseEntrance);
            yield return WaitUntilNear(killHouseEntrance, 4f);
            hud.SetObjective(null);
            hud.SetWaypoint(null);
            yield return hud.ShowBanner("0.1 ARRIVAL COMPLETE", "Next: 0.2 Kill house drill (in development)", 5f);
        }

        IEnumerator Arrival()
        {
            yield return Say(tony, "Whoa. Okay. That was higher than it looked.");
            hud.SetObjective("Meet the new recruit at the helipad");
            hud.SetWaypoint(tony.transform);
            yield return WaitUntilNear(tony.transform, 4f);

            yield return Say(tony, "Corporal Tony Spencer, reporting for duty, sir!");
            yield return Dev("At ease, Spencer. Grab your bag. I'll show you around.");
            yield return Say(tony, "Yes sir. Bag. Got it.");
            tony.FollowTarget = player;
        }

        IEnumerator SniperRange()
        {
            hud.SetObjective("Sniper range: find Bishop");
            yield return TalkTo(bishop);
            yield return Say(bishop, "Three targets. Use your sights.");

            touchHud.SetControlVisible(TouchHud.Control.Fire, true);
            touchHud.SetControlVisible(TouchHud.Control.Aim, true);
            hud.SetWaypoint(null);
            UpdateRangeObjective();
            Target.AnyDowned += OnRangeTargetDowned;
            yield return new WaitUntil(() => rangeHits >= RangeHitsNeeded);
            Target.AnyDowned -= OnRangeTargetDowned;

            yield return Say(bishop, "Two out of three. The rookie will do worse.");
            yield return Say(tony, "Hey! ...Yeah, okay, fair.");
        }

        void OnRangeTargetDowned(Target target)
        {
            if (Array.IndexOf(rangeTargets, target) < 0) return;
            if (!intent.Aiming)
            {
                if (!hintedAim) StartCoroutine(Say(bishop, "Sights. Tap AIM first."));
                hintedAim = true;
                return;
            }
            rangeHits++;
            UpdateRangeObjective();
        }

        void UpdateRangeObjective() =>
            hud.SetObjective($"Hit {RangeHitsNeeded} targets while aiming down sights ({rangeHits}/{RangeHitsNeeded})");

        IEnumerator Armory()
        {
            hud.SetObjective("Armory: see Okafor");
            yield return TalkTo(okafor);
            yield return Say(okafor, "So this is the new guy. I hear you talked the whole flight in.");
            yield return Say(tony, "I sent one message! And a few photos.");
            yield return Say(okafor, "A few photos. From now on you're Postcard.");
            yield return Say(tony, "That's... not a great callsign.");
            yield return Say(okafor, "It's the one you've got. Grab a loadout from the bench.");

            hud.SetObjective("Pick up your loadout");
            hud.SetWaypoint(loadoutBench.transform, 1.2f);
            yield return WaitForUse(loadoutBench);
            weapons.UnlockAll();
            weapons.DrainMagazine(8);
            touchHud.SetControlVisible(TouchHud.Control.Reload, true);
            touchHud.SetControlVisible(TouchHud.Control.Swap, true);
            hud.SetWaypoint(null);

            yield return Say(okafor, "Reload. Always reload before you need to.");
            hud.SetObjective("Reload your weapon");
            weapons.DrainMagazine(8); // in case they already reloaded during the line above
            yield return WaitForEvent(h => weapons.Reloaded += h, h => weapons.Reloaded -= h);

            yield return Say(okafor, "Now swap to your sidearm. Fast.");
            hud.SetObjective("Swap weapons");
            yield return WaitForEvent(h => weapons.Swapped += h, h => weapons.Swapped -= h);
            yield return Say(okafor, "Good. Don't drop that one, Postcard.");
        }

        IEnumerator OpsRoom()
        {
            hud.SetObjective("Ops room: see Tamsin");
            yield return TalkTo(tamsin);
            yield return Say(tamsin, "You're the one who sent forty photos of the helicopter.");
            yield return Say(tony, "...Four.");
            yield return Say(tamsin, "That diamond on your screen is your objective marker. Follow it and you won't get lost.");
            yield return Say(tamsin, "The number under it is the distance. In the field I'll put a map in the corner too.");
        }

        IEnumerator Briefing()
        {
            hud.SetObjective("Briefing room: report to Col. Varga");
            yield return TalkTo(varga);
            yield return Say(varga, "Morning drill. The kill house, timed, full squad.");
            yield return Say(varga, "Spencer, try not to shoot anyone who's on our side.");
            yield return Say(tony, "Yes, ma'am.");
            yield return Dev("With me, Spencer.");
        }

        // ---------------------------------------------------------------- helpers

        IEnumerator Say(SquadMember speaker, string line) => hud.Say(speaker.DisplayName, speaker.Color, line);

        IEnumerator Dev(string line) => hud.Say("Dev", DevColor, line);

        IEnumerator TalkTo(SquadMember member)
        {
            hud.SetWaypoint(member.transform);
            yield return WaitForEvent(h => { member.Talkable = true; member.Talked += h; }, h => member.Talked -= h);
            hud.SetWaypoint(null);
        }

        IEnumerator WaitForUse(MissionInteractable target) =>
            WaitForEvent(h => { target.Active = true; target.Used += h; }, h => target.Used -= h);

        static IEnumerator WaitForEvent(Action<Action> subscribe, Action<Action> unsubscribe)
        {
            bool fired = false;
            Action handler = () => fired = true;
            subscribe(handler);
            yield return new WaitUntil(() => fired);
            unsubscribe(handler);
        }

        IEnumerator WaitUntilNear(Transform target, float radius) =>
            new WaitUntil(() =>
            {
                Vector3 offset = target.position - player.position;
                offset.y = 0f;
                return offset.sqrMagnitude <= radius * radius;
            });
    }
}
