# Silent Ledger: Campaign Design

2026-10-06 · Akshay Varma

Source: https://claude.ai/code/artifact/caa2819b-f513-455b-8b90-2d7c6315b3d9 (snapshot of rev 40)

## Overview

Silent Ledger is a story-driven, first-person military shooter for phones: 13 chapters, about 20 hours, set in 2034.

- **Premise:** Ashgrove Strategic, a private military company, has spent 15 years placing its people inside the defense ministries, intelligence agencies and arms suppliers of nearly every nation. It arms both sides of the wars it starts, because every war means more contracts.
- **Endgame:** Project Tessellate, a single AI security network that every government is pressured into adopting. Once it is live, Ashgrove controls the world's militaries without firing a shot.
- **The player:** Capt. Dev Rao of Unit 9, callsign LANTERN, a black-ops squad that officially does not exist.
- **The ending:** LANTERN shuts Tessellate down and the wars stop, but no record of what they did survives. Only the player knows.
- **Platform:** Android and iOS phones, landscape, touch controls. Engine to be decided once the story and design are settled.

## Characters

Three of LANTERN's six members are alive at the end: Dev, Tamsin and Bishop.

| Character | Role | Personality | Fate |
| --- | --- | --- | --- |
| Capt. Dev Rao | Squad lead, point man, player character | Calm, careful, carries the weight of every order | Survives |
| Col. Elena Varga | Commander, gives the orders, secretly WARDEN | Cold, precise, trusted by everyone | Revealed as Ashgrove's agent in Ch 7, killed by Rourke in Ch 9 |
| "Bishop" | Sniper | Says little, the squad's quiet anchor | Holds the line alone in Ch 11, presumed dead, walks out of the smoke in Ch 12 |
| Tamsin Reyes | Comms and hacking | Sharp, sarcastic, first to spot the money trail | Survives |
| Okafor | Heavy weapons | Loud, warm, the squad's big brother | Killed holding the spillway bridge in Ch 7 |
| Cpl. Tony Spencer | Rookie | 23, eager, talks too much on comms, sharper than he looks, in awe of Dev | Killed in Ch 10 shielding Dev from Rourke's bullet |
| Elias Kade | Founder of Ashgrove Strategic, main villain | Calm, charming, believes he is ending war | Arrested quietly in Ch 12 |
| Silas Rourke | Ashgrove's head of field operations | Patient, cruel, a marksman | Introduced in Ch 3, killed by Dev in Ch 11 |

## Tony Spencer's arc

The squad starts out annoyed by Tony and ends up treating him as family. His death in Ch 10 gives them the focus to finish the war.

| Chapter | Moment |
| --- | --- |
| 0 · First Light | Everyone is annoyed with him. Okafor nicknames him "Postcard" because he talks too much. He freezes in his first firefight, then saves Okafor. |
| 2 · Paper Trail | His quick thinking saves a stealth mission. Bishop gives him a nod. |
| 4 · Glass House | He and Tamsin pull off the hack together, and she starts teaching him. |
| 6 · Burned | On the run, he shares his rations and his letters home. The squad opens up to him. |
| 7 · The Ledger | Okafor dies. Tony carries his tags. |
| 8 · Iron Tide | Now a real squad member. Dev trusts him with the flank. |
| 10 · No Witnesses | During the city evacuation, Tony sees a sniper's glint and shoves Dev aside, taking the bullet. It plays in first person, and the player loses control for a few seconds. His last radio line is the same silly sign-off he used in Ch 0. |
| 11–12 | The squad's grief turns into focus. Finishing the war becomes about what Tony died for. |

## Campaign structure

The campaign opens with a 45-minute prologue followed by three acts. The tone moves from careful stealth to gritty, serious warfare.

- **Prologue (Ch 0):** introduces the squad and teaches the controls.
- **Act I · Shadows (Ch 1–4):** tactical stealth, recon, night infiltration, small firefights.
- **Act II · Unraveling (Ch 5–8):** the wars escalate, LANTERN is disavowed and betrayed, and Okafor dies.
- **Act III · The Unseen War (Ch 9–12):** gritty and serious, with heroism nobody ever hears about.

| Ch | Title | Act | Length | What happens |
| --- | --- | --- | --- | --- |
| 0 | First Light | Prologue | ~45 min | Tony arrives, the squad is introduced, a convoy ambush, the first Ashgrove crate |
| 1 | Quiet Water | I | ~1.5 h | Night infiltration of the Port of Kessa. The same supplier's markings turn up on both sides' weapons |
| 2 | Paper Trail | I | ~1.5 h | Stealth break-in at a bank in a snowbound city to steal ledgers |
| 3 | Two Flags | I | ~1.5 h | Jungle border war. Ashgrove advisors are training both armies |
| 4 | Glass House | I | ~1.5 h | Embassy gala infiltration. The name Tessellate surfaces |
| 5 | Wildfire | II | ~1.5 h | Ashgrove stages an attack and pins it on LANTERN's country. The squad is disavowed |
| 6 | Burned | II | ~1.5 h | On the run with no support. Survival |
| 7 | The Ledger | II | ~2 h | Varga's betrayal is revealed. Okafor dies covering the escape |
| 8 | Iron Tide | II | ~2 h | Full-scale war between two nations armed by Ashgrove |
| 9 | Cold Server | III | ~1.5 h | Arctic raid on Tessellate's data core. Rourke kills Varga |
| 10 | No Witnesses | III | ~2 h | A city falls and civilians are evacuated. Tony dies shielding Dev |
| 11 | Black Ledger | III | ~2 h | Assault on Ashgrove headquarters. Bishop holds the stairwell alone |
| 12 | Silent | III | ~2 h | Tessellate goes dark, the wars stop, LANTERN is erased from the record. Bishop walks out alive |

