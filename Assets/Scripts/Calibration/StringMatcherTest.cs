using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class StringMatcherTest : MonoBehaviour
{

    public string string1, string2;

    public void CalculateMatchingScore()
    {

        MatchCollection matchCollection = Regex.Matches(string1, string2, RegexOptions.IgnoreCase);
        Debug.Log(matchCollection.Count);        
        //Debug.Log(StringMatcher.LevenshteinDistance(string1, string2));
    }
}
