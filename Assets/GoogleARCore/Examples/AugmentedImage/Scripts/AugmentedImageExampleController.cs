//-----------------------------------------------------------------------
// <copyright file="AugmentedImageExampleController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.AugmentedImage
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class AugmentedImageExampleController : MonoBehaviour
    {
        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;


        // Sam's Stuff
        public Texture Test_T;
        public GameObject Test_M;

        public Text Enemy_Health_Text;
        public Text Debug;
        public Text Elements_Text;
        public Text Player_Health_Text;
        private int Poly_Health = 15;
        private int Bed_Health = 15;
        private int Jap_Health = 15;
        private int Zoro_Health = 15;

        enum Player
        {
            None,
            Poly,
            Bed,
            Jap,
            Zoro
        }

        Player player = Player.None;




        #region Enemies
        //Enemy Images:
        public Texture Ifrit_T;
        public Texture Ghoul_T;
        public Texture Angra_Mainyu_T;
        public Texture Roc_T;
        public Texture Manticore_T;
        public Texture Takam_T;
        //Enemy Models: (enemy game objects will have an integer tied to them representing health) (idk how i'm going to represent all of the stupid interactions in this god awful game) (maybe a number to represent their status with a giant switch table to represent every god damn effect in the game)
        public GameObject Ifrit_M;
        public GameObject Ghoul_M;
        public GameObject Angra_Mainyu_M;
        public GameObject Roc_M;
        public GameObject Manticore_M;
        public GameObject Takam_M;
        
        #endregion

        #region Elements
        //Element Images:
        public Texture Fire_T;
        public Texture Water_T;
        public Texture Air_T;
        public Texture Earth_T;
        //Element Models:
        public GameObject Fire_M;
        public GameObject Water_M;
        public GameObject Air_M;
        public GameObject Earth_M;

        //Combined element models:
        public GameObject Lightning_M;
        public GameObject Magma_M;
        public GameObject Steam_M;
        public GameObject Ice_M;
        #endregion

        #region Spells
        //Spell Models: (spell game objects will have a integer tied to them representing damage and perhaps another representing an effect)
        public GameObject Energy_Ball_M;
        public GameObject Perma_Frost_M;
        public GameObject Eruption_M;
        public GameObject One_Million_Volts_M;

        #endregion

        #region Items
        //Item Images:
        public Texture Healing_Potion_T;
        //Item Models:
        public GameObject Healing_Potion_M;
        #endregion

        #region Wizards
        //Wizard Images:
        public Texture Polynesian_T;
        public Texture Zoroastrianism_T;
        public Texture Beduin_T;
        public Texture Japanese_T;
        //Wizard Models: (wizard models will keep track of wizard health via some integer)
        public GameObject Polynesian_M;
        public GameObject Zoroastrianism_M;
        public GameObject Beduin_M;
        public GameObject Japanese_M;
        #endregion

        //Cast Card
        public Texture Cast_T;

        //List of enemies active with the first being the one targeted by the spell
        private List<GameObject> Enemies = new List<GameObject>();

        //List of the player game models that are in the game
        //TODO: need to make code for a list of item game objects tied to each player character game object
        private List<GameObject> Players = new List<GameObject>();

        //A list of elements currently in play
        private List<GameObject> Elements = new List<GameObject>();
        
        //List of individual elements in the list
        private List<GameObject> Airs = new List<GameObject>();
        private List<GameObject> Earths = new List<GameObject>();
        private List<GameObject> Fires = new List<GameObject>();
        private List<GameObject> Waters = new List<GameObject>();

        //Average postion of all element cards in play
        private Vector3 avg_pos;

        //Current spell enemy is trying to cast
        private Texture Active_Spell_T;
        private GameObject Active_Spell_M;

        //The player that is currently going
        private GameObject Active_Player;

        //The enemy that is currently being attacked
        private GameObject Active_Enemy;

        //The Active enemies status
        private Status.State Enemy_State;

        // Time to move to a location
        //private float t = 0;
        //private float t_final = 0;
        public float speed = 0.1f; //meant to replace t and t_final
        private float step = 0;

        //handles cooldown of scanning elements
        private float cooldown;
        private float cooldown_Period = 1;

        //see if cast has been played
        private bool cast_Tracked = false;

        private bool merge_finished = false;

        public GameObject kaboomAnim;
        public GameObject mergeAnim;
        private GameObject kaboomAnim_Active;
        private GameObject mergeAnim_Active;

        private bool spellfizzlecheck = false;


        // start the game by initiating the Active game objects with some test ones
        public void Start()
        {
            Active_Enemy = Test_M;

            Active_Spell_T = null;
            Active_Spell_M = null;
            FitToScanOverlay.SetActive(true);
            Poly_Health = 15;
            Bed_Health = 15;
            Jap_Health = 15;
            Zoro_Health = 15;
            kaboomAnim_Active = null;
            mergeAnim_Active = null;
            spellfizzlecheck = false;
        }


        /// <summary>
        /// The Unity Update method.
        /// </summary>
        public void Update()
        {
            step = Time.deltaTime * speed;

            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            Elements_Text.text = "Airs: " + Airs.Count.ToString() + " Earths: " + Earths.Count.ToString() + " Fires: " + Fires.Count.ToString() + " Waters: " + Waters.Count.ToString();

            if (Input.touchCount > 0)
            {
                // Get updated augmented images for this frame.
                Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

                // very exsperimental not sure if actual solution (deletes all but the last in the list which should be the scanned item)
                if(m_TempAugmentedImages.Count > 1)
                {
                    m_TempAugmentedImages.RemoveRange(0, m_TempAugmentedImages.Count - 1);
                }
                

                //Looks at all the images in the database and see if any of them are currently present
                foreach (var image in m_TempAugmentedImages)
                {

                    if (image.TrackingState == TrackingState.Tracking)
                    {

                        #region Enemy_Detection
                        //Ifrit
                        if (image.Name == Ifrit_T.name)
                        {
                            Debug.text = "Ifrit Detected";
                            //Check if the Enemy is already active
                            if (Enemies.Contains(Ifrit_M))
                            {
                                //Move the Enemy game object to new location
                                Ifrit_M.transform.SetPositionAndRotation(image.CenterPose.position, image.CenterPose.rotation);
                            }
                            else
                            {
                                //Spawn Enemy
                                Ifrit_M = Instantiate(Ifrit_M, image.CenterPose.position, image.CenterPose.rotation);
                                //Add enemy to the Enemies list
                                Enemies.Add(Ifrit_M);
                            }
                            //Set the Enemy as the active Enemy
                            Active_Enemy = Ifrit_M;
                        }
                        //Ghoul
                        if (image.Name == Ghoul_T.name)
                        {
                            Debug.text = "Ghoul Detected";
                            //Check if the Enemy is already active
                            if (Enemies.Contains(Ghoul_M))
                            {
                                //Move the Enemy game object to new location
                                Ghoul_M.transform.SetPositionAndRotation(image.CenterPose.position, image.CenterPose.rotation);
                            }
                            else
                            {
                                //Spawn Enemy
                                Ghoul_M = Instantiate(Ghoul_M, image.CenterPose.position, image.CenterPose.rotation);
                                //Add enemy to the Enemies list
                                Enemies.Add(Ghoul_M);
                            }
                            //Set the Enemy as the active Enemy
                            Active_Enemy = Ghoul_M;
                        }
                        //Takam
                        if (image.Name == Takam_T.name)
                        {
                            Debug.text = "Takam Detected";
                            //Check if the Enemy is already active
                            if (Enemies.Contains(Takam_M))
                            {
                                //Move the Enemy game object to new location
                                Takam_M.transform.SetPositionAndRotation(image.CenterPose.position, image.CenterPose.rotation);
                            }
                            else
                            {
                                //Spawn Enemy
                                Takam_M = Instantiate(Takam_M, image.CenterPose.position, image.CenterPose.rotation);
                                //Add enemy to the Enemies list
                                Enemies.Add(Takam_M);
                            }
                            //Set the Enemy as the active Enemy
                            Active_Enemy = Takam_M;
                        }
                        //Manticore
                        if (image.Name == Manticore_T.name)
                        {
                            Debug.text = "Manticore Detected";
                            //Check if the Enemy is already active
                            if (Enemies.Contains(Manticore_M))
                            {
                                //Move the Enemy game object to new location
                                Manticore_M.transform.SetPositionAndRotation(image.CenterPose.position, image.CenterPose.rotation);
                            }
                            else
                            {
                                //Spawn Enemy
                                Manticore_M = Instantiate(Manticore_M, image.CenterPose.position, image.CenterPose.rotation);
                                //Add enemy to the Enemies list
                                Enemies.Add(Manticore_M);
                            }
                            //Set the Enemy as the active Enemy
                            Active_Enemy = Manticore_M;
                        }
                        //Angra
                        if (image.Name == Angra_Mainyu_T.name)
                        {
                            Debug.text = "Angra Mainyu Detected";
                            //Check if the Enemy is already active
                            if (Enemies.Contains(Angra_Mainyu_M))
                            {
                                //Move the Enemy game object to new location
                                Angra_Mainyu_M.transform.SetPositionAndRotation(image.CenterPose.position, image.CenterPose.rotation);
                            }
                            else
                            {
                                //Spawn Enemy
                                Angra_Mainyu_M = Instantiate(Angra_Mainyu_M, image.CenterPose.position, image.CenterPose.rotation);
                                //Add enemy to the Enemies list
                                Enemies.Add(Angra_Mainyu_M);
                            }
                            //Set the Enemy as the active Enemy
                            Active_Enemy = Angra_Mainyu_M;
                        }
                        //Roc
                        if (image.Name == Roc_T.name)
                        {
                            Debug.text = "Roc Detected";
                            //Check if the Enemy is already active
                            if (Enemies.Contains(Roc_M))
                            {
                                //Move the Enemy game object to new location
                                Roc_M.transform.SetPositionAndRotation(image.CenterPose.position, image.CenterPose.rotation);
                            }
                            else
                            {
                                //Spawn Enemy
                                Roc_M = Instantiate(Roc_M, image.CenterPose.position, image.CenterPose.rotation);
                                //Add enemy to the Enemies list
                                Enemies.Add(Roc_M);
                            }
                            //Set the Enemy as the active Enemy
                            Active_Enemy = Roc_M;
                        }
                        #endregion


                        #region Element_Detection
                        if (cooldown <= Time.time)
                        {
                            //Air
                            if (image.Name == Air_T.name)
                            {
                                cooldown = Time.time + cooldown_Period;
                                Debug.text = "Air Detected";
                                //add element to list of active elements and spawn it
                                GameObject air = Instantiate(Air_M, image.CenterPose.position, image.CenterPose.rotation);
                                Elements.Add(air);
                                Airs.Add(air);
                            }
                            //Water
                            else if (image.Name == Water_T.name)
                            {
                                cooldown = Time.time + cooldown_Period;
                                Debug.text = "Water Detected";
                                //add element to list of active elements and spawn it
                                GameObject water = Instantiate(Water_M, image.CenterPose.position, image.CenterPose.rotation);
                                Elements.Add(water);
                                Waters.Add(water);
                            }
                            //Fire
                            else if (image.Name == Fire_T.name)
                            {
                                cooldown = Time.time + cooldown_Period;
                                Debug.text = "Fire Detected";
                                //add element to list of active elements and spawn it
                                GameObject fire = Instantiate(Fire_M, image.CenterPose.position, image.CenterPose.rotation);
                                Elements.Add(fire);
                                Fires.Add(fire);
                            }
                            else if (image.Name == Earth_T.name)
                            {
                                cooldown = Time.time + cooldown_Period;
                                Debug.text = "Earth Detected";
                                //add element to list of active elements and spawn it
                                GameObject earth = Instantiate(Earth_M, image.CenterPose.position, image.CenterPose.rotation);
                                Elements.Add(earth);
                                Earths.Add(earth);
                            }
                        }
                        #endregion

                        #region Player_Detection
                        if (image.Name == Polynesian_T.name)
                        {
                            Debug.text = "Trickster Detected";
                            player = Player.Poly;
                        }
                        if (image.Name == Japanese_T.name)
                        {
                            Debug.text = "Priest Detected";
                            player = Player.Jap;
                        }
                        if (image.Name == Beduin_T.name)
                        {
                            Debug.text = "Nomad Detected";
                            player = Player.Bed;
                        }
                        if (image.Name == Zoroastrianism_T.name)
                        {
                            Debug.text = "Magi Detected";
                            player = Player.Zoro;
                        }
                        #endregion

                        //Cast Card ie. spell algorithm/switch
                        //TODO: make a check for all spells that can be cast with current elemetns going from most complex to least
                        if (image.Name == Cast_T.name)
                        {
                            if (cast_Tracked == false)
                            {
                                // get the average location of all elments currently in play
                                foreach (GameObject element in Elements)
                                {
                                    avg_pos += element.transform.position;
                                }
                                avg_pos = avg_pos / Elements.Count;
                            }
                            cast_Tracked = true;
                            Debug.text = "Cast Detected";
                        }
                    }
                }
            }



            #region Casting
            if (cast_Tracked)
            {
                //casting now begins
                //Debug.text = "Cast Elements";
                //Will be a combined element or single element cast
                #region Air
                if (Airs.Count >= 1)
                {
                    //if only airs and at least 2 airs
                    if (Airs.Count >= 2 && Fires.Count == 0 && Earths.Count == 0 && Waters.Count == 0)
                    {
                        spellfizzlecheck = true;
                        // move elements to one spot
                        // if all airs are in one spot
                        //delete all airs 
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                //form master air with damage equal to 2 * (# of airs -1) at the merge position
                                Active_Spell_M = Instantiate(Air_M, avg_pos, Quaternion.identity);
                                Active_Spell_M.GetComponent<Status>().Dmg = 2 * (Airs.Count - 1);
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            // send master air at enemy
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            // once master air gets to enemy blow up and deal damage
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Normal);
                            }
                        }
                    }

                    // ice
                    if (Waters.Count >= 1 && Airs.Count == Waters.Count && Fires.Count == 0 && Earths.Count == 0)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(Ice_M, avg_pos, Quaternion.identity);
                                // ice deals no damage but applies frozen status
                                Active_Spell_M.GetComponent<Status>().Dmg = 0;
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Frozen);
                            }
                        }
                    }

                    // Lightning
                    if (Earths.Count >= 1 && Airs.Count == Earths.Count && Fires.Count == 0 && Waters.Count == 0)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(Lightning_M, avg_pos, Quaternion.identity);
                                // ice deals no damage but applies frozen status
                                Active_Spell_M.GetComponent<Status>().Dmg = 4 * (Earths.Count - 1);
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Normal);
                            }
                        }
                    }

                    // Energy ball
                    if (Waters.Count == 2 && Airs.Count == 1 && Fires.Count == 3 && Earths.Count == 0)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(Energy_Ball_M, avg_pos, Quaternion.identity);
                                // ice deals no damage but applies frozen status
                                Active_Spell_M.GetComponent<Status>().Dmg = 8;
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Steamed);
                            }
                        }
                    }

                    //Perma Frost
                    if (Waters.Count == 3 && Airs.Count == 2 && Fires.Count == 0 && Earths.Count == 1)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(Perma_Frost_M, avg_pos, Quaternion.identity);
                                // ice deals no damage but applies frozen status
                                Active_Spell_M.GetComponent<Status>().Dmg = 5;
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Frozen);
                            }
                        }
                    }

                    //One Million Volts
                    if (Waters.Count == 0 && Airs.Count == 3 && Fires.Count == 1 && Earths.Count == 2)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(One_Million_Volts_M, avg_pos, Quaternion.identity);
                                // ice deals no damage but applies frozen status
                                Active_Spell_M.GetComponent<Status>().Dmg = 5;
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Normal);
                            }
                        }
                    }

                }
                #endregion
                #region Fire
                else if (Fires.Count >= 1)
                {
                    //if only Fire and at least 2 fire
                    if (Fires.Count >= 2 && Airs.Count == 0 && Earths.Count == 0 && Waters.Count == 0)
                    {
                        spellfizzlecheck = true;
                        //delete all fire 
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                //form master air with damage equal to 2 * (# of airs -1) at the merge position
                                Active_Spell_M = Instantiate(Fire_M, avg_pos, Quaternion.identity);
                                Active_Spell_M.GetComponent<Status>().Dmg = 2 * (Fires.Count - 1);
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            // send master air at enemy
                            //TODO changing t to figure out merge time and stuff
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, Time.deltaTime);
                            // once master air gets to enemy blow up and deal damage
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Normal);
                            }
                        }
                    }

                    // steam
                    if (Waters.Count >= 1 && Fires.Count == Waters.Count && Airs.Count == 0 && Earths.Count == 0)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(Steam_M, avg_pos, Quaternion.identity);
                                Active_Spell_M.GetComponent<Status>().Dmg = 3 * (Fires.Count - 1);
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Steamed);
                            }
                        }
                    }

                    // Magma
                    if (Earths.Count >= 1 && Fires.Count == Earths.Count && Airs.Count == 0 && Waters.Count == 0)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(Magma_M, avg_pos, Quaternion.identity);
                                Active_Spell_M.GetComponent<Status>().Dmg = 3 * (Fires.Count - 1);
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Impaired);
                            }
                        }
                    }

                    // Eruption
                    if (Waters.Count == 1 && Airs.Count == 0 && Fires.Count == 2 && Earths.Count == 3)
                    {
                        spellfizzlecheck = true;
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                Active_Spell_M = Instantiate(Eruption_M, avg_pos, Quaternion.identity);
                                Active_Spell_M.GetComponent<Status>().Dmg = 9;
                            }
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Impaired);
                            }
                        }
                    }
                }
                #endregion

                #region Water
                else if (Waters.Count >= 1)
                {
                    //if only Water and at least 2 Water
                    if (Waters.Count >= 2 && Airs.Count == 0 && Earths.Count == 0 && Fires.Count == 0)
                    {
                        spellfizzlecheck = true;
                        //delete all fire 
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                //form master air with damage equal to 2 * (# of airs -1) at the merge position
                                Active_Spell_M = Instantiate(Water_M, avg_pos, Quaternion.identity);
                                Active_Spell_M.GetComponent<Status>().Dmg = 2 * (Waters.Count - 1);
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            // send master air at enemy
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            // once master air gets to enemy blow up and deal damage
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Normal);
                            }
                        }
                    }
                }
                #endregion

                #region Earth
                else if (Earths.Count >= 1)
                {
                    //if only Earth and at least 2 Earth
                    if (Earths.Count >= 2 && Airs.Count == 0 && Fires.Count == 0 && Waters.Count == 0)
                    {
                        spellfizzlecheck = true;
                        //delete all fire 
                        if (!merge_finished)
                        {
                            if (Merge(Elements))
                            {
                                merge_finished = true;
                            }
                        }
                        if (merge_finished)
                        {
                            if (Active_Spell_M == null)
                            {
                                //form master air with damage equal to 2 * (# of airs -1) at the merge position
                                Active_Spell_M = Instantiate(Earth_M, avg_pos, Quaternion.identity);
                                Active_Spell_M.GetComponent<Status>().Dmg = 2 * (Earths.Count - 1);
                                Active_Spell_M.transform.localScale = Active_Spell_M.transform.localScale * (1f + 0.25f * (Airs.Count + Fires.Count + Earths.Count + Waters.Count));
                            }
                            // send master air at enemy
                            Active_Spell_M.transform.position = Vector3.MoveTowards(Active_Spell_M.transform.position, Active_Enemy.transform.position, step);
                            // once master air gets to enemy blow up and deal damage
                            if (Active_Spell_M.transform.position == Active_Enemy.transform.position)
                            {
                                SpellHit(Active_Spell_M, Status.State.Normal);
                            }
                        }
                    }
                }
                #endregion

                //spell fizzle check to see if corect spell was found
                if (!spellfizzlecheck)
                {
                    Spell_Fizzle();
                }
            }
            #endregion

            #region Player_Update
            switch (player)
            {
                case Player.None:
                    break;
                case Player.Poly:
                    Player_Health_Text.text = "Trickster HP: " + Poly_Health.ToString();
                    break;
                case Player.Bed:
                    Player_Health_Text.text = "Nomad HP: " + Bed_Health.ToString();
                    break;
                case Player.Jap:
                    Player_Health_Text.text = "Preist HP: " + Jap_Health.ToString();
                    break;
                case Player.Zoro:
                    Player_Health_Text.text = "Magi HP: " + Zoro_Health.ToString();
                    break;
                default:
                    break;
            }
            #endregion

            #region Enemy_Update
            //update enemy health
            //Enemy_Health = Active_Enemy.GetComponent(typeof(Health)) as Health;

            //update display of the active enemy's health
            if (Active_Enemy != null)
            {
                Enemy_Health_Text.text = "Enemy Health : " + Active_Enemy.GetComponent<Status>().HP.ToString() + "\n" + " State: " + Active_Enemy.GetComponent<Status>().getState().ToString();
            }
            #endregion

            if (kaboomAnim_Active.GetComponent<ParticleSystem>())
            {
                if (!kaboomAnim_Active.GetComponent<ParticleSystem>().IsAlive())
                {
                    Destroy(kaboomAnim_Active);
                }
            }
        }

        //changes the Enemy_Health of the active Enemy when a spell hits them
        public void SpellHit(GameObject spell, Status.State status)
        {
            Debug.text = "Spell Hit";
            kaboomAnim_Active = Instantiate(kaboomAnim, Active_Enemy.transform.position, Quaternion.identity);

            //Apply damage
            if (Active_Enemy.GetComponent<Status>().affinity != Status.Affinity.None)
            {
                if (spell.GetComponent<Status>().affinity == Active_Enemy.GetComponent<Status>().affinity)
                {
                    Active_Enemy.GetComponent<Status>().HP -= spell.GetComponent<Status>().Dmg - 2;
                }
                else if (spell.GetComponent<Status>().affinity == Active_Enemy.GetComponent<Status>().weakness)
                {
                    Active_Enemy.GetComponent<Status>().HP -= spell.GetComponent<Status>().Dmg + 2;
                }
                else
                {
                    Active_Enemy.GetComponent<Status>().HP -= spell.GetComponent<Status>().Dmg;
                }
            }
            else
            {
                Active_Enemy.GetComponent<Status>().HP -= spell.GetComponent<Status>().Dmg;
            }
            
            

            //Apply status
            switch (status)
            {
                case Status.State.Normal:
                    break;
                case Status.State.Impaired:
                    Active_Enemy.GetComponent<Status>().setState(status);
                    break;
                case Status.State.Steamed:
                    Active_Enemy.GetComponent<Status>().setState(status);
                    break;
                case Status.State.Frozen:
                    Active_Enemy.GetComponent<Status>().setState(status);
                    break;
                default:
                    break;
            }



            Destroy(spell);

            //TODO: reset all list having to do with spells
            Airs.Clear();
            Earths.Clear();
            Fires.Clear();
            Waters.Clear();
            Elements.Clear();

            merge_finished = false;
            cast_Tracked = false;
            spellfizzlecheck = false;
            avg_pos = new Vector3 (0, 0, 0);
            //t = 0;
            //t_final = 0;

            //Active_Enemy = null;
            Active_Spell_M = null;

            Enemy_Attack();
        }

        public void Enemy_Attack()
        {
            //see if enemy is alive / if health is above 0
            if (Active_Enemy != null)
            {
                if (Active_Enemy.GetComponent<Status>().HP < 1)
                {
                    Debug.text = "Enemy Killed";
                    //Remove Enemy from Enemies list
                    Enemies.Remove(Active_Enemy);
                    //Blow up the enemy
                    Destroy(Active_Enemy);
                    Active_Enemy = null;
                    Enemy_Health_Text.text = "Dead";
                    //TODO: Play some animation

                    return;
                }
           
                switch (Active_Enemy.GetComponent<Status>().Current_State)
                {
                    case Status.State.Normal:
                        switch (player)
                        {
                            case Player.None:
                                break;
                            case Player.Poly:
                                Poly_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            case Player.Bed:
                                Bed_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            case Player.Jap:
                                Jap_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            case Player.Zoro:
                                Zoro_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            default:
                                break;
                        }
                        break;
                    case Status.State.Impaired:
                        switch (player)
                        {
                            case Player.None:
                                break;
                            case Player.Poly:
                                Poly_Health -= Active_Enemy.GetComponent<Status>().Dmg / 2;
                                break;
                            case Player.Bed:
                                Bed_Health -= Active_Enemy.GetComponent<Status>().Dmg / 2;
                                break;
                            case Player.Jap:
                                Jap_Health -= Active_Enemy.GetComponent<Status>().Dmg / 2;
                                break;
                            case Player.Zoro:
                                Zoro_Health -= Active_Enemy.GetComponent<Status>().Dmg / 2;
                                break;
                            default:
                                break;
                        }
                        Active_Enemy.GetComponent<Status>().Current_State = Status.State.Normal;
                        break;
                    case Status.State.Steamed:
                        switch (player)
                        {
                            case Player.None:
                                break;
                            case Player.Poly:
                                Poly_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            case Player.Bed:
                                Bed_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            case Player.Jap:
                                Jap_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            case Player.Zoro:
                                Zoro_Health -= Active_Enemy.GetComponent<Status>().Dmg;
                                break;
                            default:
                                break;
                        }
                        Active_Enemy.GetComponent<Status>().HP -= 2;
                        break;
                    case Status.State.Frozen:
                        Active_Enemy.GetComponent<Status>().Current_State = Status.State.Normal;
                        break;
                    default:
                        break;
                }

                if (Active_Enemy.GetComponent<Status>().HP < 1)
                {
                    Debug.text = "Enemy Killed";
                    //Remove Enemy from Enemies list
                    Enemies.Remove(Active_Enemy);
                    //Blow up the enemy
                    Destroy(Active_Enemy);
                    Active_Enemy = null;
                    Enemy_Health_Text.text = "Dead";
                    //TODO: Play some animation

                }
            }
        }

        public void Spell_Fizzle()
        {
            // player inputed wrong stuff
            foreach (GameObject element in Elements)
            {
                Destroy(element);
            }
            Airs.Clear();
            Earths.Clear();
            Fires.Clear();
            Waters.Clear();
            Elements.Clear();

            merge_finished = false;
            cast_Tracked = false;
            spellfizzlecheck = false;
            avg_pos = new Vector3(0, 0, 0);
            //t = 0;
            //t_final = 0;

            //Active_Enemy = null;
            Active_Spell_M = null;
        }

        //start to merge the element and returns true when they have merged and false when they have not
        public bool Merge(List<GameObject> Elements)
        {
            if (mergeAnim_Active == null)
            {
                mergeAnim_Active = Instantiate(mergeAnim, avg_pos, Quaternion.identity);
            }

            foreach (GameObject element in Elements)
            {
                if (!merge_finished)
                {
                    Debug.text = "Merging elements";
                    // Send elements toward the enemy
                    element.transform.position = Vector3.MoveTowards(element.transform.position, avg_pos, step);
                    if (element.transform.position == avg_pos)
                    {
                        //Merge:
                        //TODO: play some fusion animation or effect
                        Elements.Remove(element);
                        Destroy(element);

                    }
                }
            }
            if (Elements.Count == 0)
            {
                Destroy(mergeAnim_Active);
                return true;
            }
            return false;
        }
    }
}