Total: about 21 hours.

## Chapter 0 · First Light

A 45-minute prologue. It introduces every squad member, teaches every control, and ends with the squad holding the first piece of evidence about Ashgrove.

**Setting:** Site Hollow, LANTERN's hidden base in a decommissioned mountain airbase, at dawn. Then Kestrel Pass, a narrow mountain road 20 km away.

**Tony's catchphrase:** he signs off every radio call with "Spencer out. Wish you were here." It's why Okafor calls him "Postcard." The same line is his last words in Ch 10.

### 0.1 Arrival (~8 min)

- **Objective:** meet the new recruit and show him around the base.
- **Story:** a helicopter lands at dawn. Tony jumps out too early and drops his bag. Dev walks him through the base, and the player meets each squad member at their station.
- **Stops on the tour:**
    1. **Sniper range:** Bishop doesn't look up. The player hits three targets while aiming down sights. Bishop: "Two out of three. The rookie will do worse."
    2. **Armory:** Okafor teases Tony and coins "Postcard." The player picks a loadout, then reloads and swaps weapons.
    3. **Ops room:** Tamsin is surrounded by screens. She explains the map, the objective markers and the minimap.
    4. **Briefing room:** Col. Varga sets the morning drill. "Spencer, try not to shoot anyone who's on our side."
- **Controls taught:** move, look, aim down sights, fire, reload, swap weapon, interact.

### 0.2 Kill house drill (~15 min)

- **Objective:** clear the timed training course with the squad.
- **Story:** Tony makes small mistakes. He shoots a civilian target and breaches the wrong door. The squad ribs him over comms, and Dev quietly gives him one useful tip.
- **Sections:** crouch and sprint run, silent takedown on two dummies, breach and clear with a flashbang, and a final room with mixed enemy and civilian targets.
- **Controls taught:** crouch, sprint, melee takedown, breach, throw grenade or flashbang.
- **Replay hook:** the course gives a time and a grade, and the player can rerun it from the menu.

### 0.3 The live call (~20 min)

- **Objective:** reach the ambushed convoy at Kestrel Pass and get the survivors out.
- **Story:** an alarm cuts the drill short. A supply convoy has been hit in the pass and the squad rolls out with Tony, untested.
- **Beats:**
    1. **Insertion:** fast-rope from the helicopter onto the ridge, a short scripted sequence.
    2. **Descent:** move down the ravine as Bishop calls out targets from overwatch. Teaches cover and marking targets.
    3. **The convoy:** burning trucks, survivors pinned down. Hold position until the medevac arrives against two waves of well-equipped mercenaries with no insignia.
    4. **Tony freezes:** Okafor is pinned and Tony locks up. Dev shouts his name. Tony snaps out of it and drops the mercenary flanking Okafor. Okafor: "Not bad, Postcard."
    5. **The crate:** after the fight, the player examines a weapons crate. Tamsin scans a logo nobody recognises: Ashgrove Strategic.
- **Controls taught:** use cover, mark targets, grenades under pressure.

### 0.4 Debrief (~2 min, cutscene)

- Back at Site Hollow, Varga tells the squad the crate and the ambush never happened.
- Dev keeps a photo of the logo on his phone.
- The last shot: Tony on the radio, "Spencer out. Wish you were here." Fade to the title, then to Ch 1.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Helipad | Landing pad, hangar doors | 0.1 |
| Sniper range | 3 lanes, targets at 100 / 200 / 400 m | 0.1 |
| Armory | Weapon racks, loadout bench | 0.1 |
| Ops room | Wall of screens, map table | 0.1, 0.4 |
| Kill house | Two-storey plywood building, 6 rooms, timed start/finish gates | 0.2 |
| Kestrel Pass ridge | Rocks, a broken radio mast, a sniper perch for Bishop | 0.3 |
| Kestrel Pass ravine | Switchback road, 4 burning trucks, the Ashgrove crate in the last truck | 0.3 |

## Chapter 1 · Quiet Water

A 90-minute night stealth chapter that goes loud at the end. The squad proves Ashgrove arms both sides of a war, and learns someone tipped Ashgrove off.

**Setting:** the Port of Kessa, a container port on a fictional coast, three weeks after Ch 0. Rain, floodlights, cranes.

**Setup:** Tamsin traced the serial number on the Kestrel Pass crate to a shipment leaving Kessa tonight. Varga approves the mission as "recon only."

**Squad:** Dev (player) and Tony on the ground, Bishop on a crane for overwatch, Okafor waiting with the boat, Tamsin on comms from base.

### 1.1 Harbor approach (~15 min)

- **Objective:** get into the port unseen.
- **Story:** a rubber boat slips in under the pier. Tony fills the silence with whispers until Dev tells him to keep the channel clear. Bishop climbs to his crane.
- **Controls taught:** the visibility meter (shadows hide you), the detection meter on enemies, suppressed weapons, throwing a distraction.

