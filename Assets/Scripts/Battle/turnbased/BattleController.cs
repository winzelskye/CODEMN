using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    enum GameState
    {
        TURN_PLAYER,
        TURN_ENEMY,
        WIN,
        LOSS
    }

    public UnitHud PlayerHud;
    public UnitHud EnemyHud;
    public BattleHud BattleHud;

    public Transform playerSummon, enemySummon;
    public GameObject playerPrefab;
    public GameObject[] enemyPrefab;

    GameObject player, enemy;
    UnitController playerController, enemyController;
    GameState state;


    private void Start()
    {
        player = Instantiate(playerPrefab, playerSummon.position, Quaternion.identity);
        playerController = player.GetComponent<UnitController>();
        SummonMonster();
        state = GameState.TURN_PLAYER;
        StartCoroutine(PlayerHud.StartHud(PlayerHud, playerController));
        StartCoroutine(EnemyHud.StartHud(EnemyHud, enemyController));
        playerController.SetBattleHud(BattleHud);
        enemyController.SetBattleHud(BattleHud);
        BattleHud.ChooseText();
    }

    void TurnPlayer()
    {
        Debug.Log("PLAYER TURN");
        if (enemyController.currentHealth <= 0)
        {
            state = GameState.WIN;
            EndBattle();
        }
        else
        {
            state = GameState.TURN_ENEMY;
            StartCoroutine(TurnEnemy());
        }
    }

    IEnumerator TurnEnemy()
    {

        Debug.Log("ENEMY TURN");
        yield return new WaitForSeconds(.1f);
        enemyController.MakeTurn(playerController, () =>
        {
            state = GameState.TURN_PLAYER;
            BattleHud.ChooseText();
            if (playerController.currentHealth <= 0)
            {
                state = GameState.LOSS;
                EndBattle();
            }
        });
    }

    void EndBattle()
    {
        if (state == GameState.WIN)
        {
            enemyController.Delete();
            BattleHud.EndText(true);
            Debug.Log("YOU WON!");
        }
        else if (state == GameState.LOSS)
        {
            playerController.Delete();
            BattleHud.EndText(false);
            Debug.Log("YOU LOST!");
        }
    }

    void SummonMonster()
    {
        this.enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], enemySummon.position, Quaternion.identity);
        this.enemyController = enemy.GetComponent<UnitController>();
    }

    public void ButtonAttack()
    {
        if (state != GameState.TURN_PLAYER) return;

        state = GameState.TURN_ENEMY;
        playerController.AttackTurn(enemyController, () => TurnPlayer());
    }

    public void ButtonHeal()
    {
        if (state != GameState.TURN_PLAYER) return;
        if (playerController.EnoughManaForSpell("heal") == false)
        {
            BattleHud.ManaText(playerController.unitScriptableObject.healPower);
            return;
        }

        state = GameState.TURN_ENEMY;
        playerController.HealTurn(() => TurnPlayer());
    }

    public void ButtonRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ButtonExit()
    {
        Application.Quit();
    }

}