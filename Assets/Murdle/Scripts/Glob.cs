using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Globs
{
    public class Glob : MonoBehaviour
    {

        private static Glob instance = null;

        // [SerializeField]


        public static Glob Instance
        {
 
            get
     
            {
                if(instance == null)

                {

                    instance = FindObjectOfType<Glob>();

                    if( instance == null)
 
                    {
             
                        GameObject go = new GameObject();
                        go.name = "SingletonController";
                        instance = go.AddComponent<Glob>();
 
                        // DontDestroyOnLoad(go);
 
                    }
         
                }
          

                return instance;

            }
     
        }
    


        void Awake()
 
        {

            if(instance == null )
 
            {
                instance = this;
 
                DontDestroyOnLoad(this.gameObject);
 
            }
      
            else
            {
                Destroy(gameObject);
 
            }
        }

    }

   
    
    
    
    

}    