### 1.2 The yard (~25 min)

- **Objective:** reach the shipping office and find the container number on the manifest terminal.
- **Gameplay:** patrols with flashlights, a guard dog, and security cameras that Tamsin can loop for 20 seconds when the player taps a junction box.
- **Tony moment:** Tony steps into a flashlight beam and Dev pulls him back just in time. Afterwards he goes quiet for the first time.
- **Controls taught:** hacking a camera, silent takedowns in the field.

### 1.3 The container (~15 min)

- **Objective:** get inside container KX-4471, photograph the cargo and plant trackers.
- **Story:** the weapons are split into two lots, one stamped for the Republic of Davar, the other for the Sarn Liberation Front. The two sides are at war, and both lots carry Ashgrove serials.
- **Gameplay:** plant a tracker on each lot (hold to interact) while a patrol walks back toward the open door.

### 1.4 Going loud (~25 min)

- **Story:** trucks of Ashgrove private security arrive, with night vision and better gear than the dockside guards. Tamsin: "They're not reacting to an alarm. They knew we'd be here." This is the first hint of the leak that leads to Varga.
- **Gameplay:** a running firefight back through the container stacks, built on the browser prototype's combat. Set piece: Bishop releases a crane load to drop a container in front of an armored truck.

### 1.5 Extraction (~10 min)

- **Objective:** escape the harbor by boat.
- **Gameplay:** Okafor drives and the player mans the boat's mounted gun against pursuing patrol boats.
- **Ending:** the two trackers ping in opposite directions, one east to Davar, one south to the Sarn front. Varga on the radio: "Recon only, Captain. You'll explain the rest at debrief."

### Replay and collectibles

- **Ghost rating** for reaching the container without being detected.
- **3 intel files** hidden in the yard. These add backstory on Ashgrove and extra hours for players who explore.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Pier | Boat entry point, maintenance ladders | 1.1, 1.5 |
| Crane row | 3 gantry cranes, Bishop's perch on the middle one | 1.1, 1.4 |
| Container stacks | Maze of stacked containers, 2 to 3 high, patrol routes | 1.2, 1.4 |
| Shipping office | Two floors, manifest terminal, camera junction box | 1.2 |
| Container KX-4471 | Two marked weapon lots, one door | 1.3 |
| Truck gate | Where the Ashgrove trucks arrive, the crane drop set piece | 1.4 |
| Harbor channel | Open water for the boat chase | 1.5 |

## Chapter 2 · Paper Trail

A 90-minute stealth heist in a blizzard. The squad steals Ashgrove's secret payment ledger, Tony earns Bishop's respect, and the codename WARDEN appears for the first time.

**Setting:** Valdren, a snowbound financial city in the mountains, two weeks after Ch 1. Night, heavy snow.

**Setup:** the Kessa trackers led to shell companies that all bank at Corvin & Hale, a private bank. Tamsin believes Ashgrove keeps an offline record of every bribe, the "black ledger," on an air-gapped server in its vault.

**Squad:** Dev and Tony inside, Bishop on a hotel roof across the street, Tamsin on comms from a parked van, Okafor driving the getaway.

### 2.1 Snowfall (~15 min)

- **Objective:** scout the bank from the hotel roof.
- **Gameplay:** the player flies a palm-sized recon drone around the building, tagging guards, cameras and the roof access. Bishop confirms patrol timings.
- **Controls taught:** flying the recon drone, tagging from a distance.

### 2.2 Into the bank (~25 min)

- **Objective:** reach the vault level after hours.
- **Gameplay:** zipline from the hotel to the bank roof, then through service ducts, past laser grids and timed guard rotations. Pickpocket a keycard from a guard on his rounds.
- **Controls taught:** zipline, pickpocketing, timing around laser grids.

### 2.3 The vault (~20 min)

- **Objective:** copy the ledger from the air-gapped server.
- **Problem:** the vault door needs the night manager's thumbprint, and he's still in his office upstairs.
- **Tony moment:** Tony notices the manager's coffee cup in the break room and lifts his print from it with tape. It works. Bishop, over the radio: "Good catch, Spencer." It's the first time Bishop has used his name.
- **Gameplay:** Tamsin's hack is a short frequency-matching puzzle on the touch screen while the copy runs and a guard patrol approaches.

### 2.4 Whiteout (~20 min)

- **Story:** the copy finishes and trips a silent alarm. Ashgrove's security team is minutes away.
- **Gameplay:** escape across the rooftops in a blizzard. Visibility is almost zero, so the player switches to a thermal scope to fight. Okafor's van waits two streets over.
- **Controls taught:** thermal scope.

### 2.5 The ledger (~10 min, mostly cutscene)

- At a safehouse, Tamsin decrypts the ledger: payments to officials in 40 countries.
- One recipient is listed only by a codename, WARDEN, paid more than anyone else. WARDEN is Varga, revealed in Ch 7.
- Tony, quietly, to Dev: "Thanks for not leaving me at the port." Dev: "Thank Bishop. He's the one who likes you now."

### Replay and collectibles

