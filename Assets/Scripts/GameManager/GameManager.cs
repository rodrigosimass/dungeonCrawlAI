// Code by Joao Dias 2015-2019 and Pedro A Santos (2019)

using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Utils;
//using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameManager
{
    public class GameManager : MonoBehaviour
    {
        private const float UPDATE_INTERVAL = 1.0f;
        public const int TIME_LIMIT = 150;
        //public fields, seen by Unity in Editor
        public GameObject character;
        public AutonomousCharacter autonomousCharacter;

        public Text HPText;
        public Text ShieldHPText;
        public Text ManaText;
        public Text TimeText;
        public Text XPText;
        public Text LevelText;
        public Text MoneyText;
        public Text DiaryText;
        public GameObject GameEnd;
        public bool StochasticWorld;
        public bool SleepingNPCs;

        //fields
        public List<GameObject> chests;
        public List<GameObject> skeletons;
        public List<GameObject> orcs;
        public List<GameObject> dragons;
        public List<GameObject> enemies;
        public Dictionary<string, GameObject> disposableObjects;

        public CharacterData characterData;
        public bool WorldChanged { get; set; }
        private DynamicCharacter enemyCharacter;
        private GameObject currentEnemy;
 
        private float nextUpdateTime = 0.0f;
        private float enemyAttackCooldown = 0.0f;
        private Vector3 previousPosition;
        public Vector3 initialPosition;
        public bool gameEnded = false;

        public void Start()
        {
            this.WorldChanged = false;
            this.characterData = new CharacterData(this.character);
            this.previousPosition = this.character.transform.position;
            this.initialPosition = this.character.transform.position;

            this.enemies = new List<GameObject>();
            this.disposableObjects = new Dictionary<string, GameObject>();
            /*-----------------------------------------------------------------------------------------------------------------------------------------------*/
            /*DIFICULTY SELECTION----------------------------------------------------------------------------------------------------------------------------*/
            
            //this.SleepingNPCs = true; this.StochasticWorld = false; //LEVEL 1
            this.SleepingNPCs = false; this.StochasticWorld = false; //LEVEL 2
            //this.SleepingNPCs = false; this.StochasticWorld = true; //LEVEL 3
            /*-----------------------------------------------------------------------------------------------------------------------------------------------*/

            this.chests = GameObject.FindGameObjectsWithTag("Chest").ToList();
            this.skeletons = GameObject.FindGameObjectsWithTag("Skeleton").ToList();
            this.enemies.AddRange(this.skeletons);
            this.orcs = GameObject.FindGameObjectsWithTag("Orc").ToList();
            this.enemies.AddRange(this.orcs);
            this.dragons = GameObject.FindGameObjectsWithTag("Dragon").ToList();
            this.enemies.AddRange(this.dragons);

            //adds all enemies to the disposable objects collection
            foreach(var enemy in this.enemies)
            {
                this.disposableObjects.Add(enemy.name, enemy);
            }
            //add all chests to the disposable objects collection
            foreach (var chest in this.chests)
            {
                this.disposableObjects.Add(chest.name, chest);
            }
            //adds all health potions to the disposable objects collection
            foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion"))
            {
                this.disposableObjects.Add(potion.name, potion);
            }
            //adds all mana potions to the disposable objects collection
            foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion"))
            {
                this.disposableObjects.Add(potion.name, potion);
            }

            this.nextUpdateTime = 0;
            this.enemyAttackCooldown = 0.0f;

        }

        public void Update()
        {

            if (Time.time > this.nextUpdateTime)
            {
                this.nextUpdateTime = Time.time + UPDATE_INTERVAL;
                this.characterData.Time += UPDATE_INTERVAL;

                
            }

            if (!this.SleepingNPCs)
            {
                if (enemyCharacter != null && currentEnemy != null && currentEnemy.activeSelf)
                {
                    if ((currentEnemy.transform.position - this.character.transform.position).sqrMagnitude > 2500)
                    {
                        enemyCharacter.Movement = null;
                        currentEnemy = null;
                    }
                    else
                    {
                        this.enemyCharacter.Movement.Target.position = this.character.transform.position;
                        this.enemyCharacter.Update();
                        this.EnemyAttack(currentEnemy);
                    }
                }
                else
                {
                    foreach (var enemy in this.enemies)
                    {
                        if ((enemy.transform.position - this.character.transform.position).sqrMagnitude <= 1000)
                        {
                            this.currentEnemy = enemy;
                            this.enemyCharacter = new DynamicCharacter(enemy)
                            {
                                MaxSpeed = 50
                            };
                            enemyCharacter.Movement = new DynamicSeek()
                            {
                                Character = enemyCharacter.KinematicData,
                                MaxAcceleration = 50,
                                Target = new IAJ.Unity.Movement.KinematicData()
                            };

                            break;
                        }
                    }
                }
            }

            this.HPText.text = "HP: " + this.characterData.HP;
            this.XPText.text = "XP: " + this.characterData.XP;
            this.ShieldHPText.text = "Shield HP: " + this.characterData.ShieldHP;
            this.LevelText.text = "Level: " + this.characterData.Level;
            this.TimeText.text = "Time: " + this.characterData.Time;
            this.ManaText.text = "Mana: " + this.characterData.Mana;
            this.MoneyText.text = "Money: " + this.characterData.Money;

            if(this.characterData.HP <= 0 || this.characterData.Time >= TIME_LIMIT)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "Game Over";
            }
            else if(this.characterData.Money >= 25)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "Victory";
            }
        }

        public void SwordAttack(GameObject enemy)
        {
            int damage = 0;

            NPC enemyData = enemy.GetComponent<NPC>();

            if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
            {
                this.autonomousCharacter.DiaryText.text += Time.time + " I Sword Attacked " + enemy.name + "\n";

                if (this.StochasticWorld)
                {
                    damage = enemyData.dmgRoll.Invoke();
 
                    //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                    int attackRoll = RandomHelper.RollD20() + 7;

                    if (attackRoll >= enemyData.AC)
                    {
                        //there was an hit, enemy is destroyed, gain xp
                        this.enemies.Remove(enemy);
                        this.disposableObjects.Remove(enemy.name);
                        enemy.SetActive(false);
                        Object.Destroy(enemy);
                    }
                }
                else
                {
                    damage = enemyData.simpleDamage;
                    this.enemies.Remove(enemy);
                    this.disposableObjects.Remove(enemy.name);
                    enemy.SetActive(false);
                    Object.Destroy(enemy);
                }

                this.characterData.XP += enemyData.XPvalue;

                int remainingDamage = damage - this.characterData.ShieldHP;
                this.characterData.ShieldHP = Mathf.Max(0, this.characterData.ShieldHP - damage);

                if (remainingDamage > 0)
                {
                    this.characterData.HP -= remainingDamage;
                    this.autonomousCharacter.DiaryText.text += Time.time + " I was wounded with " + remainingDamage + " damage\n";
                }

                this.WorldChanged = true;
            }
        }

        public void EnemyAttack(GameObject enemy)
        {
            if(Time.time > this.enemyAttackCooldown)
            {
                
                int damage = 0;

                NPC enemyData = enemy.GetComponent<NPC>();

                if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
                {
                    
                    this.autonomousCharacter.DiaryText.text += Time.time + " I was Attacked by " + enemy.name + "\n";
                    this.enemyAttackCooldown = Time.time + UPDATE_INTERVAL;

                    if (this.StochasticWorld)
                    {
                        damage = enemyData.dmgRoll.Invoke();

                        //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                        int attackRoll = RandomHelper.RollD20() + 7;

                        if (attackRoll >= enemyData.AC)
                        {
                            //there was an hit, enemy is destroyed, gain xp
                            this.enemies.Remove(enemy);
                            this.disposableObjects.Remove(enemy.name);
                            enemy.SetActive(false);
                            Object.Destroy(enemy);
                        }
                    }
                    else
                    {
                        damage = enemyData.simpleDamage;
                        this.enemies.Remove(enemy);
                        this.disposableObjects.Remove(enemy.name);
                        enemy.SetActive(false);
                        Object.Destroy(enemy);
                    }

                    this.characterData.XP += enemyData.XPvalue;

                    int remainingDamage = damage - this.characterData.ShieldHP;
                    this.characterData.ShieldHP = Mathf.Max(0, this.characterData.ShieldHP - damage);

                    if (remainingDamage > 0)
                    {
                        this.characterData.HP -= remainingDamage;
                        this.autonomousCharacter.DiaryText.text += Time.time + " I was wounded with " + remainingDamage + " damage\n";
                    }

                    this.WorldChanged = true;
                }
            }
        }

        public void DivineSmite(GameObject enemy)
        {
            if (enemy != null && enemy.activeSelf && InDivineSmiteRange(enemy) && this.characterData.Mana >= 2)
            {
                if (enemy.tag.Equals("Skeleton"))
                {
                    this.characterData.XP += 3;
                    this.autonomousCharacter.DiaryText.text += Time.time + " I Smited " + enemy.name + ".\n";
                    this.enemies.Remove(enemy);
                    this.disposableObjects.Remove(enemy.name);
                    enemy.SetActive(false);
                    Object.Destroy(enemy);
                }
                this.characterData.Mana -= 2;

                this.WorldChanged = true;
            }
        }

        public void ShieldOfFaith()
        {
            if (this.characterData.Mana >= 5)
            {
                this.characterData.ShieldHP = 5;
                this.characterData.Mana -= 5;
                this.autonomousCharacter.DiaryText.text += Time.time + " My Shield of Faith will protect me!\n";
                this.WorldChanged = true;
            }
        }

        public void LayOnHands()
        {
            if (this.characterData.Level >= 2 && this.characterData.Mana >= 7)
            {
                this.autonomousCharacter.DiaryText.text += Time.time + " With my Mana I Lay Hands and recovered all my health.\n";
                this.characterData.HP = this.characterData.MaxHP;
                this.characterData.Mana -= 7;
                this.WorldChanged = true;
            }
        }

        public void DivineWrath()
        {
            if(this.characterData.Level >= 3 && this.characterData.Mana >= 10)
            {
                //kill all enemies in the map
                foreach (var enemy in this.enemies)
                {
                    this.characterData.XP += enemy.GetComponent<NPC>().XPvalue;
                    this.autonomousCharacter.DiaryText.text += Time.time + " I used the Divine Wrath and all monsters were killed! \nSo ends a day's work...";
                    enemy.SetActive(false);
                    this.disposableObjects.Remove(enemy.name);
                    Object.Destroy(enemy);
                }

                enemies.Clear();
                this.WorldChanged = true;
            }
        }

        public void PickUpChest(GameObject chest)
        {
            if (chest != null && chest.activeSelf && InChestRange(chest))
            {
                this.autonomousCharacter.DiaryText.text += Time.time + " I opened  " + chest.name + "\n";
                this.chests.Remove(chest);
                this.disposableObjects.Remove(chest.name);
                Object.Destroy(chest);
                this.characterData.Money += 5;
                this.WorldChanged = true;
            }
        }

        public void LevelUp()
        {
            if (this.characterData.Level >= 4) return;

            if(this.characterData.XP >= this.characterData.Level*10)
            {
                this.characterData.Level++;
                this.characterData.MaxHP += 10;
                this.characterData.XP = 0;
                this.WorldChanged = true;
                this.autonomousCharacter.DiaryText.text += Time.time + " I leveled up to level " + this.characterData.Level + "\n";
            }
        }

        public void GetManaPotion(GameObject manaPotion)
        {
            if (manaPotion != null && manaPotion.activeSelf && InPotionRange(manaPotion))
            {
                this.autonomousCharacter.DiaryText.text += Time.time + " I drank  " + manaPotion.name + "\n";
                this.disposableObjects.Remove(manaPotion.name);
                Object.Destroy(manaPotion);
                this.characterData.Mana = 10;
                this.WorldChanged = true;
            }
        }

        public void GetHealthPotion(GameObject potion)
        {
            if (potion != null && potion.activeSelf && InPotionRange(potion))
            {
                this.autonomousCharacter.DiaryText.text += Time.time + " I drank  " + potion.name + "\n";
                this.disposableObjects.Remove(potion.name);
                Object.Destroy(potion);
                this.characterData.HP = this.characterData.MaxHP;
                this.WorldChanged = true;
            }
        }

        public void Rest()
        {
            if (!this.autonomousCharacter.Resting)
            {
                this.autonomousCharacter.DiaryText.text += Time.time + " I am resting\n";
                this.autonomousCharacter.Resting = true;
                this.autonomousCharacter.StopRestTime = Time.time + AutonomousCharacter.RESTING_INTERVAL;
            }
            else if (this.autonomousCharacter.StopRestTime < Time.time)
            {
                this.characterData.HP += AutonomousCharacter.REST_HP_RECOVERY;
                this.characterData.HP = Mathf.Min(this.characterData.HP, this.characterData.MaxHP);
                this.autonomousCharacter.Resting = false;
                this.WorldChanged = true;
            }
        }

        public void Teleport()
        {
            if (this.characterData.Level >= 2 && this.characterData.Mana >= 5)
            {
                this.autonomousCharacter.DiaryText.text += Time.time + " With my Mana I teleported away from danger.\n";
                this.autonomousCharacter.Character.KinematicData.position  = this.initialPosition;
                this.autonomousCharacter.Character.Movement = null;
                this.characterData.Mana -= 5;
                this.WorldChanged = true;
            }

        }



        private bool CheckRange(GameObject obj, float maximumSqrDistance)
        {
            return (obj.transform.position - this.characterData.CharacterGameObject.transform.position).sqrMagnitude <= maximumSqrDistance;
        }


        public bool InMeleeRange(GameObject enemy)
        {
            return this.CheckRange(enemy, 16.0f);
        }

        public bool InDivineSmiteRange(GameObject enemy)
        {
            return this.CheckRange(enemy, 400.0f);
        }

        public bool InChestRange(GameObject chest)
        {
            return this.CheckRange(chest, 16.0f);
        }

        public bool InPotionRange(GameObject potion)
        {
            return this.CheckRange(potion, 16.0f);
        }
    }
}
