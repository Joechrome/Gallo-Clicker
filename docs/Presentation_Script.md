# Classes Project — Presentation Script

**Game:** Clicker (Unity 6 / C#)
**Slots:** 4 speakers × ~2 minutes + shared close
**Limit:** 10:00 hard stop

---

## Before you walk in (do this, not during)

- [ ] Close every other Unity project. Open **only** `D:\新建文件夹\Clicker`.
- [ ] Press Play **once**, then stop. Confirm Console is clean (no red).
- [ ] Set the shop prices **low** for the demo: `Upgrade_manager` → Upgrades → Cost = `5 / 10 / 20 / 40`.
- [ ] Record a **30-second screen capture** of a full ransom sequence. If the live demo breaks, play this instead — do not debug on stage.
- [ ] Browser tab open on the repo, on the **Commits** page.
- [ ] Do one timed rehearsal. If you run over 8:30 in rehearsal you will run over on the day.
- [ ] Decide who clicks. One person only. Two people fighting the mouse looks chaotic.

**Numbers to have on the tip of your tongue:**

| Fact | Value |
|---|---|
| Commits | 39 |
| Contributors | 3 (Joechrome, Daniel Pan, oweny314) |
| Merge commits | 7 |
| Project span | 7 Oct → 20 Oct |
| Lines added | ~88,800 |
| Scripts / lines of C# | 16 / ~1,560 |
| Scenes in build | 2 (Main, Shop) |

---

## Timing sheet

| Time | Speaker | Content | Criteria |
|---|---|---|---|
| 0:00–0:40 | **A** | Hook + scope | — |
| 0:40–2:10 | **B** | Live demo | — |
| 2:10–3:55 | **A** | Where state lives + encapsulation | A2.2 |
| 3:55–5:30 | **C** | Events = modularity without editing | A2.2 |
| 5:30–7:00 | **D** | Modifying existing code to add features | A2.3 |
| 7:00–8:30 | **B** | Project management + contributions | B1 & B2 |
| 8:30–9:40 | **All** | Reflection + speculation | Other |
| 9:40–10:00 | — | Buffer / Q&A | — |

---

## The script

### 0:00–0:40 — A — Hook and scope

> "Ours is a clicker game. You click a character, you earn points, you spend them in a shop, and the shop changes how the game plays.
>
> But the game isn't what we're presenting. The **classes** are. We want to show you three things: how we stopped one bug from breaking the whole game by moving state into the right class, how we added an entire feature without editing the class it plugs into, and how we modified our existing code three separate times to make it do more — without ever rewriting it.
>
> I'm [A], with me are [B], [C] and [D]."

**[SHOW ONE SLIDE: the criteria, A2.2 / A2.3 / B1&B2.** Keep it up for 5 seconds, then move on — don't read it out.]

### 0:40–2:10 — B — Live demo

**[UNITY, full screen, already in the Main scene, NOT already playing]**

> "Watch the top of the screen. I click the character — score goes up. I do that a few times, then I go to the shop."

**[Click the Shop button. In the shop:]**

> "Four upgrades. I'll buy the cheapest one — watch the score drop and my points-per-click go up."

**[Buy ChatGPT. Go back. Click. Score jumps by 2.]**

> "Same click, double the points, and the particle burst changed to that upgrade's icon."

**[If time allows — this is the best 15 seconds of the presentation]**

> "Now the ransom. The cloud occasionally glitches — there it is. It's red, it's shaking, there's a stop sign. If I click it, I get attacked. Let me not touch it..."

**[Wait out the warning window.]**

> "...and it just leaves. It never fires. Which is the whole point of the design — I'll hand back to [A] to explain why."

**[If the glitch doesn't appear: "The cloud rolls a 5% chance each pass, so let's not wait for it — here's the recording." Play the clip. Never stand there waiting.]**

### 2:10–3:55 — A — A2.2: where state lives, and encapsulation

**[SHOW `GameData.cs`]**

> "This is `GameData`. Three things about it.
>
> **One — it's static, and that's the fix for a real bug we had.** Originally our score was a private integer on a component sitting in the Main scene. Load the Shop scene, Unity destroys every object in Main, and the score was gone. Not wrong — *deleted*. We moved it to a static class, because static members belong to the class rather than to a scene, so nothing can unload them.
>
> **Two — encapsulation.** Look at the property: `public static int Clicks { get; private set; }`. Anyone can *read* the score. Only `GameData` can *write* it. If the setter were public, any script could set the score to 999999 and the scoreboard, the shop and the save would silently disagree, and we'd have no idea which script did it. Making the setter private means there's exactly one way to change the score, and it's a method we control.
>
> **Three — the methods enforce rules, not the callers.** `Spend(int cost)` checks the balance itself and returns false, changing nothing if you're short. A caller can't go negative and can't half-complete a purchase, because they never get to do the arithmetic themselves."

> "The test is: what would break without it? Without the private setter, one bad line anywhere corrupts the score. Without `Spend`, every caller has to remember to check the balance first — and every caller that forgets is a bug waiting to happen. We wrote it once in the class so no caller can forget."

> "And there's one more thing in this class that mattered later. `ScoreBlocked` — a plain true/false flag. When the ransom sequence is running, scoring stops. Notice `GameData` has **no idea what a ransom is**. It doesn't reference the ransom class, it doesn't know the cloud exists. It just offers a switch. That's what let us bolt the ransom system on months after `GameData` was written."

### 3:55–5:30 — C — A2.2: events make classes extensible

**[SHOW `CloudMoving.cs`, highlight the event line]**

> "This class does one job: the cloud drifts left to right, and when it leaves the screen it teleports back to the left edge at a random height.
>
> There's one line that isn't about drifting at all. `public event System.Action Wrapped;` — it fires on every wrap. That's it. `CloudMoving` doesn't know who's listening. It doesn't know the ransom exists."

**[SHOW `GlitchCloud.cs`, highlight `OnEnable`]**

> "This is the other side. `GlitchCloud` says: when the cloud wraps, tell me. `_moving.Wrapped += OnWrapped`. Then it rolls a 5% chance. Hit means the cloud turns red, wears a stop sign, and gets a collider — and only while it's glitched. Normally the cloud isn't even clickable."

> "Here's why that's better than the alternatives. The obvious version is a field on `CloudMoving`: `public bool isRansomMode`, and let the cloud check it. That makes the drift code depend on the ransom code — two classes tangled, and you can't reuse `CloudMoving` for a second cloud without dragging ransom along. The other obvious version is `GlitchCloud` polling the cloud's x position every frame looking for a teleport. That works, but it's guessing at internal state instead of being told.
>
> The event makes the dependency **one-way**. `GlitchCloud` depends on `CloudMoving`. `CloudMoving` depends on nothing. We added a whole feature and the original class didn't change at all — and if you add a third listener tomorrow, you don't touch it either."

> "Two supporting details worth knowing. `[RequireComponent(typeof(CloudMoving))]` on top of the class means Unity won't let you attach `GlitchCloud` to something that can't move — the mistake is impossible instead of runtime crashing. And `OnEnable` / `OnDisable` subscribe and unsubscribe, so when the scene unloads, the subscription goes with it instead of holding a dead reference."

### 5:30–7:00 — D — A2.3: modifying existing code to add functionality

> "A2.3 is about taking modular code you already have and making it do more. We did that three times. Every one was a small change to existing code that unlocked a large feature."

**[SHOW `clickable_behaviour.cs`, highlight `PointerIsOverThisObject`]**

> "**One.** Our click handler used to do a raycast and score if the ray hit *any* collider. That was correct when the clickable character was the only collider in the scene. Then we added a glitched cloud with a collider, and collectable coins. Those would have silently scored points — no error, just wrong numbers.
>
> We didn't rewrite the handler. We changed one condition: it now compares the hit against *its own* collider. `hit == _selfCollider`. One comparison, and the component is safe in a scene it was never designed for."

**[SHOW `RansomEvent.cs`, top config block]**

> "**Two.** Our ransom sequence is 834 lines — the biggest thing we built. It has four designer-facing numbers: the glitch chance, how long clicking is forbidden, how many coins to collect, and the time limit. Not one of those is a magic number in the logic. They're `[SerializeField]` fields on the component, grouped under `[Header]` labels, with `[Tooltip]` and `[Range]` limits — the range attribute makes a 500% glitch chance impossible to type in."

> "That's the difference between a class and a reusable class. We tuned the difficulty of that whole sequence on stage, in the Inspector, with no code change and no recompile."

**[SHOW `RansomEvent.cs`, the `Phase` enum]**

> "**Three.** The sequence isn't one giant method. It's a coroutine state machine with an explicit `Phase` enum — Idle, Warning, Jumpscare, Collect, Result. Each state can be reasoned about on its own. We can tell you exactly what the code does when it's in `Collect`, because that's the only thing it does when it's in `Collect`."

**[SHOW `RansomCoin.cs`]**

> "And this is what enabled it — `RansomCoin` is fully self-contained. Give it a sprite and a landing height, it falls, bobs, and knows when the pointer is inside its radius. It has no collider on purpose, so it can't collide with or interfere with the player's click handler. One class, one job, contained."

### 7:00–8:30 — B — B1 & B2: project management and contributions

**[SHOW the repo Commits page — real page, not a screenshot]**

> "One number so you can judge this honestly. **39 commits, 3 contributors, 7 merge commits**, over 7th to 20th October.
>
> Here's what that costs us and what it taught us."

> "**Shared source control.** `git`. Every member commits under their own identity, the commits page is the record — so "who did what" is verifiable rather than claimed."

> "**The Unity-specific problem we had to solve.** Unity scene files are YAML full of internal IDs. Two people editing the same scene produces a merge conflict you cannot realistically resolve by hand. We learned this the hard way: at one point a merge overwrote a working feature and we had to rebuild it. What actually fixed it was discipline, not a tool — small commits, commit immediately after something works, and push the same day."

> "**How we handled renaming.** When we renamed a script, we renamed the file and its `.meta` together instead of deleting and recreating. Unity stores asset references as GUIDs inside the `.meta` file — delete and recreate, and every scene reference to that component silently breaks. Rename the pair and the GUID survives. That's the kind of thing you learn once and never forget."

> "**Contributions, by the numbers.** [Read from `git log --numstat`: Joechrome +14,180/−418; Daniel Pan +67,627/−648; oweny314 +7,029/−161.] Lines are not the same as contribution — the person who found the scene-merge problem contributed more than the line count shows. Our actual division was: core game and state, shop and economy, ransom sequence, and effects."

> "**What we did not do well, in the interest of honesty.** We had no written task list at the start. We had no scheduled checkpoints — we checked in when something broke instead of on a calendar. And our largest class is 834 lines, which is the thing we'd break up first next time."

### 8:30–9:40 — All — Reflection and speculation

**[Each speaker gets one box. One person, one sentence, no summing up — the silence between them makes it land.]**

> **A:** "The bug that taught us the most wasn't a crash. Our upgrade purchase compared the text `autoclicker` against `Autoclicker` — C# is case-sensitive, so the branch never matched. It logged "pressed", accepted the click, and did nothing. No error. Silent. Everything we name now gets compared case-insensitively, and anything that fails to match logs a warning instead of quietly doing nothing."

> **B:** "Our project had domain reload disabled in the Editor settings. That's a Unity option for faster iteration — but it means static state is **not** cleared between play sessions. So a score that was supposed to reset was still sitting there from the last run, and it looked exactly like a bug in our save code. We fixed it by resetting explicitly at startup. We'd never have found it by reading the code — we had to know the runtime."

> **C:** "Something we'd do differently: our ransom sequence loads its art and sound from `Resources` folders by name, as strings. If somebody renames a file, it doesn't fail to compile — it fails at runtime with a warning in a console nobody is looking at. We'd move that to serialized references dragged in the Inspector, so a missing asset is a visible empty slot instead of a broken string."

> **D:** "And what we'd add next. The game doesn't save yet — deliberately, it resets each launch. Adding saving means a versioned save format, because the day after you ship it, your data model changes and old saves have to keep loading. We'd also break `RansomEvent` into a phase controller, an overlay spawner and an audio player, so each could be tested alone."

### 9:40–10:00 — Buffer

> "That's us — happy to take questions."

---

## Criteria coverage (know where each is proven)

| Criteria | Proven by | Where in the script |
|---|---|---|
| **A2.2** modular design, reusable code, encapsulation | `GameData` private setter + `Spend()`; `CloudMoving.Wrapped` event; `[RequireComponent]`; `[SerializeField]` config; `RansomCoin` self-contained | A (2:10), C (3:55), D (5:30) |
| **A2.3** modify existing code to enhance functionality | `clickable_behaviour` own-collider fix; `CloudMoving` gained an event; score moved out of the scene component; `Upgrade` → serialized data | A (2:10), C, D (5:30) |
| **B1** project management | 39 commits, 7 merges, build settings, `.meta`/GUID discipline, scene-merge lesson | B (7:00) |
| **B2** contributions | `git shortlog` / `--numstat` per author; merge history; honest gaps | B (7:00) |
| **Other** | runtime knowledge (domain reload), reflection, named next steps | All (8:30) |
| **Feedback** | the last section, plus honest limits ("834 lines", "no task list") | All (8:30) |

---

## Likely questions — have these ready

**"Why static? Isn't global state bad?"**
> "It is, and that's the honest trade. A static class is global mutable state, it's hard to unit-test, and it can't be swapped out. We took it because the alternative — a singleton that survives scene loads — needs an object in the first scene, needs duplicate-guarding, and can be destroyed by accident. Our data belongs to no scene, so we put it where it can't be destroyed. If we grew past a handful of values we'd move to an injected service."

**"What actually is encapsulation, in one sentence?"**
> "The class decides what other code is allowed to do to it, instead of trusting every caller to behave. Our setter is private and `Spend` returns false without changing anything — so no caller can create an impossible state."

**"Which part did you personally write?"**
> Be literal. Name the file. Then say what decision in it was yours.

**"Why is your biggest class 834 lines?"**
> "Because we grew it feature by feature and never stopped to split it. It's the first refactor on our list, and we can tell you exactly how: phase controller, overlay spawner, audio player."

**"Show me the inheritance."**
> ⚠️ **Careful.** This project uses **composition and events**, not class hierarchies. The honest answer: *"We deliberately used composition plus events instead of inheritance — our classes have distinct jobs rather than shared family trees, and an event let us extend behaviour without editing the class it plugs into."* Do not invent an inheritance chain that isn't there.

---

## Delivery notes

- **One speaker at a time, one slide at a time.** Hand over verbally: "I'll hand to [C]."
- **Never read a slide.** The slides are file names and one-line headings.
- **When you show code, zoom so a full line is readable** — a projector will destroy 11pt text.
- **Say the honest weakness before the marker finds it.** "834 lines, no written task list, no scheduled checkpoints" gets you Level 4. Being caught out gets you less.
- **If something breaks live:** say "let me show the recording", switch, keep talking. Do not debug.

---

## ⚠️ Verify these before presenting

1. **Two leftover debug lines in `GameData.cs`** — `UnityEngine.Debug.Log('a');` inside `Spend()` will print an `a` to the Console on every failed purchase, and `using UnityEngine.Diagnostics;` is unused. Both look careless on a projected Console. Remove them.
2. **`UpgradeButtonPressed.cs` still holds the class named `Autoclicker`** — so in Unity's Add Component menu it appears as **"Upgrade Button Pressed"**. Rename the file to `Autoclicker.cs` (keeping the `.meta`) if you want the name to match, but **do not do it after your final rehearsal** — it's a recompile and you don't want a surprise the night before.
3. **Confirm the shop purchase flow still works end to end** — `Upgrade_manager` → Upgrades array filled, and each button has the buy component with a matching name. If a button silently does nothing, that's the case-sensitivity class of bug again.