- **Ghost rating** for reaching the vault undetected.
- **3 intel files**, including a Corvin & Hale memo that names Ashgrove's founder.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Hotel roof | Bishop's perch, drone launch point, zipline anchor | 2.1, 2.2 |
| Bank roof | Zipline landing, roof access door, ventilation units | 2.2, 2.4 |
| Service ducts | Crawlspaces connecting floors, laser grid sections | 2.2 |
| Banking hall | Marble hall, night guard rotations, cameras | 2.2 |
| Manager's floor | Night manager's office, break room with the coffee cup | 2.3 |
| Vault level | Biometric door, air-gapped server room | 2.3 |
| Rooftops | Snow-covered roofs across three buildings, gaps to jump | 2.4 |

## Chapter 3 · Two Flags

A 90-minute jungle chapter. The squad sees Ashgrove advisors training both armies in the Davar–Sarn war and meets Silas Rourke, the man who will kill Tony in Ch 10.

**Setting:** the Lira river valley on the Davar–Sarn border, a month after Ch 2. Heat, rain, dense jungle, artillery in the distance.

**Setup:** the Kessa trackers stopped at two training camps, one on each side of the border.

**Squad:** all five in the field. Varga on the radio from base.

### 3.1 River insertion (~15 min)

- **Objective:** reach the Davar camp unseen.
- **Gameplay:** wade upriver at night, then move through tall grass in ghillie suits. Prone crawling is introduced.
- **Tony moment:** Okafor teaches Tony to read jungle tracks. Tony gets it wrong twice, then right, and Okafor stops calling him "Postcard" for the rest of the mission.

### 3.2 Camp Davar (~20 min)

- **Objective:** photograph the foreign advisors running the camp.
- **Gameplay:** stealth through a working military camp. Photos are taken with the scope camera.
- **Story:** the advisors' leader is Silas Rourke, Ashgrove's head of field operations. Bishop has a clean shot. Varga orders him to hold fire, which the squad finds strange.

### 3.3 No-man's land (~20 min)

- **Objective:** cross the front line to the Sarn camp.
- **Gameplay:** a tense crossing between two armies during an artillery exchange. Shell craters for cover, flares that light up the field.

### 3.4 Camp Sarn (~15 min)

- **Objective:** confirm Ashgrove is training this side too.
- **Story:** Rourke is here as well, giving the same speech in a different language. Tamsin cross-checks the photos: same faces, both camps.

### 3.5 Burning ground (~20 min)

- **Story:** Ashgrove triggers a clash between the two camps to cover its tracks, and the jungle catches fire.
- **Gameplay:** fight out through burning jungle with both armies firing at everything. Ends with a helicopter pickup at the river.
- **Ending:** Rourke watches the helicopter leave through binoculars and smiles.

**Collectibles:** 3 intel files, including Rourke's service record.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Lira river | Shallow water, reeds, a rope bridge | 3.1, 3.5 |
| Camp Davar | Barracks, firing range, command tent | 3.2 |
| No-man's land | Shell craters, wire, a ruined village | 3.3 |
| Camp Sarn | Bunkers dug into a hillside | 3.4 |
| Burning jungle | Fire spreads along fixed paths during the escape | 3.5 |

## Chapter 4 · Glass House

A 90-minute social-stealth chapter at an embassy gala. The squad learns the name Project Tessellate, sees Ashgrove's founder Elias Kade in person, and quietly saves a minister nobody will ever know was in danger.

**Setting:** the glass-walled Davaran embassy in Merrow, a neutral coastal capital, during a gala for twelve defense ministers.

**Setup:** the ledger shows Kade will pitch something to the ministers tonight. The squad has to hear it.

**Squad:** Dev and Tamsin undercover as guests, Tony as a waiter, Bishop on a rooftop across the bay, Okafor in the service tunnels below.

### 4.1 The guest list (~15 min)

- **Objective:** get through security and into the main hall.
- **Gameplay:** social stealth. Blend into crowds, keep suspicion low, and use conversation to stay close to targets.
- **Controls taught:** the disguise and suspicion meter, blending into groups.

### 4.2 The pitch (~20 min)

- **Objective:** get close enough to record Kade's private presentation.
- **Story:** Kade presents Project Tessellate as "the end of war": one AI network linking every nation's defense systems. Ten of the twelve ministers are already on the ledger.

### 4.3 The data tap (~20 min)

- **Objective:** plant a data tap on Kade's phone.
- **Tony moment:** Tony serves Kade's table while Tamsin talks him through the phone clone in his earpiece. She doesn't make fun of him once. Afterwards: "Not bad. I'll teach you the real version."

### 4.4 The quiet save (~25 min)

- **Story:** one minister, Ilse Moreau, refuses Tessellate. Bishop spots an Ashgrove shooter lining up on her balcony.
- **Gameplay:** Dev slips away to stop the shooter silently, while Bishop covers from across the bay and Okafor kills the power from below. The gala never notices. This is the game's first unsung act of heroism.

### 4.5 Seen (~10 min)

- **Story:** as the squad leaves, Kade looks straight at Dev across the hall and raises his glass. Ashgrove now knows LANTERN's faces.

**Collectibles:** 3 intel files, including Tessellate's rollout plan.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Security gate | Guest check, metal detectors | 4.1 |
| Main hall | Glass walls, crowds, a string quartet | 4.1, 4.5 |
| Private salon | Kade's presentation room | 4.2, 4.3 |
| Balconies | Moreau's balcony, the shooter's position | 4.4 |
| Service tunnels | Power room, kitchen access | 4.3, 4.4 |

## Chapter 5 · Wildfire

