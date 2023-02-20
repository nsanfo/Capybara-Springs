using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapyNames : MonoBehaviour
{
    static private string[] names = { "Haruto", "Minato", "Souta", "Riku", "Haruki", "Yuuto", "Hinata", "Yuito", "Aoto", "Itsuki", "Aoi", "Souma", "Kouma", "Kaito", "Sora", "Haru", "Sousuke", "Akio", "Akira", "Botan", "Hiroto", "Fuji", "Hiroshi", "Kaito", "Jiro", "Kenji", "Kiyoshi", "Asahi", "Ren", "Yuusei", "Yui", "Mei", "Emi", "Aoi", "Tsumugi", "Himari", "Mio", "Honoka", "Ichika", "Akari", "Rio", "Koharu", "Hana", "Rin", "Sana", "Riko", "Iroha", "Yua", "Hina", "Sara", "Fumiko", "Midori", "Rika", "Suki", "Yuriko", "Nori", "Hatsu", "Chiaki", "Keiko", "Miu" };

    static public string GetRandomName()
    {
        var index = Random.Range(0, names.Length);
        return names[index];
    }
}
