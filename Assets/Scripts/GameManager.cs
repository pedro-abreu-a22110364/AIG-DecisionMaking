﻿using Assets.Scripts.IAJ.Unity.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.Formations;
using System;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static class GameConstants
    {
        public const float UPDATE_INTERVAL = 2.0f;
        public const int TIME_LIMIT = 150;
        public const int PICKUP_RANGE = 8;

    }

    public AutonomousCharacter Character { get; set; }

    //[Header("UI Objects")]
    private Text HPText;
    private Text ShieldHPText;
    private Text ManaText;
    private Text TimeText;
    private Text XPText;
    private Text LevelText;
    private Text MoneyText;
    private Text DiaryText;
    private GameObject GameEnd;

    [Serializable]
    public enum MonsterControl
    {
        SleepingMonsters,
        BehaviourTreeMonsters,
        StateMachineMonsters
    }

    [Serializable]
    public enum FormationSettings
    {
        NoFormations,
        LineFormation,
        TriangleFormation
    }

    [Header("Enemy Settings")]
    [Tooltip("Here you choose what algorithms control the Monsters")]
    [SerializeField]
    public MonsterControl monsterControl;
    [Tooltip("Here you choose if formations are in use")]
    public FormationSettings formationSettings;

    [Header("To Increase the Challange")]
    public bool StochasticWorld = false;


    public List<GameObject> chests { get; set; }
    public List<GameObject> skeletons { get; set; }
    public List<GameObject> orcs { get; set; }
    public List<GameObject> dragons { get; set; }
    public List<GameObject> enemies { get; set; }
    public List<FormationManager> Formations { get; set; }
    public Dictionary<string, List<GameObject>> disposableObjects { get; set; }
    public int InitialDisposableObjectsCount { get; set; }
    public bool WorldChanged { get; set; }

    private float nextUpdateTime = 0.0f;
    private float enemyAttackCooldown = 0.0f;
    public bool gameEnded { get; set; } = false;
    public Vector3 initialPosition { get; set; }

    void Awake()
    {
        Instance = this;
        UpdateDisposableObjects();
        this.InitialDisposableObjectsCount = this.disposableObjects.Count;
        this.WorldChanged = false;


        this.Character = GameObject.FindGameObjectWithTag("Player").GetComponent<AutonomousCharacter>();
        this.HPText = GameObject.Find("Health").GetComponent<Text>();
        this.XPText = GameObject.Find("XP:").GetComponent<Text>();
        this.ShieldHPText = GameObject.Find("Shield").GetComponent<Text>();
        this.LevelText = GameObject.Find("Level").GetComponent<Text>();
        this.TimeText = GameObject.Find("Time").GetComponent<Text>();
        this.ManaText = GameObject.Find("Mana").GetComponent<Text>(); ;
        this.MoneyText = GameObject.Find("Money").GetComponent<Text>();
        this.GameEnd = GameObject.Find("EndGame");
        this.GameEnd.GetComponent<RectTransform>().localScale *= 10;
        this.GameEnd.SetActive(false);


        this.initialPosition = this.Character.gameObject.transform.position;

        // Initialize Formations List if not already
        if (this.Formations == null)
        {
            this.Formations = new List<FormationManager>();
        }

        // Only set up formations if formations are enabled
        if (formationSettings == FormationSettings.LineFormation)
        {
            SetupOrcFormation(new LineFormation());
        }
        else if (formationSettings == FormationSettings.TriangleFormation)
        {
            SetupOrcFormation(new TriangleFormation());
        }

    }

    private void SetupOrcFormation(FormationPattern formationPattern)
    {
        GameObject orc3 = GameObject.Find("Orc3");
        GameObject orc4 = GameObject.Find("Orc4");
        GameObject orc5 = GameObject.Find("Orc5");

        if (orc3 == null || orc4 == null || orc5 == null)
        {
            Debug.LogError("Orcs 3, 4, or 5 could not be found in the scene.");
            return;
        }

        List<Monster> orcMonsters = new List<Monster>
        {
            orc3.GetComponent<Monster>(),
            orc4.GetComponent<Monster>(),
            orc5.GetComponent<Monster>()
        };

        Vector3 anchorPosition = new Vector3(-26.7f, 0, -33.8f); //this is the approximate position of the new orcs
        Vector3 orientation = Vector3.forward;
        orcMonsters[2].SetFormationLeader();

        FormationManager formationManager = new FormationManager(orcMonsters, formationPattern, anchorPosition, orientation);
        this.Formations.Add(formationManager);

        formationManager.UpdateSlots();

        Debug.Log("Orc formation has been set up using " + formationPattern.GetType().Name + ".");
    }

    public void UpdateDisposableObjects()
    {
        this.enemies = new List<GameObject>();
        this.disposableObjects = new Dictionary<string, List<GameObject>>();
        this.chests = GameObject.FindGameObjectsWithTag("Chest").ToList();
        this.skeletons = GameObject.FindGameObjectsWithTag("Skeleton").ToList();
        this.orcs = GameObject.FindGameObjectsWithTag("Orc").ToList();
        this.dragons = GameObject.FindGameObjectsWithTag("Dragon").ToList();
        this.enemies.AddRange(this.skeletons);
        this.enemies.AddRange(this.orcs);
        this.enemies.AddRange(this.dragons);

     
        //adds all enemies to the disposable objects collection
        foreach (var enemy in this.enemies)
        {

            if (disposableObjects.ContainsKey(enemy.name))
            {
                this.disposableObjects[enemy.name].Add(enemy);
            }
            else this.disposableObjects.Add(enemy.name, new List<GameObject>() { enemy });
        }
        //add all chests to the disposable objects collection
        foreach (var chest in this.chests)
        {
            if (disposableObjects.ContainsKey(chest.name))
            {
                this.disposableObjects[chest.name].Add(chest);
            }
            else this.disposableObjects.Add(chest.name, new List<GameObject>() { chest });
        }
        //adds all health potions to the disposable objects collection
        foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion"))
        {
            if (disposableObjects.ContainsKey(potion.name))
            {
                this.disposableObjects[potion.name].Add(potion);
            }
            else this.disposableObjects.Add(potion.name, new List<GameObject>() { potion });
        }
        //adds all mana potions to the disposable objects collection
        foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion"))
        {
            if (disposableObjects.ContainsKey(potion.name))
            {
                this.disposableObjects[potion.name].Add(potion);
            }
            else this.disposableObjects.Add(potion.name, new List<GameObject>() { potion });
        }
    }

    void FixedUpdate()
    {
        if (!this.gameEnded)
        {
            if (Time.time > this.nextUpdateTime)
            {
                this.nextUpdateTime = Time.time + GameConstants.UPDATE_INTERVAL;
                this.Character.baseStats.Time += GameConstants.UPDATE_INTERVAL;
            }

            if (formationSettings != FormationSettings.NoFormations && Formations != null)
            {
                foreach (FormationManager formation in Formations)
                {
                    if (formation != null)
                    {
                        formation.UpdateSlots();
                    }
                }
            }

            this.HPText.text = "HP: " + this.Character.baseStats.HP;
            this.XPText.text = "XP: " + this.Character.baseStats.XP;
            this.ShieldHPText.text = "Shield HP: " + this.Character.baseStats.ShieldHP;
            this.LevelText.text = "Level: " + this.Character.baseStats.Level;
            this.TimeText.text = "Time: " + this.Character.baseStats.Time;
            this.ManaText.text = "Mana: " + this.Character.baseStats.Mana;
            this.MoneyText.text = "Money: " + this.Character.baseStats.Money;

            if (this.Character.baseStats.HP <= 0 || this.Character.baseStats.Time >= GameConstants.TIME_LIMIT)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "You Died";
            }
            else if (this.Character.baseStats.Money >= 25)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "Victory \n GG EZ";
            }
        }
    }

    public void RemoveOrcFromFormation(Monster orc)
    {
        if (Formations == null || Formations.Count == 0)
            return;

        foreach (FormationManager formation in Formations)
        {
            if (formation != null && formation.SlotAssignment.ContainsKey(orc))
            {
                formation.RemoveCharacter(orc);
                Debug.Log($"{orc.gameObject.name} has been removed from formation.");
            }
        }
    }


    public void BreakFormations()
    {
        if (formationSettings != FormationSettings.NoFormations)
        {
            foreach (FormationManager formation in Formations)
                formation.BreakFormation();
        }

        Formations.Clear();
    }

    // Action effects on the Game World
    public void SwordAttack(GameObject enemy)
    {
        int damage = 0;

        Monster monster = enemy.GetComponent<Monster>();
        Monster.EnemyStats enemyData = enemy.GetComponent<Monster>().stats;
        
        if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
        {
            this.Character.AddToDiary(" I Sword Attacked " + enemy.name);

            if (this.StochasticWorld)
            {
                damage = enemy.GetComponent<Monster>().DmgRoll.Invoke();

                //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                int attackRoll = RandomHelper.RollD20() + 7;

                if (attackRoll >= enemyData.AC)
                {
                    //there was an hit, enemy is destroyed, gain 
                    
                    this.enemies.Remove(enemy);
                    this.disposableObjects.Remove(enemy.name);

                    if (monster is Orc)
                    {
                        RemoveOrcFromFormation(monster);
                    }

                    enemy.SetActive(false);
                }
            }
            else
            {
                damage = enemyData.SimpleDamage;
                this.enemies.Remove(enemy);
                this.disposableObjects.Remove(enemy.name);

                if (monster is Orc)
                {
                    RemoveOrcFromFormation(monster);
                }

                enemy.SetActive(false);
            }

            this.Character.baseStats.XP += enemyData.XPvalue;

            int remainingDamage = damage - this.Character.baseStats.ShieldHP;
            this.Character.baseStats.ShieldHP = Mathf.Max(0, this.Character.baseStats.ShieldHP - damage);

            if (remainingDamage > 0)
            {
                this.Character.baseStats.HP -= remainingDamage;
            }

            this.WorldChanged = true;
        }
    }

    public void EnemyAttack(GameObject enemy)
    {
        if (Time.time > this.enemyAttackCooldown)
        {

            int damage = 0;

            Monster monster = enemy.GetComponent<Monster>();

            if (enemy.activeSelf && monster.InWeaponRange(GameObject.FindGameObjectWithTag("Player")))
            {

                this.Character.AddToDiary(" I was Attacked by " + enemy.name);
                this.enemyAttackCooldown = Time.time + GameConstants.UPDATE_INTERVAL;

                if (this.StochasticWorld)
                {
                    damage = monster.DmgRoll.Invoke();

                    //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                    int attackRoll = RandomHelper.RollD20() + 7;

                    if (attackRoll >= monster.stats.AC)
                    {
                        //there was an hit, enemy is destroyed, gain 
                        this.enemies.Remove(enemy);
                        this.disposableObjects.Remove(enemy.name);

                        if (monster is Orc)
                        {
                            RemoveOrcFromFormation(monster);
                        }

                        enemy.SetActive(false);
                    }
                }
                else
                {
                    damage = monster.stats.SimpleDamage;
                    this.enemies.Remove(enemy);
                    this.disposableObjects.Remove(enemy.name);

                    if (monster is Orc)
                    {
                        RemoveOrcFromFormation(monster);
                    }

                    enemy.SetActive(false);
                }

                this.Character.baseStats.XP += monster.stats.XPvalue;

                int remainingDamage = damage - this.Character.baseStats.ShieldHP;
                this.Character.baseStats.ShieldHP = Mathf.Max(0, this.Character.baseStats.ShieldHP - damage);

                if (remainingDamage > 0)
                {
                    this.Character.baseStats.HP -= remainingDamage;
                    this.Character.AddToDiary(" I was wounded with " + remainingDamage + " damage");
                }

                this.WorldChanged = true;
            }
        }
    }

    public void DivineSmite(GameObject enemy)
    {
        if (enemy != null && enemy.activeSelf && InDivineSmiteRange(enemy) && this.Character.baseStats.Mana >= 2)
        {
            if (enemy.CompareTag("Skeleton"))
            {
                this.Character.baseStats.XP += 3;
                this.Character.AddToDiary(" I Smited " + enemy.name);
                this.enemies.Remove(enemy);
                this.disposableObjects.Remove(enemy.name);
                enemy.SetActive(false);
                //Object.Destroy(enemy);
            }
            this.Character.baseStats.Mana -= 2;

            this.WorldChanged = true;
        }
    }

    public void ShieldOfFaith()
    {
        if (this.Character.baseStats.Mana >= 5)
        {
            this.Character.baseStats.ShieldHP = 5;
            this.Character.baseStats.Mana -= 5;
            this.Character.AddToDiary(" My Shield of Faith will protect me!");
            this.WorldChanged = true;
        }
    }

    public void PickUpChest(GameObject chest)
    {

        if (chest != null && chest.activeSelf && InChestRange(chest))
        {
            this.Character.AddToDiary(" I opened  " + chest.name);
            this.chests.Remove(chest);
            this.disposableObjects.Remove(chest.name);
            chest.SetActive(false);
            this.Character.baseStats.Money += 5;
            this.WorldChanged = true;
        }
    }


    public void GetManaPotion(GameObject manaPotion)
    {
        if (manaPotion != null && manaPotion.activeSelf && InPotionRange(manaPotion))
        {
            this.Character.AddToDiary(" I drank " + manaPotion.name);
            this.disposableObjects.Remove(manaPotion.name);
            manaPotion.SetActive(false);
            this.Character.baseStats.Mana = 10;
            this.WorldChanged = true;
        }
    }

    public void GetHealthPotion(GameObject potion)
    {
        if (potion != null && potion.activeSelf && InPotionRange(potion))
        {
            this.Character.AddToDiary(" I drank " + potion.name);
            this.disposableObjects.Remove(potion.name);
            potion.SetActive(false);
            this.Character.baseStats.HP = this.Character.baseStats.MaxHP;
            this.WorldChanged = true;
        }
    }

    public void LevelUp()
    {
        if (this.Character.baseStats.Level >= 4) return;

        if (this.Character.baseStats.XP >= this.Character.baseStats.Level * 10)
        {
            if (!this.Character.LevelingUp)
            {
                this.Character.AddToDiary(" I am trying to level up...");
                this.Character.LevelingUp = true;
                this.Character.StopTime = Time.time + AutonomousCharacter.LEVELING_INTERVAL;
            }
            else if (this.Character.StopTime < Time.time)
            { 
                this.Character.baseStats.Level++;
                this.Character.baseStats.MaxHP += 10;
                this.Character.baseStats.XP = 0;
                this.Character.AddToDiary(" I leveled up to level " + this.Character.baseStats.Level);
                this.Character.LevelingUp = false;
                this.WorldChanged = true;
            }
        }
    }

    public void LayOnHands()
    {
        if (this.Character.baseStats.Level >= 2 && this.Character.baseStats.Mana >= 7)
        {
            this.Character.AddToDiary(" With my Mana I Lay Hands and recovered all my health.");
            this.Character.baseStats.HP = this.Character.baseStats.MaxHP;
            this.Character.baseStats.Mana -= 7;
            this.WorldChanged = true;
        }
    }

    public void DivineWrath()
    {
        if (this.Character.baseStats.Level >= 3 && this.Character.baseStats.Mana >= 10)
        {
            //kill all enemies in the map
            foreach (var enemy in this.enemies)
            {
                this.Character.baseStats.XP += enemy.GetComponent<Monster>().stats.XPvalue;
                this.Character.AddToDiary(" I used the Divine Wrath and all monsters were killed! \nSo ends a day's work...");
                enemy.SetActive(false);
                this.disposableObjects.Remove(enemy.name);
                Object.Destroy(enemy);
            }

            enemies.Clear();
            this.WorldChanged = true;
        }
    }

    public void Rest()
    {
        if (!this.Character.Resting)
        {
            this.Character.AddToDiary(" I am resting");
            this.Character.Resting = true;
            this.Character.StopTime = Time.time + AutonomousCharacter.RESTING_INTERVAL;
        }
        else if (this.Character.StopTime < Time.time)
        {
            this.Character.baseStats.HP += AutonomousCharacter.REST_HP_RECOVERY;
            this.Character.baseStats.HP = Mathf.Min(this.Character.baseStats.HP, this.Character.baseStats.MaxHP);
            this.Character.Resting = false;
            this.WorldChanged = true;
        }
    }

    public void Teleport()
    {
        if (this.Character.baseStats.Level >= 2 && this.Character.baseStats.Mana >= 5)
        {
            this.Character.AddToDiary(" With my Mana I teleported away from danger.");
            this.Character.transform.position = this.initialPosition;
            this.Character.baseStats.Mana -= 5;
            this.WorldChanged = true;
        }

    }


    private bool CheckRange(GameObject obj, float maximumDistance)
    {
        var distance = (obj.transform.position - this.Character.gameObject.transform.position).sqrMagnitude;
        return distance <= maximumDistance;
    }


    public bool InMeleeRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.PICKUP_RANGE *2);
    }

    public bool InDivineSmiteRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.PICKUP_RANGE * 10);
    }

    public bool InChestRange(GameObject chest)
    {
        return this.CheckRange(chest, GameConstants.PICKUP_RANGE);
    }

    public bool InPotionRange(GameObject potion)
    {
        return this.CheckRange(potion, GameConstants.PICKUP_RANGE);
    }

}