A 90-minute chapter that opens Act II. Ashgrove bombs a peace summit and frames LANTERN for it. By the end, the squad's own country has disavowed them and Site Hollow is gone.

**Setting:** a peace summit at a mountain resort in Davar, then Site Hollow.

**Setup:** Varga sends the squad to quietly protect the summit, where Davar and Sarn are close to a ceasefire.

### 5.1 Advance team (~15 min)

- **Objective:** sweep the resort before the delegates arrive.
- **Story:** calm and routine. The squad jokes about the buffet. Tony is happy to be on a mission where nobody's shooting.

### 5.2 The blast (~25 min)

- **Story:** bombs go off in the conference hall. The attackers wear LANTERN-pattern gear and carry weapons with LANTERN serials.
- **Gameplay:** chaos inside the burning resort. Rescue delegates, fight the attackers room by room, and carry the wounded Sarn negotiator out.

### 5.3 Breaking news (~10 min)

- **Story:** on a TV in the lobby, the news shows the squad's faces as the attackers. Their own government announces LANTERN is a rogue unit. Varga's line goes dead.

### 5.4 Site Hollow (~30 min)

- **Objective:** get back to base, wipe the servers, get out.
- **Gameplay:** the player returns to the Ch 0 base and finds it being raided by their own country's special forces, following orders. The player can't kill friendly troops, so this is non-lethal: stun rounds, takedowns, avoidance.
- **Story:** walking through the places from Ch 0 (the range, the armory, the kill house) as they're taken apart is meant to hurt.

### 5.5 Gone (~10 min)

- **Story:** the squad escapes in a stolen truck with no support, no money, and every government hunting them. Tony asks Dev if they're going to be okay. Dev doesn't answer.

**Collectibles:** 3 intel files, including the forged LANTERN orders.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Resort grounds | Gardens, car park, security checkpoints | 5.1 |
| Conference hall | Destroyed after the blast, fires, collapsed balconies | 5.2 |
| Lobby | TV screens, evacuation point | 5.3 |
| Site Hollow | Ch 0 base, now under raid | 5.4 |

## Chapter 6 · Burned

A 90-minute survival chapter. With no support and no supplies, the squad holds together, Tony becomes family, and Varga makes contact again.

**Setting:** the Karst Highlands, a lawless mountain border region, one week after Ch 5. Dry, cold nights, smugglers' country.

**Survival rules for this chapter:** no HUD minimap (Tamsin has no satellite access), limited ammo, scavenged weapons only, and injuries that need bandaging.

### 6.1 Ditch the truck (~15 min)

- **Objective:** lose the patrol tracking the stolen truck.
- **Gameplay:** set up an ambush on a mountain road using the terrain and the little ammo they have.

### 6.2 Scavenge (~20 min)

- **Objective:** find food, medicine and weapons in an abandoned mining town.
- **Gameplay:** exploration with light scavenging. The player chooses what to carry with limited space.

### 6.3 Campfire (~10 min, playable scene)

- **Tony moment:** Tony shares his rations and reads one of his letters home, to his younger sister. The squad tells stories for the first time. Bishop tells one too, and everyone stares at him.
- The player can walk around the camp and talk to each squad member. Optional conversations add backstory.

### 6.4 The smugglers (~30 min)

- **Objective:** steal a cargo plane from a smugglers' airstrip.
- **Gameplay:** a choice of approach, stealthy at night or loud with explosives. Both end with the plane taking off under fire.

### 6.5 The call (~15 min)

- **Story:** on the plane, a satellite phone rings. It's Varga. She says she was framed too and gives coordinates for a meeting at the Orlek Dam. Tamsin doesn't trust it. Dev says yes anyway.

**Collectibles:** 3 intel files and 2 of Tony's letters, which are optional reading.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Mountain road | Switchbacks, rockfall cover | 6.1 |
| Mining town | Empty houses, a clinic, a collapsed mine entrance | 6.2 |
| Camp | Fire pit, squad members to talk to | 6.3 |
| Airstrip | Hangar, fuel trucks, watchtower, the cargo plane | 6.4 |

## Chapter 7 · The Ledger

A two-hour chapter and the turning point of the story. Varga is revealed as WARDEN, the meeting is a trap, and Okafor dies holding a bridge so the others can escape.

**Setting:** the Orlek Dam, a huge concrete dam in a river gorge, at dusk.

### 7.1 The meeting (~20 min)

- **Objective:** meet Varga at the dam's control building.
- **Story:** Varga is warm and convincing. She has a plan to clear their names. Bishop, on overwatch, counts too many vehicles on the access road.

### 7.2 WARDEN (~10 min)

- **Story:** Tamsin quietly matches the ledger's WARDEN account to a pension fund in Varga's name. She sends it to Dev's phone mid-conversation. Dev confronts Varga. Varga stops pretending: "Peace is a product, Dev. Someone was always going to sell it." She signals, and the trap closes.

### 7.3 The dam (~40 min)

- **Objective:** fight across the dam to the far side of the gorge.
- **Gameplay:** the biggest firefight so far. Ashgrove troops on both ends of the dam, a gunship overhead. The squad moves through turbine halls, maintenance tunnels and along the top of the dam wall.

### 7.4 The bridge (~25 min)

- **Story:** the only way out is a narrow bridge over the spillway. Okafor sets up his machine gun at the near end and tells the others to go.
- **Gameplay:** the player crosses under fire while Okafor holds. The player turns back once and sees him still firing.
- **Okafor's last line**, to Tony over the radio: "Look after them, Postcard." The bridge goes silent.

