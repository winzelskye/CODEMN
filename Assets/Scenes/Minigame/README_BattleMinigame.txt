BATTLE MINIGAME SETUP (CODEMN)
=============================

Flow: Player picks FIGHT/SKILLS/ITEMS -> Minigame appears -> Success = enemy takes damage (or heal/item), Fail = player takes damage -> Enemy turn -> Repeat until HP 0.

1) SCENE SETUP
   - Ensure the scene has a GameObject with BattleController (turnbased).
   - Add a GameObject with MinigameController (e.g. under Canvas).
   - On BattleController (Inspector):
     - Assign "Minigame Controller" to the MinigameController object.
     - Optionally assign "Action Buttons Container" to the parent of FIGHT/SKILLS/ITEMS buttons (so they are disabled during the minigame).
     - Tune "Attack/Skill/Item Minigame Config" (duration, key to press, prompt text).
   - On MinigameController (Inspector):
     - Optionally assign "Minigame Panel" (Panel with prompt + timer TextMeshPro). If left empty, a simple panel is created at runtime.

2) BUTTON WIRING
   - FIGHT  -> BattleController.ButtonAttack (or OnActionSelectedAttack)
   - SKILLS -> BattleController.ButtonHeal (or OnActionSelectedSkill)
   - ITEMS  -> BattleController.ButtonItems (or OnActionSelectedItem)

3) OPTIONAL FEEDBACK
   - BattleController exposes OnPlayerTookDamage(int damage) and OnEnemyTookDamage(int damage). Subscribe in code or from another component to play SFX/VFX when damage is applied.
