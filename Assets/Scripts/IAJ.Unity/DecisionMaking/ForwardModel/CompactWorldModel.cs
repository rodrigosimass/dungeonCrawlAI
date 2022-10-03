using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    public class CompactWorldModel
    {
        private float[]  properties { get; set; }
        //0:MANA 1:XP 2:MAXHP 3:HP 4:SHIELD 5:MONEY 6:TIME 7:LEVEL
        public Vector3 position { get; set; }
        public bool[] resources { get; set; }
        //0:HPPOT1 1:HPPOT2 2:MANAPOT1 3:MANAPOT2 4:CHEST1 5:CHEST2 6:CHEST3 7:CHEST4 8:CHEST5 
        public bool[] enemies { get; set; }
        //0:SKELY1 1:SKELY2 2:ORC1 3:ORC2 4:DRAKE
        public List<Action> Actions { get; set; }
        protected IEnumerator<Action> ActionEnumerator { get; set; } 
        private Dictionary<string, float> GoalValues { get; set; } 

        protected CompactWorldModel Parent { get; set; }

        public CompactWorldModel(List<Action> actions, GameManager.GameManager gm)
        {
            this.properties = new float[8];
            //TODO dow e need to init with values for start of game?
            this.properties[0]= gm.characterData.Mana; //MANA
            this.properties[1]= gm.characterData.XP; //XP
            this.properties[2]= gm.characterData.MaxHP; //MAXHP
            this.properties[3]= gm.characterData.HP; //HP
            this.properties[4]= gm.characterData.ShieldHP; //SHIELD
            this.properties[5]= gm.characterData.Money; //MONEY
            this.properties[6]= gm.characterData.Time; //TIME
            this.properties[7]= gm.characterData.Level; //LEVEL

            this.position = new Vector3();
            this.position = gm.characterData.CharacterGameObject.transform.position;
            
            this.enemies = new bool[5];
            var enems = gm.enemies;
            foreach (var enemy in enems) {
                var tag = enemy.tag;
                if(tag=="Skeleton1")
                    enemies[0] = true;
                else if(tag=="Skeleton2")
                    enemies[1] = true; 
                else if(tag=="Orc1")
                    enemies[2] = true;
                else if(tag=="Orc2")
                    enemies[3] = true;
                else if(tag=="Dragon")
                    enemies[4] = true;
            }            
            this.resources = new bool[9];
            foreach (var enemy in enems) {
                var tag = enemy.tag;
                if(tag=="HealthPotion1")
                    enemies[0] = true;
                else if(tag=="HealthPotion2")
                    enemies[1] = true; 
                else if(tag=="ManaPotion1")
                    enemies[2] = true;
                else if(tag=="ManaPotion2")
                    enemies[3] = true;
                else if(tag=="Chest1")
                    enemies[4] = true;
                else if(tag=="Chest2")
                    enemies[4] = true;
                else if(tag=="Chest3")
                    enemies[4] = true;
                else if(tag=="Chest4")
                    enemies[4] = true;
                else if(tag=="Chest5")
                    enemies[4] = true;
            }           
            this.Actions = actions;
            this.ActionEnumerator = actions.GetEnumerator();
        }
        
        public CompactWorldModel(List<Action> actions)
        {
            this.properties = new float[8];
            this.resources = new bool[9];
            this.enemies = new bool[5];
            this.Actions = actions;
            this.position = new Vector3();
            this.ActionEnumerator = actions.GetEnumerator();
        }

        public CompactWorldModel(CompactWorldModel parent)
        {
            this.Parent = parent;
            this.properties = new float[8];
            this.resources = new bool[9];
            this.enemies = new bool[5];
            this.position = new Vector3();
        }

        public virtual object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Mana":
                    return properties[0];
                case "XP":
                    return properties[1];
                case "MAXHP":
                    return properties[2];
                case "HP":
                    return properties[3];
                case "ShieldHP":
                    return properties[4];
                case "Money":
                    return properties[5];
                case "Time":
                    return properties[6];
                case "Level":
                    return properties[7];
                case "Position":
                    return this.position;
                default:
                    return null;
            }
        }

        public virtual void SetProperty(string propertyName, object value)
        {
            if(propertyName=="Position") {
                position = (Vector3)value;
                return;
            }
            int i = 0;
            switch (propertyName)
            {
                case "Mana":
                    i=0;
                    break;
                case "XP":
                    i=1;
                    break;
                case "MAXHP":
                    i=2;
                    break;
                case "HP":
                    i=3;
                    break;
                case "ShieldHP":
                    i=4;
                    break;
                case "Money":
                    i=5;
                    break;
                case "Time":
                    i=6;
                    break;
                case "Level":
                    i=7;
                    break;
            }
            this.properties[i] = (float)value;
        }

        public virtual CompactWorldModel GenerateChildWorldModel()
        {
            return new CompactWorldModel(this);
        }

        public virtual Action GetNextAction()
        {
            Action action = null;
            //returns the next action that can be executed or null if no more executable actions exist
            if (this.ActionEnumerator.MoveNext())
            {
                action = this.ActionEnumerator.Current;
            }

            while (action != null && !action.CanExecute(this))
            {
                if (this.ActionEnumerator.MoveNext())
                {
                    action = this.ActionEnumerator.Current;    
                }
                else
                {
                    action = null;
                }
            }

            return action;
        }

        public virtual Action[] GetExecutableActions()
        {
            return this.Actions.Where(a => a.CanExecute(this)).ToArray();
        }

        public virtual bool IsTerminal()
        {
            return true;
        }
        

        public virtual float GetScore()
        { 
            return 0.0f;
        }

        public virtual int GetNextPlayer()
        {
            return 0;
        }

        public virtual void CalculateNextPlayer()
        {
        }
    }
}