### 7.5 Tags (~10 min)

- **Story:** across the gorge, Tony goes back alone at night to retrieve Okafor's dog tags. Dev lets him. Tony carries them for the rest of the game. Varga escapes by helicopter.
- **Tone shift:** from here on, the game is no longer about proving something. It is about stopping Ashgrove.

**Collectibles:** 3 intel files, including Varga's first Ashgrove payment, dated 2027.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Control building | Meeting room, windows overlooking the gorge | 7.1, 7.2 |
| Turbine halls | Huge generators, catwalks | 7.3 |
| Dam wall top | Long exposed road, gunship attack runs | 7.3 |
| Spillway bridge | Narrow bridge, Okafor's last position | 7.4 |
| Far gorge | Woods, the squad's night camp | 7.5 |

## Chapter 8 · Iron Tide

A two-hour war chapter. Davar and Sarn go to full-scale war, and the squad finds the Ashgrove relay that feeds false intelligence to both armies. Tony is now someone Dev trusts with his flank.

**Setting:** the Ostrava Plains, the main front line, a month after Ch 7. Mud, trenches, burning tanks.

**Setup:** Tamsin traces Ashgrove's signals to a mobile command relay hidden behind the front. It sends both armies fake reports to keep the war going.

### 8.1 Behind the lines (~20 min)

- **Objective:** cross the Sarn rear area unseen.
- **Gameplay:** move through supply depots and field hospitals. The player sees ordinary soldiers on both sides who have no idea who started this.

### 8.2 The trench (~30 min)

- **Objective:** cross the front during a Davaran offensive.
- **Gameplay:** a trench assault in the middle of a real battle. The squad moves with the chaos rather than fighting it.
- **Tony moment:** Dev tells Tony to take the left flank alone. Tony holds it. No jokes on the radio this time, just short, clear reports.

### 8.3 Armor (~25 min)

- **Objective:** reach the relay's last known position.
- **Gameplay:** the squad commandeers an abandoned tank. The player drives and fires across the plains. This is the chapter's set piece.

### 8.4 The relay (~30 min)

- **Objective:** take the command relay intact.
- **Gameplay:** assault a heavily guarded convoy of command trucks. Tamsin needs the hardware in one piece, so no explosives near it.
- **Story:** the relay's logs show Tessellate's launch date: the day after the war ends, when exhausted governments will sign anything that promises it can't happen again.

### 8.5 Two flags down (~15 min)

- **Story:** Tamsin uses the relay to send both armies the same true message at once: a cease-fire order and the evidence. Firing slows along the front. It's a small victory nobody will credit them for.

**Collectibles:** 3 intel files and a recording of Okafor's last radio call, found in the relay logs.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Sarn rear | Supply depot, field hospital, rail yard | 8.1 |
| Front trenches | Trench lines, craters, a destroyed village | 8.2 |
| Open plains | Wide terrain for the tank section | 8.3 |
| Relay convoy | Command trucks, antennas, guard vehicles | 8.4, 8.5 |

## Chapter 9 · Cold Server

A 90-minute Arctic raid that opens Act III. The squad hits Tessellate's data core, corners Varga, and learns the system can only be shut down from Ashgrove headquarters. Rourke kills Varga before she can say more.

**Setting:** Station Nadir, a data center built into a glacier on a remote northern archipelago. Polar night, storms.

### 9.1 Ice road (~20 min)

- **Objective:** reach the station across the ice.
- **Gameplay:** snowmobile insertion through a storm, dodging drone patrols. The player switches to foot for the last kilometre.

### 9.2 Into the glacier (~25 min)

- **Objective:** get inside the station.
- **Gameplay:** cold, quiet stealth in ice tunnels, ending with a breach into the cooling halls where seawater runs past the servers.

### 9.3 The core (~20 min)

- **Objective:** copy Tessellate's architecture so Tamsin can understand how to kill it.
- **Story:** the core is only a mirror. The master control sits at Ashgrove headquarters in Halvern, and needs Kade's own key.

### 9.4 Varga (~15 min)

- **Story:** Varga is here, overseeing the launch. The player finds her alone in the control room. She tells Dev where to find Kade's key inside headquarters, maybe out of guilt, maybe to save herself.
- Before she can finish, a single shot through the glass kills her. Rourke, outside in the storm.

### 9.5 Collapse (~10 min)

- **Gameplay:** Rourke's team sets charges and the station starts to flood. The squad escapes through rising seawater and breaking ice.
- **Ending:** the squad knows where to go, and that Rourke is hunting them personally.

**Collectibles:** 3 intel files, including Varga's unsent letter to Dev.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Ice field | Open ice, storm, drone patrols | 9.1 |
| Ice tunnels | Narrow tunnels, maintenance stations | 9.2 |
| Cooling halls | Server racks over seawater channels | 9.2, 9.5 |
| Control room | Glass wall facing the storm | 9.3, 9.4 |

## Chapter 10 · No Witnesses

A two-hour chapter and the emotional peak of the game. To keep the war alive, Ashgrove turns a city into a battlefield. LANTERN gets civilians out instead of chasing Kade, and Tony dies taking a bullet meant for Dev.

