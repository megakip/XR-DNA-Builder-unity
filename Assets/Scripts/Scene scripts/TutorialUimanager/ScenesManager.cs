using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ScenesManager : MonoBehaviour
{
   public static ScenesManager instance;

   private void Awake()
   {
 
         instance = this;
    
   }
   public enum Scene
   {
      Home,
      NewProject,
      UIscene
   }
   public void LoadScene(Scene scene)
   {
      SceneManager.LoadScene(scene.ToString());
   }

   public void LoadNewGame()
   {
    SceneManager.LoadScene(Scene.NewProject.ToString());
   }

   public void LoadNextScene()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
   }

   public void LoadHome()
   {
      SceneManager.LoadScene(Scene.Home.ToString());
   }

   public void LoadUIscene()
   {
      SceneManager.LoadScene(Scene.UIscene.ToString());
   }

   public void LoadNewProject()
   {
      SceneManager.LoadScene(Scene.NewProject.ToString());
   }
}
