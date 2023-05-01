using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<Character> getAllPlayerCharacters()
    {
        return Level.instance.GetCharactersOfFaction(EFaction.Player);
    }
    
    
    void Start()
    {
        
    }

    [ContextMenu("test get players")]
    public void test()
    {
        // Debug.Log("Triggered");
        Debug.Log($"{String.Join(",",getAllPlayerCharacters().Select((ch)=>ch.name).ToList())}");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