**Setting:** Saradan, the Sarn capital, as Ashgrove-backed forces overrun it. Rain, smoke, a city in panic.

**Setup:** on the way to Halvern, the squad hears the city is falling. Going to Halvern means arriving before Tessellate launches. Stopping means thousands of people get out. Dev chooses the city.

### 10.1 The bridge crossing (~20 min)

- **Objective:** reach the old town.
- **Gameplay:** street fighting against Ashgrove contractors while civilians flee the other way. Firing near civilians costs the player.

### 10.2 The school (~30 min)

- **Objective:** get a group of children and teachers out of a school surrounded by fighting.
- **Gameplay:** escort and protect. Tony carries a frightened kid on his back the whole way and makes her laugh with his radio sign-off.

### 10.3 The station (~30 min)

- **Objective:** hold the rail station until the last train leaves.
- **Gameplay:** defensive battle. Bishop on the clock tower, Tamsin running the signals, Dev and Tony on the platform.

### 10.4 Last transport (~20 min)

- **Objective:** load the final civilians onto the last truck out.
- **The shot:** the player is lifting a civilian onto the truck. A glint flashes on a rooftop. Tony sees it first, shoves Dev aside and takes the bullet. Rourke's shot.
- **How it plays:** all in first person. The player loses control for a few seconds, the sound drops out, and Tony is on the ground.
- **Tony's last line,** on the radio, to the whole squad: "Spencer out. Wish you were here."

### 10.5 Aftermath (~20 min)

- **Story:** the truck leaves with everyone on it. The squad carries Tony out. Nobody says much.
- **Gameplay:** the player walks the empty city to the extraction point. No music, just rain.
- **Ending:** Bishop hands Dev Okafor's tags, which Tony had been carrying. Dev puts them on with Tony's. "We finish it."
- **What nobody will know:** about 4,000 people got out of Saradan. History will say the city fell.

**Collectibles:** the last 3 of Tony's letters, including one addressed to the squad.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| River bridge | Abandoned cars, fleeing crowds | 10.1 |
| Old town | Narrow streets, the school | 10.1, 10.2 |
| Rail station | Platforms, clock tower, the last train | 10.3 |
| Truck yard | The last transport, the sniper's rooftop | 10.4 |
| Empty streets | The walk to extraction | 10.5 |

## Chapter 11 · Black Ledger

A two-hour assault on Ashgrove headquarters. Dev settles things with Rourke, and Bishop stays behind alone to hold the stairwell so Dev and Tamsin can reach the top. His radio goes silent.

**Setting:** the Ashgrove Tower in Halvern, a neutral financial city. A 90-storey glass tower in a thunderstorm, the night before Tessellate launches.

**Squad:** Dev, Tamsin and Bishop. Three people against a private army.

### 11.1 The storm (~20 min)

- **Objective:** get onto the tower.
- **Gameplay:** a wingsuit glide from a neighboring tower to a maintenance deck halfway up, timed to lightning flashes.

### 11.2 Climbing (~30 min)

- **Objective:** work upward toward the executive floors.
- **Gameplay:** floor-by-floor fighting through offices, labs and an atrium. The squad uses everything the game has taught: stealth where it can, loud where it can't.

### 11.3 Rourke (~25 min)

- **Objective:** get past Rourke, who is waiting on the sky-garden floor.
- **Gameplay:** a sniper duel across the glass garden and its walkways. The player has to read glints, the way Tony did. Dev wins.
- **Rourke's last words:** "The kid was faster than you."

### 11.4 The stairwell (~30 min)

- **Story:** Ashgrove's reaction force floods up from below. The elevators are dead and only one stairwell leads to the top. Bishop sits down at the landing with every magazine they have left. "Go. I've got the stairs."
- **Gameplay:** the player climbs while hearing Bishop's fight below on the radio. The gunfire stops. Bishop's channel goes silent.

### 11.5 The top floor (~15 min)

- **Objective:** reach Kade's office with the key's location from Varga.
- **Ending:** the doors open. Kade is waiting, calm, a drink in his hand. Cut to black.

**Collectibles:** 3 intel files, including Kade's personal list of every leader he owns.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Neighboring tower | Roof launch point | 11.1 |
| Maintenance deck | Mid-height landing, exterior catwalks | 11.1 |
| Office floors | Open offices, server labs, atrium | 11.2 |
| Sky garden | Glass-walled garden, walkways, Rourke's perch | 11.3 |
| Stairwell | The only route up, Bishop's last position | 11.4 |
| Executive floor | Kade's office | 11.5 |

## Chapter 12 · Silent

A two-hour finale. Dev and Tamsin shut Tessellate down and expose Ashgrove to every government at once. The wars stop, Bishop walks out of the smoke alive, and LANTERN is erased from history.

**Setting:** the top of the Ashgrove Tower, then the tower's core, then a quiet epilogue months later.

### 12.1 Kade (~15 min, playable conversation)

- **Story:** Kade doesn't fight. He explains: wars were always going to happen, and he only made them profitable and, eventually, controllable. He offers Dev a choice: walk away and the squad's names are cleared.
- The player can't accept. Dev takes Kade's key from where Varga said it would be.

### 12.2 The core (~35 min)

- **Objective:** get Kade's key into Tessellate's master core, three floors below, and keep Tamsin alive while she works.
- **Gameplay:** the hardest fight in the game. Ashgrove's last defenders and automated turrets that Tessellate controls.

### 12.3 Shutdown (~20 min)

- **Story:** Tamsin has two choices for the ledger: give it to one government, or to all of them. She sends it to every government, every newsroom and every army at the same moment, so nobody can bury it.
- **Gameplay:** hold the core room while the upload runs. The countdown is Tessellate's own launch clock running out.
- Tessellate goes dark. Across the world, Ashgrove's systems drop offline.

### 12.4 Smoke (~15 min)

- **Gameplay:** escape down through the burning tower.
- **Story:** at the stairwell from Ch 11, the player finds empty casings and blood, but no body. In the lobby, a shape limps out of the smoke. Bishop, badly wounded, alive. "Took you long enough."

### 12.5 Epilogue: no record (~15 min)

- **Story:** news montage. Kade is arrested quietly by the governments he owned, who then bury every mention of how it happened. The wars wind down. The official story credits "international cooperation."
- LANTERN is never mentioned. Officially, it never existed.
- **Final scene:** months later, Dev visits Tony's younger sister and leaves her a postcard with Okafor's and Tony's tags. On the back: "Wish you were here." Credits.

**Collectibles:** none. After the credits, a memorial screen lists Okafor and Tony.

### Level layout

| Area | Contents | Used in |
| --- | --- | --- |
| Kade's office | Glass desk, city view in the storm | 12.1 |
| Tessellate core | Three-level server shaft, turrets | 12.2, 12.3 |
| Burning tower | Collapsing floors, the Ch 11 stairwell | 12.4 |
| Lobby | Smoke, Bishop's return | 12.4 |
| Tony's sister's home | Small house, front porch | 12.5 |

## Mobile controls and technical notes

The controls follow the standard layout of big mobile shooters: the left thumb moves and the right thumb aims and acts.

| Control | Position | Notes |
| --- | --- | --- |
| Move stick | Left side, floating | Appears wherever the thumb lands. Pushing to the edge sprints |
| Look | Right side, drag anywhere | Sensitivity setting, slower while aiming |
| Fire | Right, large button | Drag on it to aim while firing. An optional second fire button on the left |
| Aim down sights | Next to Fire | Tap to toggle or hold, set in Settings |
| Crouch / prone | Bottom right | Tap to crouch, hold to go prone |
| Reload, swap weapon | Above Fire |  |
| Grenade, melee, interact | Right edge | Interact appears only near doors, crates and squadmates |
| Mark target | Context button | Teaches squad callouts in Ch 0 |

**Technical targets:**

- 60 fps on mid-range phones from about 2022 onwards, 30 fps fallback on older ones.
- Graphics quality presets (low, medium, high) with dynamic resolution.
- Chapters download separately, so the app install stays small.
- Checkpoints within each chapter, saved to the cloud.
- A browser prototype already exists: a container-yard wave shooter that becomes the basis for Ch 1's Port of Kessa.

**Project folder:** the game project lives locally at `A:\app\gamepath`. Scripts and build files for the playable Ch 0 go there.

**Progress so far:** story, characters and all 13 chapters (Ch 0–12) are designed mission by mission, with level layouts and mobile controls. Next step: build a playable Ch 0 · First Light.

## Decision log and open questions

| Date | Decision |
| --- | --- |
| Oct 6, 2026 | Local project folder set to A:\app\gamepath. Full campaign design saved; development starts with a playable Ch 0 |
| Oct 6, 2026 | Ch 3–12 detailed. Added villains Elias Kade (Ashgrove founder) and Silas Rourke (field operations, the sniper who kills Tony) |
| Oct 6, 2026 | Ch 2 · Paper Trail detailed. The ledger introduces the codename WARDEN (Varga) |
| Oct 6, 2026 | Warring sides confirmed: the Republic of Davar and the Sarn Liberation Front |
| Oct 6, 2026 | Ch 1 · Quiet Water detailed. The Ashgrove ambush at Kessa plants the first hint of Varga's leak |
| Oct 6, 2026 | Chapter 0 approved. Tony's sign-off "Wish you were here" kept |
| Oct 6, 2026 | Bishop survives Ch 11 and returns in Ch 12 |
| Oct 6, 2026 | Tony dies in Ch 10, shielding Dev from a sniper's bullet |
| Oct 6, 2026 | Added Ch 0 · First Light, a 45-minute prologue that introduces the squad |
| Oct 6, 2026 | Added Cpl. Tony Spencer, the rookie |
| Oct 6, 2026 | Rogue PMC conflict, near-future 2030s, special forces squad. Tone moves from tactical stealth to gritty |
| Oct 6, 2026 | Story and design first, built together chapter by chapter. Engine decided later |

**Open questions:**

- [x] Choose the game engine (Unity recommended for mobile)
- [x] Confirm Tony's catchphrase
- [x] Pick the country and location of Site Hollow
- [x] Detail Ch 1 · Quiet Water, mission by mission

- [x] Confirm the two warring sides: the Republic of Davar and the Sarn Liberation Front
- [x] Detail Ch 2 · Paper Trail, mission by mission

- [x] Name Ashgrove's founder (the Ch 2 intel file names them)
- [x] Detail Ch 3 · Two Flags, mission by mission

- [x] Review Ch 3–12 and confirm Varga's death in Ch 9 and Kade's arrest in Ch 12
- [x] Choose the first build target: a playable Ch 0
